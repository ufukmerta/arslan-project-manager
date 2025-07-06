using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.DTOs;

public class TeamInviteListDto : BaseDto
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = null!;
    public int ManagerId { get; set; }
    public string InvitedEmail { get; set; } = null!;
    public int InvitedById { get; set; }
    public string InvitedByName { get; set; } = null!;
    public TeamInvite.InviteStatus Status { get; set; }
    public string? StatusChangeNote { get; set; }
}
