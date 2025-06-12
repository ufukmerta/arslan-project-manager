using System;
using System.Collections.Generic;

namespace ArslanProjectManager.Core.Models;

public partial class LogCategory : BaseEntity
{
    public string Category { get; set; } = null!;
    public virtual ICollection<TaskLog> TaskLogs { get; set; } = new List<TaskLog>();
}
