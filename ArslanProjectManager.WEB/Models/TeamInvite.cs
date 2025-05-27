using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.WEB.Models;

[Table("team_invite")]
[Index("InvitedById", Name = "IX_TeamInvites_InvitedById")]
[Index("TeamId", Name = "IX_TeamInvites_TeamId")]
public partial class TeamInvite
{
    [Key]
    public int TeamInviteId { get; set; }

    public int TeamId { get; set; }

    public string InvitedEmail { get; set; } = null!;

    public int InvitedById { get; set; }

    public DateTime InviteDate { get; set; }

    public int Status { get; set; }

    public DateTime? StatusChangeDate { get; set; }

    public string? StatusChangeNote { get; set; }

    [ForeignKey("TeamId")]
    [InverseProperty("TeamInvites")]
    public virtual Team Team { get; set; } = null!;
}
