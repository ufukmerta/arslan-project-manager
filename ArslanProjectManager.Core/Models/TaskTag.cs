using System;
using System.Collections.Generic;

namespace ArslanProjectManager.Core.Models;

public partial class TaskTag : BaseEntity
{
    public int TaskId { get; set; }
    public string Tag { get; set; } = null!;

    public virtual ProjectTask Task { get; set; } = null!;
}
