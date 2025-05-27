using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.WEB.Models;

[Table("task_log")]
[Index("AffectedTeamUserId", Name = "IX_task_log_affected_team_user_id")]
[Index("LogCategoryId", Name = "IX_task_log_log_category_id")]
[Index("TeamUserId", Name = "IX_task_log_team_user_id")]
public partial class TaskLog
{
    [Key]
    [Column("log_id")]
    public int LogId { get; set; }

    [Column("task_id")]
    public int TaskId { get; set; }

    [Column("team_user_id")]
    public int TeamUserId { get; set; }

    [Column("log_category_id")]
    public int LogCategoryId { get; set; }

    [Column("log_date", TypeName = "datetime")]
    public DateTime LogDate { get; set; }

    [Column("affected_team_user_id")]
    public int? AffectedTeamUserId { get; set; }

    [ForeignKey("LogCategoryId")]
    [InverseProperty("TaskLogs")]
    public virtual LogCategory LogCategory { get; set; } = null!;
}
