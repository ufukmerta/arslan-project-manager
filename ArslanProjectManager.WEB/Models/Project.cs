using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.WEB.Models;

[Table("project")]
[Index("TeamId", Name = "IX_project_team_id")]
public partial class Project
{
    [Key]
    [Column("project_id")]
    public int ProjectId { get; set; }

    [Column("project_name")]
    [StringLength(50)]
    public string ProjectName { get; set; } = null!;

    [Column("team_id")]
    public int TeamId { get; set; }

    [Column("project_detail")]
    [StringLength(50)]
    public string? ProjectDetail { get; set; }

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("deadline")]
    public DateOnly Deadline { get; set; }

    [InverseProperty("Project")]
    public virtual ICollection<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();

    [ForeignKey("TeamId")]
    [InverseProperty("Projects")]
    public virtual Team Team { get; set; } = null!;
}
