using ArslanProjectManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArslanProjectManager.Repository.Configurations;

public class TaskCategoryConfiguration : BaseEntityConfiguration<TaskCategory>
{
    protected override void ConfigureEntity(EntityTypeBuilder<TaskCategory> builder)
    {
        builder.ToTable("task_category");

        builder.Property(e => e.Id)
            .HasColumnName("task_category_id");

        builder.Property(e => e.Category)
            .HasColumnName("category")
            .HasMaxLength(50)
            .IsRequired();
    }
}
