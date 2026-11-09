using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.Repositories
{
    public interface IProjectTaskRepository : IGenericRepository<ProjectTask>
    {
        Task<List<ProjectTask>> GetUserTasksAsync(IReadOnlyCollection<int> teamUserIds, IReadOnlyCollection<int> viewableTeamIds);
        Task<ProjectTask?> GetTaskWithDetailsAsync(int id);
        Task<ProjectTask?> GetTaskForEditAsync(int id, bool asNoTracking = true);
        Task<ProjectTask?> GetTaskForDeleteConfirmAsync(int id);
        Task<(List<TaskUserDto> TeamMembers, List<BoardTagDto> Boards, List<TaskCategoryDto> TaskCategories)> GetTaskFormMetaAsync(int teamId);
    }
}
