namespace ArslanProjectManager.Core.DTOs.CreateDTOs;

public class TaskLogCreateDto : BaseCreateDto
{
    public int TaskId { get; set; }
    public int TeamUserId { get; set; }
    public int LogCategoryId { get; set; }
    public int? AffectedTeamUserId { get; set; }
}
