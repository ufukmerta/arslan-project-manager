using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Repository.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<Core.Models.User> builder)
    {
        builder.ToTable("user");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.Property(e => e.Id)
            .HasColumnName("user_id");

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasColumnName("email")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Password)
            .HasColumnName("password")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e=> e.ProfilePictureUrl)
            .HasColumnName("profile_picture_url")
            .HasMaxLength(200)
            .HasDefaultValue("profile.png");

        builder.Property(e => e.CreatedDate)
            .HasColumnName("register_date")
            .HasColumnType("datetime")
            .HasDefaultValueSql("(getdate())");

        builder.Property(e => e.UpdatedDate)
            .HasColumnName("updated_date")
            .HasColumnType("datetime");

        builder.Property(e=>e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.HasQueryFilter(e => e.IsActive);
    }
} 