using System;
using System.Collections.Generic;

namespace ArslanProjectManager.Core.Models;

public partial class TeamUser : BaseEntity
{
    public int TeamId { get; set; }
    public int UserId { get; set; }
    public int RoleId { get; set; }
    
    // Permission overrides (null = use role permissions, true/false = override)
    // Task permission overrides
    public bool? CanViewTasksOverride { get; set; }
    public bool? CanEditTasksOverride { get; set; }
    public bool? CanDeleteTasksOverride { get; set; }
    public bool? CanAssignTasksOverride { get; set; }
    
    // Project permission overrides
    public bool? CanViewProjectsOverride { get; set; }
    public bool? CanEditProjectsOverride { get; set; }
    public bool? CanDeleteProjectsOverride { get; set; }
    
    // Team management permission overrides
    public bool? CanInviteMembersOverride { get; set; }
    public bool? CanRemoveMembersOverride { get; set; }
    public bool? CanManageRolesOverride { get; set; }
    public bool? CanManagePermissionsOverride { get; set; }

    public virtual ICollection<ProjectTask> ProjectTaskAppointees { get; set; } = new List<ProjectTask>();
    public virtual ICollection<ProjectTask> ProjectTaskAppointers { get; set; } = new List<ProjectTask>();
    public virtual Role Role { get; set; } = null!;
    public virtual Team Team { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();
    public virtual ICollection<TaskLog> TaskLogs { get; set; } = new List<TaskLog>();
    public virtual ICollection<TaskLog> AffectedTaskLogs { get; set; } = new List<TaskLog>();
}
