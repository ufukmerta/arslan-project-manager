namespace ArslanProjectManager.Core.DTOs;

/// <summary>
/// DTO representing the effective permissions for a user (role permissions + overrides)
/// </summary>
public class UserEffectivePermissionsDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = null!;
    public bool IsSystemRole { get; set; }
    
    // Effective permissions (role + overrides)
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
    
    // Indicates which permissions are overridden
    public bool HasViewTasksOverride { get; set; }
    public bool HasEditTasksOverride { get; set; }
    public bool HasDeleteTasksOverride { get; set; }
    public bool HasAssignTasksOverride { get; set; }
    public bool HasViewProjectsOverride { get; set; }
    public bool HasEditProjectsOverride { get; set; }
    public bool HasDeleteProjectsOverride { get; set; }
    public bool HasInviteMembersOverride { get; set; }
    public bool HasRemoveMembersOverride { get; set; }
    public bool HasManageRolesOverride { get; set; }
    public bool HasManagePermissionsOverride { get; set; }
}
