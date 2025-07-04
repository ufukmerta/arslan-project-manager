using ArslanProjectManager.API.Filters;
using ArslanProjectManager.Core;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.DeleteDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.API.Controllers
{
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

        [HttpGet()]
        public async Task<IActionResult> GetByToken()
        {
            var token = base.GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in or access token is invalid"));
            }

            var teamUserIds = await _context.TeamUsers
                .Include(t => t.Team)
                .Include(t => t.User)
                .Where(t => t.UserId == token.UserId)
                .Select(t => t.Id).ToListAsync();

            var tasks = await _context.ProjectTasks
                .Include(p => p.Project)
                .ThenInclude(p => p.Team)
                .ThenInclude(t => t.TeamUsers)
                .ThenInclude(u => u.User)
                .Include(c => c.TaskCategory)
                .Include(b => b.Board)
                .Include(a => a.Appointee)
                .ThenInclude(u => u.User)
                .Include(a => a.Appointer)
                .ThenInclude(u => u.User)
                .Where(t => teamUserIds.Contains(t.AppointeeId) || teamUserIds.Contains(t.AppointerId))
                .Select(t => new ProjectTaskDto
                {
                    Id = t.Id,
                    TaskName = t.TaskName,
                    CreatedDate = t.CreatedDate,
                    UpdatedDate = t.UpdatedDate,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    ExpectedEndDate = t.ExpectedEndDate,
                    TaskCategoryId = t.TaskCategoryId,
                    TaskCategoryName = t.TaskCategory.Category,
                    BoardId = t.BoardId,
                    BoardName = t.Board.BoardName,
                    Description = t.Description,
                    AppointerId = t.AppointerId,
                    AppointerName = $"{t.Appointer.User.Name}",
                    AppointeeId = t.AppointeeId,
                    AppointeeName = $"{t.Appointee.User.Name}",
                    CanDelete = teamUserIds.Contains(t.AppointerId),
                    Priority = t.Priority,
                    ProjectId = t.ProjectId,
                    ProjectName = t.Project.ProjectName
                })
                .ToListAsync();

            if (tasks is null || tasks.Count == 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "No tasks found for you."));
            }
            return CreateActionResult(CustomResponseDto<List<ProjectTaskDto>>.Success(tasks, 200));
        }

        [HttpGet("[action]/{id}")]
        [Authorize]
        [ServiceFilter(typeof(NotFoundFilter<ProjectTask>))]
        public async Task<IActionResult> Details(int id)
        {
            var token = base.GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in or access token is invalid"));
            }

            var teamUserIds = await _context.TeamUsers
                .Where(tu => tu.UserId == token.UserId)
                .Select(tu => tu.Id).ToListAsync();

            var taskDto = await _context.ProjectTasks
                .Include(p => p.Appointee)
                .ThenInclude(p => p.User)
                .Include(p => p.Appointer)
                .ThenInclude(p => p.User)
                .Include(p => p.Board)
                .Include(p => p.Project)
                .Include(p => p.TaskCategory)
                .Include(p => p.TaskComments)
                .ThenInclude(c => c.TeamUser)
                .ThenInclude(tu => tu.User)
                .Where(t => t.Id == id)
                .Select(t => new ProjectTaskDto
                {
                    Id = t.Id,
                    TaskName = t.TaskName,
                    CreatedDate = t.CreatedDate,
                    UpdatedDate = t.UpdatedDate,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    ExpectedEndDate = t.ExpectedEndDate,
                    TaskCategoryId = t.TaskCategoryId,
                    TaskCategoryName = t.TaskCategory.Category,
                    BoardId = t.BoardId,
                    BoardName = t.Board.BoardName,
                    Description = t.Description,
                    AppointerId = t.AppointerId,
                    AppointerName = $"{t.Appointer.User.Name}",
                    AppointeeId = t.AppointeeId,
                    AppointeeName = $"{t.Appointee.User.Name}",
                    Priority = t.Priority,
                    ProjectId = t.ProjectId,
                    ProjectName = t.Project.ProjectName,
                    Comments = t.TaskComments.Select(c => new TaskCommentDto
                    {
                        Id = c.Id,
                        TaskId = c.TaskId,
                        TeamUserId = c.TeamUserId,
                        Comment = c.Comment,
                        CreatedDate = c.CreatedDate,
                        CommenterName = $"{c.TeamUser.User.Name}"
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (taskDto == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Task not found"));
            }

            if (!teamUserIds.Contains(taskDto.AppointeeId) && !teamUserIds.Contains(taskDto.AppointerId))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, "You do not have permission to view this task"));
            }

            return CreateActionResult(CustomResponseDto<ProjectTaskDto>.Success(taskDto, 200));
        }

        [HttpGet("[action]/{projectId}")]
        [Authorize]
        public async Task<IActionResult> Create(int projectId)
        {
            var token = base.GetToken();
            if (token == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in or access token is invalid"));
            }

            var project = await _projectService.GetByIdAsync(projectId);
            if (project is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Project not found"));
            }

            var isUserMemberOfProject = await _teamUserService.AnyAsync(t => t.UserId == token.UserId && t.TeamId == project.TeamId);
            if (!isUserMemberOfProject)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, "You do not have permission to create task in this project."));
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

            ProjectTaskCreateViewDto projectTaskCreateViewDto = new ProjectTaskCreateViewDto
            {
                ProjectId = project.Id,
                TeamMembers = teamMembers,
                BoardTags = boards,
                TaskCategories = taskCategories
            };
            return CreateActionResult(CustomResponseDto<ProjectTaskCreateViewDto>.Success(projectTaskCreateViewDto, 200));
        }

        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> Create(ProjectTaskCreateDto model)
        {
            var token = base.GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in or access token is invalid"));
            }

            if (model is null || model.ProjectId <= 0 || model.Priority < 0 || model.BoardId <= 0
                || model.TaskCategoryId <= 0 || model.AppointeeId <= 0 || string.IsNullOrEmpty(model.TaskName))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Invalid project data"));
            }

            var project = await _projectService.GetByIdAsync(model.ProjectId);
            if (project is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Project not found."));
            }

            var teamUser = _teamUserService.Where(t => t.UserId == token.UserId && t.TeamId == project.TeamId).FirstOrDefault();
            if (teamUser is null)
            {
                CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, "Forbidden request: You're not member of the team of the project."));
            }

            var task = new ProjectTask
            {
                ProjectId = model.ProjectId,
                TaskName = model.TaskName,
                ExpectedEndDate = model.ExpectedEndDate,
                TaskCategoryId = model.TaskCategoryId,
                BoardId = model.BoardId,
                Description = model.Description,
                AppointerId = teamUser!.Id,
                AppointeeId = model.AppointeeId,
                Priority = (ProjectTask.TaskPriority)model.Priority
            };

            var createdTask = await _taskService.AddAsync(task);
            if (createdTask is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Failed to create project"));
            }

            return CreateActionResult(CustomResponseDto<MiniProjectTaskDto>.Success(new MiniProjectTaskDto { Id = createdTask.Id }, 201));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Comment(TaskCommentCreateDto model)
        {
            var token = base.GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in or access token is invalid"));
            }

            var comment = _mapper.Map<TaskComment>(model);
            if (comment is null || comment.TaskId <= 0 || string.IsNullOrEmpty(comment.Comment))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Invalid input"));
            }

            var task = await _taskService.GetByIdAsync(comment.TaskId);
            if (task is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Task not found."));
            }

            var teamFromTask = await _context.Teams
                .Include(t => t.Projects)
                .ThenInclude(p => p.ProjectTasks)
                .Where(x => x.Projects.Any(x => x.ProjectTasks.Any(x => x.Id == task.Id))).FirstOrDefaultAsync();
            if (teamFromTask is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Not found."));
            }

            var teamUser = await _teamUserService.Where(x => x.UserId == token.UserId && x.TeamId == teamFromTask.Id).FirstOrDefaultAsync();
            if (teamUser is null)
            {
                CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, "Forbidden request: You're not member of the team of the project."));
            }

            comment.TeamUserId = teamUser!.Id;
            await _commentService.AddAsync(comment);
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(201));
        }

        [HttpGet("[action]/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var token = base.GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in or access token is invalid."));
            }

            var task = await _context.ProjectTasks.Include(pt => pt.Project).ThenInclude(p => p.Team).Where(x => x.Id == id).FirstOrDefaultAsync();
            if (task is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Task not found."));
            }

            var isUserMemberOfProject = await _teamUserService.AnyAsync(t => t.UserId == token.UserId && t.TeamId == task.Project.TeamId);
            if (!isUserMemberOfProject)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, "You're not authorized to edit this task."));
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

            var projectTaskUpdateDto = await _context.ProjectTasks
                .Include(p => p.Project)
                .Include(p => p.TaskCategory)
                .Include(p => p.Board)
                .Include(p => p.Appointee)
                .ThenInclude(a => a.User)
                .Where(t => t.Id == id)
                .Select(t => new ProjectTaskUpdateDto
                {
                    Id = t.Id,
                    TaskName = t.TaskName,
                    Description = t.Description,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    ExpectedEndDate = t.ExpectedEndDate,
                    Priority = t.Priority,
                    TaskCategoryId = t.TaskCategoryId,
                    AppointeeId = t.AppointeeId,
                    BoardId = t.BoardId,
                    TeamMembers = teamMembers,
                    BoardTags = boards,
                    TaskCategories = taskCategories
                })
                .FirstOrDefaultAsync();

            if (projectTaskUpdateDto is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Task not found."));
            }

            return CreateActionResult(CustomResponseDto<ProjectTaskUpdateDto>.Success(projectTaskUpdateDto!, 200));
        }

        [HttpPut("[action]")]
        [Authorize]
        public async Task<IActionResult> Edit(ProjectTaskUpdateDto model)
        {
            var token = base.GetToken();
            if (token == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in or access token is invalid"));
            }

            if (model is null || model.Id <= 0 || model.Priority < 0 || model.BoardId <= 0
                || model.TaskCategoryId <= 0 || model.AppointeeId <= 0 || string.IsNullOrEmpty(model.TaskName))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Invalid project data"));
            }

            var project = await _projectService.Where(x => x.ProjectTasks.Any(pt => pt.Id == model.Id)).FirstOrDefaultAsync();
            if (project is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Project not found."));
            }

            var teamUser = _teamUserService.Where(t => t.UserId == token.UserId && t.TeamId == project.TeamId).FirstOrDefault();
            if (teamUser is null)
            {
                CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, "Forbidden request: You're not member of the team of the project."));
            }

            var task = await _context.ProjectTasks.Include(pt => pt.Board).Where(x => x.Id == model.Id).FirstOrDefaultAsync();
            if (task is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Task not found."));
            }

            if (task.Board.BoardName == "Done" && task.BoardId != model.BoardId)
            {
                task.EndDate = null;
            }

            var board = await _context.BoardTags.FindAsync(model.BoardId);
            if (board!.BoardName == "Done" && task.Board.BoardName != "Done")
            {
                task.EndDate = DateTime.UtcNow;
            }

            task.TaskName = model.TaskName;
            task.Description = model.Description;
            task.StartDate = model.StartDate;
            task.ExpectedEndDate = model.ExpectedEndDate;
            task.TaskCategoryId = model.TaskCategoryId;
            task.BoardId = model.BoardId;
            task.AppointeeId = model.AppointeeId;
            task.Priority = model.Priority;
            _taskService.Update(task);

            return CreateActionResult(CustomResponseDto<MiniProjectTaskDto>.Success(new MiniProjectTaskDto { Id = model.Id }, 201));
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            var token = base.GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in or access token is invalid"));
            }

            if (id <= 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Input isn't valid for task."));
            }

            var taskDelete = _context.ProjectTasks
                .Include(p => p.Appointee)
                .ThenInclude(p => p.User)
                .Include(p => p.Appointer)
                .ThenInclude(p => p.User)
                .Include(p => p.Board)
                .Include(p => p.Project)
                .Include(p => p.TaskCategory)
                .Where(t => t.Id == id).FirstOrDefault();

            if (taskDelete is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Task not found."));
            }

            var teamUserIds = await _context.TeamUsers
                .Where(tu => tu.UserId == token.UserId)
                .Select(tu => tu.Id).ToListAsync();
            if (!teamUserIds.Contains(taskDelete.AppointerId))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, "You do not have permission to view this task"));
            }

            var taskDeleteDto = new ProjectTaskDeleteDto
            {
                Id = taskDelete.Id,
                TaskName = taskDelete.TaskName,
                ProjectName = taskDelete.Project.ProjectName,
                Description = taskDelete.Description ?? string.Empty,
                CreatedDate = taskDelete.CreatedDate,
                BoardName = taskDelete.Board.BoardName,
                TaskCategoryName = taskDelete.TaskCategory.Category,
                AppointeeName = taskDelete.Appointee.User.Name,
                AppointerName = taskDelete.Appointer.User.Name,
                Priority = taskDelete.Priority
            };

            return CreateActionResult(CustomResponseDto<ProjectTaskDeleteDto>.Success(taskDeleteDto, 200));
        }

        [HttpDelete("[action]/{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            Token? token = base.GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in or access token is invalid"));
            }

            if (id <= 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Invalid task ID"));
            }

            var task = await _taskService.GetByIdAsync(id);
            if (task is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Task not found"));
            }

            var teamUser = await _context.TeamUsers.Include(tu => tu.Team).ThenInclude(t => t.Projects).ThenInclude(p => p.ProjectTasks)
                .Where(x => x.UserId == token.UserId && x.Team.Projects.Any(x => x.ProjectTasks.Any(x => x.Id == id))).FirstOrDefaultAsync();
            if (teamUser is null || task.AppointerId != teamUser.Id)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, "You do not have permission to delete this task."));
            }

            task.IsActive = false;
            _taskService.ChangeStatus(task);
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }
    }
}