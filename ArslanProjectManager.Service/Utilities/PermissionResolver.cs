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
