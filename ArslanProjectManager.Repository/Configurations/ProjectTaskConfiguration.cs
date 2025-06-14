using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Repository.Configurations;

public class ProjectTaskConfiguration : IEntityTypeConfiguration<ProjectTask>
{
    public void Configure(EntityTypeBuilder<ProjectTask> builder)
    {
        builder.ToTable("project_task");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.AppointeeId, "IX_project_task_appointee_id");
        builder.HasIndex(e => e.AppointerId, "IX_project_task_appointer_id");
        builder.HasIndex(e => e.BoardId, "IX_project_task_board_id");
        builder.HasIndex(e => e.ProjectId, "IX_project_task_project_id");
        builder.HasIndex(e => e.TaskCategoryId, "IX_project_task_task_category_id");

        builder.Property(e => e.Id)
            .HasColumnName("task_id");

        builder.Property(e => e.ProjectId)
            .HasColumnName("project_id");

        builder.Property(e => e.TaskName)
            .HasColumnName("task_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.CreatedDate)
           .HasColumnName("created_date")
           .HasColumnType("datetime")
           .HasDefaultValueSql("(getdate())");

        builder.Property(e => e.UpdatedDate)
            .HasColumnName("updated_date")
            .HasColumnType("datetime");

        builder.Property(e => e.StartingDate)
            .HasColumnName("starting_date")
            .HasColumnType("datetime");

        builder.Property(e => e.EndDate)
            .HasColumnName("end_date")
            .HasColumnType("datetime");

        builder.Property(e => e.ExpectedEndDate)
            .HasColumnName("expected_end_date")
            .HasColumnType("datetime");

        builder.Property(e => e.TaskCategoryId)
            .HasColumnName("task_category_id");

        builder.Property(e => e.BoardId)
            .HasColumnName("board_id");

        builder.Property(e => e.Description)
            .HasColumnName("description");

        builder.Property(e => e.AppointeeId)
            .HasColumnName("appointee_id");

        builder.Property(e => e.AppointerId)
            .HasColumnName("appointer_id");

        builder.Property(e => e.Priority)
            .HasColumnName("priority")
            .HasDefaultValue(ProjectTask.TaskPriority.Medium);

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.HasOne(d => d.Appointee)
            .WithMany(p => p.ProjectTaskAppointees)
            .HasForeignKey(d => d.AppointeeId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(d => d.Appointer)
            .WithMany(p => p.ProjectTaskAppointers)
            .HasForeignKey(d => d.AppointerId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(d => d.Board)
            .WithMany(p => p.ProjectTasks)
            .HasForeignKey(d => d.BoardId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_task_board_tag");

        builder.HasOne(d => d.Project)
            .WithMany(p => p.ProjectTasks)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_task_project");

        builder.HasOne(d => d.TaskCategory)
            .WithMany(p => p.ProjectTasks)
            .HasForeignKey(d => d.TaskCategoryId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_task_task_category");

        builder.HasQueryFilter(e => e.IsActive);
    }
} 