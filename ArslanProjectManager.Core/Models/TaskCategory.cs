using System;
using System.Collections.Generic;

namespace ArslanProjectManager.Core.Models;

public partial class TaskCategory : BaseEntity
{
    public string Category { get; set; } = null!;

    public virtual ICollection<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();
}
