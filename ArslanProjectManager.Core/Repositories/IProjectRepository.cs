using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.Repositories
{
    public interface IProjectRepository : IGenericRepository<Project>
    {
        Task<Project?> GetProjectWithDetailsAsync(int id);
        Task<List<Project>> GetProjectsByTeamIdsAsync(IEnumerable<int> teamIds);
        Task<Project?> GetProjectWithTeamAsync(int id, bool asNoTracking = true, bool ignoreQueryFilters = false);
        Task<Project?> GetProjectForDeleteConfirmAsync(int id);
        Task<Project?> GetProjectByTaskIdAsync(int taskId);
    }
}
