using ArslanProjectManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArslanProjectManager.Repository.Configurations;

public class TaskCommentConfiguration : BaseEntityConfiguration<TaskComment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<TaskComment> builder)
    {
        builder.ToTable("task_comment");

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

        builder.HasOne(d => d.Task)
            .WithMany(p => p.TaskComments)
            .HasForeignKey(d => d.TaskId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(d => d.TeamUser)
            .WithMany(p => p.TaskComments)
            .HasForeignKey(d => d.TeamUserId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
