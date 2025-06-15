using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArslanProjectManager.Core.DTOs.CreateDTOs;

public class TaskLogCreateDto : BaseCreateDto
{
    public int TaskId { get; set; }
    public int TeamUserId { get; set; }
    public int LogCategoryId { get; set; }
    public int? AffectedTeamUserId { get; set; }

    public virtual ProjectTask Task { get; set; } = null!;    
    public virtual TeamUser? AffectedTeamUser { get; set; }
    public virtual LogCategory LogCategory { get; set; } = null!;
}