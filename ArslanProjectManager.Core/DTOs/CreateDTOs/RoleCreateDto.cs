using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ArslanProjectManager.Core.DTOs.CreateDTOs;

/// <summary>
/// DTO for creating a new team-specific role
/// </summary>
public class RoleCreateDto : BaseCreateDto
{
    [Required]
    [StringLength(100)]
    public string RoleName { get; set; } = null!;

    public RolePermissionsDto Permissions { get; set; } = null!;
}