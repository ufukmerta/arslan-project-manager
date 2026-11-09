using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.Repositories;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetDefaultRoleAsync();

    Task<Role?> GetSystemRoleByNameAsync(string roleName);

    Task<List<Role>> GetRolesForTeamAsync(int teamId);
}
