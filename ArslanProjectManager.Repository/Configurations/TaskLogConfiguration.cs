using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Repository.Configurations;

public class TaskLogConfiguration : IEntityTypeConfiguration<TaskLog>
{
    public void Configure(EntityTypeBuilder<TaskLog> builder)
    {
        builder.ToTable("task_log");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.LogCategoryId, "IX_task_log_log_category_id");
        builder.HasIndex(e => e.TeamUserId, "IX_task_log_team_user_id");
        builder.HasIndex(e => e.AffectedTeamUserId, "IX_task_log_affected_team_user_id");

        builder.Property(e => e.Id)
            .HasColumnName("log_id");

        builder.Property(e => e.TaskId)
            .HasColumnName("task_id");

        builder.Property(e => e.TeamUserId)
            .HasColumnName("team_user_id");

        builder.Property(e => e.LogCategoryId)
            .HasColumnName("log_category_id");

        builder.Property(e => e.CreatedDate)
           .HasColumnName("created_date")
           .HasColumnType("datetime")
           .HasDefaultValueSql("(GETUTCDATE())");

        builder.Property(e => e.UpdatedDate)
            .HasColumnName("updated_date")
            .HasColumnType("datetime");

        builder.Property(e => e.AffectedTeamUserId)
            .HasColumnName("affected_team_user_id");

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.HasOne(d => d.Task)
            .WithMany(p => p.TaskLogs)
            .HasForeignKey(d => d.TaskId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(d => d.TeamUser)
            .WithMany(p => p.TaskLogs)
            .HasForeignKey(d => d.TeamUserId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(d => d.AffectedTeamUser)
            .WithMany(p => p.AffectedTaskLogs)
            .HasForeignKey(d => d.AffectedTeamUserId);

        builder.HasOne(d => d.LogCategory)
            .WithMany(p => p.TaskLogs)
            .HasForeignKey(d => d.LogCategoryId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasQueryFilter(e => e.IsActive);
    }
}