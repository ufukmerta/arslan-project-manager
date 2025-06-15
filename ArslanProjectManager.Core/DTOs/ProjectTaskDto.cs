using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.DTOs;

public class ProjectTaskDto : BaseDto
{
    public int ProjectId { get; set; }
    public string TaskName { get; set; } = null!;
    public DateTime? StartingDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ExpectedEndDate { get; set; }
    public int TaskCategoryId { get; set; }
    public int BoardId { get; set; }
    public string? Description { get; set; }
    public int AppointeeId { get; set; }
    public string ? AppointeeName { get; set; }
    public int AppointerId { get; set; }
    public string? AppointerName { get; set; }
    public ProjectTask.TaskPriority Priority { get; set; } = ProjectTask.TaskPriority.Medium;
   
    public virtual BoardTagDto Board { get; set; } = null!;    
    public virtual TaskCategoryDto TaskCategory { get; set; } = null!;
    public virtual ICollection<TaskCommentDto> TaskComments { get; set; } = new List<TaskCommentDto>();
    public virtual ICollection<TaskLogDto> TaskLogs { get; set; } = new List<TaskLogDto>();
    public virtual ICollection<TaskTagDto> TaskTags { get; set; } = new List<TaskTagDto>();
}
