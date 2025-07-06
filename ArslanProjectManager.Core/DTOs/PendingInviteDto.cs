using System.ComponentModel.DataAnnotations;

namespace ArslanProjectManager.Core.DTOs
{
    public class PendingInviteDto : BaseDto
    {
        [Required]
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