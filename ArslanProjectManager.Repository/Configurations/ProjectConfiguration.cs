using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Repository.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("project");

        builder.HasKey(e => e.Id);

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
            .HasColumnName("start_date");

        builder.Property(e => e.CreatedDate)
            .HasColumnName("created_date")
            .HasColumnType("date")
            .HasDefaultValueSql("(getdate())");

        builder.Property(e => e.UpdatedDate)
            .HasColumnName("updated_date")
            .HasColumnType("date");

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.HasOne(d => d.Team)
            .WithMany(p => p.Projects)
            .HasForeignKey(d => d.TeamId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasQueryFilter(e => e.IsActive);
    }
}
