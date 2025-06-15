using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.DTOs;

public class TeamInviteDto : BaseDto
{
    public int TeamId { get; set; }
    public string InvitedEmail { get; set; } = null!;
    public int InvitedById { get; set; }
    public TeamInvite.InviteStatus Status { get; set; } = TeamInvite.InviteStatus.Pending;
    public string? StatusChangeNote { get; set; }

    public virtual UserDto InvitedBy { get; set; } = null!;
    public virtual TeamDto Team { get; set; } = null!;

}
