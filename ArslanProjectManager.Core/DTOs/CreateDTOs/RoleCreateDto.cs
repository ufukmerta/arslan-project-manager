using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArslanProjectManager.Core;

public class RoleCreateDto : BaseCreateDto
{
    public string RoleName { get; set; } = null!;
    public bool? ViewPermission { get; set; }
    public bool? EditPermission { get; set; }
    
}
