using System;
using System.Collections.Generic;
using ArslanProjectManager.WEB.Models;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.WEB.Data;

public partial class ProjectManagerDbContext : DbContext
{
    public ProjectManagerDbContext()
    {
    }

    public ProjectManagerDbContext(DbContextOptions<ProjectManagerDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BoardTag> BoardTags { get; set; }

    public virtual DbSet<LogCategory> LogCategories { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectTask> ProjectTasks { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TaskCategory> TaskCategories { get; set; }

    public virtual DbSet<TaskComment> TaskComments { get; set; }

    public virtual DbSet<TaskLog> TaskLogs { get; set; }

    public virtual DbSet<TaskTag> TaskTags { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<TeamInvite> TeamInvites { get; set; }

    public virtual DbSet<TeamUser> TeamUsers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(LocalDB)\\MSSQLLocalDB;Database=ProjectManagerDB;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK__project__BC799E1FB8EEC3B3");

            entity.HasOne(d => d.Team).WithMany(p => p.Projects)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_project_team");
        });

        modelBuilder.Entity<ProjectTask>(entity =>
        {
            entity.Property(e => e.BoardId).HasDefaultValue(1);
            entity.Property(e => e.CreationDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Board).WithMany(p => p.ProjectTasks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_task_board_tag");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectTasks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_task_project");

            entity.HasOne(d => d.TaskCategory).WithMany(p => p.ProjectTasks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_task_task_category");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__role__760965CC39F4EC7B");

            entity.Property(e => e.EditPermission).HasDefaultValue(true);
            entity.Property(e => e.ViewPermission).HasDefaultValue(true);
        });

        modelBuilder.Entity<TaskComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__task_com__E7957687B656B40A");

            entity.Property(e => e.Date).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskComments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_task_comment_project_task");
        });

        modelBuilder.Entity<TaskLog>(entity =>
        {
            entity.Property(e => e.LogDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.LogCategory).WithMany(p => p.TaskLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_task_log_log_category");
        });

        modelBuilder.Entity<TaskTag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK__task_tag__4296A2B697A6CEFB");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskTags)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_task_tag_task");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.TeamId).HasName("PK__team__F82DEDBC0B030B30");

            entity.HasOne(d => d.Manager).WithMany(p => p.Teams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_team_user");
        });

        modelBuilder.Entity<TeamInvite>(entity =>
        {
            entity.HasKey(e => e.TeamInviteId).HasName("PK__team_invite");

            entity.Property(e => e.InviteDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Team).WithMany(p => p.TeamInvites)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_team_invite_team");
        });

        modelBuilder.Entity<TeamUser>(entity =>
        {
            entity.HasKey(e => e.TeamUserId).HasName("PK__team_use__0C4481CC09402993");

            entity.Property(e => e.RoleId).HasDefaultValue(1);

            entity.HasOne(d => d.Role).WithMany(p => p.TeamUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_team_user_role");

            entity.HasOne(d => d.Team).WithMany(p => p.TeamUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_team_user_team");

            entity.HasOne(d => d.User).WithMany(p => p.TeamUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_team_user_user");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__user__B9BE370FA9D8EA79");

            entity.Property(e => e.RegisterDate).HasDefaultValueSql("(getdate())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
