using ArslanProjectManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArslanProjectManager.Repository.Configurations;

public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    protected virtual string CreatedDateColumnName => "created_date";
    protected virtual string UpdatedDateColumnName => "updated_date";

    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedDate)
            .HasColumnName(CreatedDateColumnName)
            .HasColumnType("datetime")
            .HasDefaultValueSql("(GETUTCDATE())");

        builder.Property(e => e.UpdatedDate)
            .HasColumnName(UpdatedDateColumnName)
            .HasColumnType("datetime");

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.HasQueryFilter(e => e.IsActive);

        ConfigureEntity(builder);
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<T> builder);
}
