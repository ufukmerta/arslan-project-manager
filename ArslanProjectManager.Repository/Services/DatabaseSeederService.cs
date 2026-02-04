using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Repository.Services
{
    /// <summary>
    /// Seeds initial static data for the application.
    /// This is intended to be idempotent: it only inserts when the corresponding tables are empty.
    /// </summary>
    public class DatabaseSeederService(ProjectManagerDbContext context) : IDatabaseSeederService
    {
        public async Task SeedAsync()
        {
            // Board tags: To Do, In Progress, Done
            if (!await context.BoardTags.AnyAsync())
            {
                context.BoardTags.AddRange(
                    new BoardTag { BoardName = "To Do", BoardOrder = 1 },
                    new BoardTag { BoardName = "In Progress", BoardOrder = 2 },
                    new BoardTag { BoardName = "Done", BoardOrder = 3 }
                );
            }

            // Log categories
            if (!await context.LogCategories.AnyAsync())
            {
                context.LogCategories.AddRange(
                    new LogCategory { Category = "Created" },
                    new LogCategory { Category = "Updated" },
                    new LogCategory { Category = "Deleted" },
                    new LogCategory { Category = "Commented" },
                    new LogCategory { Category = "Assigned" },
                    new LogCategory { Category = "Reassigned" },
                    new LogCategory { Category = "Status Changed" },
                    new LogCategory { Category = "Tag Added" },
                    new LogCategory { Category = "Tag Removed" },
                    new LogCategory { Category = "Priority Changed" }
                );
            }

            // Task categories
            if (!await context.TaskCategories.AnyAsync())
            {
                context.TaskCategories.AddRange(
                    new TaskCategory { Category = "User Story" },
                    new TaskCategory { Category = "Task" },
                    new TaskCategory { Category = "Issue" },
                    new TaskCategory { Category = "Bug" },
                    new TaskCategory { Category = "Ticket" }
                );
            }

            // System roles
            if (!await context.Roles.AnyAsync())
            {
                // Default system roles (insert order matters to keep stable IDs on fresh DBs)
                // 1 - Manager: full control
                var managerRole = new Role
                {
                    RoleName = SystemRoles.Manager,
                    IsSystemRole = true,
                    TeamId = null,
                    CanViewTasks = true,
                    CanEditTasks = true,
                    CanDeleteTasks = true,
                    CanAssignTasks = true,
                    CanViewProjects = true,
                    CanEditProjects = true,
                    CanDeleteProjects = true,
                    CanInviteMembers = true,
                    CanRemoveMembers = true,
                    CanManageRoles = true,
                    CanManagePermissions = true
                };

                // 2 - Contributor: can work on tasks, cannot manage team/roles
                var memberRole = new Role
                {
                    RoleName = SystemRoles.Member,
                    IsSystemRole = true,
                    TeamId = null,
                    CanViewTasks = true,
                    CanEditTasks = true,
                    CanDeleteTasks = false,
                    CanAssignTasks = true,
                    CanViewProjects = true,
                    CanEditProjects = false,
                    CanDeleteProjects = false,
                    CanInviteMembers = false,
                    CanRemoveMembers = false,
                    CanManageRoles = false,
                    CanManagePermissions = false
                };

                // 3 - Viewer: can only view projects/tasks.
                var viewerRole = new Role
                {
                    RoleName = SystemRoles.Viewer,
                    IsSystemRole = true,
                    TeamId = null,
                    CanViewTasks = true,
                    CanEditTasks = false,
                    CanDeleteTasks = false,
                    CanAssignTasks = false,
                    CanViewProjects = true,
                    CanEditProjects = false,
                    CanDeleteProjects = false,
                    CanInviteMembers = false,
                    CanRemoveMembers = false,
                    CanManageRoles = false,
                    CanManagePermissions = false
                };

                // 4 - Guest: minimal access (no projects/tasks by default; can be overridden per-user)
                var guestRole = new Role
                {
                    RoleName = SystemRoles.Guest,
                    IsSystemRole = true,
                    TeamId = null,
                    CanViewTasks = false,
                    CanEditTasks = false,
                    CanDeleteTasks = false,
                    CanAssignTasks = false,
                    CanViewProjects = false,
                    CanEditProjects = false,
                    CanDeleteProjects = false,
                    CanInviteMembers = false,
                    CanRemoveMembers = false,
                    CanManageRoles = false,
                    CanManagePermissions = false
                };

                context.Roles.AddRange(managerRole, memberRole, viewerRole, guestRole);
            }

            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }
        }
    }
}

