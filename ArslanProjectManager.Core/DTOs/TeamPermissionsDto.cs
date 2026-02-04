namespace ArslanProjectManager.Core.DTOs;

/// <summary>
/// DTO for the team permissions page (team info + list of users with their roles and permissions).
/// </summary>
public class TeamPermissionsDto
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = null!;
    public int ManagerId { get; set; }
    public bool CanManagePermissions { get; set; }
    public List<TeamUserRoleDto> Users { get; set; } = [];
}
