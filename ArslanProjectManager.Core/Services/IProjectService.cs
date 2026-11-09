using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.DeleteDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.Services
{
    public interface IProjectService : IGenericService<Project>
    {
        Task<ProjectDetailsDto?> GetProjectDetailsAsync(int id);
        Task<CustomResponseDto<IEnumerable<UserProjectDto>>> GetUserProjectsAsync(int userId);
        Task<CustomResponseDto<ProjectDetailsDto>> GetProjectDetailsForUserAsync(int userId, int projectId);
        Task<CustomResponseDto<List<MiniTeamDto>>> GetCreateMetaAsync(int userId);
        Task<CustomResponseDto<MiniProjectDto>> CreateProjectAsync(int userId, ProjectCreateDto model);
        Task<CustomResponseDto<ProjectUpdateDto>> GetEditMetaAsync(int userId, int projectId);
        Task<CustomResponseDto<MiniProjectDto>> UpdateProjectAsync(int userId, ProjectUpdateDto model);
        Task<CustomResponseDto<ProjectDeleteDto>> GetDeleteConfirmAsync(int userId, int projectId);
        Task<CustomResponseDto<NoContentDto>> DeleteProjectAsync(int userId, int projectId);
    }
}
