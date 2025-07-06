using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArslanProjectManager.Core.DTOs.CreateDTOs;

public class TeamInviteCreateViewDto : BaseCreateDto
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = null!;
    public string InvitedEmail { get; set; } = null!;
    public int InvitedById { get; set; }
    public string InviterName { get; set; } = null!;
}
