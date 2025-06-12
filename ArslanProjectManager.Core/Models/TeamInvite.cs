using System;
using System.Collections.Generic;

namespace ArslanProjectManager.Core.Models;

public partial class TeamInvite : BaseEntity
{
    public int TeamId { get; set; }
    public string InvitedEmail { get; set; } = null!;
    public int InvitedById { get; set; }
    public InviteStatus Status { get; set; } = InviteStatus.Pending;
    public string? StatusChangeNote { get; set; }

    public virtual User InvitedBy { get; set; } = null!;
    public virtual Team Team { get; set; } = null!;

    public enum InviteStatus
    {
        Pending,
        Accepted,
        Rejected,
        Expired
    }
}
