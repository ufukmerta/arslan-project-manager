using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Repository.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("team");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("team_id");

        builder.Property(e => e.TeamName)
            .HasColumnName("team_name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ManagerId)
            .HasColumnName("manager_id");

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

        builder.HasOne(d => d.Manager)
            .WithMany(p => p.Teams)
            .HasForeignKey(d => d.ManagerId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_team_user");

        builder.HasQueryFilter(e => e.IsActive);
    }
}