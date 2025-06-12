using System;
using System.Collections.Generic;

namespace ArslanProjectManager.Core.Models;

public partial class Team : BaseEntity
{
    public string TeamName { get; set; } = null!;
    public int ManagerId { get; set; }

    public virtual User Manager { get; set; } = null!;
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    public virtual ICollection<TeamInvite> TeamInvites { get; set; } = new List<TeamInvite>();
    public virtual ICollection<TeamUser> TeamUsers { get; set; } = new List<TeamUser>();
}
