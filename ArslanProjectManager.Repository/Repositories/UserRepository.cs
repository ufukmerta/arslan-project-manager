using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Repository.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly ProjectManagerDbContext _context;

        public UserRepository(ProjectManagerDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User?> GetUserWithTeamsProjectsTasksAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.TeamUsers)
                    .ThenInclude(tu => tu.Team)
                        .ThenInclude(t => t.Projects)
                            .ThenInclude(p => p.ProjectTasks)
                                    .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
