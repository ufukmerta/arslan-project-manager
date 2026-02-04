using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Repository.Repositories
{
    public class TeamRepository(ProjectManagerDbContext context): GenericRepository<Team>(context), ITeamRepository
    {
        public async Task<Team?> GetTeamWithUsersAndRolesAsync(int teamId)
        {
            var teamWithUsersAndRoles = await Where(x => x.Id == teamId)
                .Include(x => x.TeamUsers)
                .ThenInclude(x => x.Role)
                .Include(x => x.TeamUsers)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == teamId);
            return teamWithUsersAndRoles;
        }
    }
}
