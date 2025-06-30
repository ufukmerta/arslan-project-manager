using ArslanProjectManager.API.Filters;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.DeleteDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController(ProjectManagerDbContext context, IProjectService projectService, ITokenService tokenService) : CustomBaseController(tokenService)
    {
        private readonly ProjectManagerDbContext _context = context;
        private readonly IProjectService _projectService = projectService;
        private readonly ITokenService _tokenService = tokenService;

        [Authorize]
        [HttpGet()]
        public async Task<IActionResult> GetByToken()
        {
            Token? token = base.GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in or access token is invalid"));
            }

            var x = await _projectService.AnyAsync(x => x.Team.TeamUsers.Any(x => x.UserId == token.UserId));
            if (!x)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "No projects found for this user"));
            }

            var projects = await _context.Projects
               .Include(x => x.Team)
               .ThenInclude(x => x.TeamUsers)
               .Include(x => x.ProjectTasks)
               .Where(x => x.Team.TeamUsers.Any(tu => tu.UserId == token.UserId))
               .Select(p => new UserProjectDto
               {
                   ProjectId = p.Id,
                   ProjectName = p.ProjectName,
                   Description = p.ProjectDetail,
                   StartDate = p.StartDate,
                   TeamName = p.Team.TeamName,
                   TeamId = p.TeamId,
                   ManagerId = p.Team.ManagerId,
                   TaskCount = p.ProjectTasks.Count,
                   CompletedTaskCount = p.ProjectTasks.Count(t => t.BoardId == 3)
               })
               .ToListAsync();

            return CreateActionResult(CustomResponseDto<IEnumerable<UserProjectDto>>.Success(projects, 200));
        }

        [Authorize]
        [HttpGet("[action]/{id}")]
        [ServiceFilter(typeof(NotFoundFilter<Project>))]
        public async Task<IActionResult> Details(int id)
        {
            Token? token = base.GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in or access token is invalid"));
            }

            if (id <= 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Invalid project ID"));
            }

            var userProject = _projectService.Where(x => x.Id == id && x.Team.TeamUsers.Any(tu => tu.UserId == token.UserId));
            if (!userProject.Any())
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Access denied"));
            }

            var projectDetailsDto = await _projectService.GetProjectDetailsAsync(id);
            if (projectDetailsDto is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Project not found"));
            }

            return CreateActionResult(CustomResponseDto<ProjectDetailsDto>.Success(projectDetailsDto, 200));
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<IActionResult> Create()
        {
            Token? token = base.GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in or access token is invalid"));
            }

            var userTeamDto = await _context.TeamUsers
                .Include(tu => tu.Team)
                .Where(tu => tu.UserId == token.UserId)
                .Select(tu => new MiniTeamDto
                {
                    Id = tu.TeamId,
                    TeamName = tu.Team.TeamName
                })
                .ToListAsync();
            if (userTeamDto.Count == 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "No teams found for this user"));
            }

            return CreateActionResult(CustomResponseDto<List<MiniTeamDto>>.Success(userTeamDto, 200));
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> Create(ProjectCreateDto model)
        {
            Token? token = base.GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in or access token is invalid"));
            }

            if (model is null || string.IsNullOrWhiteSpace(model.ProjectName) || model.TeamId <= 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Invalid project data"));
            }

            var team = await _context.Teams
                .Include(t => t.TeamUsers)
                .FirstOrDefaultAsync(t => t.Id == model.TeamId && t.TeamUsers.Any(tu => tu.UserId == token.UserId));
            if (team is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Team not found or access denied"));
            }

            var project = new Project
            {
                ProjectName = model.ProjectName,
                ProjectDetail = model.ProjectDetail,
                StartDate = model.StartDate,
                TeamId = model.TeamId
            };

            var createdProjectEntry = await _context.Projects.AddAsync(project);
            if (createdProjectEntry is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Failed to create project"));
            }

            var createdProject = createdProjectEntry.Entity;
            var createdProjectDto = new MiniProjectDto
            {
                Id = createdProject.Id,
                CreatedDate = createdProject.CreatedDate,
                ProjectName = createdProject.ProjectName
            };

            await _context.SaveChangesAsync();
            return CreateActionResult(CustomResponseDto<MiniProjectDto>.Success(createdProjectDto, 201));
        }

        [Authorize]
        [HttpGet("[action]/{id}")]
        [ServiceFilter(typeof(NotFoundFilter<Project>))]
        public async Task<IActionResult> Edit(int id)
        {
            Token? token = base.GetToken();
            if (token == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in or access token is invalid"));
            }

            if (id <= 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Invalid project ID"));
            }

            var project = await _context.Projects
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Project not found"));
            }
            if (project.Team.ManagerId != token.UserId)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, "You're not authorized to edit this project"));
            }

            var projectUpdateDto = new ProjectUpdateDto
            {
                Id = project.Id,
                ProjectName = project.ProjectName,
                ProjectDetail = project.ProjectDetail,
                StartDate = project.StartDate
            };

            return CreateActionResult(CustomResponseDto<ProjectUpdateDto>.Success(projectUpdateDto, 200));
        }

        [Authorize]
        [HttpPut("[action]")]
        public async Task<IActionResult> Edit(ProjectUpdateDto model)
        {
            Token? token = base.GetToken();
            if (token == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in or access token is invalid"));
            }

            if (model == null || model.Id <= 0 || string.IsNullOrWhiteSpace(model.ProjectName))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Invalid project data"));
            }

            var project = await _context.Projects
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == model.Id && p.Team.TeamUsers.Any(tu => tu.UserId == token.UserId));
            if (project == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Project not found or access denied"));
            }

            project.ProjectName = model.ProjectName;
            project.ProjectDetail = model.ProjectDetail;
            project.StartDate = model.StartDate;
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            var updatedProjectDto = new MiniProjectDto
            {
                Id = project.Id,
                ProjectName = project.ProjectName,
                CreatedDate = project.CreatedDate
            };

            return CreateActionResult(CustomResponseDto<MiniProjectDto>.Success(updatedProjectDto, 200));
        }

        [Authorize]
        [HttpGet("delete/{id}")]
        [ServiceFilter(typeof(NotFoundFilter<Project>))]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            Token? token = base.GetToken();
            if (token == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in or access token is invalid"));
            }

            if (id <= 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Invalid project ID"));
            }

            // Check if the project exists and the user has access
            var project = await _context.Projects
                .Include(p => p.Team)
                .Include(p => p.ProjectTasks)
                .FirstOrDefaultAsync(p => p.Id == id && p.Team.ManagerId == token.UserId);
            if (project == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, "Access denied"));
            }

            var projectDeleteDto = new ProjectDeleteDto
            {
                Id = project.Id,
                ProjectName = project.ProjectName,
                ProjectDetail = project.ProjectDetail,
                StartDate = project.StartDate,
                TeamName = project.Team.TeamName,
                TaskCount = project.ProjectTasks.Count,
                CompletedTaskCount = project.ProjectTasks.Count(t => t.BoardId == 3)
            };

            return CreateActionResult(CustomResponseDto<ProjectDeleteDto>.Success(projectDeleteDto, 200));
        }

        [Authorize]
        [HttpDelete("[action]/{id}")]
        [ServiceFilter(typeof(NotFoundFilter<Project>))]
        public async Task<IActionResult> Delete(int id)
        {
            Token? token = base.GetToken();
            if (token == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in or access token is invalid"));
            }

            if (id <= 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Invalid project ID"));
            }

            // Check if the project exists and the user has access
            var project = await _context.Projects.Include(p => p.Team).FirstOrDefaultAsync(p => p.Id == id && p.Team.ManagerId == token.UserId);
            if (project == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Project not found or access denied"));
            }

            // Soft delete the project by changing its status
            project.IsActive = false;
            _projectService.ChangeStatus(project);
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }
    }
}
