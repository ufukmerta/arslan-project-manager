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
    public class ProjectTaskService(
        IProjectTaskRepository taskRepository,
        IProjectRepository projectRepository,
        ITeamUserRepository teamUserRepository,
        ITeamRepository teamRepository,
        IBoardTagRepository boardTagRepository,
        ITaskCommentRepository commentRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : GenericService<ProjectTask>(taskRepository, unitOfWork), IProjectTaskService
    {
        public async Task<CustomResponseDto<List<ProjectTaskDto>>> GetUserTasksAsync(int userId)
        {
            var teamUsers = await teamUserRepository.GetUserTeamMembershipsWithRoleAndTeamAsync(userId);
            if (teamUsers.Count == 0)
            {
                return CustomResponseDto<List<ProjectTaskDto>>.Fail(404, ErrorMessages.NoTasksFound);
            }

            var teamUserIds = teamUsers.Select(tu => tu.Id).ToList();
            var teamPermissions = teamUsers.ToDictionary(
                tu => tu.TeamId,
                tu => (
                    CanViewTasks: PermissionResolver.HasPermission(tu, tu.Role, p => p.CanViewTasks),
                    CanEditTasks: PermissionResolver.HasPermission(tu, tu.Role, p => p.CanEditTasks),
                    CanDeleteTasks: PermissionResolver.HasPermission(tu, tu.Role, p => p.CanDeleteTasks)
                )
            );

            var viewableTeamIds = teamPermissions
                .Where(kv => kv.Value.CanViewTasks)
                .Select(kv => kv.Key)
                .ToHashSet();

            if (viewableTeamIds.Count == 0)
            {
                return CustomResponseDto<List<ProjectTaskDto>>.Fail(404, ErrorMessages.NoTasksFound);
            }

            var taskEntities = await taskRepository.GetUserTasksAsync(teamUserIds, viewableTeamIds);
            if (taskEntities.Count == 0)
            {
                return CustomResponseDto<List<ProjectTaskDto>>.Fail(404, ErrorMessages.NoTasksFound);
            }

            var tasks = mapper.Map<List<ProjectTaskDto>>(taskEntities);
            var taskById = taskEntities.ToDictionary(t => t.Id);

            foreach (var task in tasks)
            {
                if (!taskById.TryGetValue(task.Id, out var entity))
                {
                    continue;
                }

                if (!teamPermissions.TryGetValue(entity.Project.TeamId, out var perms))
                {
                    task.CanEdit = false;
                    task.CanDelete = false;
                    continue;
                }

                var isUserAppointer = teamUserIds.Contains(entity.AppointerId);
                task.CanEdit = perms.CanEditTasks;
                task.CanDelete = isUserAppointer && perms.CanDeleteTasks;
            }

            return CustomResponseDto<List<ProjectTaskDto>>.Success(tasks, 200);
        }

        public async Task<CustomResponseDto<ProjectTaskDto>> GetTaskDetailsForUserAsync(int userId, int taskId)
        {
            var project = await projectRepository.GetProjectByTaskIdAsync(taskId);
            if (project is null)
            {
                return CustomResponseDto<ProjectTaskDto>.Fail(404, ErrorMessages.TaskNotFound);
            }

            var (accessError, teamUser) = await ValidateTeamTaskAccessAsync(userId, project.TeamId, requireViewTasks: true);
            if (accessError is not null)
            {
                return CustomResponseDto<ProjectTaskDto>.Fail(accessError.StatusCode, accessError.Errors ?? []);
            }

            var taskEntity = await taskRepository.GetTaskWithDetailsAsync(taskId);
            if (taskEntity is null)
            {
                return CustomResponseDto<ProjectTaskDto>.Fail(404, ErrorMessages.TaskNotFound);
            }

            var taskDto = mapper.Map<ProjectTaskDto>(taskEntity);
            var isAppointee = teamUser!.Id == taskDto.AppointeeId;
            var isAppointer = teamUser.Id == taskDto.AppointerId;
            taskDto.CanEdit = (isAppointee || isAppointer) && PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanEditTasks);
            taskDto.CanDelete = isAppointer && PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanDeleteTasks);

            return CustomResponseDto<ProjectTaskDto>.Success(taskDto, 200);
        }

        public async Task<CustomResponseDto<ProjectTaskCreateViewDto>> GetCreateMetaAsync(int userId, int projectId)
        {
            var (accessError, project, _) = await ValidateProjectTaskAccessAsync(
                userId,
                projectId,
                requireEditTasks: true,
                requireAssignTasks: true);
            if (accessError is not null)
            {
                return CustomResponseDto<ProjectTaskCreateViewDto>.Fail(accessError.StatusCode, accessError.Errors ?? []);
            }

            var (teamMembers, boards, taskCategories) = await taskRepository.GetTaskFormMetaAsync(project!.TeamId);
            var viewDto = new ProjectTaskCreateViewDto
            {
                ProjectId = project.Id,
                TeamMembers = teamMembers,
                BoardTags = boards,
                TaskCategories = taskCategories
            };

            return CustomResponseDto<ProjectTaskCreateViewDto>.Success(viewDto, 200);
        }

        public async Task<CustomResponseDto<MiniProjectTaskDto>> CreateTaskAsync(int userId, ProjectTaskCreateDto model)
        {
            var (accessError, _, teamUser) = await ValidateProjectTaskAccessAsync(
                userId,
                model.ProjectId,
                requireEditTasks: true,
                requireAssignTasks: true);
            if (accessError is not null)
            {
                return CustomResponseDto<MiniProjectTaskDto>.Fail(accessError.StatusCode, accessError.Errors ?? []);
            }

            var task = mapper.Map<ProjectTask>(model);
            task.AppointerId = teamUser!.Id;

            var createdTask = await AddAsync(task);
            if (createdTask is null)
            {
                return CustomResponseDto<MiniProjectTaskDto>.Fail(500, ErrorMessages.FailedToCreateTask);
            }

            return CustomResponseDto<MiniProjectTaskDto>.Success(new MiniProjectTaskDto { Id = createdTask.Id }, 201);
        }

        public async Task<CustomResponseDto<NoContentDto>> AddCommentAsync(int userId, TaskCommentCreateDto model)
        {
            var project = await projectRepository.GetProjectByTaskIdAsync(model.TaskId);
            if (project is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound);
            }

            var (accessError, teamUser) = await ValidateTeamTaskAccessAsync(userId, project.TeamId, requireViewTasks: true);
            if (accessError is not null)
            {
                return accessError;
            }

            var comment = mapper.Map<TaskComment>(model);
            comment.TeamUserId = teamUser!.Id;

            await commentRepository.AddAsync(comment);
            await UnitOfWork.CommitAsync();

            return CustomResponseDto<NoContentDto>.Success(201);
        }

        public async Task<CustomResponseDto<ProjectTaskUpdateDto>> GetEditMetaAsync(int userId, int taskId)
        {
            var task = await taskRepository.GetTaskForEditAsync(taskId, asNoTracking: true);
            if (task is null)
            {
                return CustomResponseDto<ProjectTaskUpdateDto>.Fail(404, ErrorMessages.TaskNotFound);
            }

            var (accessError, teamUser) = await ValidateTeamTaskAccessAsync(userId, task.Project.TeamId, requireEditTasks: true);
            if (accessError is not null)
            {
                return CustomResponseDto<ProjectTaskUpdateDto>.Fail(accessError.StatusCode, accessError.Errors ?? []);
            }

            if (task.AppointeeId != teamUser!.Id && task.AppointerId != teamUser.Id)
            {
                return CustomResponseDto<ProjectTaskUpdateDto>.Fail(403, ErrorMessages.NoPermissionToEditTask);
            }

            var (teamMembers, boards, taskCategories) = await taskRepository.GetTaskFormMetaAsync(task.Project.TeamId);
            var projectTaskUpdateDto = mapper.Map<ProjectTaskUpdateDto>(task);
            projectTaskUpdateDto.TeamMembers = teamMembers;
            projectTaskUpdateDto.BoardTags = boards;
            projectTaskUpdateDto.TaskCategories = taskCategories;

            return CustomResponseDto<ProjectTaskUpdateDto>.Success(projectTaskUpdateDto, 200);
        }

        public async Task<CustomResponseDto<MiniProjectTaskDto>> UpdateTaskAsync(int userId, ProjectTaskUpdateDto model)
        {
            var project = await projectRepository.GetProjectByTaskIdAsync(model.Id);
            if (project is null)
            {
                return CustomResponseDto<MiniProjectTaskDto>.Fail(404, ErrorMessages.ProjectNotFound);
            }

            var (accessError, teamUser) = await ValidateTeamTaskAccessAsync(userId, project.TeamId, requireEditTasks: true);
            if (accessError is not null)
            {
                return CustomResponseDto<MiniProjectTaskDto>.Fail(accessError.StatusCode, accessError.Errors ?? []);
            }

            var task = await taskRepository.GetTaskForEditAsync(model.Id, asNoTracking: false);
            if (task is null)
            {
                return CustomResponseDto<MiniProjectTaskDto>.Fail(404, ErrorMessages.TaskNotFound);
            }

            if (task.AppointeeId != teamUser!.Id && task.AppointerId != teamUser.Id)
            {
                return CustomResponseDto<MiniProjectTaskDto>.Fail(403, ErrorMessages.NoPermissionToEditTask);
            }

            var isAppointeeChanged = task.AppointeeId != model.AppointeeId;
            if (isAppointeeChanged)
            {
                var canAssign = PermissionResolver.HasPermission(teamUser!, teamUser!.Role, p => p.CanAssignTasks);
                var isModifiedByAppointer = task.AppointerId == teamUser.Id;
                if (!canAssign || !isModifiedByAppointer)
                {
                    return CustomResponseDto<MiniProjectTaskDto>.Fail(403, ErrorMessages.NoPermissionToChangeAppointee);
                }
            }

            var oldBoardName = task.Board.BoardName;
            var newBoard = await boardTagRepository.GetByIdAsync(model.BoardId);
            var newBoardName = newBoard?.BoardName;
            if (oldBoardName == SystemBoards.DoneName && model.BoardId != task.BoardId)
            {
                task.EndDate = null;
            }

            if (newBoardName == SystemBoards.DoneName && oldBoardName != SystemBoards.DoneName && model.EndDate is null)
            {
                model.EndDate = DateTime.UtcNow;
            }

            mapper.Map(model, task);
            await UpdateAsync(task);

            return CustomResponseDto<MiniProjectTaskDto>.Success(new MiniProjectTaskDto { Id = model.Id }, 201);
        }

        public async Task<CustomResponseDto<ProjectTaskDeleteDto>> GetDeleteConfirmAsync(int userId, int taskId)
        {
            var taskDelete = await taskRepository.GetTaskForDeleteConfirmAsync(taskId);
            if (taskDelete is null)
            {
                return CustomResponseDto<ProjectTaskDeleteDto>.Fail(404, ErrorMessages.TaskNotFound);
            }

            var deleteAccessError = await ValidateTaskDeleteAccessAsync(userId, taskDelete.Project.TeamId, taskDelete.AppointerId);
            if (deleteAccessError is not null)
            {
                return CustomResponseDto<ProjectTaskDeleteDto>.Fail(deleteAccessError.StatusCode, deleteAccessError.Errors ?? []);
            }

            var taskDeleteDto = mapper.Map<ProjectTaskDeleteDto>(taskDelete);
            return CustomResponseDto<ProjectTaskDeleteDto>.Success(taskDeleteDto, 200);
        }

        public async Task<CustomResponseDto<NoContentDto>> DeleteTaskAsync(int userId, int taskId)
        {
            var task = await GetByIdAsync(taskId);
            if (task is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound);
            }

            var project = await projectRepository.GetProjectByTaskIdAsync(taskId);
            if (project is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound);
            }

            var deleteAccessError = await ValidateTaskDeleteAccessAsync(userId, project.TeamId, task.AppointerId);
            if (deleteAccessError is not null)
            {
                return deleteAccessError;
            }

            await ChangeStatusAsync(task);
            return CustomResponseDto<NoContentDto>.Success(204);
        }

        private async Task<CustomResponseDto<NoContentDto>?> ValidateTaskDeleteAccessAsync(int userId, int teamId, int appointerId)
        {
            var teamUser = await teamUserRepository.GetTeamUserWithRoleAsync(userId, teamId);
            if (teamUser is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotTeamMember);
            }

            if (!PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanDeleteTasks) ||
                appointerId != teamUser.Id)
            {
                return CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToDeleteTask);
            }

            return null;
        }

        private async Task<(CustomResponseDto<NoContentDto>? Error, Project? Project, TeamUser? TeamUser)> ValidateProjectTaskAccessAsync(
            int userId,
            int projectId,
            bool requireViewTasks = false,
            bool requireEditTasks = false,
            bool requireDeleteTasks = false,
            bool requireAssignTasks = false)
        {
            var project = await projectRepository.GetProjectWithTeamAsync(projectId, asNoTracking: true);
            if (project is null)
            {
                return (CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound), null, null);
            }

            var (error, teamUser) = await ValidateTeamTaskPermissionsAsync(
                userId,
                project.TeamId,
                requireViewTasks,
                requireEditTasks,
                requireDeleteTasks,
                requireAssignTasks);

            return (error, project, teamUser);
        }

        private async Task<(CustomResponseDto<NoContentDto>? Error, TeamUser? TeamUser)> ValidateTeamTaskAccessAsync(
            int userId,
            int teamId,
            bool requireViewTasks = false,
            bool requireEditTasks = false,
            bool requireDeleteTasks = false,
            bool requireAssignTasks = false)
        {
            var team = await teamRepository.GetByIdAsync(teamId);
            if (team is null)
            {
                return (CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound), null);
            }

            return await ValidateTeamTaskPermissionsAsync(
                userId,
                teamId,
                requireViewTasks,
                requireEditTasks,
                requireDeleteTasks,
                requireAssignTasks);
        }

        private async Task<(CustomResponseDto<NoContentDto>? Error, TeamUser? TeamUser)> ValidateTeamTaskPermissionsAsync(
            int userId,
            int teamId,
            bool requireViewTasks,
            bool requireEditTasks,
            bool requireDeleteTasks,
            bool requireAssignTasks)
        {
            var teamUser = await teamUserRepository.GetTeamUserWithRoleAsync(userId, teamId);
            if (teamUser is null)
            {
                return (CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotTeamMember), null);
            }

            if (requireViewTasks && !PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanViewTasks))
            {
                return (CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToViewTask), teamUser);
            }

            if (requireEditTasks && !PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanEditTasks))
            {
                return (CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToEditTask), teamUser);
            }

            if (requireDeleteTasks && !PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanDeleteTasks))
            {
                return (CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToDeleteTask), teamUser);
            }

            if (requireAssignTasks && !PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanAssignTasks))
            {
                return (CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToEditTask), teamUser);
            }

            return (null, teamUser);
        }
    }
}
