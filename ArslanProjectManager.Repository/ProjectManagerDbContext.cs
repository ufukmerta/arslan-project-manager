using ArslanProjectManager.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Repository;

public partial class ProjectManagerDbContext : DbContext
{
    public ProjectManagerDbContext()
    {
    }

    public ProjectManagerDbContext(DbContextOptions<ProjectManagerDbContext> options) : base(options)
    {
    }
    //Migrations: dotnet ef migrations add InitialCreate --context ProjectManagerDbContext --project ArslanProjectManager.Repository --startup-project ArslanProjectManager.API
    //dotnet ef database update --project ArslanProjectManager.Repository --startup-project ArslanProjectManager.API
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

    public virtual DbSet<Token> Tokens { get; set; }

    public virtual DbSet<User> Users { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectManagerDbContext).Assembly);
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public override int SaveChanges()
    {
        var entries = ChangeTracker.Entries().Where(e => e.Entity is BaseEntity && (e.State == EntityState.Modified));
        foreach (var entityEntry in entries)
        {
            ((BaseEntity)entityEntry.Entity).UpdatedDate = DateTime.UtcNow;
        }

        return base.SaveChanges();
    }
}