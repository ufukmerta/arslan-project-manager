namespace ArslanProjectManager.Core.DTOs.UpdateDTOs;

/// <summary>
/// DTO for updating user permissions (role and/or permission overrides)
/// </summary>
public class UserPermissionUpdateDto : BaseUpdateDto
{
    /// <summary>
    /// Optional: Change the user's role. If null, role remains unchanged.
    /// </summary>
    public int? RoleId { get; set; }
    
    /// <summary>
    /// Permission overrides (null = use role default, true/false = override)
    /// </summary>
    public bool? CanViewTasks { get; set; }
    public bool? CanEditTasks { get; set; }
    public bool? CanDeleteTasks { get; set; }
    public bool? CanAssignTasks { get; set; }
    public bool? CanViewProjects { get; set; }
    public bool? CanEditProjects { get; set; }
    public bool? CanDeleteProjects { get; set; }
    public bool? CanInviteMembers { get; set; }
    public bool? CanRemoveMembers { get; set; }
    public bool? CanManageRoles { get; set; }
    public bool? CanManagePermissions { get; set; }
}
