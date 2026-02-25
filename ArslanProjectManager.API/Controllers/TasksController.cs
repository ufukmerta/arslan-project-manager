using ArslanProjectManager.API.Filters;
using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.DeleteDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Repository;
using ArslanProjectManager.Service.Utilities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.API.Controllers
{
    /// <summary>
    /// Manages project task operations including CRUD operations, task assignments, and comments
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController(ProjectManagerDbContext context, IProjectTaskService taskService, ITokenService tokenService,
        ITeamUserService teamUserService, IProjectService projectService, ITaskCommentService commentService, IMapper mapper) : CustomBaseController(tokenService)
    {
        /// <summary>
        /// Board status constants for task management
        /// </summary>
        public static class BoardStatus
        {
            /// <summary>
            /// Task completion status
            /// </summary>
            public const string Done = "Done";
        }

        /// <summary>
        /// Retrieves all tasks for the authenticated user (assigned to or created by)
        /// </summary>
        /// <returns>List of tasks that the user has access to</returns>
        /// <response code="200">Returns the list of user's tasks</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If no tasks are found for the user</response>
        [HttpGet()]
        [Authorize]
        public async Task<IActionResult> GetByToken()
        {
            var token = (await GetToken())!;

            var teamUsers = await context.TeamUsers
                .Include(tu => tu.Role)
                .Where(tu => tu.UserId == token!.UserId)
                .ToListAsync();

            if (teamUsers.Count == 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.NoTasksFound));
            }

            var teamUserIds = teamUsers.Select(tu => tu.Id).ToList();

            var teamPermissions = teamUsers.ToDictionary(
                tu => tu.TeamId,
                tu =>
                (
                    CanViewTasks: PermissionResolver.HasPermission(tu, tu.Role, p => p.CanViewTasks),
                    CanDeleteTasks: PermissionResolver.HasPermission(tu, tu.Role, p => p.CanDeleteTasks)
                )
            );

            var viewableTeamIds = teamPermissions
                .Where(kv => kv.Value.CanViewTasks)
                .Select(kv => kv.Key)
                .ToHashSet();

            if (viewableTeamIds.Count == 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.NoTasksFound));
            }

            var taskEntities = await context.ProjectTasks
                .Include(p => p.Project).ThenInclude(p => p.Team).ThenInclude(t => t.TeamUsers).ThenInclude(u => u.User)
                .Include(c => c.TaskCategory)
                .Include(b => b.Board)
                .Include(a => a.Appointee).ThenInclude(u => u.User)
                .Include(a => a.Appointer).ThenInclude(u => u.User)
                .Where(t =>
                    (teamUserIds.Contains(t.AppointeeId) || teamUserIds.Contains(t.AppointerId)) &&
                    viewableTeamIds.Contains(t.Project.TeamId))
                .ToListAsync();

            if (taskEntities.Count == 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.NoTasksFound));
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
                
                // Allow delete only if the current user is the appointer AND has CanDeleteTasks for that team                
                var isUserAppointer = teamUserIds.Contains(entity.AppointerId);                
                task.CanDelete = isUserAppointer && perms.CanDeleteTasks;
            }

            return CreateActionResult(CustomResponseDto<List<ProjectTaskDto>>.Success(tasks, 200));
        }

        /// <summary>
        /// Retrieves detailed information about a specific task
        /// </summary>
        /// <param name="id">The unique identifier of the task</param>
        /// <returns>Detailed task information including comments and assignments</returns>
        /// <response code="200">Returns the task details</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have access to this task</response>
        /// <response code="404">If the task is not found</response>
        [HttpGet("{id:int}")]
        [Authorize]
        [ServiceFilter(typeof(NotFoundFilter<ProjectTask>))]
        public async Task<IActionResult> Details(int id)
        {
            var team = await context.Teams
                .Where(t => t.Projects.Any(p => p.ProjectTasks.Any(pt => pt.Id == id)))
                .FirstOrDefaultAsync();
            if (team is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound));
            }

            var token = await (GetToken())!;
            var accessValidation = await ValidateTeamAccess(token, team.Id, requireViewTasks: true);
            if (accessValidation is not null)
            {
                return accessValidation;
            }            

            var taskDto = await context.ProjectTasks
            .Include(p => p.Appointee).ThenInclude(p => p.User)
            .Include(p => p.Appointer).ThenInclude(p => p.User)
            .Include(p => p.Board)
            .Include(p => p.Project)
            .Include(p => p.TaskCategory)
            .Include(p => p.TaskComments).ThenInclude(c => c.TeamUser).ThenInclude(tu => tu.User)
            .Where(t => t.Id == id)
            .ProjectTo<ProjectTaskDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
            if (taskDto is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound));
            }

            var teamUser = await context.TeamUsers
                .Include(tu => tu.Role)
                .FirstOrDefaultAsync(tu => tu.UserId == token!.UserId && tu.TeamId == team.Id);
            var isAppointee = teamUser!.Id == taskDto.AppointeeId;
            var isAppointer = teamUser.Id == taskDto.AppointerId;
            taskDto.CanEdit = (isAppointee || isAppointer) && PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanEditTasks);
            taskDto.CanDelete = isAppointer && PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanDeleteTasks);
            return CreateActionResult(CustomResponseDto<ProjectTaskDto>.Success(taskDto, 200));
        }

        /// <summary>
        /// Retrieves the metadata needed to create a new task for a specific project
        /// </summary>
        /// <param name="projectId">The unique identifier of the project</param>
        /// <returns>Task creation metadata including team members, board tags, and task categories</returns>
        /// <response code="200">Returns the task creation metadata</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the project is not found</response>
        [HttpGet("{projectId}/[action]-meta")]
        [Authorize]
        public async Task<IActionResult> Create(int projectId)
        {
            var project = await projectService.GetByIdAsync(projectId);
            if (project is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound));
            }

            var token = await (GetToken())!;
            var accessValidation = await ValidateProjectAccess(token, projectId, requireEditTasks: true, requireAssignTasks: true);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var teamMembers = await context.TeamUsers
                .Include(x => x.User)
                .Where(x => x.TeamId == project.TeamId)
                .Select(x => new TaskUserDto
                {
                    TeamUserId = x.Id,
                    Name = x.User.Name
                })
                .ToListAsync();

            var boards = await context.BoardTags
                .Select(x => new BoardTagDto
                {
                    Id = x.Id,
                    BoardName = x.BoardName
                })
                .ToListAsync();

            var taskCategories = await context.TaskCategories
                .Select(x => new TaskCategoryDto
                {
                    Id = x.Id,
                    Category = x.Category
                })
                .ToListAsync();

            ProjectTaskCreateViewDto projectTaskCreateViewDto = new()
            {
                ProjectId = project.Id,
                TeamMembers = teamMembers,
                BoardTags = boards,
                TaskCategories = taskCategories
            };
            return CreateActionResult(CustomResponseDto<ProjectTaskCreateViewDto>.Success(projectTaskCreateViewDto, 200));
        }

        /// <summary>
        /// Creates a new task for a specific project
        /// </summary>
        /// <param name="model">The data for creating the task</param>
        /// <returns>The created task's minimal details</returns>
        /// <response code="201">Returns the created task's minimal details</response>
        /// <response code="400">If the model is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the project is not found</response>
        /// <response code="403">If the user doesn't have access to the project or team</response>
        /// <response code="500">If task creation fails</response>
        [HttpPost()]
        [Authorize]
        public async Task<IActionResult> Create(ProjectTaskCreateDto model)
        {
            var validationResult = ValidateModel(
                model,
                m => m != null
                    && m.ProjectId > 0
                    && m.Priority >= 0
                    && m.BoardId > 0
                    && m.TaskCategoryId > 0
                    && m.AppointeeId > 0
                    && !string.IsNullOrEmpty(m.TaskName),
                ErrorMessages.InvalidTaskData
            );
            if (validationResult != null)
            {
                return validationResult;
            }

            var project = await projectService.GetByIdAsync(model.ProjectId);
            if (project is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound));
            }

            var token = await (GetToken())!;
            var accessValidation = await ValidateProjectAccess(token, model.ProjectId, requireEditTasks: true);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var teamUser = teamUserService.Where(t => t.UserId == token!.UserId && t.TeamId == project.TeamId).FirstOrDefault();

            var task = mapper.Map<ProjectTask>(model);
            task.AppointerId = teamUser!.Id;

            var createdTask = await taskService.AddAsync(task);
            if (createdTask is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToCreateTask));
            }

            return CreateActionResult(CustomResponseDto<MiniProjectTaskDto>.Success(new MiniProjectTaskDto { Id = createdTask.Id }, 201));
        }

        /// <summary>
        /// Adds a comment to a specific task
        /// </summary>
        /// <param name="model">The data for creating the comment</param>
        /// <returns>Success response</returns>
        /// <response code="201">Returns success response</response>
        /// <response code="400">If the model is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the task is not found</response>
        /// <response code="403">If the user doesn't have access to the task's team</response>
        /// <response code="500">If comment creation fails</response>
        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> Comment(TaskCommentCreateDto model)
        {
            var validationResult = ValidateModel(
                model,
                m => m != null
                    && m.TaskId > 0
                    && !string.IsNullOrEmpty(m.Comment),
                ErrorMessages.InvalidInput
            );
            if (validationResult != null)
            {
                return validationResult;
            }

            var comment = mapper.Map<TaskComment>(model);

            var task = await taskService.GetByIdAsync(comment.TaskId);
            if (task is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound));
            }

            var teamFromTask = await context.Teams
                .Include(t => t.Projects)
                .ThenInclude(p => p.ProjectTasks)
                .Where(x => x.Projects.Any(x => x.ProjectTasks.Any(x => x.Id == task.Id))).FirstOrDefaultAsync();
            if (teamFromTask is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound));
            }

            var token = await (GetToken())!;
            var accessValidation = await ValidateTeamAccess(token, teamFromTask.Id, requireViewTasks: true);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var teamUser = await teamUserService.Where(x => x.UserId == token!.UserId && x.TeamId == teamFromTask.Id).FirstOrDefaultAsync();

            comment.TeamUserId = teamUser!.Id;
            await commentService.AddAsync(comment);
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(201));
        }

        /// <summary>
        /// Retrieves the metadata needed to edit a specific task
        /// </summary>
        /// <param name="id">The unique identifier of the task</param>
        /// <returns>Task edit metadata including team members, board tags, and task categories</returns>
        /// <response code="200">Returns the task edit metadata</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the task is not found</response>
        [HttpGet("{id:int}/[action]-meta")]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await context.ProjectTasks
                 .Include(p => p.Project)
                 .Include(p => p.TaskCategory)
                 .Include(p => p.Board)
                 .Include(p => p.Appointee)
                 .ThenInclude(a => a.User)
                 .FirstOrDefaultAsync(t => t.Id == id);
            if (task is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound));
            }

            var token = await (GetToken())!;
            var accessValidation = await ValidateTeamAccess(token, task.Project.TeamId, requireEditTasks: true);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var teamMembers = await context.TeamUsers
                .Include(x => x.User)
                .Where(x => x.TeamId == task.Project.TeamId)
                .Select(x => new TaskUserDto
                {
                    TeamUserId = x.Id,
                    Name = x.User.Name
                })
                .ToListAsync();

            var boards = await context.BoardTags
                .Select(x => new BoardTagDto
                {
                    Id = x.Id,
                    BoardName = x.BoardName
                })
                .ToListAsync();

            var taskCategories = await context.TaskCategories
                .Select(x => new TaskCategoryDto
                {
                    Id = x.Id,
                    Category = x.Category
                })
                .ToListAsync();

            var projectTaskUpdateDto = mapper.Map<ProjectTaskUpdateDto>(task);
            projectTaskUpdateDto.TeamMembers = teamMembers;
            projectTaskUpdateDto.BoardTags = boards;
            projectTaskUpdateDto.TaskCategories = taskCategories;

            if (projectTaskUpdateDto is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound));
            }

            return CreateActionResult(CustomResponseDto<ProjectTaskUpdateDto>.Success(projectTaskUpdateDto, 200));
        }

        /// <summary>
        /// Updates an existing task
        /// </summary>
        /// <param name="model">The updated task data</param>
        /// <returns>The updated task's minimal details</returns>
        /// <response code="201">Returns the updated task's minimal details</response>
        /// <response code="400">If the model is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the task is not found</response>
        /// <response code="403">If the user doesn't have access to the task's project or team</response>
        /// <response code="500">If task update fails</response>
        [HttpPut()]
        [Authorize]
        public async Task<IActionResult> Edit(ProjectTaskUpdateDto model)
        {
            var validationResult = ValidateModel(
                model,
                m => m != null
                    && m.Id > 0
                    && m.Priority >= 0
                    && m.BoardId > 0
                    && m.TaskCategoryId > 0
                    && m.AppointeeId > 0
                    && !string.IsNullOrEmpty(m.TaskName),
                ErrorMessages.InvalidTaskData
            );
            if (validationResult != null)
            {
                return validationResult;
            }

            var project = await projectService.Where(x => x.ProjectTasks.Any(pt => pt.Id == model.Id)).FirstOrDefaultAsync();
            if (project is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound));
            }

            var token = await (GetToken())!;
            var accessValidation = await ValidateTeamAccess(token, project.TeamId, requireEditTasks: true, requireAssignTasks: true);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var task = await context.ProjectTasks.Include(pt => pt.Board).Where(x => x.Id == model.Id).FirstOrDefaultAsync();
            if (task is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound));
            }

            var isAppointeeChanged = task.AppointeeId != model.AppointeeId;
            if (isAppointeeChanged)
            {
                var accessValidationToAssign = await ValidateTeamAccess(token, project.TeamId, requireAssignTasks: true);
                var isModifiedByAppointer = task.AppointerId == teamUserService.Where(t => t.UserId == token!.UserId && t.TeamId == project.TeamId).FirstOrDefault()?.Id;
                if (accessValidationToAssign is not null || !isModifiedByAppointer)
                {
                    return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToChangeAppointee));
                }
            }            

            var oldBoardName = task.Board.BoardName;
            var newBoard = await context.BoardTags.FindAsync(model.BoardId);
            var newBoardName = newBoard?.BoardName;
            if (oldBoardName == BoardStatus.Done && model.BoardId != task.BoardId)
            {
                task.EndDate = null;
            }

            if (newBoardName == BoardStatus.Done && oldBoardName != BoardStatus.Done && model.EndDate is null)
            {
                model.EndDate = DateTime.UtcNow;
            }

            mapper.Map(model, task);
            taskService.Update(task);
            return CreateActionResult(CustomResponseDto<MiniProjectTaskDto>.Success(new MiniProjectTaskDto { Id = model.Id }, 201));
        }

        /// <summary>
        /// Retrieves the data needed to confirm task deletion
        /// </summary>
        /// <param name="id">The unique identifier of the task</param>
        /// <returns>Task deletion confirmation data</returns>
        /// <response code="200">Returns the task deletion confirmation data</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the task is not found</response>
        /// <response code="403">If the user doesn't have access to this task</response>
        [HttpGet("{id:int}/delete-confirm")]
        [Authorize]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            var idValidation = ValidateId(id, ErrorMessages.InvalidTaskId);
            if (idValidation is not null)
            {
                return idValidation;
            }

            var taskDelete = await context.ProjectTasks
                .Include(p => p.Appointee)
                    .ThenInclude(p => p.User)
                .Include(p => p.Appointer)
                    .ThenInclude(p => p.User)
                .Include(p => p.Board)
                .Include(p => p.Project)
                .Include(p => p.TaskCategory)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (taskDelete is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound));
            }

            var token = await (GetToken())!;

            var teamUser = await context.TeamUsers
                .Include(tu => tu.Role)
                .FirstOrDefaultAsync(tu => tu.UserId == token!.UserId && tu.TeamId == taskDelete.Project.TeamId);

            if (teamUser is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotTeamMember));
            }

            if (!PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanDeleteTasks) ||
                taskDelete.AppointerId != teamUser.Id)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToDeleteTask));
            }

            var taskDeleteDto = mapper.Map<ProjectTaskDeleteDto>(taskDelete);
            return CreateActionResult(CustomResponseDto<ProjectTaskDeleteDto>.Success(taskDeleteDto, 200));
        }

        /// <summary>
        /// Deletes a specific task
        /// </summary>
        /// <param name="id">The unique identifier of the task</param>
        /// <returns>Success response</returns>
        /// <response code="204">Returns success response</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the task is not found</response>
        /// <response code="403">If the user doesn't have access to this task</response>
        /// <response code="500">If task deletion fails</response>
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var idValidation = ValidateId(id, ErrorMessages.InvalidTaskId);
            if (idValidation is not null)
            {
                return idValidation;
            }

            var task = await taskService.GetByIdAsync(id);
            if (task is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound));
            }

            var token = await (GetToken())!;

            var project = await context.Projects
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.ProjectTasks.Any(pt => pt.Id == id));

            if (project is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound));
            }

            var teamUser = await context.TeamUsers
                .Include(tu => tu.Role)
                .FirstOrDefaultAsync(tu => tu.UserId == token!.UserId && tu.TeamId == project.TeamId);

            if (teamUser is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotTeamMember));
            }

            if (!PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanDeleteTasks) ||
                task.AppointerId != teamUser.Id)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToDeleteTask));
            }

            task.IsActive = false;
            taskService.ChangeStatus(task);
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }

        protected async Task<IActionResult?> ValidateProjectAccess(
            Token? token,
            int projectId,
            bool requireViewTasks = false,
            bool requireEditTasks = false,
            bool requireDeleteTasks = false,
            bool requireAssignTasks = false)
        {
            var project = await context.Projects
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound));
            }

            var teamUser = await context.TeamUsers
                .Include(tu => tu.Role)
                .FirstOrDefaultAsync(tu => tu.UserId == token!.UserId && tu.TeamId == project.TeamId);

            if (teamUser is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotTeamMember));
            }

            if (requireViewTasks && !PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanViewTasks))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToViewTask));
            }

            if (requireEditTasks && !PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanEditTasks))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToEditTask));
            }

            if (requireDeleteTasks && !PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanDeleteTasks))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToDeleteTask));
            }

            if (requireAssignTasks && !PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanAssignTasks))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToEditTask));
            }

            return null;
        }

        protected async Task<IActionResult?> ValidateTeamAccess(
            Token? token,
            int teamId,
            bool requireViewTasks = false,
            bool requireEditTasks = false,
            bool requireDeleteTasks = false,
            bool requireAssignTasks = false)
        {
            var team = await context.Teams.FindAsync(teamId);
            if (team is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound));
            }

            var teamUser = await context.TeamUsers
                .Include(tu => tu.Role)
                .FirstOrDefaultAsync(tu => tu.UserId == token!.UserId && tu.TeamId == teamId);

            if (teamUser is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotTeamMember));
            }

            if (requireViewTasks && !PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanViewTasks))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToViewTask));
            }

            if (requireEditTasks && !PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanEditTasks))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToEditTask));
            }

            if (requireDeleteTasks && !PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanDeleteTasks))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToDeleteTask));
            }

            if (requireAssignTasks && !PermissionResolver.HasPermission(teamUser, teamUser.Role, p => p.CanAssignTasks))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToEditTask));
            }

            return null;
        }
    }
}