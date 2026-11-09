using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.Services
{
    public interface ITeamUserService : IGenericService<TeamUser>
    {
        Task<UserEffectivePermissionsDto?> GetTeamUserWithPermissionsAsync(int teamId, int userId);
    }
}
