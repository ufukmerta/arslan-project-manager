using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Service.Utilities;

/// <summary>
/// Utility class to resolve effective permissions for team users
/// Combines role permissions with user-specific overrides
/// </summary>
public static class PermissionResolver
{
    /// <summary>
    /// Gets the effective permissions for a team user by combining role permissions with overrides
    /// </summary>
    /// <param name="teamUser">The team user entity</param>
    /// <param name="role">The role assigned to the user</param>
    /// <returns>Effective permissions object</returns>
    public static UserEffectivePermissions GetEffectivePermissions(TeamUser teamUser, Role role)
    {
        return new UserEffectivePermissions
        {
            CanViewTasks = teamUser.CanViewTasksOverride ?? role.CanViewTasks,
            CanEditTasks = teamUser.CanEditTasksOverride ?? role.CanEditTasks,
            CanDeleteTasks = teamUser.CanDeleteTasksOverride ?? role.CanDeleteTasks,
            CanAssignTasks = teamUser.CanAssignTasksOverride ?? role.CanAssignTasks,
            CanViewProjects = teamUser.CanViewProjectsOverride ?? role.CanViewProjects,
            CanEditProjects = teamUser.CanEditProjectsOverride ?? role.CanEditProjects,
            CanDeleteProjects = teamUser.CanDeleteProjectsOverride ?? role.CanDeleteProjects,
            CanInviteMembers = teamUser.CanInviteMembersOverride ?? role.CanInviteMembers,
            CanRemoveMembers = teamUser.CanRemoveMembersOverride ?? role.CanRemoveMembers,
            CanManageRoles = teamUser.CanManageRolesOverride ?? role.CanManageRoles,
            CanManagePermissions = teamUser.CanManagePermissionsOverride ?? role.CanManagePermissions
        };
    }

    /// <summary>
    /// Checks if a user has a specific permission
    /// </summary>
    public static bool HasPermission(TeamUser teamUser, Role role, Func<UserEffectivePermissions, bool> permissionCheck)
    {
        var effectivePermissions = GetEffectivePermissions(teamUser, role);
        return permissionCheck(effectivePermissions);
    }

    /// <summary>
    /// Applies permission overrides to a TeamUser by comparing the requested values against the current Role defaults.
    /// When a requested permission matches the Role default, the override is cleared (set to null).
    /// </summary>
    public static void ApplyPermissionOverrides(TeamUser teamUser, Role currentRole, UserPermissionUpdateDto model)
    {
        static bool? ResolveOverride(bool? requested, bool roleDefault, bool? existingOverride) =>
            requested.HasValue
                ? (requested.Value == roleDefault ? null : requested.Value)
                : existingOverride;

        teamUser.CanViewTasksOverride = ResolveOverride(model.CanViewTasks, currentRole.CanViewTasks, teamUser.CanViewTasksOverride);
        teamUser.CanEditTasksOverride = ResolveOverride(model.CanEditTasks, currentRole.CanEditTasks, teamUser.CanEditTasksOverride);
        teamUser.CanDeleteTasksOverride = ResolveOverride(model.CanDeleteTasks, currentRole.CanDeleteTasks, teamUser.CanDeleteTasksOverride);
        teamUser.CanAssignTasksOverride = ResolveOverride(model.CanAssignTasks, currentRole.CanAssignTasks, teamUser.CanAssignTasksOverride);
        teamUser.CanViewProjectsOverride = ResolveOverride(model.CanViewProjects, currentRole.CanViewProjects, teamUser.CanViewProjectsOverride);
        teamUser.CanEditProjectsOverride = ResolveOverride(model.CanEditProjects, currentRole.CanEditProjects, teamUser.CanEditProjectsOverride);
        teamUser.CanDeleteProjectsOverride = ResolveOverride(model.CanDeleteProjects, currentRole.CanDeleteProjects, teamUser.CanDeleteProjectsOverride);
        teamUser.CanInviteMembersOverride = ResolveOverride(model.CanInviteMembers, currentRole.CanInviteMembers, teamUser.CanInviteMembersOverride);
        teamUser.CanRemoveMembersOverride = ResolveOverride(model.CanRemoveMembers, currentRole.CanRemoveMembers, teamUser.CanRemoveMembersOverride);
        teamUser.CanManageRolesOverride = ResolveOverride(model.CanManageRoles, currentRole.CanManageRoles, teamUser.CanManageRolesOverride);
        teamUser.CanManagePermissionsOverride = ResolveOverride(model.CanManagePermissions, currentRole.CanManagePermissions, teamUser.CanManagePermissionsOverride);
    }
}

/// <summary>
/// Represents the effective permissions for a user (role permissions + overrides)
/// </summary>
public class UserEffectivePermissions
{
    public bool CanViewTasks { get; set; }
    public bool CanEditTasks { get; set; }
    public bool CanDeleteTasks { get; set; }
    public bool CanAssignTasks { get; set; }
    public bool CanViewProjects { get; set; }
    public bool CanEditProjects { get; set; }
    public bool CanDeleteProjects { get; set; }
    public bool CanInviteMembers { get; set; }
    public bool CanRemoveMembers { get; set; }
    public bool CanManageRoles { get; set; }
    public bool CanManagePermissions { get; set; }
}
