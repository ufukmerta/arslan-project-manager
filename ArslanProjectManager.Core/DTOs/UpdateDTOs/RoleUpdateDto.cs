using System.ComponentModel.DataAnnotations;

namespace ArslanProjectManager.Core.DTOs.UpdateDTOs
{
    /// <summary>DTO for updating a team-specific role.</summary>
    public class RoleUpdateDto : BaseUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } = null!;

        public RolePermissionsDto Permissions { get; set; } = null!;
    }
}
