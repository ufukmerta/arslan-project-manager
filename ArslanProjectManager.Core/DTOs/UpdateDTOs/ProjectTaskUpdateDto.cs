using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.DTOs.UpdateDTOs
{
    public class ProjectTaskUpdateDto : BaseUpdateDto
    {
        public string TaskName { get; set; } = default!;
        public string? Description { get; set; } = null;
        public DateOnly? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public DateOnly? ExpectedEndDate { get; set; } = null;
        public ProjectTask.TaskPriority Priority { get; set; }
        public int TaskCategoryId { get; set; }
        public int AppointeeId { get; set; }
        public int BoardId { get; set; }

        public List<TaskUserDto> TeamMembers { get; set; } = [];
        public List<BoardTagDto> BoardTags { get; set; } = [];
        public List<TaskCategoryDto> TaskCategories { get; set; } = [];

    }
}
