using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.ViewModels;

public class TeamInviteListViewModel
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = null!;
    public int ManagerId { get; set; }
    public List<TeamInviteItemViewModel> Invites { get; set; } = [];
}

public class TeamInviteItemViewModel
{
    public int Id { get; set; }
    public string InvitedEmail { get; set; } = null!;
    public string InvitedByName { get; set; } = null!;
    public int InvitedById { get; set; }
    public TeamInvite.InviteStatus Status { get; set; }
    public string StatusDisplay => Status switch
    {
        TeamInvite.InviteStatus.Pending => "Pending",
        TeamInvite.InviteStatus.Accepted => "Accepted",
        TeamInvite.InviteStatus.Rejected => "Rejected",
        TeamInvite.InviteStatus.Expired => "Expired",
        _ => "Unknown"
    };
    public string StatusBadgeClass => Status switch
    {
        TeamInvite.InviteStatus.Pending => "badge bg-warning",
        TeamInvite.InviteStatus.Accepted => "badge bg-success",
        TeamInvite.InviteStatus.Rejected => "badge bg-danger",
        TeamInvite.InviteStatus.Expired => "badge bg-secondary",
        _ => "badge bg-secondary"
    };
    public string? StatusChangeNote { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
} 