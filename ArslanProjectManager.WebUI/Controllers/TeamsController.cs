using ArslanProjectManager.Core;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using static ArslanProjectManager.Core.Models.TeamInvite;

namespace ArslanProjectManager.WebUI.Controllers
{
    public class TeamsController(IHttpClientFactory httpClientFactory, IMapper mapper, IAuthStorage authStorage) : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IMapper _mapper = mapper;
        private readonly IAuthStorage _authStorage = authStorage;

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            string? accessToken = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                var loginviewModel = new LoginViewModel
                {
                    ReturnUrl = Url.Action("Index", "Teams")
                };
                return RedirectToAction("Login", "User", loginviewModel);
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync("teams");
            if (!response.IsSuccessStatusCode)
            {

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
            var teamViewModels = _mapper.Map<IEnumerable<TeamViewModel>>(teams);
            return View(teamViewModels);
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
                    ReturnUrl = Url.Action("Details", "Teams", new { id })
                };
                return RedirectToAction("Login", "User", loginViewModel);
            }

            if (id <= 0)
            {
                TempData["errorMessage"] = "Invalid team ID.";
                return RedirectToAction(nameof(Index));
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync($"teams/details/{id}");
            if (!response.IsSuccessStatusCode)
            {
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
            var teamViewModel = _mapper.Map<TeamDetailsViewModel>(wrapper.Data);
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
            string? accessToken = await _authStorage.GetAccessTokenAsync();
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

            var teamDto = _mapper.Map<TeamCreateDto>(model);
            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.PostAsJsonAsync("teams/create", teamDto);
            if (!response.IsSuccessStatusCode)
            {
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
            string? accessToken = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                var loginViewModel = new LoginViewModel
                {
                    ReturnUrl = Url.Action("Create", "Teams")
                };
                return RedirectToAction("Login", "User", loginViewModel);
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync($"teams/invite/{id}");
            if (!response.IsSuccessStatusCode)
            {
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
            var teamInviteViewModel = _mapper.Map<TeamInviteViewModel>(teams);
            return View(teamInviteViewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Invite(TeamInviteViewModel model)
        {
            string? accessToken = await _authStorage.GetAccessTokenAsync();
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

            var teamInviteCreateDto = _mapper.Map<TeamInviteCreateDto>(model);
            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");

            var response = await client.PostAsJsonAsync("teams/invite", teamInviteCreateDto);
            if (!response.IsSuccessStatusCode)
            {
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
            string? accessToken = await _authStorage.GetAccessTokenAsync();
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

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync($"teams/invites/{id}");
            if (!response.IsSuccessStatusCode)
            {
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
                Invites = _mapper.Map<List<TeamInviteItemViewModel>>(teamInvites)
            };

            return View(teamInviteListViewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelInvite(int inviteId, int teamId)
        {
            string? accessToken = await _authStorage.GetAccessTokenAsync();
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

            var cancelInviteDto = new CancelInviteDto { InviteId = inviteId };
            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.PostAsJsonAsync("teams/cancelinvite", cancelInviteDto);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await GetErrorMessageAsync(response);
                TempData["errorMessage"] = errorMessage;
            }
            else
            {
                TempData["successMessage"] = "Invitation canceled successfully";
            }

            return RedirectToAction("Invites", new { id = teamId });
        }
    }
}
