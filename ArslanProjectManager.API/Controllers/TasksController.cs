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
    /// Manages project task operations including CRUD operations, task assignments, and comments
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController(IProjectTaskService taskService) : CustomBaseController
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetByToken()
        {
            return CreateActionResult(await taskService.GetUserTasksAsync(CurrentUserId));
        }

        [HttpGet("{id:int}")]
        [Authorize]
        [ServiceFilter(typeof(NotFoundFilter<ProjectTask>))]
        public async Task<IActionResult> Details(int id)
        {
            return CreateActionResult(await taskService.GetTaskDetailsForUserAsync(CurrentUserId, id));
        }

        [HttpGet("{projectId}/[action]-meta")]
        [Authorize]
        public async Task<IActionResult> Create(int projectId)
        {
            return CreateActionResult(await taskService.GetCreateMetaAsync(CurrentUserId, projectId));
        }

        [HttpPost]
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

            return CreateActionResult(await taskService.CreateTaskAsync(CurrentUserId, model));
        }

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

            return CreateActionResult(await taskService.AddCommentAsync(CurrentUserId, model));
        }

        [HttpGet("{id:int}/[action]-meta")]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            return CreateActionResult(await taskService.GetEditMetaAsync(CurrentUserId, id));
        }

        [HttpPut]
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

            return CreateActionResult(await taskService.UpdateTaskAsync(CurrentUserId, model));
        }

        [HttpGet("{id:int}/delete-confirm")]
        [Authorize]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            var idValidation = ValidateId(id, ErrorMessages.InvalidTaskId);
            if (idValidation is not null)
            {
                return idValidation;
            }

            return CreateActionResult(await taskService.GetDeleteConfirmAsync(CurrentUserId, id));
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var idValidation = ValidateId(id, ErrorMessages.InvalidTaskId);
            if (idValidation is not null)
            {
                return idValidation;
            }

            return CreateActionResult(await taskService.DeleteTaskAsync(CurrentUserId, id));
        }
    }
}
