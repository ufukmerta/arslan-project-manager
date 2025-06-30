using System.ComponentModel.DataAnnotations;

namespace ArslanProjectManager.Core.ViewModels
{
    public class ProjectViewModel
    {
        public int ProjectId { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateOnly StartDate { get; set; }

        public string TeamName { get; set; } = string.Empty;

        public int TeamId { get; set; }

        public int ManagerId { get; set; }

        public int TaskCount { get; set; }

        public int CompletedTaskCount { get; set; }
    }

    public class ProjectDetailsViewModel
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? ProjectDetail { get; set; }
        public DateOnly StartDate { get; set; }
        public List<ProjectTaskViewModel> Tasks { get; set; } = [];
    }

    public class CreateProjectViewModel
    {
        [Required(ErrorMessage = "Project name is required.")]
        [StringLength(100)]
        public string ProjectName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateOnly StartDate { get; set; }


        [Required(ErrorMessage ="Team is required to create project.")]
        public int TeamId { get; set; }
    }

    public class ChooseTeamViewModel
    {
        [Required]
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
    }

    public class EditProjectViewModel
    {
        public int ProjectId { get; set; }

        [Required]
        [StringLength(100)]
        public string ProjectName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public int TeamId { get; set; }
    }

    public class DeleteProjectViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateOnly StartDate { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public int TaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
    }
}