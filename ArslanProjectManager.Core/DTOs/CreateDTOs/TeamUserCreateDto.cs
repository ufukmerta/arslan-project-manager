namespace ArslanProjectManager.Core.DTOs.CreateDTOs;

public class TeamUserCreateDto : BaseCreateDto
{
    public int TeamId { get; set; }
    public int UserId { get; set; }
    public int RoleId { get; set; }
}
