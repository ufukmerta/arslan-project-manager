using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.DTOs;

public class TaskCommentDto : BaseDto
{
    public int TaskId { get; set; }
    public int TeamUserId { get; set; }
    public string Comment { get; set; } = null!;

    public virtual ProjectTaskDto Task { get; set; } = null!;
    public virtual TeamUserDto TeamUser { get; set; } = null!;
}