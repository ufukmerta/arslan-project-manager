using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Service.Services;

public class RoleService(
    IRoleRepository repository,
    ITeamRepository teamRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : GenericService<Role>(repository, unitOfWork), IRoleService
{
    /// <summary>
    /// Returns the default role for new team members (first non-Manager system role).
    /// Priority: "Member" first, then "Viewer", then others. Used when a user accepts an invite.
    /// </summary>
    public async Task<Role?> GetDefaultRoleAsync()
    {
        return await repository.GetDefaultRoleAsync();
    }

    /// <summary>
    /// Returns all roles available for the team (system roles + team-specific roles) with user count per role.
    /// Uses <see cref="ITeamRepository.GetTeamWithUsersAndRolesAsync"/> to get member counts in one call.
    /// </summary>
    public async Task<List<RoleDto>> GetRolesForTeamAsync(int teamId)
    {
        var roles = await repository.GetRolesForTeamAsync(teamId);

        var team = await teamRepository.GetTeamWithUsersAndRolesAsync(teamId);
        var userCountByRoleId = team?.TeamUsers
            .GroupBy(tu => tu.RoleId)
            .ToDictionary(g => g.Key, g => g.Count()) ?? [];

        var dtos = new List<RoleDto>();
        foreach (var role in roles)
        {
            var dto = mapper.Map<RoleDto>(role);
            dto.UserCount = userCountByRoleId.GetValueOrDefault(role.Id, 0);
            dtos.Add(dto);
        }

        return dtos;
    }
}
