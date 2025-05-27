using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.WEB.Models;

[Table("log_category")]
public partial class LogCategory
{
    [Key]
    [Column("log_category_id")]
    public int LogCategoryId { get; set; }

    [Column("log_category")]
    [StringLength(50)]
    public string LogCategory1 { get; set; } = null!;

    [InverseProperty("LogCategory")]
    public virtual ICollection<TaskLog> TaskLogs { get; set; } = new List<TaskLog>();
}
