using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.Repositories
{
    public interface ITeamInviteRepository : IGenericRepository<TeamInvite>
    {
        Task<List<TeamInvite>> GetPendingInvitesByEmailAsync(string email);
        Task<TeamInvite?> GetInviteWithTeamByIdAndEmailAsync(int id, string email);
        Task<TeamInvite?> GetInviteWithTeamByIdAsync(int id);
    }
}
