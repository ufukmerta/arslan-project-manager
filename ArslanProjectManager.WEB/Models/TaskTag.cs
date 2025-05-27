using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.WEB.Models;

[Table("task_tag")]
[Index("TaskId", Name = "IX_task_tag_task_id")]
public partial class TaskTag
{
    [Key]
    [Column("tag_id")]
    public int TagId { get; set; }

    [Column("task_id")]
    public int TaskId { get; set; }

    [Column("tag")]
    [StringLength(50)]
    public string Tag { get; set; } = null!;

    [ForeignKey("TaskId")]
    [InverseProperty("TaskTags")]
    public virtual ProjectTask Task { get; set; } = null!;
}
