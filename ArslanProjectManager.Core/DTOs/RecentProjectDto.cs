namespace ArslanProjectManager.Core.DTOs
{
    public class RecentProjectDto
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
    }

}