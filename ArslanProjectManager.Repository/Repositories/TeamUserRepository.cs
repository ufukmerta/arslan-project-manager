using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;

namespace ArslanProjectManager.Repository.Repositories
{
    public class TeamUserRepository(ProjectManagerDbContext context): GenericRepository<TeamUser>(context), ITeamUserRepository
    {
    }
}
