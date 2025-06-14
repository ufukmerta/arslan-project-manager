using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core;

public class RoleDto : BaseDto
{
    public string RoleName { get; set; } = null!;
    public bool? ViewPermission { get; set; }
    public bool? EditPermission { get; set; }

    public virtual ICollection<TeamUserDto> TeamUsers { get; set; } = new List<TeamUserDto>();
}
