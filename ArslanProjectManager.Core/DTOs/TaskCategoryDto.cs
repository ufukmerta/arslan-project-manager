using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core;

public class TaskCategoryDto : BaseDto
{
    public string Category { get; set; } = null!;

    public virtual ICollection<ProjectTaskDto> ProjectTasks { get; set; } = new List<ProjectTaskDto>();
}