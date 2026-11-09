using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Repository.Repositories
{
    public class TeamInviteRepository(ProjectManagerDbContext context) : GenericRepository<TeamInvite>(context), ITeamInviteRepository
    {
        public Task<List<TeamInvite>> GetPendingInvitesByEmailAsync(string email)
        {
            return _dbSet
                .AsNoTracking()
                .Include(x => x.Team)
                .Include(x => x.InvitedBy)
                .Where(x => x.InvitedEmail == email && x.Status == TeamInvite.InviteStatus.Pending)
                .ToListAsync();
        }

        public Task<TeamInvite?> GetInviteWithTeamByIdAndEmailAsync(int id, string email)
        {
            return _dbSet
                .Include(x => x.Team)
                .FirstOrDefaultAsync(x => x.Id == id && x.InvitedEmail == email);
        }

        public Task<TeamInvite?> GetInviteWithTeamByIdAsync(int id)
        {
            return _dbSet
                .Include(x => x.Team)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
