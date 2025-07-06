using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.DTOs;

public class RoleDto : BaseDto
{
    public string RoleName { get; set; } = null!;
    public bool? ViewPermission { get; set; }
    public bool? EditPermission { get; set; }
}
