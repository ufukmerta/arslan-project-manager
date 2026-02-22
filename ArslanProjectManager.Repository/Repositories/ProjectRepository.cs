using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Repository.Repositories
{
    public class ProjectRepository(ProjectManagerDbContext context) : GenericRepository<Project>(context), IProjectRepository
    {
        private readonly ProjectManagerDbContext _context = context;
        public async Task<Project?> GetProjectWithDetailsAsync(int id)
        {
            var project = await _context.Projects
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
            return project;
        }
    }
}
