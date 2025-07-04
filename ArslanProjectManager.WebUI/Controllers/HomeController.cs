using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.ViewModels;
using ArslanProjectManager.WebUI.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ArslanProjectManager.WEBUI.Controllers
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? code = null)
        {

            ViewBag.StatusCode = code;
            if (code == 400)
            {
                return RedirectToAction(nameof(BadRequest), "Home");
            }
            else if (code == 401)
            {
                return RedirectToAction(nameof(Unauthorized), "Home");
            }
            else if (code == 403)
            {
                return RedirectToAction(nameof(Forbidden), "Home");
            }
            else if (code == 404)
            {
                return RedirectToAction(nameof(NotFound), "Home");
            }
            else
            {
                return RedirectToAction(nameof(NotFound), "Home");
            }
        }

        [HttpGet]
        public new IActionResult BadRequest()
        {
            return View();
        }

        [HttpGet]
        public new IActionResult Unauthorized()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Forbidden()
        {
            return View();
        }

        [HttpGet]
        public new IActionResult NotFound()
        {
            return View();
        }
    }
}
