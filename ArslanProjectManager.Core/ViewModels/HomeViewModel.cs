using System.ComponentModel.DataAnnotations;

namespace ArslanProjectManager.Core.ViewModels
{
    public class HomeViewModel
    {
        public int TotalProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int ProjectCompletionRate => TotalProjects == 0 ? 0 : (CompletedProjects * 100 / TotalProjects);

        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int TaskCompletionRate => TotalTasks == 0 ? 0 : (CompletedTasks * 100 / TotalTasks);

        public int TotalTeams { get; set; }
        public int TotalMembers { get; set; }

        public List<RecentTaskViewModel> RecentTasks { get; set; } = [];
        public List<RecentProjectViewModel> RecentProjects { get; set; } = [];
    }
    public class RecentTaskViewModel
    {
        public int Id { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int BoardId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string AppointeeName { get; set; } = string.Empty;
    }

    public class RecentProjectViewModel
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateOnly StartDate { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
    }
}