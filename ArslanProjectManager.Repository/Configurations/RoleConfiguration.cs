using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Repository.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("role");

        builder.HasKey(e => e.Id);

        // Unique constraint: RoleName must be unique per team
        // For team roles: (RoleName, TeamId) must be unique where TeamId IS NOT NULL
        // For system roles: RoleName must be unique where TeamId IS NULL
        // Using a filtered unique index to handle both cases
        // Only active roles are considered for uniqueness (soft-deleted roles can reuse names)
        builder.HasIndex(e => new { e.RoleName, e.TeamId })
            .IsUnique()
            .HasFilter("[is_active] = 1");
        
        // Additional unique index for system roles (RoleName must be unique globally when TeamId is NULL)
        // Note: SQL Server allows multiple NULLs in unique indexes, so we use a filtered index
        // Only active roles are considered for uniqueness (soft-deleted roles can reuse names)
        builder.HasIndex(e => e.RoleName)
            .IsUnique()
            .HasFilter("[team_id] IS NULL AND [is_active] = 1");

        builder.Property(e => e.Id)
            .HasColumnName("role_id");

        builder.Property(e => e.RoleName)
            .HasColumnName("role_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.TeamId)
            .HasColumnName("team_id");

        builder.Property(e => e.IsSystemRole)
            .HasColumnName("is_system_role")
            .HasDefaultValue(false);

        // Granular task permissions
        builder.Property(e => e.CanViewTasks)
            .HasColumnName("can_view_tasks")
            .HasDefaultValue(false);

        builder.Property(e => e.CanEditTasks)
            .HasColumnName("can_edit_tasks")
            .HasDefaultValue(false);

        builder.Property(e => e.CanDeleteTasks)
            .HasColumnName("can_delete_tasks")
            .HasDefaultValue(false);

        builder.Property(e => e.CanAssignTasks)
            .HasColumnName("can_assign_tasks")
            .HasDefaultValue(false);

        // Project permissions
        builder.Property(e => e.CanViewProjects)
            .HasColumnName("can_view_projects")
            .HasDefaultValue(false);

        builder.Property(e => e.CanEditProjects)
            .HasColumnName("can_edit_projects")
            .HasDefaultValue(false);

        builder.Property(e => e.CanDeleteProjects)
            .HasColumnName("can_delete_projects")
            .HasDefaultValue(false);

        // Team management permissions
        builder.Property(e => e.CanInviteMembers)
            .HasColumnName("can_invite_members")
            .HasDefaultValue(false);

        builder.Property(e => e.CanRemoveMembers)
            .HasColumnName("can_remove_members")
            .HasDefaultValue(false);

        builder.Property(e => e.CanManageRoles)
            .HasColumnName("can_manage_roles")
            .HasDefaultValue(false);

        builder.Property(e => e.CanManagePermissions)
            .HasColumnName("can_manage_permissions")
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedDate)
           .HasColumnName("created_date")
           .HasColumnType("datetime")
           .HasDefaultValueSql("(GETUTCDATE())");

        builder.Property(e => e.UpdatedDate)
            .HasColumnName("updated_date")
            .HasColumnType("datetime");

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        // Relationship: Role -> Team (optional, for team-specific roles)
        builder.HasOne(d => d.Team)
            .WithMany(p => p.Roles)
            .HasForeignKey(d => d.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => e.IsActive);
    }
}