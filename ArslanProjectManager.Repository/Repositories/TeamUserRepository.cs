using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Repository.Repositories
{
    public class TeamUserRepository(ProjectManagerDbContext context) : GenericRepository<TeamUser>(context), ITeamUserRepository
    {
        public Task<TeamUser?> GetTeamUserWithRoleAsync(int userId, int teamId)
        {
            return Where(tu => tu.TeamId == teamId && tu.UserId == userId)
                .Include(tu => tu.Role)
                .Include(tu=> tu.User)
                .FirstOrDefaultAsync();
        }
    }
}
