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
    /// Manages project-related operations including CRUD operations for projects
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController(ProjectManagerDbContext context, IProjectService projectService, ITokenService tokenService, IMapper mapper) : CustomBaseController(tokenService)
    {
        /// <summary>
        /// Retrieves all projects for the authenticated user
        /// </summary>
        /// <returns>List of projects that the user has access to</returns>
        /// <response code="200">Returns the list of user's projects</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If no projects are found for the user</response>
        [HttpGet()]
        [Authorize]
        public async Task<IActionResult> GetByToken()
        {
            var token = (await GetToken())!;
            var teamUsersWithRole = await context.TeamUsers
                .Include(tu => tu.Role)
                .Where(tu => tu.UserId == token.UserId)
                .ToListAsync();
            var teamIdsWithViewAccess = teamUsersWithRole
                .Where(tu => PermissionResolver.HasPermission(tu, tu.Role, p => p.CanViewProjects))
                .Select(tu => tu.TeamId)
                .ToHashSet();
            if (teamIdsWithViewAccess.Count == 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.NoProjectsFound));
            }

            var teamPermissions = teamUsersWithRole
                .Where(tu => teamIdsWithViewAccess.Contains(tu.TeamId))
                .ToDictionary(tu => tu.TeamId, tu => (
                    CanEdit: PermissionResolver.HasPermission(tu, tu.Role, p => p.CanEditProjects),
                    CanDelete: PermissionResolver.HasPermission(tu, tu.Role, p => p.CanDeleteProjects)
                ));

            var projects = await context.Projects
                .Where(p => teamIdsWithViewAccess.Contains(p.TeamId))
                .ProjectTo<UserProjectDto>(mapper.ConfigurationProvider)
                .ToListAsync();

            foreach (var project in projects)
            {
                if (teamPermissions.TryGetValue(project.TeamId, out var perms))
                {
                    project.CanEdit = perms.CanEdit;
                    project.CanDelete = perms.CanDelete;
                }
            }

            return CreateActionResult(CustomResponseDto<IEnumerable<UserProjectDto>>.Success(projects, 200));
        }

        /// <summary>
        /// Retrieves detailed information about a specific project
        /// </summary>
        /// <param name="id">The unique identifier of the project</param>
        /// <returns>Detailed project information including tasks and team members</returns>
        /// <response code="200">Returns the project details</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have access to this project</response>
        /// <response code="404">If the project is not found</response>
        [HttpGet("{id:int}")]
        [Authorize]
        [ServiceFilter(typeof(NotFoundFilter<Project>))]
        public async Task<IActionResult> Details(int id)
        {
            var idValidation = ValidateId(id, ErrorMessages.InvalidProjectId);
            if (idValidation is not null)
            {
                return idValidation;
            }

            var token = (await GetToken())!;

            var accessValidation = await ValidateProjectAccess(token, id, requireViewAccess: true);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var projectDetailsDto = await projectService.GetProjectDetailsAsync(id);
            // no need to check for null, NotFoundFilter will handle it
            return CreateActionResult(CustomResponseDto<ProjectDetailsDto>.Success(projectDetailsDto!, 200));
        }

        /// <summary>
        /// Retrieves the metadata needed to create a new project
        /// </summary>
        /// <returns>Teams that the user can create projects for</returns>
        /// <response code="200">Returns available teams for project creation</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet("[action]-meta")]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var token = (await GetToken())!;

            var teamUsersWithRole = await context.TeamUsers
                .Include(tu => tu.Role)
                .Include(tu => tu.Team)
                .Where(tu => tu.UserId == token.UserId)
                .ToListAsync();
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
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToCreateProject));
            }

            return CreateActionResult(CustomResponseDto<List<MiniTeamDto>>.Success(teamsWithEditPermission, 200));
        }

        /// <summary>
        /// Creates a new project
        /// </summary>
        /// <param name="model">The project creation data</param>
        /// <returns>The created project's details</returns>
        /// <response code="201">Returns the created project</response>
        /// <response code="400">If the model is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have access to the team</response>
        /// <response code="404">If the team is not found</response>
        /// <response code="500">If the project creation fails</response>
        [HttpPost()]
        [Authorize]
        public async Task<IActionResult> Create(ProjectCreateDto model)
        {
            var validationResult = ValidateModel(
                model,
                m => m != null && !string.IsNullOrWhiteSpace(m.ProjectName) && m.TeamId > 0,
                ErrorMessages.InvalidProjectData
            );
            if (validationResult is not null)
            {
                return validationResult;
            }

            var token = (await GetToken())!;
            var team = await context.Teams
                .Include(t => t.TeamUsers)
                .AnyAsync(t => t.Id == model.TeamId);
            if (!team)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound));
            }

            var teamByTeamUser = await context.Teams
                .Include(t => t.TeamUsers)
                .ThenInclude(tu => tu.Role)
                .FirstOrDefaultAsync(t => t.Id == model.TeamId && t.TeamUsers.Any(tu => tu.UserId == token!.UserId));
            if (teamByTeamUser is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.AccessDenied));
            }

            var canUserCreateProjects = teamByTeamUser.TeamUsers
                .Any(tu => PermissionResolver.HasPermission(tu, tu.Role, p => p.CanEditProjects));
            if (!canUserCreateProjects)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToCreateProject));
            }

            var project = mapper.Map<Project>(model);
            var createdProject = await projectService.AddAsync(project);
            if (createdProject is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToCreateProject));
            }

            var createdProjectDto = mapper.Map<MiniProjectDto>(createdProject);
            return CreateActionResult(CustomResponseDto<MiniProjectDto>.Success(createdProjectDto, 201));
        }

        /// <summary>
        /// Retrieves the metadata needed to edit a specific project
        /// </summary>
        /// <param name="id">The unique identifier of the project</param>
        /// <returns>The project metadata for editing</returns>
        /// <response code="200">Returns the project metadata for editing</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have access to this project</response>
        /// <response code="404">If the project is not found</response>
        [HttpGet("{id:int}/[action]-meta")]
        [Authorize]
        [ServiceFilter(typeof(NotFoundFilter<Project>))]
        public async Task<IActionResult> Edit(int id)
        {
            var idValidation = ValidateId(id, ErrorMessages.InvalidProjectId);
            if (idValidation is not null)
            {
                return idValidation;
            }

            var token = (await GetToken())!;

            var accessValidation = await ValidateProjectAccess(token, id, requireEditAccess: true);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var project = await context.Projects
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == id);

            var projectUpdateDto = mapper.Map<ProjectUpdateDto>(project);
            return CreateActionResult(CustomResponseDto<ProjectUpdateDto>.Success(projectUpdateDto, 200));
        }

        /// <summary>
        /// Updates an existing project
        /// </summary>
        /// <param name="model">The updated project data</param>
        /// <returns>The updated project's details</returns>
        /// <response code="200">Returns the updated project</response>
        /// <response code="400">If the model is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have access to this project</response>
        /// <response code="404">If the project is not found</response>
        [HttpPut()]
        [Authorize]
        public async Task<IActionResult> Edit(ProjectUpdateDto model)
        {
            var validationResult = ValidateModel(
                model,
                m => m != null && m.Id > 0 && !string.IsNullOrWhiteSpace(m.ProjectName),
                ErrorMessages.InvalidProjectData
            );
            if (validationResult is not null)
            {
                return validationResult;
            }

            var token = (await GetToken())!;
            var accessValidation = await ValidateProjectAccess(token, model.Id, requireEditAccess: true);
            if (accessValidation is not null)
            {
                return accessValidation;
            }
            // null check is not required due to NotFoundFilter.
            var project = await context.Projects
                .Include(p => p.Team)
                .FirstAsync(p => p.Id == model.Id);

            project.ProjectName = model.ProjectName;
            project.ProjectDetail = model.ProjectDetail;
            project.StartDate = model.StartDate;
            projectService.Update(project);
            var updatedProjectDto = mapper.Map<MiniProjectDto>(project);
            return CreateActionResult(CustomResponseDto<MiniProjectDto>.Success(updatedProjectDto, 200));
        }

        /// <summary>
        /// Retrieves the data needed to confirm project deletion
        /// </summary>
        /// <param name="id">The unique identifier of the project</param>
        /// <returns>The project data for deletion confirmation</returns>
        /// <response code="200">Returns the project data for deletion confirmation</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have access to this project</response>
        /// <response code="404">If the project is not found</response>
        [Authorize]
        [HttpGet("{id:int}/delete-confirm")]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            var idValidation = ValidateId(id, ErrorMessages.InvalidProjectId);
            if (idValidation is not null)
            {
                return idValidation;
            }

            var token = (await GetToken())!;
            var accessValidation = await ValidateProjectAccess(token, id, requireDeleteAccess: true);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var project = await context.Projects
                .Include(p => p.Team)
                .Include(p => p.ProjectTasks)
                .FirstOrDefaultAsync(p => p.Id == id);

            var projectDeleteDto = mapper.Map<ProjectDeleteDto>(project);
            return CreateActionResult(CustomResponseDto<ProjectDeleteDto>.Success(projectDeleteDto, 200));
        }

        /// <summary>
        /// Deletes a specific project
        /// </summary>
        /// <param name="id">The unique identifier of the project</param>
        /// <returns>No content response</returns>
        /// <response code="204">Project deleted successfully</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have access to this project</response>
        /// <response code="404">If the project is not found</response>
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var idValidation = ValidateId(id, ErrorMessages.InvalidProjectId);
            if (idValidation is not null)
            {
                return idValidation;
            }

            var token = (await GetToken())!;
            var accessValidation = await ValidateProjectAccess(token, id, requireDeleteAccess: true);
            if (accessValidation is not null)
            {
                return accessValidation;
            }

            var project = await context.Projects.Include(p => p.Team).FirstAsync(p => p.Id == id);
            project.IsActive = false;
            projectService.ChangeStatus(project);
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }

        /// <summary>
        /// Validates if the user has the required access to the project based on the specified permissions
        /// </summary>
        /// <param name="token">The user's authentication token</param>
        /// <param name="projectId">The unique identifier of the project</param>
        /// <param name="requireViewAccess">Whether view access is required</param>
        /// <param name="requireEditAccess">Whether edit access is required</param>
        /// <param name="requireDeleteAccess">Whether delete access is required</param>
        /// <returns>An IActionResult if access is denied or the project is not found, otherwise null. Special case: If requireDeleteAccess is true and the project is already deleted, returns 204.</returns>        
        protected async Task<IActionResult?> ValidateProjectAccess(Token? token, int projectId, bool requireViewAccess = false, bool requireEditAccess = false, bool requireDeleteAccess = false)
        {
            if (requireDeleteAccess)
            {
                var project = await context.Projects.IgnoreQueryFilters()
                    .Include(p => p.Team)
                    .FirstOrDefaultAsync(p => p.Id == projectId);
                if (project is null)
                {
                    return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound));
                }

                var teamMember = await context.TeamUsers.Include(x => x.Role).FirstOrDefaultAsync(t => t.UserId == token!.UserId && t.TeamId == project.TeamId);
                if (teamMember is null)
                {
                    return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotTeamMember));
                }
                if (!PermissionResolver.HasPermission(teamMember, teamMember.Role, p => p.CanDeleteProjects))
                {
                    return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToDeleteProject));
                }
                if (!project.IsActive)
                {
                    return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
                }
                return null;
            }

            var projectForViewEdit = await context.Projects
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (projectForViewEdit is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.ProjectNotFound));
            }

            var teamMemberForViewEdit = await context.TeamUsers.Include(x => x.Role).FirstOrDefaultAsync(t => t.UserId == token!.UserId && t.TeamId == projectForViewEdit.TeamId);
            if (teamMemberForViewEdit is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotTeamMember));
            }
            if (requireViewAccess && !PermissionResolver.HasPermission(teamMemberForViewEdit, teamMemberForViewEdit.Role, p => p.CanViewProjects))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToViewProject));
            }
            if (requireEditAccess && !PermissionResolver.HasPermission(teamMemberForViewEdit, teamMemberForViewEdit.Role, p => p.CanEditProjects))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NoPermissionToEditProject));
            }
            return null;
        }
    }
}
