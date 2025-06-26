using ArslanProjectManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Repository.Configurations
{
    public class TokenConfiguration : IEntityTypeConfiguration<Token>
    {
        public void Configure(EntityTypeBuilder<Token> builder)
        {
            builder.ToTable("token");

            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.UserId, "IX_token_user_id");

            builder.Property(e => e.Id)
                .HasColumnName("token_id");

            builder.Property(e => e.UserId)
                .HasColumnName("user_id");

            builder.Property(e => e.AccessToken)
                .HasColumnName("access_token")
                .IsRequired()
                .HasMaxLength(1024);

            builder.Property(e => e.RefreshToken)
                .HasColumnName("refresh_token")
                .IsRequired()
                .HasMaxLength(1024);

            builder.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedDate)
           .HasColumnName("created_date")
           .HasColumnType("datetime")
           .HasDefaultValueSql("(GETUTCDATE())");

            builder.Property(e => e.UpdatedDate)
                .HasColumnName("updated_date")
                .HasColumnType("datetime");

            builder.HasOne(d => d.User)
                .WithMany(p => p.Tokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasQueryFilter(e => e.IsActive);
        }
    }
}