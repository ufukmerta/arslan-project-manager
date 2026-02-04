using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.Repositories;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetDefaultRoleAsync();

    Task<List<Role>> GetRolesForTeamAsync(int teamId);
}
