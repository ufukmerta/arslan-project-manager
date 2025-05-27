using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.WEB.Models;

[Table("task_comment")]
[Index("TaskId", Name = "IX_task_comment_task_id")]
[Index("TeamUserId", Name = "IX_task_comment_team_user_id")]
public partial class TaskComment
{
    [Key]
    [Column("comment_id")]
    public int CommentId { get; set; }

    [Column("task_id")]
    public int TaskId { get; set; }

    [Column("team_user_id")]
    public int TeamUserId { get; set; }

    [Column("comment")]
    public string Comment { get; set; } = null!;

    [Column("date", TypeName = "datetime")]
    public DateTime? Date { get; set; }

    [ForeignKey("TaskId")]
    [InverseProperty("TaskComments")]
    public virtual ProjectTask Task { get; set; } = null!;
}
