namespace ArslanProjectManager.Core.DTOs;

/// <summary>
/// DTO for a user row on the team permissions page (user + role + effective permissions).
/// </summary>
public class TeamUserRoleDto
{
    public int UserId { get; set; }
    public int TeamUserId { get; set; }
    public string Name { get; set; } = null!;
    public int RoleId { get; set; }
    public string Role { get; set; } = null!;
    public bool IsSystemRole { get; set; }

    // All effective permissions
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
