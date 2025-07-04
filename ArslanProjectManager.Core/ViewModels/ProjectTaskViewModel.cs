using ArslanProjectManager.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace ArslanProjectManager.Core.ViewModels
{
    public class ProjectTaskViewModel
    {
        public int Id { get; set; }

        public string TaskName { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateOnly? ExpectedEndDate { get; set; }

        public int TaskCategoryId { get; set; }
        public string TaskCategoryName { get; set; } = string.Empty;

        public int BoardId { get; set; }
        public string BoardName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int AppointerId { get; set; }
        public string AppointerName { get; set; } = string.Empty;

        public bool CanDelete { get; set; }

        [Required]
        public int AppointeeId { get; set; }
        public string AppointeeName { get; set; } = string.Empty;

        [Required]
        public ProjectTask.TaskPriority Priority { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;

        public List<TaskCommentViewModel> Comments { get; set; } = new();
        public CreateTaskCommentViewModel NewComment { get; set; } = new();
    }


    public class CreateTaskViewViewModel
    {
        public int ProjectId { get; set; }
        public List<TaskUserViewModel> TeamMembers { get; set; } = new List<TaskUserViewModel>();
        public List<BoardTagViewModel> BoardTags { get; set; } = new List<BoardTagViewModel>();
        public List<TaskCategoryViewModel> TaskCategories { get; set; } = new List<TaskCategoryViewModel>();
    }

    public class TaskUserViewModel
    {
        public required int TeamUserId { get; set; }
        public required string Name { get; set; }
    }

    public class BoardTagViewModel
    {
        public required int BoardId { get; set; }
        public required string BoardName { get; set; }
    }

    public class TaskCategoryViewModel
    {
        public required int TaskCategoryId { get; set; }
        public required string Category { get; set; } = string.Empty;
    }

    public class CreateTaskViewModel
    {
        [Required(ErrorMessage = "Task name is required.")]
        [StringLength(100)]
        public string TaskName { get; set; } = string.Empty;

        public DateOnly? StartDate { get; set; }

        public DateOnly? ExpectedEndDate { get; set; }

        [Required(ErrorMessage = "Task category is required.")]
        public int TaskCategoryId { get; set; }

        [Required(ErrorMessage = "Board is required.")]
        public int BoardId { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Task must be appointed to one of the team member.")]
        public int AppointeeId { get; set; }

        [Required(ErrorMessage = "Task priority is required.")]
        public ProjectTask.TaskPriority Priority { get; set; }

        [Required]
        public int ProjectId { get; set; }
    }

    public class EditTaskViewViewModel
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateOnly? ExpectedEndDate { get; set; }
        public ProjectTask.TaskPriority Priority { get; set; }
        public int TaskCategoryId { get; set; }
        public int AppointeeId { get; set; }
        public int BoardId { get; set; }

        public int TeamId { get; set; }
        public List<TaskUserViewModel> TeamMembers { get; set; } = new List<TaskUserViewModel>();
        public List<BoardTagViewModel> BoardTags { get; set; } = new List<BoardTagViewModel>();
        public List<TaskCategoryViewModel> TaskCategories { get; set; } = new List<TaskCategoryViewModel>();
    }

    public class EditTaskViewModel
    {
        public int TaskId { get; set; }

        [Required]
        [StringLength(100)]
        public string TaskName { get; set; } = string.Empty;
        public DateOnly? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateOnly? ExpectedEndDate { get; set; }

        [Required]
        public int TaskCategoryId { get; set; }

        [Required]
        public int BoardId { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int AppointeeId { get; set; }

        [Required]
        public ProjectTask.TaskPriority Priority { get; set; }
    }

    public class DeleteTaskViewModel
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string BoardName { get; set; } = string.Empty;
        public string TaskCategoryName { get; set; } = string.Empty;
        public string AppointeeName { get; set; } = string.Empty;
        public string AppointerName { get; set; } = string.Empty;
        public ProjectTask.TaskPriority Priority { get; set; }
    }
}