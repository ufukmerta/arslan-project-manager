using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core;

public class BoardTagDto : BaseDto
{
    public string BoardName { get; set; } = null!;

    public byte BoardOrder { get; set; }

    public virtual ICollection<ProjectTaskDto> ProjectTasks { get; set; } = new List<ProjectTaskDto>();
}
