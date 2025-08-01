using ArslanProjectManager.API.Filters;
using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.DeleteDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Repository;
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
        private readonly ProjectManagerDbContext _context = context;
        private readonly IProjectTaskService _taskService = taskService;
        private readonly IProjectService _projectService = projectService;
        private readonly ITeamUserService _teamUserService = teamUserService;
        private readonly ITaskCommentService _commentService = commentService;
        private readonly IMapper _mapper = mapper;

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
            var tokenValidation = await ValidateToken();
            if (tokenValidation is not null)
            {
                return tokenValidation;
            }

            var token = await GetToken();

            var teamUserIds = await _context.TeamUsers
                .Include(t => t.Team)
                .Include(t => t.User)
                .Where(t => t.UserId == token!.UserId)
                .Select(t => t.Id).ToListAsync();

            var tasks = await _context.ProjectTasks
                .Include(p => p.Project).ThenInclude(p => p.Team).ThenInclude(t => t.TeamUsers).ThenInclude(u => u.User)
                .Include(c => c.TaskCategory)
                .Include(b => b.Board)
                .Include(a => a.Appointee).ThenInclude(u => u.User)
                .Include(a => a.Appointer).ThenInclude(u => u.User)
                .Where(t => teamUserIds.Contains(t.AppointeeId) || teamUserIds.Contains(t.AppointerId))
                .ProjectTo<ProjectTaskDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            if (tasks is null || tasks.Count == 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.NoTasksFound));
            }

            foreach (var task in tasks)
            {
                task.CanDelete = teamUserIds.Contains(task.AppointerId);
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
        [HttpGet("[action]/{id}")]
        [Authorize]
        [ServiceFilter(typeof(NotFoundFilter<ProjectTask>))]
        public async Task<IActionResult> Details(int id)
        {
            var tokenValidation = await ValidateToken();
            if (tokenValidation is not null)
            {
                return tokenValidation;
            }

            var team = await _context.Teams.Where(t => t.Projects.Any(p => p.ProjectTasks.Any(pt => pt.Id == id))).FirstOrDefaultAsync();
            if (team is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound));
            }

            var token = await GetToken();
            var accessValidation = await ValidateTeamAccess(token, team.Id);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var taskDto = await _context.ProjectTasks
            .Include(p => p.Appointee).ThenInclude(p => p.User)
            .Include(p => p.Appointer).ThenInclude(p => p.User)
            .Include(p => p.Board)
            .Include(p => p.Project)
            .Include(p => p.TaskCategory)
            .Include(p => p.TaskComments).ThenInclude(c => c.TeamUser).ThenInclude(tu => tu.User)
            .Where(t => t.Id == id)
            .ProjectTo<ProjectTaskDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
            if (taskDto is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound));
            }

            return CreateActionResult(CustomResponseDto<ProjectTaskDto>.Success(taskDto, 200));
        }

        /// <summary>
        /// Retrieves the data needed to create a new task for a specific project
        /// </summary>
        /// <param name="projectId">The unique identifier of the project</param>
        /// <returns>Data for creating a new task, including team members, board tags, and task categories</returns>
        /// <response code="200">Returns the data for creating a new task</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the project is not found</response>
        [HttpGet("[action]/{projectId}")]
        [Authorize]
        public async Task<IActionResult> Create(int projectId)
        {
            var tokenValidation = await ValidateToken();
            if (tokenValidation is not null)
            {
                return tokenValidation;
            }

            var project = await _projectService.GetByIdAsync(projectId);
            if (project is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound));
            }

            var token = await GetToken();
            var accessValidation = await ValidateProjectAccess(token, projectId);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var teamMembers = await _context.TeamUsers
                .Include(x => x.User)
                .Where(x => x.TeamId == project.TeamId)
                .Select(x => new TaskUserDto
                {
                    TeamUserId = x.Id,
                    Name = x.User.Name
                })
                .ToListAsync();

            var boards = await _context.BoardTags
                .Select(x => new BoardTagDto
                {
                    Id = x.Id,
                    BoardName = x.BoardName
                })
                .ToListAsync();

            var taskCategories = await _context.TaskCategories
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
        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> Create(ProjectTaskCreateDto model)
        {
            var tokenValidation = await ValidateToken();
            if (tokenValidation is not null)
            {
                return tokenValidation;
            }

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

            var project = await _projectService.GetByIdAsync(model.ProjectId);
            if (project is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound));
            }

            var token = await GetToken();
            var accessValidation = await ValidateProjectAccess(token, model.ProjectId);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            accessValidation = await ValidateTeamAccess(token, project.TeamId);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var teamUser = _teamUserService.Where(t => t.UserId == token!.UserId && t.TeamId == project.TeamId).FirstOrDefault();

            var task = _mapper.Map<ProjectTask>(model);
            task.AppointerId = teamUser!.Id;

            var createdTask = await _taskService.AddAsync(task);
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
            var tokenValidation = await ValidateToken();
            if (tokenValidation is not null)
            {
                return tokenValidation;
            }

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

            var comment = _mapper.Map<TaskComment>(model);

            var task = await _taskService.GetByIdAsync(comment.TaskId);
            if (task is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound));
            }

            var teamFromTask = await _context.Teams
                .Include(t => t.Projects)
                .ThenInclude(p => p.ProjectTasks)
                .Where(x => x.Projects.Any(x => x.ProjectTasks.Any(x => x.Id == task.Id))).FirstOrDefaultAsync();
            if (teamFromTask is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound));
            }

            var token = await GetToken();
            var accessValidation = await ValidateTeamAccess(token, teamFromTask.Id);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var teamUser = await _teamUserService.Where(x => x.UserId == token!.UserId && x.TeamId == teamFromTask.Id).FirstOrDefaultAsync();

            comment.TeamUserId = teamUser!.Id;
            await _commentService.AddAsync(comment);
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(201));
        }

        /// <summary>
        /// Retrieves the data needed to edit a specific task
        /// </summary>
        /// <param name="id">The unique identifier of the task</param>
        /// <returns>Data for editing the task, including team members, board tags, and task categories</returns>
        /// <response code="200">Returns the data for editing the task</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the task is not found</response>
        [HttpGet("[action]/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var tokenValidation = await ValidateToken();
            if (tokenValidation is not null)
            {
                return tokenValidation;
            }

            var task = await _context.ProjectTasks
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

            var token = await GetToken();
            var accessValidation = await ValidateTeamAccess(token, task.Project.TeamId);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var teamMembers = await _context.TeamUsers
                .Include(x => x.User)
                .Where(x => x.TeamId == task.Project.TeamId)
                .Select(x => new TaskUserDto
                {
                    TeamUserId = x.Id,
                    Name = x.User.Name
                })
                .ToListAsync();

            var boards = await _context.BoardTags
                .Select(x => new BoardTagDto
                {
                    Id = x.Id,
                    BoardName = x.BoardName
                })
                .ToListAsync();

            var taskCategories = await _context.TaskCategories
                .Select(x => new TaskCategoryDto
                {
                    Id = x.Id,
                    Category = x.Category
                })
                .ToListAsync();

            var projectTaskUpdateDto = _mapper.Map<ProjectTaskUpdateDto>(task);
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
        [HttpPut("[action]")]
        [Authorize]
        public async Task<IActionResult> Edit(ProjectTaskUpdateDto model)
        {
            var tokenValidation = await ValidateToken();
            if (tokenValidation is not null)
            {
                return tokenValidation;
            }

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

            var project = await _projectService.Where(x => x.ProjectTasks.Any(pt => pt.Id == model.Id)).FirstOrDefaultAsync();
            if (project is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound));
            }

            var token = await GetToken();
            var accessValidation = await ValidateTeamAccess(token, project.TeamId);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var task = await _context.ProjectTasks.Include(pt => pt.Board).Where(x => x.Id == model.Id).FirstOrDefaultAsync();
            if (task is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound));
            }

            var oldBoardName = task.Board.BoardName;
            var newBoard = await _context.BoardTags.FindAsync(model.BoardId);
            var newBoardName = newBoard?.BoardName;
            if (oldBoardName == BoardStatus.Done && model.BoardId != task.BoardId)
            {
                task.EndDate = null;
            }

            if (newBoardName == BoardStatus.Done && oldBoardName != BoardStatus.Done && model.EndDate is null)
            {
                model.EndDate = DateTime.UtcNow;
            }

            _mapper.Map(model, task);
            _taskService.Update(task);
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
        [HttpGet("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            var tokenValidation = await ValidateToken();
            if (tokenValidation is not null)
            {
                return tokenValidation;
            }

            var idValidation = ValidateId(id, ErrorMessages.InvalidTaskId);
            if (idValidation is not null)
            {
                return idValidation;
            }

            var taskDelete = await _context.ProjectTasks
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

            var token = await GetToken();
            var teamUserIds = await _context.TeamUsers
                .Where(tu => tu.UserId == token!.UserId)
                .Select(tu => tu.Id)
                .ToListAsync();

            if (!teamUserIds.Contains(taskDelete.AppointerId))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToViewTask));
            }

            var taskDeleteDto = _mapper.Map<ProjectTaskDeleteDto>(taskDelete);

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
        [HttpDelete("[action]/{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var tokenValidation = await ValidateToken();
            if (tokenValidation is not null)
            {
                return tokenValidation;
            }

            var idValidation = ValidateId(id, ErrorMessages.InvalidTaskId);
            if (idValidation is not null)
            {
                return idValidation;
            }

            var task = await _taskService.GetByIdAsync(id);
            if (task is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TaskNotFound));
            }

            var token = await GetToken();
            var teamUser = await _context.TeamUsers
                .Include(tu => tu.Team)
                    .ThenInclude(t => t.Projects)
                        .ThenInclude(p => p.ProjectTasks)
                .Where(x => x.UserId == token!.UserId && x.Team.Projects.Any(x => x.ProjectTasks.Any(x => x.Id == id)))
                .FirstOrDefaultAsync();
            if (teamUser is null || task.AppointerId != teamUser.Id)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToDeleteTask));
            }

            task.IsActive = false;
            _taskService.ChangeStatus(task);
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }
        protected async Task<IActionResult?> ValidateProjectAccess(Token? token, int projectId, bool requireManagerAccess = false)
        {
            var project = await _projectService.GetByIdAsync(projectId);
            if (project is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound));
            }

            var isMember = await _context.TeamUsers.AnyAsync(t => t.UserId == token!.UserId && t.TeamId == project.TeamId);
            if (!isMember)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotTeamMember));
            }

            if (requireManagerAccess && project.Team.ManagerId != token!.UserId)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotAuthorizedToEditProject));
            }

            return null;
        }
        protected async Task<IActionResult?> ValidateTeamAccess(Token? token, int teamId, bool requireManagerAccess = false)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound));
            }

            var isMember = await _context.TeamUsers.AnyAsync(t => t.UserId == token!.UserId && t.TeamId == teamId);
            if (!isMember)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotTeamMember));
            }

            if (requireManagerAccess && team.ManagerId != token!.UserId)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotAuthorizedToEditProject));
            }

            return null;
        }
    }
}