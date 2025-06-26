namespace ArslanProjectManager.Core.DTOs
{
    public class RecentTaskDto : BaseDto
    {
        public string TaskName { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public DateOnly? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int BoardId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string AppointeeName { get; set; } = string.Empty;
    }
}