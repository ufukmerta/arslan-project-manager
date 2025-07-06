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
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController(ProjectManagerDbContext context, IProjectService projectService, ITokenService tokenService, IMapper mapper) : CustomBaseController(tokenService)
    {
        private readonly ProjectManagerDbContext _context = context;
        private readonly IProjectService _projectService = projectService;
        private readonly IMapper _mapper = mapper;

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
            var doesProjectExist = await _projectService.AnyAsync(x => x.Team.TeamUsers.Any(x => x.UserId == token!.UserId));
            if (!doesProjectExist)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.NoProjectsFound));
            }

            var projects = await _context.Projects
                 .Where(x => x.Team.TeamUsers.Any(tu => tu.UserId == token!.UserId))
                 .ProjectTo<UserProjectDto>(_mapper.ConfigurationProvider)
                 .ToListAsync();

            return CreateActionResult(CustomResponseDto<IEnumerable<UserProjectDto>>.Success(projects, 200));
        }

        [HttpGet("[action]/{id}")]
        [Authorize]
        [ServiceFilter(typeof(NotFoundFilter<Project>))]
        public async Task<IActionResult> Details(int id)
        {
            var tokenValidation = await ValidateToken();
            if (tokenValidation is not null)
            {
                return tokenValidation;
            }

            var idValidation = ValidateId(id, ErrorMessages.InvalidProjectId);
            if (idValidation is not null)
            {
                return idValidation;
            }

            var token = await GetToken();
            var accessValidation = await ValidateProjectAccess(token, id);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var projectDetailsDto = await _projectService.GetProjectDetailsAsync(id);
            if (projectDetailsDto is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound));
            }

            return CreateActionResult(CustomResponseDto<ProjectDetailsDto>.Success(projectDetailsDto, 200));
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var tokenValidation = await ValidateToken();
            if (tokenValidation is not null)
            {
                return tokenValidation;
            }

            var token = await GetToken();
            var userTeamDto = await _context.TeamUsers
                .Include(tu => tu.Team)
                .Where(tu => tu.UserId == token!.UserId)
                .Select(tu => new MiniTeamDto
                {
                    Id = tu.TeamId,
                    TeamName = tu.Team.TeamName
                })
                .ToListAsync();
            if (userTeamDto.Count is 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.NoTeamsFound));
            }

            return CreateActionResult(CustomResponseDto<List<MiniTeamDto>>.Success(userTeamDto, 200));
        }

        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> Create(ProjectCreateDto model)
        {
            var tokenValidation = await ValidateToken();
            if (tokenValidation is not null)
            {
                return tokenValidation;
            }

            var validationResult = ValidateModel(
                model,
                m => m != null && !string.IsNullOrWhiteSpace(m.ProjectName) && m.TeamId > 0,
                ErrorMessages.InvalidProjectData
            );
            if (validationResult is not null)
            {
                return validationResult;
            }

            var token = await GetToken();
            var team = await _context.Teams
                .Include(t => t.TeamUsers)
                .AnyAsync(t => t.Id == model.TeamId);
            if (!team)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound));
            }

            var teamByTeamUser = await _context.Teams
                .Include(t => t.TeamUsers)
                .FirstOrDefaultAsync(t => t.Id == model.TeamId && t.TeamUsers.Any(tu => tu.UserId == token!.UserId));
            if (teamByTeamUser is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.AccessDenied));
            }

            var project = _mapper.Map<Project>(model);
            var createdProject = await _projectService.AddAsync(project);
            if (createdProject is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToCreateProject));
            }

            var createdProjectDto = _mapper.Map<MiniProjectDto>(createdProject);
            return CreateActionResult(CustomResponseDto<MiniProjectDto>.Success(createdProjectDto, 201));
        }

        [HttpGet("[action]/{id}")]
        [Authorize]
        [ServiceFilter(typeof(NotFoundFilter<Project>))]
        public async Task<IActionResult> Edit(int id)
        {
            var tokenValidation = await ValidateToken();
            if (tokenValidation is not null)
            {
                return tokenValidation;
            }

            var idValidation = ValidateId(id, ErrorMessages.InvalidProjectId);
            if (idValidation is not null)
            {
                return idValidation;
            }

            var token = await GetToken();
            var accessValidation = await ValidateProjectAccess(token, id, requireManagerAccess: true);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var project = await _context.Projects
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound));
            }

            var projectUpdateDto = _mapper.Map<ProjectUpdateDto>(project);
            return CreateActionResult(CustomResponseDto<ProjectUpdateDto>.Success(projectUpdateDto, 200));
        }

        [HttpPut("[action]")]
        [Authorize]
        public async Task<IActionResult> Edit(ProjectUpdateDto model)
        {
            var tokenValidation = await ValidateToken();
            if (tokenValidation is not null)
            {
                return tokenValidation;
            }

            var validationResult = ValidateModel(
                model,
                m => m != null && m.Id > 0 && !string.IsNullOrWhiteSpace(m.ProjectName),
                ErrorMessages.InvalidProjectData
            );
            if (validationResult is not null)
            {
                return validationResult;
            }

            var token = await GetToken();
            var accessValidation = await ValidateProjectAccess(token, model.Id, requireManagerAccess: true);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var project = await _context.Projects
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            if (project is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound));
            }

            project.ProjectName = model.ProjectName;
            project.ProjectDetail = model.ProjectDetail;
            project.StartDate = model.StartDate;
            _projectService.Update(project);
            var updatedProjectDto = _mapper.Map<MiniProjectDto>(project);
            return CreateActionResult(CustomResponseDto<MiniProjectDto>.Success(updatedProjectDto, 200));
        }

        [Authorize]
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            var tokenValidation = await ValidateToken();
            if (tokenValidation is not null)
            {
                return tokenValidation;
            }

            var idValidation = ValidateId(id, ErrorMessages.InvalidProjectId);
            if (idValidation is not null)
            {
                return idValidation;
            }

            var token = await GetToken();
            var accessValidation = await ValidateProjectAccess(token, id, requireManagerAccess: true);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var project = await _context.Projects
                .Include(p => p.Team)
                .Include(p => p.ProjectTasks)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound));
            }

            var projectDeleteDto = _mapper.Map<ProjectDeleteDto>(project);
            return CreateActionResult(CustomResponseDto<ProjectDeleteDto>.Success(projectDeleteDto, 200));
        }

        [Authorize]
        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tokenValidation = await ValidateToken();
            if (tokenValidation is not null)
            {
                return tokenValidation;
            }

            var idValidation = ValidateId(id, ErrorMessages.InvalidProjectId);
            if (idValidation is not null)
            {
                return idValidation;
            }

            var token = await GetToken();

            var accessValidation = await ValidateProjectAccess(token, id, requireManagerAccess: true);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var project = await _context.Projects.Include(p => p.Team).FirstOrDefaultAsync(p => p.Id == id);

            if (project is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound));
            }

            project.IsActive = false;
            _projectService.ChangeStatus(project);
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }

        protected async Task<IActionResult?> ValidateProjectAccess(Token? token, int projectId, bool requireManagerAccess = false)
        {
            var project = await _context.Projects
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound));
            }

            if (requireManagerAccess)
            {
                if (project.Team.ManagerId != token!.UserId)
                {
                    return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotAuthorizedToEditProject));
                }
            }
            else
            {
                var isMember = await _context.TeamUsers.AnyAsync(t => t.UserId == token!.UserId && t.TeamId == project.TeamId);
                if (!isMember)
                {
                    return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotTeamMember));
                }
            }

            return null;
        }
    }
}
