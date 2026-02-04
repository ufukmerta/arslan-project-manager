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
        return await Where(r => r.TeamId == null && r.IsSystemRole && r.RoleName != SystemRoles.Manager)
            .OrderBy(r => r.RoleName == SystemRoles.Member ? 0 : r.RoleName == SystemRoles.Viewer ? 1 : 2)
            .ThenBy(r => r.Id)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Returns all roles available for the team (system roles + team-specific roles),
    /// ordered by system role first, then by role name.
    /// </summary>
    public async Task<List<Role>> GetRolesForTeamAsync(int teamId)
    {
        return await Where(r => r.TeamId == null || r.TeamId == teamId)
            .OrderByDescending(r => r.IsSystemRole)
            .ThenBy(r => r.RoleName == "Manager" ? 0 : r.RoleName == "Member" ? 1 : 2)
            .ToListAsync();
    }
}
