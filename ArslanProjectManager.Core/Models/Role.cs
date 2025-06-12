using System;
using System.Collections.Generic;

namespace ArslanProjectManager.Core.Models;

public partial class Role : BaseEntity
{
    public string RoleName { get; set; } = null!;
    public bool? ViewPermission { get; set; }
    public bool? EditPermission { get; set; }

    public virtual ICollection<TeamUser> TeamUsers { get; set; } = new List<TeamUser>();
}
