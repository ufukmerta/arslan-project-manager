using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;
using ArslanProjectManager.Service.Utilities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using static ArslanProjectManager.Core.Models.TeamInvite;

namespace ArslanProjectManager.Service.Services
{
    public class TeamService(
        ITeamRepository repository,
        ITeamUserRepository teamUserRepository,
        ITeamInviteRepository teamInviteRepository,
        IRoleRepository roleRepository,
        IRoleService roleService,
        ITeamUserService teamUserService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : GenericService<Team>(repository, unitOfWork), ITeamService
    {
        public async Task<TeamUser?> GetTeamUserAsync(int teamId, int userId)
        {
            return await teamUserRepository
                .Where(x => x.TeamId == teamId && x.UserId == userId)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<TeamUser> AddTeamUserAsync(TeamUser teamUser)
        {
            await teamUserRepository.AddAsync(teamUser);
            return teamUser;
        }

        public async Task<TeamPermissionsDto> GetUserTeamPermissionsAsync(int teamId, Token token) =>
            await BuildTeamPermissionsDtoAsync(teamId, token.UserId);

        public async Task<CustomResponseDto<IEnumerable<TeamDto>>> GetUserTeamsAsync(int userId)
        {
            var doesTeamExist = await AnyAsync(x => x.ManagerId == userId || x.TeamUsers.Any(tu => tu.UserId == userId));
            if (!doesTeamExist)
            {
                return CustomResponseDto<IEnumerable<TeamDto>>.Fail(404, ErrorMessages.TeamNotFound);
            }

            var teams = await repository.GetUserTeamsAsync(userId);
            return CustomResponseDto<IEnumerable<TeamDto>>.Success(teams, 200);
        }

        public async Task<CustomResponseDto<TeamDetailsDto>> GetTeamDetailsAsync(int userId, int teamId)
        {
            var accessError = await ValidateTeamAccessAsync(teamId, userId);
            if (accessError is not null)
            {
                return CustomResponseDto<TeamDetailsDto>.Fail(accessError.StatusCode, accessError.Errors ?? []);
            }

            var teamDetailsDto = await repository.GetTeamDetailsForUserAsync(teamId, userId);
            if (teamDetailsDto is null)
            {
                return CustomResponseDto<TeamDetailsDto>.Fail(404, ErrorMessages.TeamNotFound);
            }

            var currentTeamUser = await teamUserRepository.GetTeamUserWithRoleAsync(userId, teamId);
            if (currentTeamUser != null)
            {
                var effective = PermissionResolver.GetEffectivePermissions(currentTeamUser, currentTeamUser.Role);
                teamDetailsDto.CanInviteMembers = effective.CanInviteMembers;
                teamDetailsDto.CanRemoveMembers = effective.CanRemoveMembers;
                teamDetailsDto.CanEditProjects = effective.CanEditProjects;
                teamDetailsDto.CanDeleteProjects = effective.CanDeleteProjects;
                if (!effective.CanViewProjects)
                {
                    teamDetailsDto.Projects = [];
                }
            }

            return CustomResponseDto<TeamDetailsDto>.Success(teamDetailsDto, 200);
        }

        public async Task<CustomResponseDto<MiniTeamDto>> CreateTeamAsync(int userId, TeamCreateDto model)
        {
            try
            {
                var managerRole = await roleRepository.GetSystemRoleByNameAsync(SystemRoles.Manager);
                var managerRoleId = managerRole?.Id ?? 1;

                var team = new Team
                {
                    TeamName = model.TeamName,
                    Description = model.Description,
                    ManagerId = userId,
                    TeamUsers =
                    [
                        new TeamUser
                        {
                            UserId = userId,
                            RoleId = managerRoleId
                        }
                    ]
                };

                await repository.AddAsync(team);
                await UnitOfWork.CommitAsync();

                return CustomResponseDto<MiniTeamDto>.Success(new MiniTeamDto { Id = team.Id }, 200);
            }
            catch
            {
                return CustomResponseDto<MiniTeamDto>.Fail(500, ErrorMessages.FailedToCreateTeam);
            }
        }

        public async Task<CustomResponseDto<TeamInviteCreateViewDto>> GetInviteMetaAsync(int userId, int teamId)
        {
            var permissionError = await ValidateCanInviteMembersAsync(userId, teamId);
            if (permissionError is not null)
            {
                return CustomResponseDto<TeamInviteCreateViewDto>.Fail(permissionError.StatusCode, permissionError.Errors ?? []);
            }

            var user = await userRepository.GetByIdAsync(userId);
            var team = await repository.GetTeamWithUsersAsync(teamId, asNoTracking: true);
            if (user is null || team is null)
            {
                return CustomResponseDto<TeamInviteCreateViewDto>.Fail(404, ErrorMessages.TeamNotFound);
            }

            var viewDto = new TeamInviteCreateViewDto
            {
                TeamId = team.Id,
                TeamName = team.TeamName,
                InviterName = user.Name
            };

            return CustomResponseDto<TeamInviteCreateViewDto>.Success(viewDto, 200);
        }

        public async Task<CustomResponseDto<NoContentDto>> InviteMemberAsync(int userId, int teamId, TeamInviteCreateDto model)
        {
            var permissionError = await ValidateCanInviteMembersAsync(userId, teamId);
            if (permissionError is not null)
            {
                return permissionError;
            }

            var team = await repository.GetTeamWithUsersAndInvitesAsync(teamId);
            if (team is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound);
            }

            var invitedUser = await userRepository.Where(u => u.Email == model.InvitedEmail)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (invitedUser != null && team.TeamUsers.Any(x => x.UserId == invitedUser.Id))
            {
                return CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.UserAlreadyTeamMember);
            }

            if (team.TeamInvites.Any(x => x.InvitedEmail == model.InvitedEmail && x.Status == InviteStatus.Pending))
            {
                return CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvitationAlreadySent);
            }

            var teamInvite = new TeamInvite
            {
                TeamId = teamId,
                InvitedEmail = model.InvitedEmail,
                InvitedById = userId,
                Status = InviteStatus.Pending
            };

            await teamInviteRepository.AddAsync(teamInvite);
            await UnitOfWork.CommitAsync();

            if (teamInvite.Id <= 0)
            {
                return CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToInvite);
            }

