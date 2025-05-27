using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.WEB.Models;

[Table("project_task")]
[Index("AppointeeId", Name = "IX_project_task_appointee_id")]
[Index("AppointerId", Name = "IX_project_task_appointer_id")]
[Index("BoardId", Name = "IX_project_task_board_id")]
[Index("ProjectId", Name = "IX_project_task_project_id")]
[Index("TaskCategoryId", Name = "IX_project_task_task_category_id")]
public partial class ProjectTask
{
    [Key]
    [Column("task_id")]
    public int TaskId { get; set; }

    [Column("project_id")]
    public int ProjectId { get; set; }

    [Column("task_name")]
    [StringLength(50)]
    public string TaskName { get; set; } = null!;

    [Column("creation_date")]
    public DateOnly CreationDate { get; set; }

    [Column("updated_date")]
    public DateOnly UpdatedDate { get; set; }

    [Column("starting_date")]
    public DateOnly? StartingDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("expected_end_date")]
    public DateOnly? ExpectedEndDate { get; set; }

    [Column("task_category_id")]
    public int TaskCategoryId { get; set; }

    [Column("board_id")]
    public int BoardId { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("appointer_id")]
    public int AppointerId { get; set; }

    [Column("appointee_id")]
    public int AppointeeId { get; set; }

    [Column("priority")]
    public int? Priority { get; set; }

    [ForeignKey("BoardId")]
    [InverseProperty("ProjectTasks")]
    public virtual BoardTag Board { get; set; } = null!;

    [ForeignKey("ProjectId")]
    [InverseProperty("ProjectTasks")]
    public virtual Project Project { get; set; } = null!;

    [ForeignKey("TaskCategoryId")]
    [InverseProperty("ProjectTasks")]
    public virtual TaskCategory TaskCategory { get; set; } = null!;

    [InverseProperty("Task")]
    public virtual ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();

    [InverseProperty("Task")]
    public virtual ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}
