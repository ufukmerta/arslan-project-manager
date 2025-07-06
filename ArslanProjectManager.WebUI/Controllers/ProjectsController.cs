using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.DeleteDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.ViewModels;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Text.Json;

namespace ArslanProjectManager.WebUI.Controllers
{
    public class ProjectsController(IHttpClientFactory httpClientFactory, IMapper mapper, IAuthStorage authStorage) : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IMapper _mapper = mapper;
        private readonly IAuthStorage _authStorage = authStorage;

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(string? from)
        {
            var token = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                var loginViewModel = new LoginViewModel
                {
                    ReturnUrl = Url.Action("Index", "Projects")
                };
                return RedirectToAction("Login", "User", loginViewModel);
            }

            if (from == "Tasks")
            {
                TempData["informationMessage"] = "Choose project to create a task.";
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync("projects");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var loginViewModel = new LoginViewModel
                    {
                        ReturnUrl = Url.Action("Index", "Projects")
                    };
                    return RedirectToAction("Login", "User", loginViewModel);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return View(Enumerable.Empty<ProjectViewModel>());
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return View(Enumerable.Empty<ProjectViewModel>());
                }
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<IEnumerable<UserProjectDto>>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                TempData["errorMessage"] = "Failed to retrieve projects.";
                return View(Enumerable.Empty<ProjectViewModel>());
            }

            var projectViewModel = _mapper.Map<IEnumerable<ProjectViewModel>>(wrapper.Data);
            
            return View(projectViewModel);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var token = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                LoginViewModel loginViewModel = new()
                {
                    ReturnUrl = Url.Action("Index", "Projects")
                };
                return RedirectToAction("Login", "User", loginViewModel);
            }

            if (id <= 0)
            {
                return RedirectToAction("Index", "Projects");
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync($"projects/details/{id}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "project";
                    return Unauthorized();
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "project";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<ProjectDetailsDto>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                TempData["errorMessage"] = "Failed to retrieve projects.";
                return View(Enumerable.Empty<ProjectViewModel>());
            }

            var projectDetailsViewModel = _mapper.Map<ProjectDetailsViewModel>(wrapper.Data);
            return View(projectDetailsViewModel);
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create(int? id)
        {
            string? token = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "User");
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync("projects/create");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "project";
                    return Unauthorized();
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "project";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["informationMessage"] = "You need to join or create a team to create a project.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<List<MiniTeamDto>>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                TempData["errorMessage"] = "Failed to retrieve teams.";
                return View(new CreateProjectViewModel());
            }

            var teams = wrapper.Data;
            if (teams.Count == 0)
            {
                TempData["errorMessage"] = "An error occurred while fetching teams. Reasons: <p>* You are not a member of any team</p><p>* Server Error</p>";
                return View(new CreateProjectViewModel());
            }

            ViewData["TeamId"] = new SelectList(teams, "Id", "TeamName");
            var createProjectViewModel = new CreateProjectViewModel
            {
                TeamId = id ?? 0
            };
            return View(createProjectViewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProjectViewModel model)
        {
            string? token = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "User");
            }

            if (model == null || string.IsNullOrWhiteSpace(model.ProjectName) || model.TeamId <= 0)
            {
                TempData["errorMessage"] = "Project name and team selection are required.";
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
                var response = await client.GetAsync("projects/create");
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        TempData["UnauthorizedFrom"] = "project";
                        return Unauthorized();
                    }
                    else if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        TempData["ForbiddenFrom"] = "project";
                        return StatusCode(StatusCodes.Status403Forbidden);
                    }
                    else
                    {
                        TempData["errorMessage"] = await GetErrorMessageAsync(response);
                        return RedirectToAction(nameof(Index));
                    }
                }

                var json = await response.Content.ReadAsStreamAsync();
                var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<List<MiniTeamDto>>>(json, _jsonSerializerOptions);
                if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
                {
                    TempData["errorMessage"] = "Invalid input.";
                    return RedirectToAction(nameof(Index));
                }

                var teams = wrapper.Data;
                if (teams.Count == 0)
                {
                    TempData["errorMessage"] = "You are not a member of any team.";
                    return View(model);
                }

                ViewData["TeamId"] = new SelectList(teams, "Id", "TeamName");
                return View(model);
            }

            var projectDto = _mapper.Map<ProjectCreateDto>(model);
            var client2 = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response2 = await client2.PostAsJsonAsync("projects/create", projectDto);
            if (!response2.IsSuccessStatusCode)
            {
                if (response2.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "project";
                    return Unauthorized();
                }
                else if (response2.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "project";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response2);
                    return RedirectToAction(nameof(Index));
                }
            }

            var json2 = await response2.Content.ReadAsStreamAsync();
            var wrapper2 = await JsonSerializer.DeserializeAsync<CustomResponseDto<MiniProjectDto>>(json2, _jsonSerializerOptions);
            if (wrapper2 == null || !wrapper2.IsSuccess || wrapper2.Data == null)
            {
                TempData["errorMessage"] = "Failed to retrieve created project.";
                return View(model);
            }

            var createdProjectDto = wrapper2.Data;
            TempData["successMessage"] = "Project created successfully!";
            return RedirectToAction("Details", new { id = createdProjectDto.Id });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            string? token = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "User");
            }

            if (id <= 0)
            {
                return RedirectToAction("Index", "Projects");
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync($"projects/edit/{id}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "project";
                    return Unauthorized();
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "project";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<ProjectUpdateDto>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                TempData["errorMessage"] = "Failed to retrieve project details.";
                return RedirectToAction("Index", "Projects");
            }

            var editProjectDto = wrapper.Data;
            var editProjectViewModel = _mapper.Map<EditProjectViewModel>(editProjectDto);
            return View(editProjectViewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProjectViewModel model)
        {
            string? token = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "User");
            }

            if (model == null || model.ProjectId <= 0 || string.IsNullOrWhiteSpace(model.ProjectName))
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                TempData["errorMessage"] = "Fill all required fields.";
                return View(model);
            }

            var projectUpdateDto = _mapper.Map<ProjectUpdateDto>(model);
            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.PutAsJsonAsync($"projects/edit", projectUpdateDto);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "project";
                    return Unauthorized();
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "project";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<MiniProjectDto>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                TempData["errorMessage"] = "Failed to update project. Please check your input and try again.";
                return View(model);
            }

            TempData["successMessage"] = "Project updated successfully!";
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
                TempData["errorMessage"] = "Invalid project data.";
                return RedirectToAction("Index", "Projects");
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync($"projects/delete/{id}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "project";
                    return Unauthorized();
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "project";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<ProjectDeleteDto>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                TempData["errorMessage"] = "Failed to retrieve project details.";
                return RedirectToAction("Index", "Projects");
            }

            var projectDto = wrapper.Data;
            var projectViewModel = _mapper.Map<DeleteProjectViewModel>(projectDto);
            return View(projectViewModel);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirm(int ProjectId)
        {
            string? token = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "User");
            }

            if (ProjectId <= 0)
            {
                TempData["errorMessage"] = "Invalid project data.";
                return RedirectToAction("Index");
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.DeleteAsync($"projects/delete/{ProjectId}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "project";
                    return Unauthorized();
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "project";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["successMessage"] = "Project deleted successfully!";
            return RedirectToAction("Index", "Projects");
        }
    }
}
