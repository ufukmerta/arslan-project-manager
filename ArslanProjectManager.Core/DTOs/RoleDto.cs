namespace ArslanProjectManager.Core.DTOs;

/// <summary>
/// DTO for team roles (system roles + team-specific roles)
/// </summary>
public class RoleDto : BaseDto
{
    public string RoleName { get; set; } = null!;
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsTeamRole => !IsSystemRole;
    public RolePermissionsDto Permissions { get; set; } = null!;
    public int UserCount { get; set; } // Number of users with this role in the team
}
