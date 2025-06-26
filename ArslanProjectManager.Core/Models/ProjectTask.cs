using System;
using System.Collections.Generic;

namespace ArslanProjectManager.Core.Models;

public partial class ProjectTask : BaseEntity
{
    public int ProjectId { get; set; }
    public string TaskName { get; set; } = null!;
    public DateOnly? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateOnly? ExpectedEndDate { get; set; }
    public int TaskCategoryId { get; set; }
    public int BoardId { get; set; }
    public string? Description { get; set; }
    public int AppointeeId { get; set; }
    public int AppointerId { get; set; }
    public TaskPriority? Priority { get; set; }

    public virtual TeamUser Appointee { get; set; } = null!;
    public virtual TeamUser Appointer { get; set; } = null!;
    public virtual BoardTag Board { get; set; } = null!;
    public virtual Project Project { get; set; } = null!;
    public virtual TaskCategory TaskCategory { get; set; } = null!;
    public virtual ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();
    public virtual ICollection<TaskLog> TaskLogs { get; set; } = new List<TaskLog>();
    public virtual ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();

    public enum TaskPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
    }
}
