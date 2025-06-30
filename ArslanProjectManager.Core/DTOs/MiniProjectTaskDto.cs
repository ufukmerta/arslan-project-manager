using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.DTOs
{
    public class MiniProjectTaskDto: BaseDto
    {
        public string TaskName { get; set; } = string.Empty;

        public int AppointeeId { get; set; }

        public string AppointeeName { get; set; } = string.Empty;

        public ProjectTask.TaskPriority Priority { get; set; }

        public int BoardId { get; set; }
    }
}