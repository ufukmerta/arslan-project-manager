using System.ComponentModel.DataAnnotations;

namespace ArslanProjectManager.Core.ViewModels
{
    public class ProjectViewModel
    {
        public int ProjectId { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }        

        public string TeamName { get; set; } = string.Empty;

        public int TeamId { get; set; }

        public int TaskCount { get; set; }

        public int CompletedTaskCount { get; set; }
    }

    public class ProjectDetailsViewModel
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? ProjectDetail { get; set; }
        public DateTime StartDate { get; set; }
        public List<ProjectTaskViewModel> Tasks { get; set; } = new();
    }

    public class CreateProjectViewModel
    {
        [Required]
        [StringLength(100)]
        public string ProjectName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }


        [Required]
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
        public DateTime StartDate { get; set; }      

        [Required]
        public int TeamId { get; set; }
    }

    public class DeleteProjectViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public int TaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
    }
}