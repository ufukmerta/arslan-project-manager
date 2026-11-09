using ArslanProjectManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArslanProjectManager.Repository.Configurations;

public class LogCategoryConfiguration : BaseEntityConfiguration<LogCategory>
{
    protected override void ConfigureEntity(EntityTypeBuilder<LogCategory> builder)
    {
        builder.ToTable("log_category");

        builder.Property(e => e.Id)
            .HasColumnName("log_category_id");

        builder.Property(e => e.Category)
            .HasColumnName("log_category")
            .HasMaxLength(50)
            .IsRequired();
    }
}
