using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Repository.Configurations;

public class TaskCommentConfiguration : IEntityTypeConfiguration<TaskComment>
{
    public void Configure(EntityTypeBuilder<TaskComment> builder)
    {
        builder.ToTable("task_comment");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.TaskId, "IX_task_comment_task_id");
        builder.HasIndex(e => e.TeamUserId, "IX_task_comment_team_user_id");

        builder.Property(e => e.Id)
            .HasColumnName("comment_id");

        builder.Property(e => e.TaskId)
            .HasColumnName("task_id");

        builder.Property(e => e.TeamUserId)
            .HasColumnName("team_user_id");

        builder.Property(e => e.Comment)
            .HasColumnName("comment")
            .IsRequired();

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

        builder.HasOne(d => d.Task)
            .WithMany(p => p.TaskComments)
            .HasForeignKey(d => d.TaskId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(d => d.TeamUser)
            .WithMany(p => p.TaskComments)
            .HasForeignKey(d => d.TeamUserId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasQueryFilter(e => e.IsActive);
    }
}