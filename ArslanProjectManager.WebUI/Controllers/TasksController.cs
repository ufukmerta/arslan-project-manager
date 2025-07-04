using ArslanProjectManager.Core;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.DeleteDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.ViewModels;
using ArslanProjectManager.WebUI.Controllers;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArslanProjectManager.WEBUI.Controllers
{
    public class TasksController : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;
        private readonly IAuthStorage _authStorage;

        public TasksController(IHttpClientFactory httpClientFactory, IMapper mapper, IAuthStorage authStorage)
        {
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;
            _authStorage = authStorage;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            string? accessToken = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                var loginviewModel = new LoginViewModel
                {
                    ReturnUrl = Url.Action("Index", "Tasks")
                };
                return RedirectToAction("Login", "User", loginviewModel);
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync("tasks");
            if (!response.IsSuccessStatusCode)
            {
                TempData["errorMessage"] = await GetErrorMessageAsync(response);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "User");
                }
                else
                {
                    return View(new List<ProjectTaskViewModel>());
                }
            }
            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<IEnumerable<ProjectTaskDto>>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                TempData["errorMessage"] = "An error occurred while fetching tasks. Please try again later.";
                return View(new List<ProjectTaskViewModel>());
            }
            var tasks = wrapper.Data;
            var taskViewModels = _mapper.Map<IEnumerable<ProjectTaskViewModel>>(tasks);
            return View(taskViewModels);

        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            string? accessToken = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                var loginViewModel = new LoginViewModel
                {
                    ReturnUrl = Url.Action("Details", "Tasks", new { id })
                };
                return RedirectToAction("Login", "User", loginViewModel);
            }

            if (id <= 0)
            {
                TempData["errorMessage"] = "Invalid task ID.";
                return RedirectToAction(nameof(Index));
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync($"tasks/details/{id}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "task";
                    return Unauthorized();
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "task";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
            }
            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<ProjectTaskDto>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                TempData["errorMessage"] = "An error occurred while fetching the task details. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
            var taskViewModel = _mapper.Map<ProjectTaskViewModel>(wrapper.Data);
            return View(taskViewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int id, CreateTaskCommentViewModel model)
        {
            string? accessToken = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                var loginViewModel = new LoginViewModel
                {
                    ReturnUrl = Url.Action("Details", "Tasks", new { model.TaskId })
                };
                return RedirectToAction("Login", "User", loginViewModel);
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Details", new { model.TaskId });
            }

            var taskComment = _mapper.Map<TaskCommentCreateDto>(model);
            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.PostAsJsonAsync($"tasks/comment/", taskComment);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "task";
                    return Unauthorized();
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "task";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
            }
            else if (response.StatusCode == HttpStatusCode.Created)
            {
                return RedirectToAction(nameof(Details), new { id });
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create(int id)
        {
            string? accessToken = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                var loginViewModel = new LoginViewModel
                {
                    ReturnUrl = Url.Action("Create", "Tasks")
                };
                return RedirectToAction("Login", "User", loginViewModel);
            }

            if (id <= 0)
            {
                TempData["errorMessage"] = "Invalid project ID.";
                return RedirectToAction(nameof(Index));
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync($"tasks/create/{id}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "task";
                    return Unauthorized();
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "task";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<ProjectTaskCreateViewDto>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                TempData["errorMessage"] = "An error occurred while preparing the task creation form. Please try again later.";
                return RedirectToAction(nameof(Index));
            }

            var projectTaskCreateViewDto = wrapper.Data;
            ViewData["AppointeeId"] = new SelectList(projectTaskCreateViewDto.TeamMembers, "TeamUserId", "Name");
            ViewData["BoardId"] = new SelectList(projectTaskCreateViewDto.BoardTags, "Id", "BoardName");
            ViewData["TaskCategoryId"] = new SelectList(projectTaskCreateViewDto.TaskCategories, "Id", "Category");
            return View(new CreateTaskViewModel { ProjectId = wrapper.Data.ProjectId });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTaskViewModel model)
        {
            string? accessToken = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                var loginViewModel = new LoginViewModel
                {
                    ReturnUrl = Url.Action("Create", "Tasks")
                };

                return RedirectToAction("Login", "User", loginViewModel);
            }

            if (!ModelState.IsValid)
            {
                if (model.ProjectId <= 0)
                {
                    TempData["errorMessage"] = "At least one input is invalid";
                    return RedirectToAction(nameof(Index));
                }

                var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
                var response = await client.GetAsync($"tasks/create/{model.ProjectId}");
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        TempData["UnauthorizedFrom"] = "task";
                        return Unauthorized();
                    }
                    else if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        TempData["ForbiddenFrom"] = "task";
                        return StatusCode(StatusCodes.Status403Forbidden);
                    }
                    else
                    {
                        TempData["errorMessage"] = await GetErrorMessageAsync(response);
                        return RedirectToAction(nameof(Index));
                    }
                }

                var json = await response.Content.ReadAsStreamAsync();
                var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<ProjectTaskCreateViewDto>>(json, _jsonSerializerOptions);
                if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
                {
                    TempData["errorMessage"] = "An error occurred while preparing the task creation. Please try again later.";
                    return RedirectToAction(nameof(Index));
                }

                var projectTaskCreateViewDto = wrapper.Data;
                ViewData["AppointeeId"] = new SelectList(projectTaskCreateViewDto.TeamMembers, "TeamUserId", "Name");
                ViewData["BoardId"] = new SelectList(projectTaskCreateViewDto.BoardTags, "Id", "BoardName");
                ViewData["TaskCategoryId"] = new SelectList(projectTaskCreateViewDto.TaskCategories, "Id", "Category");
                return View(model);
            }


            var projectTaskDto = _mapper.Map<ProjectTaskCreateDto>(model);
            var client2 = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response2 = await client2.PostAsJsonAsync("tasks/create", projectTaskDto);
            if (!response2.IsSuccessStatusCode)
            {
                if (response2.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "task";
                    return Unauthorized();
                }
                else if (response2.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "task";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response2);
                    return View(model);
                }
            }

            var json2 = await response2.Content.ReadAsStreamAsync();
            var wrapper2 = await JsonSerializer.DeserializeAsync<CustomResponseDto<MiniProjectTaskDto>>(json2, _jsonSerializerOptions);
            if (wrapper2 == null || !wrapper2.IsSuccess || wrapper2.Data == null)
            {
                TempData["errorMessage"] = "Failed to create task. Please check your input and try again.";
                return View(model);
            }

            return RedirectToAction(nameof(Details), new { id = wrapper2.Data.Id });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            string? accessToken = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                var loginViewModel = new LoginViewModel
                {
                    ReturnUrl = Url.Action("Create", "Tasks")
                };
                return RedirectToAction("Login", "User", loginViewModel);
            }

            if (id <= 0)
            {
                TempData["errorMessage"] = "Invalid project ID.";
                return RedirectToAction(nameof(Index));
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync($"tasks/edit/{id}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "task";
                    return Unauthorized();
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "task";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<ProjectTaskUpdateDto>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                TempData["errorMessage"] = "Failed to retrieve project details.";
                return RedirectToAction("Index", "Projects");
            }

            var editTaskDto = wrapper.Data;
            var editTaskViewModel = _mapper.Map<EditTaskViewModel>(editTaskDto);
            ViewData["AppointeeId"] = new SelectList(editTaskDto.TeamMembers, "TeamUserId", "Name", editTaskDto.AppointeeId);
            ViewData["BoardId"] = new SelectList(editTaskDto.BoardTags, "Id", "BoardName", editTaskDto.BoardId);
            ViewData["TaskCategoryId"] = new SelectList(editTaskDto.TaskCategories, "Id", "Category", editTaskDto.TaskCategoryId);
            return View(editTaskViewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditTaskViewModel model)
        {
            string? token = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "User");
            }

            if (!ModelState.IsValid || model == null || model.TaskId <= 0 || model.Priority < 0 || model.BoardId <= 0
                 || model.TaskCategoryId <= 0 || model.AppointeeId <= 0 || string.IsNullOrEmpty(model.TaskName))
            {
                TempData["errorMessage"] = "Fill all required fields.";
                return View(model);
            }

            var taskUpdateDto = _mapper.Map<ProjectTaskUpdateDto>(model);
            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.PutAsJsonAsync($"tasks/edit", taskUpdateDto);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "task";
                    return Unauthorized();
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "task";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<MiniProjectTaskDto>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                TempData["errorMessage"] = "Failed to update task. Please check your input and try again.";
                return View(model);
            }

            TempData["successMessage"] = "Task updated successfully!";
            return RedirectToAction("Details", new { id = wrapper.Data.Id });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            string? token = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "User");
            }

            if (id <= 0)
            {
                TempData["errorMessage"] = "Invalid task data.";
                return RedirectToAction("Index", "Tasks");
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync($"tasks/delete/{id}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "task";
                    return Unauthorized();
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "task";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<ProjectTaskDeleteDto>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                TempData["errorMessage"] = "Failed to retrieve project details.";
                return RedirectToAction("Index", "Projects");
            }

            var taskDto = wrapper.Data;
            var taskViewModel = _mapper.Map<DeleteTaskViewModel>(taskDto);
            return View(taskViewModel);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int TaskId)
        {
            string? token = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "User");
            }

            if (TaskId <= 0)
            {
                TempData["errorMessage"] = "Invalid project data.";
                return RedirectToAction("Index");
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.DeleteAsync($"tasks/delete/{TaskId}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "task";
                    return Unauthorized();
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "task";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["successMessage"] = "Task deleted successfully!";
            return RedirectToAction("Index", "Projects");
        }
    }
}
