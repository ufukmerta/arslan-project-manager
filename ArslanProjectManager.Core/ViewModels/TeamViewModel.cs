using System.ComponentModel.DataAnnotations;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.ViewModels
{
    public class TeamViewModel
    {
        public int TeamId { get; set; }

        [Required]
        [StringLength(100)]
        public string TeamName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string ManagerName { get; set; } = string.Empty;

        public int MemberCount { get; set; }

        public int ProjectCount { get; set; }
    }

    public class TeamDetailsViewModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = null!;
        public string? Description { get; set; }
        public int ManagerId { get; set; }
        public string ManagerName { get; set; } = null!;
        public List<TeamMemberViewModel> Members { get; set; } = [];
        public List<ProjectViewModel> Projects { get; set; } = [];
    }

    public class TeamCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string TeamName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class TeamEditViewModel
    {
        public int TeamId { get; set; }

        [Required]
        [StringLength(100)]
        public string TeamName { get; set; } = string.Empty;

        [Required]
        public int ManagerId { get; set; }
    }

    public class TeamMemberViewModel
    {
        public int TeamUserId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class TeamInviteViewModel
    {
        public int InviteId { get; set; }
        
        [Required]
        public int TeamId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string TeamName { get; set; } = string.Empty;
        
        // Inviter information
        [Required]
        public int InvitedById { get; set; }
        
        [Required]
        [StringLength(100)]
        public string InviterName { get; set; } = string.Empty;
        
        // Invitee information       
        
        [Required]
        [EmailAddress]
        [StringLength(50)]
        public string InvitedEmail { get; set; } = string.Empty;
        
        [Required]
        public DateTime InviteDate { get; set; }
        
        public DateTime? StatusChangeDate { get; set; }
        
        [Required]
        public TeamInvite.InviteStatus Status { get; set; } = TeamInvite.InviteStatus.Pending;
        
        public string? StatusChangeNote { get; set; }
    }

    public class PendingInviteViewModel
    {
        public int TeamInviteId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string TeamName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string InvitedByName { get; set; } = string.Empty;
        
        [Required]
        public DateTime InviteDate { get; set; }
    }
} 