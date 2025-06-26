using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.DTOs;

public class ProjectDto : BaseDto
{
    public string ProjectName { get; set; } = null!;
    public int TeamId { get; set; }
    public string? ProjectDetail { get; set; }
    public DateOnly StartDate { get; set; }

    public virtual ICollection<ProjectTaskDto> ProjectTasks { get; set; } = new List<ProjectTaskDto>();
    public virtual TeamDto Team { get; set; } = null!;
}
