using ArslanProjectManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArslanProjectManager.Repository.Configurations;

public class ProjectConfiguration : BaseEntityConfiguration<Project>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("project");

        builder.HasIndex(e => e.TeamId, "IX_project_team_id");

        builder.Property(e => e.Id)
            .HasColumnName("project_id");

        builder.Property(e => e.ProjectName)
            .HasColumnName("project_name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.TeamId)
            .HasColumnName("team_id");

        builder.Property(e => e.ProjectDetail)
            .HasColumnName("project_detail")
            .HasMaxLength(50);

        builder.Property(e => e.StartDate)
            .HasColumnName("start_date")
            .HasColumnType("date");

        builder.HasOne(d => d.Team)
            .WithMany(p => p.Projects)
            .HasForeignKey(d => d.TeamId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
