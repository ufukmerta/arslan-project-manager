using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Repository.Configurations;

public class TeamUserConfiguration : IEntityTypeConfiguration<TeamUser>
{
    public void Configure(EntityTypeBuilder<TeamUser> builder)
    {
        builder.ToTable("team_user");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.RoleId, "IX_team_user_role_id");
        builder.HasIndex(e => e.TeamId, "IX_team_user_team_id");
        builder.HasIndex(e => e.UserId, "IX_team_user_user_id");

        builder.Property(e => e.Id)
            .HasColumnName("team_user_id");

        builder.Property(e => e.TeamId)
            .HasColumnName("team_id");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.Property(e => e.RoleId)
            .HasColumnName("role_id");

        // Permission overrides (nullable = use role default, true/false = override)
        // Task permission overrides
        builder.Property(e => e.CanViewTasksOverride)
            .HasColumnName("can_view_tasks_override");

        builder.Property(e => e.CanEditTasksOverride)
            .HasColumnName("can_edit_tasks_override");

        builder.Property(e => e.CanDeleteTasksOverride)
            .HasColumnName("can_delete_tasks_override");

        builder.Property(e => e.CanAssignTasksOverride)
            .HasColumnName("can_assign_tasks_override");

        // Project permission overrides
        builder.Property(e => e.CanViewProjectsOverride)
            .HasColumnName("can_view_projects_override");

        builder.Property(e => e.CanEditProjectsOverride)
            .HasColumnName("can_edit_projects_override");

        builder.Property(e => e.CanDeleteProjectsOverride)
            .HasColumnName("can_delete_projects_override");

        // Team management permission overrides
        builder.Property(e => e.CanInviteMembersOverride)
            .HasColumnName("can_invite_members_override");

        builder.Property(e => e.CanRemoveMembersOverride)
            .HasColumnName("can_remove_members_override");

        builder.Property(e => e.CanManageRolesOverride)
            .HasColumnName("can_manage_roles_override");

        builder.Property(e => e.CanManagePermissionsOverride)
            .HasColumnName("can_manage_permissions_override");

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

        builder.HasOne(d => d.Role)
            .WithMany(p => p.TeamUsers)
            .HasForeignKey(d => d.RoleId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(d => d.Team)
            .WithMany(p => p.TeamUsers)
            .HasForeignKey(d => d.TeamId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(d => d.User)
            .WithMany(p => p.TeamUsers)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasQueryFilter(e => e.IsActive);
    }
}