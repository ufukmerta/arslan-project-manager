using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;
using ArslanProjectManager.Service.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Service.Services
{
    public class TeamService(ITeamRepository repository, ITeamUserRepository teamUserRepository, IUnitOfWork unitOfWork) : GenericService<Team>(repository, unitOfWork), ITeamService
    {
        public async Task<TeamUser?> GetTeamUserAsync(int teamId, int userId)
        {
            return await teamUserRepository
                .Where(x => x.TeamId == teamId && x.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<TeamUser> AddTeamUserAsync(TeamUser teamUser)
        {
            await teamUserRepository.AddAsync(teamUser);
            return teamUser;
        }

        public async Task<TeamPermissionsDto> GetUserTeamPermissionsAsync(int teamId, Token token)
        {
            var teamWithUsersAndRoles = await repository.GetTeamWithUsersAndRolesAsync(teamId);

            // Get current user's team user record to check permissions
            var currentTeamUser = teamWithUsersAndRoles!.TeamUsers.FirstOrDefault(x => x.UserId == token.UserId);
            var currentUserCanManagePermissions = false;
            if (currentTeamUser != null)
            {
                var currentUserEffectivePermissions = PermissionResolver.GetEffectivePermissions(currentTeamUser, currentTeamUser.Role);
                currentUserCanManagePermissions = currentUserEffectivePermissions.CanManagePermissions;
            }

            var teamUserRoles = teamWithUsersAndRoles!.TeamUsers
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

            var teamPermissionsDto = new TeamPermissionsDto
            {
                TeamId = teamWithUsersAndRoles.Id,
                TeamName = teamWithUsersAndRoles.TeamName,
                ManagerId = teamWithUsersAndRoles.ManagerId,
                CanManagePermissions = currentUserCanManagePermissions,
                Users = teamUserRoles
            };

            return teamPermissionsDto;
        }
    }
}
