using ArslanProjectManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArslanProjectManager.Repository.Configurations;

public class TeamInviteConfiguration : BaseEntityConfiguration<TeamInvite>
{
    protected override string CreatedDateColumnName => "invite_date";
    protected override string UpdatedDateColumnName => "status_change_date";

    protected override void ConfigureEntity(EntityTypeBuilder<TeamInvite> builder)
    {
        builder.ToTable("team_invite");

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

        builder.Property(e => e.Status)
            .HasColumnName("status");

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
    }
}
