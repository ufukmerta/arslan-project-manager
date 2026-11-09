using ArslanProjectManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArslanProjectManager.Repository.Configurations;

public class TaskTagConfiguration : BaseEntityConfiguration<TaskTag>
{
    protected override void ConfigureEntity(EntityTypeBuilder<TaskTag> builder)
    {
        builder.ToTable("task_tag");

        builder.HasIndex(e => e.TaskId, "IX_task_tag_task_id");

        builder.Property(e => e.Id)
            .HasColumnName("tag_id");

        builder.Property(e => e.TaskId)
            .HasColumnName("task_id");

        builder.Property(e => e.Tag)
            .HasColumnName("tag")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(d => d.Task)
            .WithMany(p => p.TaskTags)
            .HasForeignKey(d => d.TaskId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
