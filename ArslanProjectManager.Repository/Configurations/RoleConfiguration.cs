using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Repository.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("role");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.RoleName)
            .IsUnique();

        builder.Property(e => e.Id)
            .HasColumnName("role_id");

        builder.Property(e => e.RoleName)
            .HasColumnName("role_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.ViewPermission)
            .HasColumnName("view_permission")
            .HasDefaultValue(true);

        builder.Property(e => e.EditPermission)
            .HasColumnName("edit_permission")
            .HasDefaultValue(true);

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