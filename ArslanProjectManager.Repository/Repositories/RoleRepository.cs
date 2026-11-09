using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Repository.Repositories;

public class RoleRepository(ProjectManagerDbContext context) : GenericRepository<Role>(context), IRoleRepository
{
    /// <summary>
    /// Returns the default role for new team members (first non-Manager system role).
    /// Priority: "Member" first, then "Viewer", then others.
    /// </summary>
    public async Task<Role?> GetDefaultRoleAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.TeamId == null && r.IsSystemRole && r.RoleName != SystemRoles.Manager)
            .OrderBy(r => r.RoleName == SystemRoles.Member ? 0 : r.RoleName == SystemRoles.Viewer ? 1 : 2)
            .ThenBy(r => r.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<Role?> GetSystemRoleByNameAsync(string roleName)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.TeamId == null && r.IsSystemRole && r.RoleName == roleName);
    }

    /// <summary>
    /// Returns all roles available for the team (system roles + team-specific roles),
    /// ordered by system role first, then by role name.
    /// </summary>
    public async Task<List<Role>> GetRolesForTeamAsync(int teamId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.Team)
            .Where(r => r.TeamId == null || r.TeamId == teamId)
            .OrderByDescending(r => r.IsSystemRole)
            .ThenBy(r => r.RoleName == SystemRoles.Manager ? 0 : r.RoleName == SystemRoles.Member ? 1 : 2)
            .ToListAsync();
    }
}
