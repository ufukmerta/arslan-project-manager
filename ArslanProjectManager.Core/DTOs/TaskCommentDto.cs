namespace ArslanProjectManager.Core.DTOs;

public class TaskCommentDto : BaseDto
{
    public int TaskId { get; set; }
    public int TeamUserId { get; set; }
    public string Comment { get; set; } = null!;
    public string CommenterName { get; set; } = string.Empty;
}
