using ArslanProjectManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArslanProjectManager.Repository.Configurations;

public class TokenConfiguration : BaseEntityConfiguration<Token>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Token> builder)
    {
        builder.ToTable("token");

        builder.HasIndex(e => e.UserId, "IX_token_user_id");

        builder.Property(e => e.Id)
            .HasColumnName("token_id");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.Property(e => e.RefreshToken)
            .HasColumnName("refresh_token")
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(e => e.RefreshTokenExpiration)
            .HasColumnName("refresh_token_expiration")
            .HasColumnType("datetime");

        builder.HasOne(d => d.User)
            .WithMany(p => p.Tokens)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
