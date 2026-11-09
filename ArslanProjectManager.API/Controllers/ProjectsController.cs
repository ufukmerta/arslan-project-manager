using ArslanProjectManager.API.Filters;
using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArslanProjectManager.API.Controllers
{
    /// <summary>
    /// Manages project-related operations including CRUD operations for projects
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController(IProjectService projectService) : CustomBaseController
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetByToken()
        {
            return CreateActionResult(await projectService.GetUserProjectsAsync(CurrentUserId));
        }

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

            return CreateActionResult(await projectService.GetProjectDetailsForUserAsync(CurrentUserId, id));
        }

        [HttpGet("[action]-meta")]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            return CreateActionResult(await projectService.GetCreateMetaAsync(CurrentUserId));
        }

        [HttpPost]
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

            return CreateActionResult(await projectService.CreateProjectAsync(CurrentUserId, model));
        }

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

            return CreateActionResult(await projectService.GetEditMetaAsync(CurrentUserId, id));
        }

        [HttpPut]
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

            return CreateActionResult(await projectService.UpdateProjectAsync(CurrentUserId, model));
        }

        [Authorize]
        [HttpGet("{id:int}/delete-confirm")]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            var idValidation = ValidateId(id, ErrorMessages.InvalidProjectId);
            if (idValidation is not null)
            {
                return idValidation;
            }

            return CreateActionResult(await projectService.GetDeleteConfirmAsync(CurrentUserId, id));
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var idValidation = ValidateId(id, ErrorMessages.InvalidProjectId);
            if (idValidation is not null)
            {
                return idValidation;
            }

            return CreateActionResult(await projectService.DeleteProjectAsync(CurrentUserId, id));
        }
    }
}
