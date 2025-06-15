using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.DTOs;

public class TaskLogDto : BaseDto
{
    public int TaskId { get; set; }
    public int TeamUserId { get; set; }
    public int LogCategoryId { get; set; }
    public int? AffectedTeamUserId { get; set; }

    public virtual ProjectTaskDto Task { get; set; } = null!;
    public virtual TeamUserDto TeamUser { get; set; } = null!;
    public virtual TeamUserDto? AffectedTeamUser { get; set; }
    public virtual LogCategoryDto LogCategory { get; set; } = null!;
}