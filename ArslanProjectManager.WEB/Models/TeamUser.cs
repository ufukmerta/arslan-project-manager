using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.WEB.Models;

[Table("team_user")]
[Index("RoleId", Name = "IX_team_user_role_id")]
[Index("TeamId", Name = "IX_team_user_team_id")]
[Index("UserId", Name = "IX_team_user_user_id")]
public partial class TeamUser
{
    [Key]
    [Column("team_user_id")]
    public int TeamUserId { get; set; }

    [Column("team_id")]
    public int TeamId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("TeamUsers")]
    public virtual Role Role { get; set; } = null!;

    [ForeignKey("TeamId")]
    [InverseProperty("TeamUsers")]
    public virtual Team Team { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("TeamUsers")]
    public virtual User User { get; set; } = null!;
}
