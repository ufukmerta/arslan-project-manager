using ArslanProjectManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArslanProjectManager.Repository.Configurations;

public class TeamConfiguration : BaseEntityConfiguration<Team>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("team");

        builder.Property(e => e.Id)
            .HasColumnName("team_id");

        builder.Property(e => e.TeamName)
            .HasColumnName("team_name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName("description");

        builder.Property(e => e.ManagerId)
            .HasColumnName("manager_id");

        builder.HasOne(d => d.Manager)
            .WithMany(p => p.Teams)
            .HasForeignKey(d => d.ManagerId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_team_user");
    }
}
