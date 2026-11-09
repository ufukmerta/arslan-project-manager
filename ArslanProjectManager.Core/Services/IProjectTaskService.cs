using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.DeleteDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.Services
{
    public interface IProjectTaskService : IGenericService<ProjectTask>
    {
        Task<CustomResponseDto<List<ProjectTaskDto>>> GetUserTasksAsync(int userId);
        Task<CustomResponseDto<ProjectTaskDto>> GetTaskDetailsForUserAsync(int userId, int taskId);
        Task<CustomResponseDto<ProjectTaskCreateViewDto>> GetCreateMetaAsync(int userId, int projectId);
        Task<CustomResponseDto<MiniProjectTaskDto>> CreateTaskAsync(int userId, ProjectTaskCreateDto model);
        Task<CustomResponseDto<NoContentDto>> AddCommentAsync(int userId, TaskCommentCreateDto model);
        Task<CustomResponseDto<ProjectTaskUpdateDto>> GetEditMetaAsync(int userId, int taskId);
        Task<CustomResponseDto<MiniProjectTaskDto>> UpdateTaskAsync(int userId, ProjectTaskUpdateDto model);
        Task<CustomResponseDto<ProjectTaskDeleteDto>> GetDeleteConfirmAsync(int userId, int taskId);
        Task<CustomResponseDto<NoContentDto>> DeleteTaskAsync(int userId, int taskId);
    }
}
