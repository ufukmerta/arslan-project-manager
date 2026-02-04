using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;
using ArslanProjectManager.Service.Utilities;

namespace ArslanProjectManager.Service.Services
{
    public class TeamUserService(ITeamUserRepository repository, IUnitOfWork unitOfWork) : GenericService<TeamUser>(repository, unitOfWork), ITeamUserService
    {
        public async Task<UserEffectivePermissionsDto> GetTeamUserWithPermissionsAsync(int teamId, int userId)
        {
            var teamUser = await repository.GetTeamUserWithRoleAsync(userId, teamId);
            var effectivePermissions = PermissionResolver.GetEffectivePermissions(teamUser!, teamUser!.Role);

            var permissionsDto = new UserEffectivePermissionsDto
            {
                UserId = teamUser.UserId,
                UserName = teamUser.User.Name,
                RoleId = teamUser.RoleId,
                RoleName = teamUser.Role.RoleName,
                IsSystemRole = teamUser.Role.IsSystemRole,
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
                CanManagePermissions = effectivePermissions.CanManagePermissions,
                HasViewTasksOverride = teamUser.CanViewTasksOverride.HasValue,
                HasEditTasksOverride = teamUser.CanEditTasksOverride.HasValue,
                HasDeleteTasksOverride = teamUser.CanDeleteTasksOverride.HasValue,
                HasAssignTasksOverride = teamUser.CanAssignTasksOverride.HasValue,
                HasViewProjectsOverride = teamUser.CanViewProjectsOverride.HasValue,
                HasEditProjectsOverride = teamUser.CanEditProjectsOverride.HasValue,
                HasDeleteProjectsOverride = teamUser.CanDeleteProjectsOverride.HasValue,
                HasInviteMembersOverride = teamUser.CanInviteMembersOverride.HasValue,
                HasRemoveMembersOverride = teamUser.CanRemoveMembersOverride.HasValue,
                HasManageRolesOverride = teamUser.CanManageRolesOverride.HasValue,
                HasManagePermissionsOverride = teamUser.CanManagePermissionsOverride.HasValue
            };

            return permissionsDto;
        }
    }
}
