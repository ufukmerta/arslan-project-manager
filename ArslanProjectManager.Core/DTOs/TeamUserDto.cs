using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.DTOs;

public class TeamUserDto : BaseDto
{
    public int TeamId { get; set; }
    public int UserId { get; set; }
    public int RoleId { get; set; }

    public virtual ICollection<ProjectTaskDto> ProjectTaskAppointees { get; set; } = new List<ProjectTaskDto>();
    public virtual ICollection<ProjectTaskDto> ProjectTaskAppointers { get; set; } = new List<ProjectTaskDto>();
    public virtual RoleDto Role { get; set; } = null!;
    public virtual TeamDto Team { get; set; } = null!;
    public virtual UserDto User { get; set; } = null!;
    public virtual ICollection<TaskCommentDto> TaskComments { get; set; } = new List<TaskCommentDto>();
    public virtual ICollection<TaskLogDto> TaskLogs { get; set; } = new List<TaskLogDto>();
    public virtual ICollection<TaskLogDto> AffectedTaskLogs { get; set; } = new List<TaskLogDto>();
}