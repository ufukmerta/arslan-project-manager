using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Repository.Configurations;

public class LogCategoryConfiguration : IEntityTypeConfiguration<LogCategory>
{
    public void Configure(EntityTypeBuilder<LogCategory> builder)
    {
        builder.ToTable("log_category");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("log_category_id");

        builder.Property(e => e.Category)
            .HasColumnName("log_category")
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

        builder.HasQueryFilter(e => e.IsActive);
    }
}