            return CustomResponseDto<NoContentDto>.Success(201);
        }

        public async Task<CustomResponseDto<IEnumerable<TeamInviteListDto>>> GetTeamInvitesAsync(int userId, int teamId)
        {
            var accessError = await ValidateTeamAccessAsync(teamId, userId);
            if (accessError is not null)
            {
                return CustomResponseDto<IEnumerable<TeamInviteListDto>>.Fail(accessError.StatusCode, accessError.Errors ?? []);
            }

            var team = await repository.GetTeamWithInvitesAndInvitersAsync(teamId);
            if (team is null)
            {
                return CustomResponseDto<IEnumerable<TeamInviteListDto>>.Fail(404, ErrorMessages.TeamNotFound);
            }

            var teamInvites = team.TeamInvites
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new TeamInviteListDto
                {
                    Id = x.Id,
                    TeamId = x.TeamId,
                    TeamName = team.TeamName,
                    ManagerId = team.ManagerId,
                    InvitedEmail = x.InvitedEmail,
                    InvitedById = x.InvitedById,
                    InvitedByName = x.InvitedBy.Name,
                    Status = x.Status,
                    StatusChangeNote = x.StatusChangeNote,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .ToList();

            return CustomResponseDto<IEnumerable<TeamInviteListDto>>.Success(teamInvites, 200);
        }

        public async Task<CustomResponseDto<NoContentDto>> CancelInviteAsync(int userId, int inviteId)
        {
            var teamInvite = await teamInviteRepository.GetInviteWithTeamByIdAsync(inviteId);
            if (teamInvite is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.InvitationNotFound);
            }

            if (teamInvite.Status != InviteStatus.Pending)
            {
                return CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.OnlyPendingInvitationsCanBeCanceled);
            }

            var isAuthorized = teamInvite.Team.ManagerId == userId || teamInvite.InvitedById == userId;
            if (!isAuthorized)
            {
                return CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotAuthorizedToCancelInvitation);
            }

            teamInvite.Status = InviteStatus.Rejected;
            teamInvite.StatusChangeNote = "Canceled by " + (teamInvite.InvitedById == userId ? "inviter" : "team manager");
            teamInviteRepository.Update(teamInvite);
            await UnitOfWork.CommitAsync();

            return CustomResponseDto<NoContentDto>.Success(200);
        }

        public async Task<CustomResponseDto<TeamPermissionsDto>> GetPermissionsAsync(int userId, int teamId)
        {
            var accessError = await ValidateTeamAccessAsync(teamId, userId);
            if (accessError is not null)
            {
                return CustomResponseDto<TeamPermissionsDto>.Fail(accessError.StatusCode, accessError.Errors ?? []);
            }

            var dto = await BuildTeamPermissionsDtoAsync(teamId, userId);
            return CustomResponseDto<TeamPermissionsDto>.Success(dto, 200);
        }

        public async Task<CustomResponseDto<IEnumerable<RoleDto>>> GetRolesAsync(int userId, int teamId)
        {
            var accessError = await ValidateTeamAccessAsync(teamId, userId);
            if (accessError is not null)
            {
                return CustomResponseDto<IEnumerable<RoleDto>>.Fail(accessError.StatusCode, accessError.Errors ?? []);
            }

            var allRoles = await roleService.GetRolesForTeamAsync(teamId);
            return CustomResponseDto<IEnumerable<RoleDto>>.Success(allRoles, 200);
        }

        public async Task<CustomResponseDto<NoContentDto>> CreateRoleAsync(int userId, int teamId, RoleCreateDto model)
        {
            var permissionError = await ValidateCanManageRolesAsync(userId, teamId);
            if (permissionError is not null)
            {
                return permissionError;
            }

            var roleExists = await roleRepository.AnyAsync(r => r.TeamId == teamId && r.RoleName == model.RoleName);
            if (roleExists)
            {
                return CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.RoleNameAlreadyExists);
            }

            var role = mapper.Map<Role>(model);
            role.TeamId = teamId;

            try
            {
                await roleService.AddAsync(role);
                return CustomResponseDto<NoContentDto>.Success(204);
            }
            catch
            {
                return CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToCreateRole);
            }
        }

        public async Task<CustomResponseDto<NoContentDto>> UpdateRoleAsync(int userId, int teamId, int roleId, RoleUpdateDto model)
        {
            var permissionError = await ValidateCanManageRolesAsync(userId, teamId);
            if (permissionError is not null)
            {
                return permissionError;
            }

            var role = await roleRepository.Where(r => r.Id == roleId && r.TeamId == teamId).FirstOrDefaultAsync();
            if (role is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.RoleNotFound);
            }

            if (role.IsSystemRole)
            {
                return CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.SystemRoleCannotBeModified);
            }

            var roleNameExists = await roleRepository.AnyAsync(r => r.TeamId == teamId && r.RoleName == model.RoleName && r.Id != roleId);
            if (roleNameExists)
            {
                return CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.RoleNameAlreadyExists);
            }

            mapper.Map(model, role);

            try
            {
                await roleService.UpdateAsync(role);
                return CustomResponseDto<NoContentDto>.Success(200);
            }
            catch
            {
                return CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToUpdateRole);
            }
        }

        public async Task<CustomResponseDto<NoContentDto>> DeleteRoleAsync(int userId, int teamId, int roleId)
        {
            var permissionError = await ValidateCanManageRolesAsync(userId, teamId);
            if (permissionError is not null)
            {
                return permissionError;
            }

            var role = await roleRepository.Where(r => r.Id == roleId && r.TeamId == teamId).FirstOrDefaultAsync();
            if (role is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.RoleNotFound);
            }

            if (role.IsSystemRole)
            {
                return CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.SystemRoleCannotBeModified);
            }

            var roleInUse = await teamUserRepository.AnyAsync(tu => tu.TeamId == teamId && tu.RoleId == roleId);
            if (roleInUse)
            {
                return CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.RoleInUse);
            }

            try
            {
                await roleService.ChangeStatusAsync(role);
                return CustomResponseDto<NoContentDto>.Success(200);
            }
            catch
            {
                return CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToDeleteRole);
            }
        }

        public async Task<CustomResponseDto<NoContentDto>> CloneRoleAsync(int userId, int teamId, RoleCloneDto model)
        {
            var permissionError = await ValidateCanManageRolesAsync(userId, teamId);
            if (permissionError is not null)
            {
                return permissionError;
            }

            var sourceRole = await roleRepository
                .Where(r => r.Id == model.SourceRoleId && r.IsSystemRole && r.TeamId == null)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (sourceRole is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.RoleNotFound);
            }

            var roleExists = await roleRepository.AnyAsync(r => r.TeamId == teamId && r.RoleName == model.NewRoleName);
            if (roleExists)
            {
                return CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.RoleNameAlreadyExists);
            }

            var newRole = new Role
            {
                RoleName = model.NewRoleName,
                TeamId = teamId,
                IsSystemRole = false,
                CanViewTasks = sourceRole.CanViewTasks,
                CanEditTasks = sourceRole.CanEditTasks,
                CanDeleteTasks = sourceRole.CanDeleteTasks,
                CanAssignTasks = sourceRole.CanAssignTasks,
                CanViewProjects = sourceRole.CanViewProjects,
                CanEditProjects = sourceRole.CanEditProjects,
                CanDeleteProjects = sourceRole.CanDeleteProjects,
                CanInviteMembers = sourceRole.CanInviteMembers,
                CanRemoveMembers = sourceRole.CanRemoveMembers,
                CanManageRoles = sourceRole.CanManageRoles,
                CanManagePermissions = sourceRole.CanManagePermissions
            };

            try
            {
                await roleService.AddAsync(newRole);
                return CustomResponseDto<NoContentDto>.Success(204);
            }
            catch
            {
                return CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToCreateRole);
            }
        }

        public async Task<CustomResponseDto<NoContentDto>> UpdateUserPermissionsAsync(
            int currentUserId,
            int teamId,
            int targetUserId,
            UserPermissionUpdateDto model)
        {
            var permissionError = await ValidateCanManagePermissionsAsync(currentUserId, teamId);
            if (permissionError is not null)
            {
                return permissionError;
            }

            var team = await repository.GetTeamWithUsersAsync(teamId, asNoTracking: false);
            if (team is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound);
            }

            var teamUser = team.TeamUsers.FirstOrDefault(tu => tu.UserId == targetUserId);
            if (teamUser is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound);
            }

            if (targetUserId == team.ManagerId)
            {
                return CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.CannotChangeManagerPermissions);
            }

            if (model.RoleId.HasValue)
            {
                var role = await roleRepository.GetByIdAsync(model.RoleId.Value);
                if (role is null || (role.TeamId.HasValue && role.TeamId != teamId))
                {
                    return CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvalidRoleId);
                }

                teamUser.RoleId = model.RoleId.Value;
            }

            var currentRole = await roleRepository.GetByIdAsync(teamUser.RoleId);
            if (currentRole is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.RoleNotFound);
            }

            PermissionResolver.ApplyPermissionOverrides(teamUser, currentRole, model);

            try
            {
                teamUserRepository.Update(teamUser);
                await UnitOfWork.CommitAsync();
                return CustomResponseDto<NoContentDto>.Success(200);
            }
            catch
            {
                return CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToUpdatePermissions);
            }
        }

        public async Task<CustomResponseDto<UserEffectivePermissionsDto>> GetUserPermissionsAsync(
            int currentUserId,
            int teamId,
            int targetUserId)
        {
            var accessError = await ValidateTeamAccessAsync(teamId, currentUserId);
            if (accessError is not null)
            {
                return CustomResponseDto<UserEffectivePermissionsDto>.Fail(accessError.StatusCode, accessError.Errors ?? []);
            }

            var permissionsDto = await teamUserService.GetTeamUserWithPermissionsAsync(teamId, targetUserId);
            if (permissionsDto is null)
            {
                return CustomResponseDto<UserEffectivePermissionsDto>.Fail(404, ErrorMessages.UserNotFound);
            }

            return CustomResponseDto<UserEffectivePermissionsDto>.Success(permissionsDto, 200);
        }

        private async Task<TeamPermissionsDto> BuildTeamPermissionsDtoAsync(int teamId, int userId)
        {
            var teamWithUsersAndRoles = await repository.GetTeamWithUsersAndRolesAsync(teamId);
            if (teamWithUsersAndRoles is null)
            {
                return new TeamPermissionsDto { TeamId = teamId };
            }

            var currentTeamUser = teamWithUsersAndRoles.TeamUsers.FirstOrDefault(x => x.UserId == userId);
            var currentUserCanManagePermissions = false;
            if (currentTeamUser != null)
            {
                var currentUserEffectivePermissions = PermissionResolver.GetEffectivePermissions(currentTeamUser, currentTeamUser.Role);
                currentUserCanManagePermissions = currentUserEffectivePermissions.CanManagePermissions;
            }

            var teamUserRoles = teamWithUsersAndRoles.TeamUsers
                .Select(x =>
                {
                    var effectivePermissions = PermissionResolver.GetEffectivePermissions(x, x.Role);
                    return new TeamUserRoleDto
                    {
                        UserId = x.UserId,
                        TeamUserId = x.Id,
                        Name = x.User.Name,
                        RoleId = x.RoleId,
                        Role = x.Role.RoleName,
                        IsSystemRole = x.Role.IsSystemRole,
                        CanViewTasks = effectivePermissions.CanViewTasks,
                        CanEditTasks = effectivePermissions.CanEditTasks,
                        CanDeleteTasks = effectivePermissions.CanDeleteTasks,
                        CanAssignTasks = effectivePermissions.CanAssignTasks,
                        CanViewProjects = effectivePermissions.CanViewProjects,
                        CanEditProjects = effectivePermissions.CanEditProjects,
                        CanDeleteProjects = effectivePermissions.CanDeleteProjects,
                        CanInviteMembers = effectivePermissions.CanInviteMembers,
                        CanRemoveMembers = effectivePermissions.CanRemoveMembers,
                        CanManageRoles = effectivePermissions.CanManageRoles,
                        CanManagePermissions = effectivePermissions.CanManagePermissions
                    };
                })
                .ToList();

            return new TeamPermissionsDto
            {
                TeamId = teamWithUsersAndRoles.Id,
                TeamName = teamWithUsersAndRoles.TeamName,
                ManagerId = teamWithUsersAndRoles.ManagerId,
                CanManagePermissions = currentUserCanManagePermissions,
                Users = teamUserRoles
            };
        }

        private async Task<CustomResponseDto<NoContentDto>?> ValidateTeamAccessAsync(int teamId, int userId)
        {
            var team = await repository.GetTeamWithUsersAsync(teamId, asNoTracking: true);
            if (team is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound);
            }

            var isUserInTeam = team.ManagerId == userId || team.TeamUsers.Any(x => x.UserId == userId);
            if (!isUserInTeam)
            {
                return CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotTeamMember);
            }

            return null;
        }

        private async Task<CustomResponseDto<NoContentDto>?> ValidateTeamManagerAccessAsync(int userId, int teamId)
        {
            var team = await repository.GetByIdAsync(teamId);
            if (team is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound);
            }

            if (team.ManagerId != userId)
            {
                return CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotAuthorizedToManageRoles);
            }

            return null;
        }

        private async Task<CustomResponseDto<NoContentDto>?> ValidateCanManageRolesAsync(int userId, int teamId)
        {
            var accessError = await ValidateTeamAccessAsync(teamId, userId);
            if (accessError is not null)
            {
                return accessError;
            }

            var teamUser = await teamUserRepository.GetTeamUserWithRoleAsync(userId, teamId);
            if (teamUser is null || !PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanManageRoles))
            {
                return CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotAuthorizedToManageRoles);
            }

            return null;
        }

        private async Task<CustomResponseDto<NoContentDto>?> ValidateCanManagePermissionsAsync(int userId, int teamId)
        {
            var accessError = await ValidateTeamAccessAsync(teamId, userId);
            if (accessError is not null)
            {
                return accessError;
            }

            var teamUser = await teamUserRepository.GetTeamUserWithRoleAsync(userId, teamId);
            if (teamUser is null || !PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanManagePermissions))
            {
                return CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotAuthorizedToManagePermissions);
            }

            return null;
        }

        private async Task<CustomResponseDto<NoContentDto>?> ValidateCanInviteMembersAsync(int userId, int teamId)
        {
            var accessError = await ValidateTeamAccessAsync(teamId, userId);
            if (accessError is not null)
            {
                return accessError;
            }

            var teamUser = await teamUserRepository.GetTeamUserWithRoleAsync(userId, teamId);
            if (teamUser is null || !PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanInviteMembers))
            {
                return CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToInviteMembers);
            }

            return null;
        }
    }
}
