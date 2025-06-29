using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ArslanProjectManager.WebUI.Controllers
{
    public class HomeController(IHttpClientFactory httpClientFactory, IAuthStorage authStorage, IMapper mapper) : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IAuthStorage _authStorage = authStorage;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index2()
        {
            string? token = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "User");
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync("home");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await _authStorage.ClearTokensAsync();
                }

                TempData["ErrorMessage"] = GetErrorMessageAsync(response);
                return RedirectToAction("Login", "User");
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<HomeDto>>(json, _jsonSerializerOptions);
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
            {
                return View("Error");
            }

            var viewModel = _mapper.Map<HomeViewModel>(wrapper.Data);
            return View(viewModel);
        }
    }
}
