using ArslanProjectManager.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Repository;

public partial class ProjectManagerDbContext(DbContextOptions<ProjectManagerDbContext> options) : DbContext(options)
{
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
        ApplyAuditTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditTimestamps()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.CreatedDate == default)
            {
                entry.Entity.CreatedDate = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedDate = now;
            }
        }
    }
}
