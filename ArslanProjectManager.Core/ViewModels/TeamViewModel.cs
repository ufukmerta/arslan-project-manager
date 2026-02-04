using System.ComponentModel.DataAnnotations;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.ViewModels
{
    public class TeamViewModel
    {
        public int TeamId { get; set; }

        [Required]
        [StringLength(100)]
        public string TeamName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string ManagerName { get; set; } = string.Empty;

        public int MemberCount { get; set; }

        public int ProjectCount { get; set; }
    }

    public class TeamDetailsViewModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = null!;
        public string? Description { get; set; }
        public int ManagerId { get; set; }
        public string ManagerName { get; set; } = null!;
        public bool CanRemoveMembers { get; set; }
        public List<TeamMemberViewModel> Members { get; set; } = [];
        public List<ProjectViewModel> Projects { get; set; } = [];
    }

    public class TeamCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string TeamName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class TeamEditViewModel
    {
        public int TeamId { get; set; }

        [Required]
        [StringLength(100)]
        public string TeamName { get; set; } = string.Empty;

        [Required]
        public int ManagerId { get; set; }
    }

    public class TeamMemberViewModel
    {
        public int TeamUserId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class TeamInviteViewModel
    {
        public int InviteId { get; set; }
        
        [Required]
        public int TeamId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string TeamName { get; set; } = string.Empty;
        
        // Inviter information
        [Required]
        public int InvitedById { get; set; }
        
        [Required]
        [StringLength(100)]
        public string InviterName { get; set; } = string.Empty;
        
        // Invitee information       
        
        [Required]
        [EmailAddress]
        [StringLength(50)]
        public string InvitedEmail { get; set; } = string.Empty;
        
        [Required]
        public DateTime InviteDate { get; set; }
        
        public DateTime? StatusChangeDate { get; set; }
        
        [Required]
        public TeamInvite.InviteStatus Status { get; set; } = TeamInvite.InviteStatus.Pending;
        
        public string? StatusChangeNote { get; set; }
    }

    public class PendingInviteViewModel
    {
        public int TeamInviteId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string TeamName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string InvitedByName { get; set; } = string.Empty;
        
        [Required]
        public DateTime InviteDate { get; set; }
    }

    public class TeamPermissionsViewModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public int ManagerId { get; set; }
        public bool CanManagePermissions { get; set; }
        public List<TeamUserPermissionViewModel> Users { get; set; } = [];
    }

    public class TeamUserPermissionViewModel
    {
        public int UserId { get; set; }
        public int TeamUserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsSystemRole { get; set; }
        
        // Task permissions
        public bool CanViewTasks { get; set; }
        public bool CanEditTasks { get; set; }
        public bool CanDeleteTasks { get; set; }
        public bool CanAssignTasks { get; set; }
        
        // Project permissions
        public bool CanViewProjects { get; set; }
        public bool CanEditProjects { get; set; }
        public bool CanDeleteProjects { get; set; }
        
        // Team management permissions
        public bool CanInviteMembers { get; set; }
        public bool CanRemoveMembers { get; set; }
        public bool CanManageRoles { get; set; }
        public bool CanManagePermissions { get; set; }
    }

    public class TeamRolesViewModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public bool CanManageRoles { get; set; }
        public List<TeamRoleViewModel> Roles { get; set; } = [];
    }

    public class TeamRoleViewModel
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
        public bool IsSystemRole { get; set; }
        public bool IsTeamRole => !IsSystemRole;
        public int UserCount { get; set; }
        public RolePermissionsViewModel Permissions { get; set; } = new();
    }

    public class RolePermissionsViewModel
    {
        // Task permissions
        public bool CanViewTasks { get; set; }
        public bool CanEditTasks { get; set; }
        public bool CanDeleteTasks { get; set; }
        public bool CanAssignTasks { get; set; }
        
        // Project permissions
        public bool CanViewProjects { get; set; }
        public bool CanEditProjects { get; set; }
        public bool CanDeleteProjects { get; set; }
        
        // Team management permissions
        public bool CanInviteMembers { get; set; }
        public bool CanRemoveMembers { get; set; }
        public bool CanManageRoles { get; set; }
        public bool CanManagePermissions { get; set; }
    }

    public class TeamRoleCreateViewModel
    {
        public int TeamId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;
        
        public RolePermissionsViewModel Permissions { get; set; } = new();
    }

    public class TeamRoleUpdateViewModel
    {
        public int TeamId { get; set; }
        public int RoleId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;
        
        public RolePermissionsViewModel Permissions { get; set; } = new();
    }

    public class UserEffectivePermissionsViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsSystemRole { get; set; }
        
        // Effective permissions (role + overrides)
        public bool CanViewTasks { get; set; }
        public bool CanEditTasks { get; set; }
        public bool CanDeleteTasks { get; set; }
        public bool CanAssignTasks { get; set; }
        public bool CanViewProjects { get; set; }
        public bool CanEditProjects { get; set; }
        public bool CanDeleteProjects { get; set; }
        public bool CanInviteMembers { get; set; }
        public bool CanRemoveMembers { get; set; }
        public bool CanManageRoles { get; set; }
        public bool CanManagePermissions { get; set; }
        
        // Indicates which permissions are overridden
        public bool HasViewTasksOverride { get; set; }
        public bool HasEditTasksOverride { get; set; }
        public bool HasDeleteTasksOverride { get; set; }
        public bool HasAssignTasksOverride { get; set; }
        public bool HasViewProjectsOverride { get; set; }
        public bool HasEditProjectsOverride { get; set; }
        public bool HasDeleteProjectsOverride { get; set; }
        public bool HasInviteMembersOverride { get; set; }
        public bool HasRemoveMembersOverride { get; set; }
        public bool HasManageRolesOverride { get; set; }
        public bool HasManagePermissionsOverride { get; set; }
    }

    public class UserPermissionUpdateViewModel
    {
        public int TeamId { get; set; }
        public int UserId { get; set; }
        
        public int? RoleId { get; set; }
        
        // Permission overrides (null = use role default, true/false = override)
        public bool? CanViewTasks { get; set; }
        public bool? CanEditTasks { get; set; }
        public bool? CanDeleteTasks { get; set; }
        public bool? CanAssignTasks { get; set; }
        public bool? CanViewProjects { get; set; }
        public bool? CanEditProjects { get; set; }
        public bool? CanDeleteProjects { get; set; }
        public bool? CanInviteMembers { get; set; }
        public bool? CanRemoveMembers { get; set; }
        public bool? CanManageRoles { get; set; }
        public bool? CanManagePermissions { get; set; }
    }
} 