using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.WEB.Models;

[Table("team")]
[Index("ManagerId", Name = "IX_team_manager_id")]
public partial class Team
{
    [Key]
    [Column("team_id")]
    public int TeamId { get; set; }

    [Column("team_name")]
    [StringLength(50)]
    public string TeamName { get; set; } = null!;

    [Column("manager_id")]
    public int ManagerId { get; set; }

    [ForeignKey("ManagerId")]
    [InverseProperty("Teams")]
    public virtual User Manager { get; set; } = null!;

    [InverseProperty("Team")]
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    [InverseProperty("Team")]
    public virtual ICollection<TeamInvite> TeamInvites { get; set; } = new List<TeamInvite>();

    [InverseProperty("Team")]
    public virtual ICollection<TeamUser> TeamUsers { get; set; } = new List<TeamUser>();
}
