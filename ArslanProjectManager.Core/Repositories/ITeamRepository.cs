using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.Repositories
{
    public interface ITeamRepository : IGenericRepository<Team>
    {
        Task<Team?> GetTeamWithUsersAndRolesAsync(int teamId);
        Task<List<TeamDto>> GetUserTeamsAsync(int userId);
        Task<TeamDetailsDto?> GetTeamDetailsForUserAsync(int teamId, int userId);
        Task<Team?> GetTeamWithUsersAsync(int teamId, bool asNoTracking = true);
        Task<Team?> GetTeamWithUsersAndInvitesAsync(int teamId);
        Task<Team?> GetTeamWithInvitesAndInvitersAsync(int teamId);
    }
}
