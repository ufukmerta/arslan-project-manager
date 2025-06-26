namespace ArslanProjectManager.Core.DTOs
{
    public class RecentProjectDto : BaseDto
    {
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
    }

}