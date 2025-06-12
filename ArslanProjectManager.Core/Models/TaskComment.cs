using System;
using System.Collections.Generic;

namespace ArslanProjectManager.Core.Models;

public partial class TaskComment : BaseEntity
{
    public int TaskId { get; set; }
    public int TeamUserId { get; set; }
    public string Comment { get; set; } = null!;

    public virtual ProjectTask Task { get; set; } = null!;
    public virtual TeamUser TeamUser { get; set; } = null!;
}
