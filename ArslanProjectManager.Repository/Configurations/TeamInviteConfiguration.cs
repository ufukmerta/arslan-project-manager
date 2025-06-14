using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Repository.Configurations;

public class TeamInviteConfiguration : IEntityTypeConfiguration<TeamInvite>
{
    public void Configure(EntityTypeBuilder<TeamInvite> builder)
    {
        builder.ToTable("team_invite");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("team_invite_id");

        builder.Property(e => e.TeamId)
            .HasColumnName("team_id");

        builder.Property(e => e.InvitedById)
            .HasColumnName("invited_by_id");

        builder.Property(e => e.InvitedEmail)
            .HasColumnName("invited_email")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.CreatedDate)
            .HasColumnName("invite_date")
            .HasColumnType("datetime")
            .HasDefaultValueSql("(getdate())");

        builder.Property(e => e.UpdatedDate)
            .HasColumnName("updated_date")
            .HasColumnType("datetime");

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(e => e.Status)
            .HasColumnName("status");

        builder.Property(e => e.UpdatedDate)
            .HasColumnName("status_change_date")
            .HasColumnType("datetime");

        builder.Property(e => e.StatusChangeNote)
            .HasColumnName("status_change_note")
            .HasMaxLength(500);

        builder.HasOne(d => d.Team)
            .WithMany(p => p.TeamInvites)
            .HasForeignKey(d => d.TeamId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_team_invite_team");

        builder.HasOne(d => d.InvitedBy)
            .WithMany(p => p.TeamInvites)
            .HasForeignKey(d => d.InvitedById)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasQueryFilter(e => e.IsActive);
    }
} 