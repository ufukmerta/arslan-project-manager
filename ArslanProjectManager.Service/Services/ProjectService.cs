using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.DeleteDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;
using ArslanProjectManager.Service.Utilities;
using AutoMapper;

namespace ArslanProjectManager.Service.Services
{
    public class ProjectService(
        IProjectRepository projectRepository,
        ITeamUserRepository teamUserRepository,
        ITeamRepository teamRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : GenericService<Project>(projectRepository, unitOfWork), IProjectService
    {
        public async Task<ProjectDetailsDto?> GetProjectDetailsAsync(int id)
        {
            var project = await projectRepository.GetProjectWithDetailsAsync(id);
            if (project is null)
            {
                return null;
            }

            return new ProjectDetailsDto
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
                        AppointeeName = t.Appointee?.User?.Name ?? string.Empty,
                        Priority = t.Priority,
                        CreatedDate = t.CreatedDate
                    })
                    .OrderByDescending(t => t.CreatedDate)
                    .ThenByDescending(t => t.Priority)
                    .ToList()
            };
        }

        public async Task<CustomResponseDto<IEnumerable<UserProjectDto>>> GetUserProjectsAsync(int userId)
        {
            var teamUsersWithRole = await teamUserRepository.GetUserTeamMembershipsWithRoleAndTeamAsync(userId);
            var teamIdsWithViewAccess = teamUsersWithRole
                .Where(tu => PermissionResolver.HasPermission(tu, tu.Role, p => p.CanViewProjects))
                .Select(tu => tu.TeamId)
                .ToHashSet();

            if (teamIdsWithViewAccess.Count == 0)
            {
                return CustomResponseDto<IEnumerable<UserProjectDto>>.Fail(404, ErrorMessages.NoProjectsFound);
            }

            var teamPermissions = teamUsersWithRole
                .Where(tu => teamIdsWithViewAccess.Contains(tu.TeamId))
                .ToDictionary(tu => tu.TeamId, tu => (
                    CanEdit: PermissionResolver.HasPermission(tu, tu.Role, p => p.CanEditProjects),
                    CanDelete: PermissionResolver.HasPermission(tu, tu.Role, p => p.CanDeleteProjects)
                ));

            var projectEntities = await projectRepository.GetProjectsByTeamIdsAsync(teamIdsWithViewAccess);
            var projects = mapper.Map<List<UserProjectDto>>(projectEntities);

            foreach (var project in projects)
            {
                if (teamPermissions.TryGetValue(project.TeamId, out var perms))
                {
                    project.CanEdit = perms.CanEdit;
                    project.CanDelete = perms.CanDelete;
                }
            }

            return CustomResponseDto<IEnumerable<UserProjectDto>>.Success(projects, 200);
        }

        public async Task<CustomResponseDto<ProjectDetailsDto>> GetProjectDetailsForUserAsync(int userId, int projectId)
        {
            var accessError = await ValidateProjectAccessAsync(userId, projectId, requireViewAccess: true);
            if (accessError is not null)
            {
                return CustomResponseDto<ProjectDetailsDto>.Fail(accessError.StatusCode, accessError.Errors ?? []);
            }

            var project = await projectRepository.GetProjectWithTeamAsync(projectId, asNoTracking: true);
            if (project is null)
            {
                return CustomResponseDto<ProjectDetailsDto>.Fail(404, ErrorMessages.ProjectNotFound);
            }

            var projectDetailsDto = await GetProjectDetailsAsync(projectId);
            if (projectDetailsDto is null)
            {
                return CustomResponseDto<ProjectDetailsDto>.Fail(404, ErrorMessages.ProjectNotFound);
            }

            var teamMember = await teamUserRepository.GetTeamUserWithRoleAsync(userId, project.TeamId);
            if (teamMember is not null)
            {
                var effective = PermissionResolver.GetEffectivePermissions(teamMember, teamMember.Role);
                projectDetailsDto.CanEdit = effective.CanEditProjects;
                projectDetailsDto.CanDelete = effective.CanDeleteProjects;
                projectDetailsDto.CanViewTasks = effective.CanViewTasks;
                projectDetailsDto.CanCreateTasks = effective.CanEditTasks && effective.CanAssignTasks;

                if (!effective.CanViewTasks)
                {
                    projectDetailsDto.Tasks = [];
                }
            }

            return CustomResponseDto<ProjectDetailsDto>.Success(projectDetailsDto, 200);
        }

        public async Task<CustomResponseDto<List<MiniTeamDto>>> GetCreateMetaAsync(int userId)
        {
            var teamUsersWithRole = await teamUserRepository.GetUserTeamMembershipsWithRoleAndTeamAsync(userId);
            var teamsWithEditPermission = teamUsersWithRole
                .Where(tu => PermissionResolver.HasPermission(tu, tu.Role, p => p.CanEditProjects))
                .Select(tu => new MiniTeamDto
                {
                    Id = tu.TeamId,
                    TeamName = tu.Team.TeamName
                })
                .ToList();

            if (teamsWithEditPermission.Count == 0)
            {
                return CustomResponseDto<List<MiniTeamDto>>.Fail(403, ErrorMessages.NoPermissionToCreateProject);
            }

            return CustomResponseDto<List<MiniTeamDto>>.Success(teamsWithEditPermission, 200);
        }

        public async Task<CustomResponseDto<MiniProjectDto>> CreateProjectAsync(int userId, ProjectCreateDto model)
        {
            var teamExists = await teamRepository.AnyAsync(t => t.Id == model.TeamId);
            if (!teamExists)
            {
                return CustomResponseDto<MiniProjectDto>.Fail(404, ErrorMessages.TeamNotFound);
            }

            var teamUser = await teamUserRepository.GetTeamUserWithRoleAsync(userId, model.TeamId);
            if (teamUser is null)
            {
                return CustomResponseDto<MiniProjectDto>.Fail(403, ErrorMessages.AccessDenied);
            }

            if (!PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanEditProjects))
            {
                return CustomResponseDto<MiniProjectDto>.Fail(403, ErrorMessages.NoPermissionToCreateProject);
            }

            var project = mapper.Map<Project>(model);
            var createdProject = await AddAsync(project);
            if (createdProject is null)
            {
                return CustomResponseDto<MiniProjectDto>.Fail(500, ErrorMessages.FailedToCreateProject);
            }

            var createdProjectDto = mapper.Map<MiniProjectDto>(createdProject);
            return CustomResponseDto<MiniProjectDto>.Success(createdProjectDto, 201);
        }

        public async Task<CustomResponseDto<ProjectUpdateDto>> GetEditMetaAsync(int userId, int projectId)
        {
            var accessError = await ValidateProjectAccessAsync(userId, projectId, requireEditAccess: true);
            if (accessError is not null)
            {
                return CustomResponseDto<ProjectUpdateDto>.Fail(accessError.StatusCode, accessError.Errors ?? []);
            }

            var project = await projectRepository.GetProjectWithTeamAsync(projectId, asNoTracking: true);
            var projectUpdateDto = mapper.Map<ProjectUpdateDto>(project);
            return CustomResponseDto<ProjectUpdateDto>.Success(projectUpdateDto, 200);
        }

        public async Task<CustomResponseDto<MiniProjectDto>> UpdateProjectAsync(int userId, ProjectUpdateDto model)
        {
            var accessError = await ValidateProjectAccessAsync(userId, model.Id, requireEditAccess: true);
            if (accessError is not null)
            {
                return CustomResponseDto<MiniProjectDto>.Fail(accessError.StatusCode, accessError.Errors ?? []);
            }

            var project = await projectRepository.GetProjectWithTeamAsync(model.Id, asNoTracking: false);
            if (project is null)
            {
                return CustomResponseDto<MiniProjectDto>.Fail(404, ErrorMessages.ProjectNotFound);
            }

            project.ProjectName = model.ProjectName;
            project.ProjectDetail = model.ProjectDetail;
            project.StartDate = model.StartDate;
            await UpdateAsync(project);

            var updatedProjectDto = mapper.Map<MiniProjectDto>(project);
            return CustomResponseDto<MiniProjectDto>.Success(updatedProjectDto, 200);
        }

        public async Task<CustomResponseDto<ProjectDeleteDto>> GetDeleteConfirmAsync(int userId, int projectId)
        {
            var accessError = await ValidateProjectAccessAsync(userId, projectId, requireDeleteAccess: true);
            if (accessError is not null)
            {
                if (accessError.StatusCode == 204)
                {
                    return CustomResponseDto<ProjectDeleteDto>.Fail(404, ErrorMessages.ProjectNotFound);
                }
                return CustomResponseDto<ProjectDeleteDto>.Fail(accessError.StatusCode, accessError.Errors ?? []);
            }

            var project = await projectRepository.GetProjectForDeleteConfirmAsync(projectId);
            var projectDeleteDto = mapper.Map<ProjectDeleteDto>(project);
            return CustomResponseDto<ProjectDeleteDto>.Success(projectDeleteDto, 200);
        }

        public async Task<CustomResponseDto<NoContentDto>> DeleteProjectAsync(int userId, int projectId)
        {
            var accessError = await ValidateProjectAccessAsync(userId, projectId, requireDeleteAccess: true);
            if (accessError is not null)
            {
                return accessError;
            }

            var project = await projectRepository.GetProjectWithTeamAsync(projectId, asNoTracking: false);
            if (project is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound);
            }

            await ChangeStatusAsync(project);
            return CustomResponseDto<NoContentDto>.Success(204);
        }

        private async Task<CustomResponseDto<NoContentDto>?> ValidateProjectAccessAsync(
            int userId,
            int projectId,
            bool requireViewAccess = false,
            bool requireEditAccess = false,
            bool requireDeleteAccess = false)
        {
            var project = await projectRepository.GetProjectWithTeamAsync(
                projectId,
                asNoTracking: true,
                ignoreQueryFilters: requireDeleteAccess);

            if (project is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound);
            }

            var teamMember = await teamUserRepository.GetTeamUserWithRoleAsync(userId, project.TeamId);
            if (teamMember is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotTeamMember);
            }

            if (requireDeleteAccess)
            {
                if (!PermissionResolver.HasPermission(teamMember, teamMember.Role, p => p.CanDeleteProjects))
                {
                    return CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToDeleteProject);
                }

                if (!project.IsActive)
                {
                    return CustomResponseDto<NoContentDto>.Success(204);
                }

                return null;
            }

            if (requireViewAccess && !PermissionResolver.HasPermission(teamMember, teamMember.Role, p => p.CanViewProjects))
            {
                return CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToViewProject);
            }

            if (requireEditAccess && !PermissionResolver.HasPermission(teamMember, teamMember.Role, p => p.CanEditProjects))
            {
                return CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToEditProject);
            }

            return null;
        }
    }
}
