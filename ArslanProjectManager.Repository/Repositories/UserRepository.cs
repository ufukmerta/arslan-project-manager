using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Repository.Repositories
{
    public class UserRepository(ProjectManagerDbContext context) : GenericRepository<User>(context), IUserRepository
    {
        private readonly ProjectManagerDbContext _context = context;

        public async Task<User?> GetUserWithTeamsProjectsTasksAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.TeamUsers)
                    .ThenInclude(tu => tu.Team)
                        .ThenInclude(t => t.TeamUsers)
                .Include(t => t.TeamUsers)
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
