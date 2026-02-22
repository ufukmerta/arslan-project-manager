using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;
using ArslanProjectManager.Service.Utilities;

namespace ArslanProjectManager.Service.Services
{
    public class ProjectService(IGenericRepository<Project> repository, IProjectRepository projectRepository, IUnitOfWork unitOfWork) : GenericService<Project>(repository, unitOfWork), IProjectService
    {
        public async Task<ProjectDetailsDto?> GetProjectDetailsAsync(int id)
        {
            var project = await projectRepository.GetProjectWithDetailsAsync(id);
            if (project is null) return null;

            var projectDetailsDto = new ProjectDetailsDto
            {
                Id = project.Id,
                ProjectName = project.ProjectName,
                ProjectDetail = project.ProjectDetail,
                StartDate = project.StartDate,
                Tasks = project.ProjectTasks
                    .Select(t => new MiniProjectTaskDto
                    {
                        Id = t.Id,
                        TaskName = t.TaskName,
                        BoardId = t.BoardId,
                        AppointeeId = t.AppointeeId,
                        AppointeeName = t.Appointee != null && t.Appointee.User != null
                            ? $"{t.Appointee.User.Name}"
                            : string.Empty,
                        Priority = t.Priority,
                        CreatedDate = t.CreatedDate
                    })
                    .OrderByDescending(t => t.CreatedDate)
                    .ThenByDescending(t => t.Priority)
                    .ToList()
            };

            return projectDetailsDto;
        }

    }
}
