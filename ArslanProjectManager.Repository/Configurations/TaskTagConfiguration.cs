using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Repository.Configurations;

public class TaskTagConfiguration : IEntityTypeConfiguration<TaskTag>
{
    public void Configure(EntityTypeBuilder<TaskTag> builder)
    {
        builder.ToTable("task_tag");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.TaskId, "IX_task_tag_task_id");

        builder.Property(e => e.Id)
            .HasColumnName("tag_id");

        builder.Property(e => e.TaskId)
            .HasColumnName("task_id");

        builder.Property(e => e.Tag)
            .HasColumnName("tag")
            .HasMaxLength(50)
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
            .WithMany(p => p.TaskTags)
            .HasForeignKey(d => d.TaskId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasQueryFilter(e => e.IsActive);
    }
}