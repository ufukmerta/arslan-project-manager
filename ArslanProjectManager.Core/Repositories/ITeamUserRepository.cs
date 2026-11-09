using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.Repositories
{
    public interface ITeamUserRepository : IGenericRepository<TeamUser>
    {
        Task<TeamUser?> GetTeamUserWithRoleAsync(int userId, int teamId);
        Task<List<TeamUser>> GetUserTeamMembershipsWithRoleAndTeamAsync(int userId);
    }
}
