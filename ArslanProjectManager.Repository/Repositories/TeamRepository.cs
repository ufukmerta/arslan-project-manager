using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;

namespace ArslanProjectManager.Repository.Repositories
{
    public class TeamRepository(ProjectManagerDbContext context): GenericRepository<Team>(context), ITeamRepository
    {
    }
}
