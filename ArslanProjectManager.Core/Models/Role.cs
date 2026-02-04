using System;
using System.Collections.Generic;

namespace ArslanProjectManager.Core.Models;

public partial class Role : BaseEntity
{
    public string RoleName { get; set; } = null!;
    
    // Scope: null = system role, TeamId = team-specific role
    public int? TeamId { get; set; }
    
    // Indicates if this is a system default role
    public bool IsSystemRole { get; set; } = false;
    
    // Granular task permissions
    public bool CanViewTasks { get; set; } = false;
    public bool CanEditTasks { get; set; } = false;
    public bool CanDeleteTasks { get; set; } = false;
    public bool CanAssignTasks { get; set; } = false;
    
    // Project permissions
    public bool CanViewProjects { get; set; } = false;
    public bool CanEditProjects { get; set; } = false;
    public bool CanDeleteProjects { get; set; } = false;
    
    // Team management permissions
    public bool CanInviteMembers { get; set; } = false;
    public bool CanRemoveMembers { get; set; } = false;
    public bool CanManageRoles { get; set; } = false; // Create/edit team roles
    public bool CanManagePermissions { get; set; } = false; // Manage user permissions
    
    // Navigation properties
    public virtual Team? Team { get; set; }
    public virtual ICollection<TeamUser> TeamUsers { get; set; } = new List<TeamUser>();
}
