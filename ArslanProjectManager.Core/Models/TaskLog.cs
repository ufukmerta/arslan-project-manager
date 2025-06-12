using System;
using System.Collections.Generic;

namespace ArslanProjectManager.Core.Models;

public partial class TaskLog : BaseEntity
{
    public int TaskId { get; set; }
    public int TeamUserId { get; set; }
    public int LogCategoryId { get; set; }
    public int? AffectedTeamUserId { get; set; }

    public virtual ProjectTask Task { get; set; } = null!;
    public virtual TeamUser TeamUser { get; set; } = null!;
    public virtual TeamUser? AffectedTeamUser { get; set; }
    public virtual LogCategory LogCategory { get; set; } = null!;
}
