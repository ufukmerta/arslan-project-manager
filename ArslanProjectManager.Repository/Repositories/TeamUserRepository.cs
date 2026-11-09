using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Repository.Repositories
{
    public class TeamUserRepository(ProjectManagerDbContext context) : GenericRepository<TeamUser>(context), ITeamUserRepository
    {
        public Task<TeamUser?> GetTeamUserWithRoleAsync(int userId, int teamId)
        {
            return _dbSet
                .AsNoTracking()
                .Include(tu => tu.Role)
                .Include(tu => tu.User)
                .FirstOrDefaultAsync(tu => tu.TeamId == teamId && tu.UserId == userId);
        }

        public Task<List<TeamUser>> GetUserTeamMembershipsWithRoleAndTeamAsync(int userId)
        {
            return _dbSet
                .AsNoTracking()
                .Include(tu => tu.Role)
                .Include(tu => tu.Team)
                .Where(tu => tu.UserId == userId)
                .ToListAsync();
        }
    }
}
