using ArslanProjectManager.API.Filters;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArslanProjectManager.Core.Constants;
using static ArslanProjectManager.Core.Models.TeamInvite;
using ArslanProjectManager.Service.Utilities;
using AutoMapper;

namespace ArslanProjectManager.API.Controllers
{
    /// <summary>
    /// Manages team operations including team creation, member management, and team invitations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController(ProjectManagerDbContext context, ITokenService tokenService, ITeamService teamService, ITeamInviteService teamInviteService, ITeamUserService teamUserService, IUserService userService, IRoleService roleService, IMapper mapper) : CustomBaseController(tokenService)
    {
        /// <summary>
        /// Retrieves all teams for the authenticated user (as manager or member)
        /// </summary>
        /// <returns>List of teams that the user has access to</returns>
        /// <response code="200">Returns the list of user's teams</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If no teams are found for the user</response>
        [HttpGet()]
        [Authorize]
        public async Task<IActionResult> GetByToken()
        {
            var token = (await GetToken())!;
            var doesTeamExist = await teamService.AnyAsync(x => x.TeamUsers.Any(x => x.UserId == token.UserId));
            if (!doesTeamExist)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound));
            }

            var teams = await context.Teams
                .Include(x => x.TeamUsers)
                .Include(x => x.Manager)
                .Include(x => x.Projects)
                .Where(x => x.Manager.Id == token.UserId ||
                           x.TeamUsers.Any(x => x.UserId == token.UserId))
                .OrderBy(x => x.Id)
                .Select(x => new TeamDto
                {
                    Id = x.Id,
                    TeamName = x.TeamName,
                    Description = x.TeamName,
                    ManagerName = x.Manager.Name,
                    MemberCount = x.TeamUsers.Count,
                    ProjectCount = x.Projects.Count
                })
                .ToListAsync();

            return CreateActionResult(CustomResponseDto<IEnumerable<TeamDto>>.Success(teams, 200));
        }

        /// <summary>
        /// Retrieves detailed information about a specific team
        /// </summary>
        /// <param name="id">The unique identifier of the team</param>
        /// <returns>Detailed team information including members and projects</returns>
        /// <response code="200">Returns the team details</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have access to this team</response>
        /// <response code="404">If the team is not found</response>
        [HttpGet("{id:int}")]
        [Authorize]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        public async Task<IActionResult> Details(int id)
        {
            var token = (await GetToken())!;

            var validationResult = ValidateModel(id, x => x > 0, ErrorMessages.InvalidTeamId);
            if (validationResult != null) return validationResult;

            var teamAccessResult = await ValidateTeamAccess(id, token.UserId);
            if (teamAccessResult != null) return teamAccessResult;

            var teamDetailsDto = await context.Teams
            .Include(x => x.Manager)
            .Include(x => x.TeamUsers)
            .ThenInclude(x => x.User)
            .Include(x => x.TeamUsers)
            .ThenInclude(x => x.Role)
            .Include(x => x.Projects)
            .ThenInclude(x => x.ProjectTasks)
            .Where(x => x.Id == id && x.TeamUsers.Any(tu => tu.UserId == token.UserId))
            .Select(x => new TeamDetailsDto
            {
                Id = x.Id,
                TeamName = x.TeamName,
                Description = x.TeamName,
                ManagerId = x.ManagerId,
                ManagerName = x.Manager.Name,
                Members = x.TeamUsers.Select(u => new TeamUserDto
                {
                    UserId = u.UserId,
                    Name = u.User.Name,
                    Email = u.User.Email,
                    Role = u.Role.RoleName
                }).ToList(),
                Projects = x.Projects.Select(p => new TeamProjectDto
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName,
                    Description = p.ProjectDetail,
                    TaskCount = p.ProjectTasks.Count,
                    CompletedTaskCount = p.ProjectTasks.Count(t => t.BoardId == 3)
                }).ToList()
            })
            .FirstOrDefaultAsync();
            if (teamDetailsDto is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound));
            }

            var currentTeamUser = await context.TeamUsers
                .Include(tu => tu.Role)
                .FirstOrDefaultAsync(tu => tu.TeamId == id && tu.UserId == token.UserId);
            if (currentTeamUser != null)
            {
                var effective = PermissionResolver.GetEffectivePermissions(currentTeamUser, currentTeamUser.Role);
                teamDetailsDto.CanRemoveMembers = effective.CanRemoveMembers;
                teamDetailsDto.CanEditProjects = effective.CanEditProjects;
                if (!effective.CanViewProjects)
                {
                    teamDetailsDto.Projects = [];
                }
            }

            return CreateActionResult(CustomResponseDto<TeamDetailsDto>.Success(teamDetailsDto, 200));
        }

        /// <summary>
        /// Creates a new team
        /// </summary>
        /// <param name="model">The team creation details</param>
        /// <returns>The ID of the created team</returns>
        /// <response code="200">Returns the ID of the created team</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If the team creation fails</response>
        [HttpPost()]
        [Authorize]
        public async Task<IActionResult> Create(TeamCreateDto model)
        {
            var token = (await GetToken())!;

            var team = new Team
            {
                TeamName = model.TeamName,
                Description = model.Description,
                ManagerId = token.UserId
            };

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                context.Teams.Add(team);
                await context.SaveChangesAsync();

                context.TeamUsers.Add(new TeamUser
                {
                    UserId = token.UserId,
                    RoleId = 1,
                    TeamId = team.Id
                });

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreateActionResult(CustomResponseDto<MiniTeamDto>.Success(new MiniTeamDto { Id = team.Id }, 200));
            }
            catch
            {
                await transaction.RollbackAsync();
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToCreateTeam));
            }
        }

        /// <summary>
        /// Retrieves invitation metadata for a specific team
        /// </summary>
        /// <param name="id">The unique identifier of the team</param>
        /// <returns>Team invitation metadata including team name and inviter information</returns>
        /// <response code="200">Returns the invitation metadata</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the team is not found</response>
        [HttpGet("{id:int}/[action]-meta")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> Invite(int id)
        {
            var token = (await GetToken())!;
            var user = await userService.GetByIdAsync(token.UserId);

            var team = await context.Teams
               .Include(x => x.Manager)
               .Include(x => x.TeamUsers)
               .ThenInclude(x => x.User)
               .FirstOrDefaultAsync(x => x.Id == id);

            var teamAccessResult = await ValidateTeamAccess(id, token.UserId);
            if (teamAccessResult != null) return teamAccessResult;

            var teamInviteCreateViewDto = new TeamInviteCreateViewDto
            {
                TeamId = team!.Id,
                TeamName = team.TeamName,
                InviterName = user.Name,
            };

            return CreateActionResult(CustomResponseDto<TeamInviteCreateViewDto>.Success(teamInviteCreateViewDto, 200));
        }

        /// <summary>
        /// Sends an invitation to join a team
        /// </summary>
        /// <param name="id">The unique identifier of the team</param>
        /// <param name="model">The invitation details</param>
        /// <returns>Success or failure</returns>
        /// <response code="201">Invitation sent successfully</response>
        /// <response code="400">If the invitation already exists or user is already a member</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the team is not found</response>
        /// <response code="500">If invitation sending fails</response>
        [HttpPost("{id:int}/invites")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> Invite(int id, TeamInviteCreateDto model)
        {
            var token = (await GetToken())!;

            if (id <= 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvalidTeamId));
            }

            var team = await context.Teams
                .Include(x => x.TeamUsers)
                .ThenInclude(x => x.User)
                .Include(x => x.TeamInvites)
                .FirstOrDefaultAsync(x => x.Id == id);

            var teamAccessResult = await ValidateTeamAccess(id, token.UserId);
            if (teamAccessResult != null) return teamAccessResult;

            var invitedUser = await userService.GetByEmailAsync(model.InvitedEmail);

            if (invitedUser != null && team!.TeamUsers.Any(x => x.UserId == invitedUser.Id))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.UserAlreadyTeamMember));
            }

            if (team!.TeamInvites.Any(x => x.InvitedEmail == model.InvitedEmail && x.Status == InviteStatus.Pending))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvitationAlreadySent));
            }

            var teamInvite = new TeamInvite
            {
                TeamId = id,
                InvitedEmail = model.InvitedEmail,
                InvitedById = token.UserId,
                Status = InviteStatus.Pending,
            };

            var createdInvite = await teamInviteService.AddAsync(teamInvite);
            if (createdInvite.Id <= 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToInvite));
            }
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(201));
        }

        /// <summary>
        /// Retrieves all invitations for a specific team
        /// </summary>
        /// <param name="id">The unique identifier of the team</param>
        /// <returns>List of invitations</returns>
        /// <response code="200">Returns the list of invitations</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the team is not found</response>
        [HttpGet("{id:int}/[action]")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> Invites(int id)
        {
            var token = (await GetToken())!;

            var validationResult = ValidateModel(id, x => x > 0, ErrorMessages.InvalidTeamId);
            if (validationResult != null) return validationResult;

            var team = await context.Teams
                .Include(x => x.TeamUsers)
                .Include(x => x.TeamInvites)
                .ThenInclude(x => x.InvitedBy)
                .FirstOrDefaultAsync(x => x.Id == id);

            var teamAccessResult = await ValidateTeamAccess(id, token.UserId);
            if (teamAccessResult != null) return teamAccessResult;

            var teamInvites = team!.TeamInvites
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

            return CreateActionResult(CustomResponseDto<IEnumerable<TeamInviteListDto>>.Success(teamInvites, 200));
        }

        /// <summary>
        /// Cancels an invitation
        /// </summary>
        /// <param name="teamId">The unique identifier of the team</param>
        /// <param name="id">The invitation ID to cancel</param>
        /// <returns>Success or failure</returns>
        /// <response code="200">Invitation cancelled successfully</response>
        /// <response code="400">If the invitation ID is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized to cancel the invitation</response>
        /// <response code="404">If the invitation is not found</response>
        [HttpDelete("/api/invites/{id:int}")]
        [ServiceFilter(typeof(NotFoundFilter<TeamInvite>))]
        [Authorize]
        public async Task<IActionResult> CancelInvite(int id)
        {
            var token = (await GetToken())!;

            if (id <= 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvalidInviteId));
            }

            var teamInvite = await context.TeamInvites
                .Include(x => x.Team)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (teamInvite!.Status != InviteStatus.Pending)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.OnlyPendingInvitationsCanBeCanceled));
            }

            var isAuthorized = teamInvite.Team.ManagerId == token.UserId ||
                              teamInvite.InvitedById == token.UserId;

            if (!isAuthorized)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotAuthorizedToCancelInvitation));
            }

            teamInvite.Status = InviteStatus.Rejected;
            teamInvite.StatusChangeNote = "Canceled by " + (teamInvite.InvitedById == token.UserId ? "inviter" : "team manager");
            teamInviteService.Update(teamInvite);
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(200));
        }

        /// <summary>
        /// Retrieves permissions for all team members
        /// </summary>
        /// <param name="id">The unique identifier of the team</param>
        /// <returns>List of team members with their permissions</returns>
        /// <response code="200">Returns the list of team members with permissions</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have access to this team</response>
        /// <response code="404">If the team is not found</response>
        [HttpGet("{id:int}/permissions")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> GetPermissions(int id)
        {
            var token = (await GetToken())!;

            var validationResult = ValidateModel(id, x => x > 0, ErrorMessages.InvalidTeamId);
            if (validationResult != null) return validationResult;

            var teamAccessResult = await ValidateTeamAccess(id, token.UserId);
            if (teamAccessResult != null) return teamAccessResult;            

            var teamPermissionsDto = await teamService.GetUserTeamPermissionsAsync(id, token);
            return CreateActionResult(CustomResponseDto<TeamPermissionsDto>.Success(teamPermissionsDto, 200));
        }

        /// <summary>
        /// Gets all available roles for a team (system roles + team-specific roles)
        /// </summary>
        /// <param name="teamId">The unique identifier of the team</param>
        /// <returns>List of available roles</returns>
        /// <response code="200">Returns the list of roles</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have access to this team</response>
        /// <response code="404">If the team is not found</response>
        [HttpGet("{teamId:int}/roles")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> GetRoles(int teamId)
        {
            var token = (await GetToken())!;

            var teamAccessResult = await ValidateTeamAccess(teamId, token.UserId);
            if (teamAccessResult != null) return teamAccessResult;

            var allRoles = await roleService.GetRolesForTeamAsync(teamId);
            return CreateActionResult(CustomResponseDto<IEnumerable<RoleDto>>.Success(allRoles, 200));
        }

        /// <summary>
        /// Creates a new team-specific role
        /// </summary>
        /// <param name="teamId">The unique identifier of the team</param>
        /// <param name="model">The role creation details</param>
        /// <returns>Success or failure</returns>
        /// <response code="204">Role created successfully</response>
        /// <response code="400">If the role name already exists or validation fails</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not a team manager</response>
        /// <response code="404">If the team is not found</response>
        [HttpPost("{teamId:int}/roles")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> CreateRole(int teamId, RoleCreateDto model)
        {
            var token = (await GetToken())!;

            var permissionResult = await ValidateCanManageRoles(token, teamId);
            if (permissionResult != null) return permissionResult;

            // Check if role name already exists for this team
            var roleExists = await context.Roles
                .AnyAsync(r => r.TeamId == teamId && r.RoleName == model.RoleName);
            if (roleExists)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.RoleNameAlreadyExists));
            }

            var role = mapper.Map<Role>(model);
            role.TeamId = teamId;

            try
            {
                await roleService.AddAsync(role);
                return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
            }
            catch
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToCreateRole));
            }
        }

        /// <summary>
        /// Updates a team-specific role
        /// </summary>
        /// <param name="teamId">The unique identifier of the team</param>
        /// <param name="roleId">The unique identifier of the role</param>
        /// <param name="model">The role update details</param>
        /// <returns>Success or failure</returns>
        /// <response code="200">Role updated successfully</response>
        /// <response code="400">If validation fails</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not a team manager or role is system role</response>
        /// <response code="404">If the team or role is not found</response>
        [HttpPut("{teamId:int}/roles/{roleId:int}")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> UpdateRole(int teamId, int roleId, RoleUpdateDto model)
        {
            var token = (await GetToken())!;

            var permissionResult = await ValidateCanManageRoles(token, teamId);
            if (permissionResult != null) return permissionResult;

            var role = await context.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId && r.TeamId == teamId);

            if (role == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.RoleNotFound));
            }

            if (role.IsSystemRole)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.SystemRoleCannotBeModified));
            }

            // Check if role name already exists for this team (excluding current role)
            var roleNameExists = await context.Roles
                .AnyAsync(r => r.TeamId == teamId && r.RoleName == model.RoleName && r.Id != roleId);

            if (roleNameExists)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.RoleNameAlreadyExists));
            }

            var roleUpdate = mapper.Map<RoleUpdateDto, Role>(model, role);

            try
            {
                roleService.Update(role);

                return CreateActionResult(CustomResponseDto<NoContentDto>.Success(200));
            }
            catch
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToUpdateRole));
            }
        }

        /// <summary>
        /// Deletes a team-specific role
        /// </summary>
        /// <param name="teamId">The unique identifier of the team</param>
        /// <param name="roleId">The unique identifier of the role</param>
        /// <returns>Success or failure</returns>
        /// <response code="200">Role deleted successfully</response>
        /// <response code="400">If the role is in use</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not a team manager or role is system role</response>
        /// <response code="404">If the team or role is not found</response>
        [HttpDelete("{teamId:int}/roles/{roleId:int}")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> DeleteRole(int teamId, int roleId)
        {
            var token = (await GetToken())!;

            var managerResult = await ValidateTeamManagerAccess(teamId);
            if (managerResult is not null) return managerResult;

            var role = await context.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId && r.TeamId == teamId);

            if (role is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.RoleNotFound));
            }

            if (role.IsSystemRole)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.SystemRoleCannotBeModified));
            }

            // Check if role is in use
            var roleInUse = await context.TeamUsers
                .AnyAsync(tu => tu.TeamId == teamId && tu.RoleId == roleId);

            if (roleInUse)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.RoleInUse));
            }

            try
            {
                role.IsActive = false;
                roleService.Update(role);

                return CreateActionResult(CustomResponseDto<NoContentDto>.Success(200));
            }
            catch
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToDeleteRole));
            }
        }

        /// <summary>
        /// Clones a system role as a team-specific role
        /// </summary>
        /// <param name="teamId">The unique identifier of the team</param>
        /// <param name="model">The clone details (source role ID and new role name)</param>
        /// <returns>The created role</returns>
        /// <response code="201">Role cloned successfully</response>
        /// <response code="400">If the role name already exists or source role is not a system role</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not a team manager</response>
        /// <response code="404">If the team or source role is not found</response>
        [HttpPost("{teamId:int}/roles/clone")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> CloneRole(int teamId, RoleCloneDto model)
        {
            var token = (await GetToken())!;

            var permissionResult = await ValidateCanManageRoles(token, teamId);
            if (permissionResult != null) return permissionResult;

            var sourceRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Id == model.SourceRoleId && r.IsSystemRole && r.TeamId == null);

            if (sourceRole == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.RoleNotFound));
            }

            // Check if role name already exists for this team
            var roleExists = await context.Roles
                .AnyAsync(r => r.TeamId == teamId && r.RoleName == model.NewRoleName);

            // Only active roles are considered duplicates. Soft-deleted roles
            // (IsActive = false) are ignored so the user can safely reuse names
            // without ever knowing about soft deletion.
            if (roleExists)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.RoleNameAlreadyExists));
            }

            // Create a new role instance by copying permissions from the source role.
            var newRole = new Role
            {
                RoleName = model.NewRoleName,
                TeamId = teamId,
                IsSystemRole = false,
                
                // Copy task permissions
                CanViewTasks = sourceRole.CanViewTasks,
                CanEditTasks = sourceRole.CanEditTasks,
                CanDeleteTasks = sourceRole.CanDeleteTasks,
                CanAssignTasks = sourceRole.CanAssignTasks,

                // Copy project permissions
                CanViewProjects = sourceRole.CanViewProjects,
                CanEditProjects = sourceRole.CanEditProjects,
                CanDeleteProjects = sourceRole.CanDeleteProjects,

                // Copy team management permissions
                CanInviteMembers = sourceRole.CanInviteMembers,
                CanRemoveMembers = sourceRole.CanRemoveMembers,
                CanManageRoles = sourceRole.CanManageRoles,
                CanManagePermissions = sourceRole.CanManagePermissions,

                // Base entity fields – rely on defaults for IsActive / CreatedDate
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = null,
                IsActive = true
            };

            try
            {
                await roleService.AddAsync(newRole);
                return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
            }
            catch
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToCreateRole));
            }
        }

        /// <summary>
        /// Updates permissions for a specific team user (Team Manager only)
        /// </summary>
        /// <param name="teamId">The unique identifier of the team</param>
        /// <param name="userId">The unique identifier of the user</param>
        /// <param name="model">The permission update details</param>
        /// <returns>Success or failure</returns>
        /// <response code="200">Permissions updated successfully</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not a team manager</response>
        /// <response code="404">If the team or user is not found</response>
        [HttpPut("{teamId:int}/users/{userId:int}/permissions")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> UpdateUserPermissions(int teamId, int userId, UserPermissionUpdateDto model)
        {
            var token = (await GetToken())!;

            var permissionResult = await ValidateCanManagePermissions(token, teamId);
            if (permissionResult != null) return permissionResult;

            var team = await context.Teams
                .Include(t => t.TeamUsers)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            // Validate the user is in the team
            var teamUser = team!.TeamUsers.FirstOrDefault(tu => tu.UserId == userId);
            if (teamUser == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound));
            }

            // Prevent manager from changing their own permissions
            if (userId == team.ManagerId)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.CannotChangeManagerPermissions));
            }

            // Update role if provided
            if (model.RoleId.HasValue)
            {
                var role = await context.Roles.FindAsync(model.RoleId.Value);
                if (role == null)
                {
                    return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvalidRoleId));
                }

                // Ensure the role is either a system role or belongs to this team
                if (role.TeamId.HasValue && role.TeamId != teamId)
                {
                    return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvalidRoleId));
                }

                teamUser.RoleId = model.RoleId.Value;
            }

            // Get the current role to check default permissions
            var currentRole = await context.Roles.FindAsync(teamUser.RoleId);
            if (currentRole == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.RoleNotFound));
            }

            // Update permission overrides
            if (model.CanViewTasks.HasValue)
            {
                teamUser.CanViewTasksOverride = model.CanViewTasks.Value == currentRole.CanViewTasks ? null : model.CanViewTasks.Value;
            }
            if (model.CanEditTasks.HasValue)
            {
                teamUser.CanEditTasksOverride = model.CanEditTasks.Value == currentRole.CanEditTasks ? null : model.CanEditTasks.Value;
            }
            if (model.CanDeleteTasks.HasValue)
            {
                teamUser.CanDeleteTasksOverride = model.CanDeleteTasks.Value == currentRole.CanDeleteTasks ? null : model.CanDeleteTasks.Value;
            }
            if (model.CanAssignTasks.HasValue)
            {
                teamUser.CanAssignTasksOverride = model.CanAssignTasks.Value == currentRole.CanAssignTasks ? null : model.CanAssignTasks.Value;
            }
            if (model.CanViewProjects.HasValue)
            {
                teamUser.CanViewProjectsOverride = model.CanViewProjects.Value == currentRole.CanViewProjects ? null : model.CanViewProjects.Value;
            }
            if (model.CanEditProjects.HasValue)
            {
                teamUser.CanEditProjectsOverride = model.CanEditProjects.Value == currentRole.CanEditProjects ? null : model.CanEditProjects.Value;
            }
            if (model.CanDeleteProjects.HasValue)
            {
                teamUser.CanDeleteProjectsOverride = model.CanDeleteProjects.Value == currentRole.CanDeleteProjects ? null : model.CanDeleteProjects.Value;
            }
            if (model.CanInviteMembers.HasValue)
            {
                teamUser.CanInviteMembersOverride = model.CanInviteMembers.Value == currentRole.CanInviteMembers ? null : model.CanInviteMembers.Value;
            }
            if (model.CanRemoveMembers.HasValue)
            {
                teamUser.CanRemoveMembersOverride = model.CanRemoveMembers.Value == currentRole.CanRemoveMembers ? null : model.CanRemoveMembers.Value;
            }

            if (model.CanManageRoles.HasValue)
            {
                teamUser.CanManageRolesOverride = model.CanManageRoles.Value == currentRole.CanManageRoles ? null : model.CanManageRoles.Value;
            }

            if (model.CanManagePermissions.HasValue)
            {
                teamUser.CanManagePermissionsOverride = model.CanManagePermissions.Value == currentRole.CanManagePermissions ? null : model.CanManagePermissions.Value;
            }

            teamUser.UpdatedDate = DateTime.UtcNow;

            try
            {
                context.TeamUsers.Update(teamUser);
                await context.SaveChangesAsync();

                return CreateActionResult(CustomResponseDto<NoContentDto>.Success(200));
            }
            catch
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToUpdatePermissions));
            }
        }

        /// <summary>
        /// Gets effective permissions for a specific team user
        /// </summary>
        /// <param name="teamId">The unique identifier of the team</param>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>Effective permissions for the user</returns>
        /// <response code="200">Returns the effective permissions</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have access to this team</response>
        /// <response code="404">If the team or user is not found</response>
        [HttpGet("{teamId:int}/users/{userId:int}/permissions")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> GetUserPermissions(int teamId, int userId)
        {
            var token = (await GetToken())!;

            var teamAccessResult = await ValidateTeamAccess(teamId, token.UserId);
            if (teamAccessResult != null) return teamAccessResult;

            var permissionsDto = await teamUserService.GetTeamUserWithPermissionsAsync(teamId, userId);

            return CreateActionResult(CustomResponseDto<UserEffectivePermissionsDto>.Success(permissionsDto, 200));
        }

        protected async Task<IActionResult?> ValidateTeamAccess(int teamId, int userId)
        {
            var token = (await GetToken())!;
            if (token.UserId != userId)
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.AccessDenied));

            var team = await context.Teams
                .Include(t => t.TeamUsers)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team is null)
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound));

            var isUserInTeam = team.ManagerId == token.UserId ||
                              team.TeamUsers.Any(x => x.UserId == token.UserId);

            if (!isUserInTeam)
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotTeamMember));

            return null;
        }

        /// <summary>
        /// Validates that the current user is a team manager
        /// </summary>
        protected async Task<IActionResult?> ValidateTeamManagerAccess(int teamId)
        {
            var token = (await GetToken())!;
            var team = await context.Teams.FindAsync(teamId);
            if (team == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound));
            }

            if (team.ManagerId != token.UserId)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotAuthorizedToManageRoles));
            }

            return null;
        }

        /// <summary>
        /// Validates that the current user has CanManageRoles permission
        /// </summary>
        protected async Task<IActionResult?> ValidateCanManageRoles(Token token, int teamId)
        {
            var teamAccessResult = await ValidateTeamAccess(teamId, token.UserId);
            if (teamAccessResult != null) return teamAccessResult;

            var teamUser = await context.TeamUsers
                .Include(tu => tu.Role)
                .FirstOrDefaultAsync(tu => tu.TeamId == teamId && tu.UserId == token.UserId);

            //teamUser cannot be null here because of ValidateTeamAccess check
            var effectivePermissions = PermissionResolver.GetEffectivePermissions(teamUser!, teamUser!.Role);
            if (!effectivePermissions.CanManageRoles)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotAuthorizedToManageRoles));
            }

            return null;
        }

        /// <summary>
        /// Validates that the current user has CanManagePermissions permission
        /// </summary>
        protected async Task<IActionResult?> ValidateCanManagePermissions(Token token, int teamId)
        {
            var teamAccessResult = await ValidateTeamAccess(teamId, token.UserId);
            if (teamAccessResult != null) return teamAccessResult;

            var teamUser = await context.TeamUsers
                .Include(tu => tu.Role)
                .FirstOrDefaultAsync(tu => tu.TeamId == teamId && tu.UserId == token.UserId);

            //teamUser cannot be null here because of ValidateTeamAccess check
            var effectivePermissions = PermissionResolver.GetEffectivePermissions(teamUser!, teamUser!.Role);
            if (!effectivePermissions.CanManagePermissions)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotAuthorizedToManageRoles));
            }

            return null;
        }
    }
}
