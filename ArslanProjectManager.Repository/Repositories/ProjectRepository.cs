using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Repository.Repositories
{
    public class ProjectRepository(ProjectManagerDbContext context) : GenericRepository<Project>(context), IProjectRepository
    {
        public async Task<Project?> GetProjectWithDetailsAsync(int id)
        {
            return await _context.Projects
                .AsNoTracking()
                .AsSplitQuery()
                .Include(p => p.ProjectTasks)
                    .ThenInclude(t => t.TaskCategory)
                .Include(p => p.ProjectTasks)
                    .ThenInclude(t => t.Board)
                .Include(p => p.ProjectTasks)
                    .ThenInclude(t => t.Appointee)
                        .ThenInclude(a => a.User)
                .Include(p => p.Team)
                    .ThenInclude(t => t.TeamUsers)
                        .ThenInclude(tu => tu.User)
                .Include(p => p.Team)
                    .ThenInclude(t => t.TeamUsers)
                        .ThenInclude(tu => tu.Role)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Project>> GetProjectsByTeamIdsAsync(IEnumerable<int> teamIds)
        {
            var idSet = teamIds.ToHashSet();
            return await _context.Projects
                .AsNoTracking()
                .Include(p => p.Team)
                .Include(p => p.ProjectTasks)
                .Where(p => idSet.Contains(p.TeamId))
                .ToListAsync();
        }

        public async Task<Project?> GetProjectWithTeamAsync(int id, bool asNoTracking = true, bool ignoreQueryFilters = false)
        {
            IQueryable<Project> query = _context.Projects;
            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Project?> GetProjectForDeleteConfirmAsync(int id)
        {
            return await _context.Projects
                .AsNoTracking()
                .Include(p => p.Team)
                .Include(p => p.ProjectTasks)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Project?> GetProjectByTaskIdAsync(int taskId)
        {
            return await _context.Projects
                .AsNoTracking()
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.ProjectTasks.Any(pt => pt.Id == taskId));
        }
    }
}
