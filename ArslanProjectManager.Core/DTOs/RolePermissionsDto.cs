namespace ArslanProjectManager.Core.DTOs;

/// <summary>
/// Represents the permission structure for roles
/// </summary>
public class RolePermissionsDto
{
    // Task permissions
    public bool CanViewTasks { get; set; }
    public bool CanEditTasks { get; set; }
    public bool CanDeleteTasks { get; set; }
    public bool CanAssignTasks { get; set; }
    
    // Project permissions
    public bool CanViewProjects { get; set; }
    public bool CanEditProjects { get; set; }
    public bool CanDeleteProjects { get; set; }
    
    // Team management permissions
    public bool CanInviteMembers { get; set; }
    public bool CanRemoveMembers { get; set; }
    public bool CanManageRoles { get; set; }
    public bool CanManagePermissions { get; set; }
}
