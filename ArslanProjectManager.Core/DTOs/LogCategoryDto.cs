using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core;

public class LogCategoryDto : BaseDto
{
    public string Category { get; set; } = null!;
    public virtual ICollection<TaskLogDto> TaskLogs { get; set; } = new List<TaskLogDto>();
}