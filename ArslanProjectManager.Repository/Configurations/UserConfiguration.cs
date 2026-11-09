using ArslanProjectManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArslanProjectManager.Repository.Configurations;

public class UserConfiguration : BaseEntityConfiguration<User>
{
    protected override string CreatedDateColumnName => "register_date";

    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user");

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

        builder.Property(e => e.ProfilePicture)
            .HasColumnName("profile_picture")
            .HasColumnType("varbinary(max)")
            .HasMaxLength(200);

        builder.Property(e => e.SecurityStamp)
            .HasColumnName("security_stamp")
            .HasMaxLength(100)
            .IsRequired();
    }
}
