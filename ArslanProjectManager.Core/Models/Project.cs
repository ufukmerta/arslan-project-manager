using System;
using System.Collections.Generic;

namespace ArslanProjectManager.Core.Models;

public partial class Project : BaseEntity
{
    public string ProjectName { get; set; } = null!;
    public int TeamId { get; set; }
    public string? ProjectDetail { get; set; }
    public DateOnly StartDate { get; set; }
    public virtual ICollection<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();
    public virtual Team Team { get; set; } = null!;
}
