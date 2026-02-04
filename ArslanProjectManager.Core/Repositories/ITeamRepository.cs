using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.Repositories
{
    public interface ITeamRepository : IGenericRepository<Team>
    {
        Task<Team?> GetTeamWithUsersAndRolesAsync(int teamId);
    }
}
