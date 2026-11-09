using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Repository.Repositories
{
    public class UserRepository(ProjectManagerDbContext context) : GenericRepository<User>(context), IUserRepository
    {
        public async Task<User?> GetUserWithTeamsProjectsTasksAsync(int userId)
        {
            return await _dbSet
                .AsNoTrackingWithIdentityResolution()
                .AsSplitQuery()
                .Include(u => u.TeamUsers)
                    .ThenInclude(tu => tu.Team)
                        .ThenInclude(t => t.TeamUsers)
                            .ThenInclude(tu => tu.Role)
                .Include(u => u.TeamUsers)
                    .ThenInclude(tu => tu.Team)
                        .ThenInclude(t => t.Projects)
                            .ThenInclude(p => p.ProjectTasks)
                                .ThenInclude(pt => pt.Board)
                .Include(u => u.TeamUsers)
                    .ThenInclude(tu => tu.Team)
                        .ThenInclude(t => t.Projects)
                            .ThenInclude(p => p.ProjectTasks)
                                .ThenInclude(pt => pt.Appointee)
                                    .ThenInclude(a => a.User)
                .Include(u => u.TeamUsers)
                    .ThenInclude(tu => tu.Team)
                        .ThenInclude(t => t.Projects)
                            .ThenInclude(p => p.ProjectTasks)
                                .ThenInclude(pt => pt.Appointer)
                                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
