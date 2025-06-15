﻿using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArslanProjectManager.Core.DTOs.CreateDTOs;

public class TeamInviteCreateDto : BaseCreateDto
{
    public int TeamId { get; set; }
    public string InvitedEmail { get; set; } = null!;
    public int InvitedById { get; set; }
    public TeamInvite.InviteStatus Status { get; set; } = TeamInvite.InviteStatus.Pending;
    public string? StatusChangeNote { get; set; }

    public virtual User InvitedBy { get; set; } = null!;
    public virtual Team Team { get; set; } = null!;

}
