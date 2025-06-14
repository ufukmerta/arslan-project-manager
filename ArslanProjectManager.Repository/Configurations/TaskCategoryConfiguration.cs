using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Repository.Configurations;

public class TaskCategoryConfiguration : IEntityTypeConfiguration<TaskCategory>
{
    public void Configure(EntityTypeBuilder<TaskCategory> builder)
    {
        builder.ToTable("task_category");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("task_category_id");

        builder.Property(e => e.Category)
            .HasColumnName("category")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.CreatedDate)
            .HasColumnName("created_date")
            .HasColumnType("date")
            .HasDefaultValueSql("(getdate())");

        builder.Property(e => e.UpdatedDate)
            .HasColumnName("updated_date")
            .HasColumnType("date");

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.HasQueryFilter(e => e.IsActive);
    }
} 