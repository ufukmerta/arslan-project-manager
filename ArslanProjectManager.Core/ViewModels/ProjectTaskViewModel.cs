using ArslanProjectManager.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace ArslanProjectManager.Core.ViewModels
{
    public class ProjectTaskViewModel
    {
        public int Id { get; set; }

        public string TaskName { get; set; } = string.Empty;

        public DateTime CreationDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        
        public int TaskCategoryId { get; set; }
        public string TaskCategoryName { get; set; } = string.Empty;

        public int BoardId { get; set; }
        public string BoardName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int AppointerId { get; set; }
        public string AppointerName { get; set; } = string.Empty;

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

    public class CreateTaskViewModel
    {
        [Required]
        [StringLength(100)]
        public string TaskName { get; set; } = string.Empty;

        [Required]
        public DateTime? ExpectedEndDate { get; set; }

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

        [Required]
        public int ProjectId { get; set; }
    }

    public class EditTaskViewModel
    {
        public int TaskId { get; set; }

        [Required]
        [StringLength(100)]
        public string TaskName { get; set; } = string.Empty;

        public DateTime? StartingDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }

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
        [Required]
        public int TeamId { get; set; }
    }

    public class DeleteTaskViewModel
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
        public string BoardName { get; set; } = string.Empty;
        public string TaskCategoryName { get; set; } = string.Empty;
        public string AppointeeName { get; set; } = string.Empty;
        public string AppointerName { get; set; } = string.Empty;
        public int Priority { get; set; }
    }
} 