using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;

namespace ArslanProjectManager.Repository.Repositories
{
    public class TeamInviteRepository(ProjectManagerDbContext context): GenericRepository<TeamInvite>(context), ITeamInviteRepository
    {
    }
}
