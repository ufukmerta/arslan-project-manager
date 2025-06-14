using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core;

public class TaskTagDto : BaseDto
{
    public int TaskId { get; set; }
    public string Tag { get; set; } = null!;

    public virtual ProjectTaskDto Task { get; set; } = null!;
}