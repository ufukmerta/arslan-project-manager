using System.ComponentModel.DataAnnotations;

namespace ArslanProjectManager.Core.ViewModels;
public class TaskCommentViewModel
{
    public int CommentId { get; set; }
    public int TaskId { get; set; }
    public int TeamUserId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CommenterName { get; set; } = string.Empty;
}

public class CreateTaskCommentViewModel
{
    public int TaskId { get; set; }

    [Required]
    [StringLength(1000)]
    public string Comment { get; set; } = string.Empty;
}

public class TaskCommentsViewModel
{
    public int TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public List<TaskCommentViewModel> Comments { get; set; } = new();
    public CreateTaskCommentViewModel NewComment { get; set; } = new();
} 