using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArslanProjectManager.Core.DTOs.CreateDTOs;

public class RoleCreateDto : BaseCreateDto
{
    public string RoleName { get; set; } = null!;
    public bool? ViewPermission { get; set; }
    public bool? EditPermission { get; set; }
    
}
