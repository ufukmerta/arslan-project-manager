using ArslanProjectManager.Core;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.ViewModels;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using static ArslanProjectManager.Core.Models.TeamInvite;

namespace ArslanProjectManager.WebUI.Controllers
{
    public class TeamsController(IHttpClientFactory httpClientFactory, IMapper mapper, IAuthStorage authStorage) : BaseController
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            string? accessToken = await authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                var loginviewModel = new LoginViewModel
                {
                    ReturnUrl = Url.Action("Index", "Teams")
                };
                return RedirectToAction("Login", "User", loginviewModel);
            }

            var client = httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync("teams");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return RedirectToTooManyRequests();
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction("Login", "User");
                }
                else if(response.StatusCode == HttpStatusCode.NotFound)
                {
                    return View(new List<TeamViewModel>());
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return View(new List<TeamViewModel>());
                }
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<IEnumerable<TeamDto>>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                TempData["errorMessage"] = "An error occurred while fetching teams. Please try again later.";
                return View(new List<TeamViewModel>());
            }
            var teams = wrapper.Data;
            var teamViewModels = mapper.Map<IEnumerable<TeamViewModel>>(teams);
            return View(teamViewModels);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            string? accessToken = await authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                var loginViewModel = new LoginViewModel
                {
                    ReturnUrl = Url.Action("Details", "Teams", new { id })
                };
                return RedirectToAction("Login", "User", loginViewModel);
            }

            if (id <= 0)
            {
                TempData["errorMessage"] = "Invalid team ID.";
                return RedirectToAction(nameof(Index));
            }

            var client = httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync($"teams/{id}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return RedirectToTooManyRequests();
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "team";
                    return Unauthorized();
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "team";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<TeamDetailsDto>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                TempData["errorMessage"] = "An error occurred while fetching the task details. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
            var teamViewModel = mapper.Map<TeamDetailsViewModel>(wrapper.Data);
            return View(teamViewModel);
        }

        public IActionResult Create()
        {
            return View(new TeamCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeamCreateViewModel model)
        {
            string? accessToken = await authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                var loginViewModel = new LoginViewModel
                {
                    ReturnUrl = Url.Action("Create", "Teams")
                };
                return RedirectToAction("Login", "User", loginViewModel);
            }

            if (!ModelState.IsValid)
            {
                TempData["errorMessage"] = "Invalid input.";
                return View(model);
            }

            var teamDto = mapper.Map<TeamCreateDto>(model);
            var client = httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.PostAsJsonAsync("teams", teamDto);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return RedirectToTooManyRequests();
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "team";
                    return Unauthorized();
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "team";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<MiniTeamDto>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                TempData["errorMessage"] = "Failed to retrieve created project.";
                return View(model);
            }

            TempData["successMessage"] = "Team created successfully!";
            return RedirectToAction("Details", new { id = wrapper.Data.Id });
        }

        public async Task<IActionResult> Invite(int id)
        {
            string? accessToken = await authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                var loginViewModel = new LoginViewModel
                {
                    ReturnUrl = Url.Action("Create", "Teams")
                };
                return RedirectToAction("Login", "User", loginViewModel);
            }

            var client = httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync($"teams/{id}/invite-meta");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return RedirectToTooManyRequests();
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction("Login", "User");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return View(new List<TeamViewModel>());
                }
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<TeamInviteCreateViewDto>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                TempData["errorMessage"] = "An error occurred while fetching teams. Please try again later.";
                return View(new List<TeamInviteViewModel>());
            }
            var teams = wrapper.Data;
            var teamInviteViewModel = mapper.Map<TeamInviteViewModel>(teams);
            return View(teamInviteViewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Invite(TeamInviteViewModel model)
        {
            string? accessToken = await authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                var loginViewModel = new LoginViewModel
                {
                    ReturnUrl = Url.Action("Create", "Teams")
                };
                return RedirectToAction("Login", "User", loginViewModel);
            }

            if (!ModelState.IsValid)
            {
                TempData["errorMessage"] = "Invalid input.";
                return View(model);
            }

            var teamInviteCreateDto = mapper.Map<TeamInviteCreateDto>(model);
            var client = httpClientFactory.CreateClient("ArslanProjectManagerAPI");

            var response = await client.PostAsJsonAsync($"teams/{model.TeamId}/invites", teamInviteCreateDto);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return RedirectToTooManyRequests();
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction("Login", "User");
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return View(model);
                }
            }
            if (response.StatusCode == HttpStatusCode.Created)
            {
                TempData["successMessage"] = "Invitation sent successfully.";
            }
            return RedirectToAction(nameof(Details), new { id = model.TeamId });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Invites(int id)
        {
            string? accessToken = await authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                var loginViewModel = new LoginViewModel
                {
                    ReturnUrl = Url.Action("Invites", "Teams", new { id })
                };
                return RedirectToAction("Login", "User", loginViewModel);
            }

            if (id <= 0)
            {
                TempData["errorMessage"] = "Invalid team ID.";
                return RedirectToAction(nameof(Index));
            }

            var client = httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync($"teams/{id}/invites");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return RedirectToTooManyRequests();
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "team";
                    return Unauthorized();
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "team";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<IEnumerable<TeamInviteListDto>>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                TempData["errorMessage"] = "An error occurred while fetching team invites. Please try again later.";
                return RedirectToAction(nameof(Index));
            }

            var teamInvites = wrapper.Data.ToList();
            var teamInviteListViewModel = new TeamInviteListViewModel
            {
                TeamId = id,
                TeamName = teamInvites.FirstOrDefault()?.TeamName ?? "Unknown Team",
                ManagerId = teamInvites.FirstOrDefault()?.ManagerId ?? 0,
                Invites = mapper.Map<List<TeamInviteItemViewModel>>(teamInvites)
            };

            return View(teamInviteListViewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelInvite(int inviteId, int teamId)
        {
            string? accessToken = await authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                TempData["errorMessage"] = "Not logged in or access token is invalid";
                return RedirectToAction("Invites", new { id = teamId });
            }

            if (inviteId <= 0)
            {
                TempData["errorMessage"] = "Invalid invite ID";
                return RedirectToAction("Invites", new { id = teamId });
            }

            var client = httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.DeleteAsync($"invites/{inviteId}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return RedirectToTooManyRequests();
                }
                var errorMessage = await GetErrorMessageAsync(response);
                TempData["errorMessage"] = errorMessage;
            }
            else
            {
                TempData["successMessage"] = "Invitation canceled successfully";
            }

            return RedirectToAction("Invites", new { id = teamId });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Permissions(int id)
        {
            string? accessToken = await authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                var loginViewModel = new LoginViewModel
                {
                    ReturnUrl = Url.Action("Permissions", "Teams", new { id })
                };
                return RedirectToAction("Login", "User", loginViewModel);
            }

            if (id <= 0)
            {
                TempData["errorMessage"] = "Invalid team ID.";
                return RedirectToAction(nameof(Index));
            }

            var client = httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync($"teams/{id}/permissions");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return RedirectToTooManyRequests();
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "team";
                    return Unauthorized();
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "team";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                    return RedirectToAction(nameof(Index));
                }
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<TeamPermissionsDto>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                TempData["errorMessage"] = "An error occurred while fetching team permissions. Please try again later.";
                return RedirectToAction(nameof(Index));
            }

            var permissionsViewModel = new TeamPermissionsViewModel
            {
                TeamId = wrapper.Data.TeamId,
                TeamName = wrapper.Data.TeamName,
                ManagerId = wrapper.Data.ManagerId,
                CanManagePermissions = wrapper.Data.CanManagePermissions,
                Users = mapper.Map<List<TeamUserPermissionViewModel>>(wrapper.Data.Users)
            };

            return View(permissionsViewModel);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetRolesJson(int id)
        {
            string? accessToken = await authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return Json(new { isSuccess = false, errors = "Unauthorized" });
            }

            var client = httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync($"teams/{id}/roles");
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return RedirectToTooManyRequests();
                }
                var errorMessage = await GetErrorMessageAsync(response);
                return Json(new { isSuccess = false, errors = errorMessage });
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<IEnumerable<RoleDto>>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                return Json(new { isSuccess = false, errorMessage = "Failed to fetch roles" });
            }

            return Json(new { isSuccess = true, data = wrapper.Data });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserPermissions(int teamId, int userId)
        {
            string? accessToken = await authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return Json(new { isSuccess = false, errorMessage = "Unauthorized" });
            }

            var client = httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync($"teams/{teamId}/users/{userId}/permissions");
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return RedirectToTooManyRequests();
                }
                var errorMessage = await GetErrorMessageAsync(response);
                return Json(new { isSuccess = false, errorMessage });
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<UserEffectivePermissionsDto>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                return Json(new { isSuccess = false, errorMessage = "Failed to fetch user permissions" });
            }

            var permissionsViewModel = mapper.Map<UserEffectivePermissionsViewModel>(wrapper.Data);
            return Json(new { isSuccess = true, data = permissionsViewModel });
        }

        [HttpPut]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserPermissions(int teamId, int userId, [FromBody] UserPermissionUpdateViewModel model)
        {
            string? accessToken = await authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return Json(new { isSuccess = false, errors = "Unauthorized" });
            }

            model.TeamId = teamId;
            model.UserId = userId;

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { isSuccess = false, errorMessage = errors });
            }

            var client = httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var permissionDto = mapper.Map<UserPermissionUpdateDto>(model);
            var response = await client.PutAsJsonAsync($"teams/{teamId}/users/{userId}/permissions", permissionDto);
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return RedirectToTooManyRequests();
                }
                var errorMessage = await GetErrorMessageAsync(response);
                return Json(new { isSuccess = false, errorMessage });
            }

            return Json(new { isSuccess = true });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Roles(int id)
        {
            string? accessToken = await authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                var loginViewModel = new LoginViewModel
                {
                    ReturnUrl = Url.Action("Roles", "Teams", new { id })
                };
                return RedirectToAction("Login", "User", loginViewModel);
            }

            if (id <= 0)
            {
                TempData["errorMessage"] = "Invalid team ID.";
                return RedirectToAction(nameof(Index));
            }

            
            var client = httpClientFactory.CreateClient("ArslanProjectManagerAPI");            
            var rolesResponse = await client.GetAsync($"teams/{id}/roles");
            if (!rolesResponse.IsSuccessStatusCode)
            {
                if (rolesResponse.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return RedirectToTooManyRequests();
                }
                if (rolesResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["UnauthorizedFrom"] = "team";
                    return Unauthorized();
                }
                else if (rolesResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ForbiddenFrom"] = "team";
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
                else if (rolesResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(rolesResponse);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(rolesResponse);
                    return RedirectToAction(nameof(Index));
                }
            }

            var rolesJson = await rolesResponse.Content.ReadAsStreamAsync();
            var rolesWrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<IEnumerable<RoleDto>>>(rolesJson, _jsonSerializerOptions);
            if (rolesWrapper == null || !rolesWrapper.IsSuccess || rolesWrapper.Data == null)
            {
                TempData["errorMessage"] = "An error occurred while fetching team roles. Please try again later.";
                return RedirectToAction(nameof(Index));
            }

            // Get current user's permissions to check if they can manage roles
            var currentUserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userPermissionsResponse = await client.GetAsync($"teams/{id}/users/{currentUserId}/permissions");
            var canManageRoles = false;
            
            if (userPermissionsResponse.IsSuccessStatusCode)
            {
                var userPermsJson = await userPermissionsResponse.Content.ReadAsStreamAsync();
                var userPermsWrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<UserEffectivePermissionsDto>>(userPermsJson, _jsonSerializerOptions);
                if (userPermsWrapper?.IsSuccess == true && userPermsWrapper.Data != null)
                {
                    canManageRoles = userPermsWrapper.Data.CanManageRoles;
                }
            }

            var rolesViewModel = new TeamRolesViewModel
            {
                TeamId = id,
                TeamName = rolesWrapper.Data.FirstOrDefault()!.TeamName!,
                CanManageRoles = canManageRoles,
                Roles = mapper.Map<List<TeamRoleViewModel>>(rolesWrapper.Data)
            };

            return View(rolesViewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(int teamId, TeamRoleCreateViewModel model)
        {
            string? accessToken = await authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return Json(new { isSuccess = false, errorMessage = "Unauthorized" });
            }

            model.TeamId = teamId; // Ensure TeamId is set from route

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { isSuccess = false, errorMessage = errors });
            }

            var client = httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var roleDto = mapper.Map<RoleCreateDto>(model);
            var response = await client.PostAsJsonAsync($"teams/{teamId}/roles", roleDto);
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return RedirectToTooManyRequests();
                }
                var errorMessage = await GetErrorMessageAsync(response);
                return Json(new { isSuccess = false, errorMessage });
            }

            return Json(new { isSuccess = true });
        }

        [HttpPut]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(int teamId, int roleId, TeamRoleUpdateViewModel model)
        {
            string? accessToken = await authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return Json(new { isSuccess = false, errorMessage = "Unauthorized" });
            }

            model.TeamId = teamId; // Ensure TeamId is set from route
            model.RoleId = roleId; // Ensure RoleId is set from route

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { isSuccess = false, errorMessage = errors });
            }

            var client = httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var roleDto = mapper.Map<RoleUpdateDto>(model);
            var response = await client.PutAsJsonAsync($"teams/{teamId}/roles/{roleId}", roleDto);
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return RedirectToTooManyRequests();
                }
                var errorMessage = await GetErrorMessageAsync(response);
                return Json(new { isSuccess = false, errorMessage });
            }

            return Json(new { isSuccess = true });
        }

        [HttpDelete]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(int teamId, int roleId)
        {
            string? accessToken = await authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return Json(new { isSuccess = false, errorMessage = "Unauthorized" });
            }

            var client = httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.DeleteAsync($"teams/{teamId}/roles/{roleId}");
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return RedirectToTooManyRequests();
                }
                var errorMessage = await GetErrorMessageAsync(response);
                return Json(new { isSuccess = false, errorMessage });
            }

            return Json(new { isSuccess = true });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloneRole(int teamId, RoleCloneDto model)
        {
            string? accessToken = await authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return Json(new { isSuccess = false, errorMessage = "Unauthorized" });
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { isSuccess = false, errorMessage = errors });
            }

            var client = httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.PostAsJsonAsync($"teams/{teamId}/roles/clone", model);            
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return RedirectToTooManyRequests();
                }
                var errorMessage = await GetErrorMessageAsync(response);
                return Json(new { isSuccess = false, errorMessage });
            }

            return Json(new { isSuccess = true });
        }
    }
}
