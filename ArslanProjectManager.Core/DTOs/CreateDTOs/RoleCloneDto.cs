using System.ComponentModel.DataAnnotations;

namespace ArslanProjectManager.Core.DTOs.CreateDTOs;

/// <summary>
/// DTO for cloning a system role as a team-specific role
/// </summary>
public class RoleCloneDto : BaseCreateDto
{
    [Required]
    public int SourceRoleId { get; set; }

    [Required]
    [StringLength(100)]
    public string NewRoleName { get; set; } = null!;
}
