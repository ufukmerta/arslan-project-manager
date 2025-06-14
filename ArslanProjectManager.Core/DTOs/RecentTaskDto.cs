namespace ArslanProjectManager.Core.DTOs
{
    public class RecentTaskDto
    {
        public int Id { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? StartingDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int BoardId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string AppointeeName { get; set; } = string.Empty;
    }

}