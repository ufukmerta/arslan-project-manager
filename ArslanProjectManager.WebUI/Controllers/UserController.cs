using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using ArslanProjectManager.WebUI.Services;
using System.Text;

namespace ArslanProjectManager.WebUI.Controllers
{
    public class UserController(IHttpClientFactory httpClientFactory, IMapper mapper, IAuthStorage authStorage, IConfiguration configuration) : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IMapper _mapper = mapper;
        private readonly IAuthStorage _authStorage = authStorage;
        private readonly IConfiguration _configuration = configuration;

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            string? token = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["errorMessage"] = "You must be logged in to view your profile.";
                return RedirectToAction(nameof(Login), nameof(User));
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync("user/profile");
            if (!response.IsSuccessStatusCode)
            {
                TempData["errorMessage"] = await GetErrorMessageAsync(response);
                return RedirectToAction(nameof(Login), nameof(User));
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<UserProfileDto>>(json, _jsonSerializerOptions);
            if (wrapper is null || !wrapper.IsSuccess || wrapper.Data is null)
            {
                TempData["errorMessage"] = "An error occurred while fetching user data. Please try again later.";
                return View(new LoginViewModel());
            }

            var userProfile = wrapper.Data;
            var userProfileViewModel = _mapper.Map<UserProfileViewModel>(userProfile);
            return View(userProfileViewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? ReturnUrl = null, string? RedirectUrl = null)
        {
            if (RedirectUrl == "Unauthorized")
            {
                return RedirectToAction("Unauthorized", "Home");
            }
            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync("User");
            if (!response.IsSuccessStatusCode)
            {
                return View(new LoginViewModel());
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<TokenDto>>(json, _jsonSerializerOptions);
            if (wrapper is null || !wrapper.IsSuccess || wrapper.Data is null)
            {
                TempData["errorMessage"] = "An error occurred while fetching user data. Please try again later.";
                return View(new LoginViewModel());
            }

            if (wrapper.Data.User is not null && !String.IsNullOrEmpty(wrapper.Data.AccessToken))
            {
                var token = wrapper.Data;
                await AddTokenToCookies(token);
                if (RedirectUrl == "Home")
                {
                    return RedirectToAction("Index2", "Home");
                }
                else if (ReturnUrl != null && Url.IsLocalUrl(ReturnUrl))
                {
                    TempData["errorMessage2"] = "You are already logged in. If you're redirected to login page, the reason maybe that you tried to access somewhere you are not authorized.";
                }
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginViewModel);
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var loginDto = _mapper.Map<UserLoginDto>(loginViewModel);
            var response = await client.PostAsJsonAsync("user/login", loginDto);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["errorMessage"] = "Invalid email or password.";
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                }

                return View(loginViewModel);
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<TokenDto>>(json, _jsonSerializerOptions);
            var token = wrapper?.Data;
            if (token?.AccessToken != null)
            {
                await AddTokenToCookies(token);

                if (loginViewModel.ReturnUrl != null && Url.IsLocalUrl(loginViewModel.ReturnUrl))
                {
                    return Redirect(loginViewModel.ReturnUrl);
                }
                else
                {
                    return RedirectToAction("Index2", "Home");
                }
            }
            else
            {
                TempData["errorMessage"] = "Login failed. Please try again.";
                return View(loginViewModel);
            }
        }

        [NonAction]
        private async Task AddTokenToCookies(TokenDto token)
        {
            var accessCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = token.Expiration
            };
            Response.Cookies.Append("AccessToken", token.AccessToken, accessCookieOptions);

            var refreshCokieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = token.RefreshTokenExpiration
            };
            Response.Cookies.Append("RefreshToken", token.RefreshToken, refreshCokieOptions);

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token.AccessToken);
            var claims = jsonToken.Claims;
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerViewModel);
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var registerDto = _mapper.Map<UserCreateDto>(registerViewModel);
            var response = await client.PostAsJsonAsync("user/register", registerDto);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    TempData["errorMessage"] = "Email already exists.";
                }
                else
                {
                    TempData["errorMessage"] = await GetErrorMessageAsync(response);
                }
                return View(registerViewModel);
            }

            var jsonResponse = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<UserCreateDto>>(jsonResponse, _jsonSerializerOptions);
            if (wrapper is null || !wrapper.IsSuccess || wrapper.Data is null)
            {
                TempData["errorMessage"] = "An error occurred while registering. Please try again later.";
                return View(registerViewModel);
            }

            TempData["successMessage"] = "Registration successful. You can now log in.";
            TempData["email"] = registerViewModel.Email;

            return RedirectToAction(nameof(Login), nameof(User));
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            await client.GetAsync("user/logout");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            string? token = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["errorMessage"] = "You must be logged in to edit your profile.";
                return RedirectToAction(nameof(Login), nameof(User));
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync("user/update");
            if (!response.IsSuccessStatusCode)
            {
                TempData["errorMessage"] = await GetErrorMessageAsync(response);
                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<UserUpdateDto>>(json, _jsonSerializerOptions);
            if (wrapper is null || !wrapper.IsSuccess || wrapper.Data is null)
            {
                TempData["errorMessage"] = "An error occurred while fetching user data. Please try again later.";
                return RedirectToAction(nameof(Index));
            }

            var userProfileViewModel = _mapper.Map<EditUserViewModel>(wrapper.Data);
            return View(userProfileViewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel editUserViewModel)
        {
            var token = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["errorMessage"] = "You must be logged in to edit your profile.";
                return RedirectToAction(nameof(Login), nameof(User));
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var userUpdateDto = _mapper.Map<UserUpdateDto>(editUserViewModel);

            var imgFile = editUserViewModel.ProfilePictureFile;
            var base64Image = string.Empty;
            string[] allowedImageTypes = ["image/jpeg", "image/png", "image/jpg"];
            if (imgFile != null && imgFile.Length > 0 && allowedImageTypes.Contains(editUserViewModel.ProfilePictureFile!.ContentType.ToLower()))
            {
                if (imgFile.Length > 1 * 1024 * 1024)
                {
                    TempData["errorMessage"] = "The image file size exceeds the maximum limit of 1 MB.";
                    return RedirectToAction(nameof(Edit));
                }

                using var memoryStream = new MemoryStream();
                await imgFile.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                base64Image = Convert.ToBase64String(imageBytes);
            }

            userUpdateDto.ProfilePicture = base64Image;
            var response = await client.PutAsJsonAsync("user/update", userUpdateDto);
            if (!response.IsSuccessStatusCode)
            {
                TempData["errorMessage"] = await GetErrorMessageAsync(response);
                return RedirectToAction(nameof(Edit));
            }

            TempData["successMessage"] = "Profile updated successfully. Some changes may not be seen before re-login";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> RemovePicture()
        {
            var token = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                TempData["errorMessage"] = "You must be logged in to delete your avatar.";
                return RedirectToAction(nameof(Login));
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.DeleteAsync("user/remove-picture");
            if (!response.IsSuccessStatusCode)
            {
                TempData["errorMessage"] = await GetErrorMessageAsync(response);
                return RedirectToAction(nameof(Edit));
            }

            TempData["successMessage"] = "Avatar deleted successfully.";
            return RedirectToAction(nameof(Edit));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            var token = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                TempData["errorMessage"] = "You must be logged in to change your password.";
                return RedirectToAction(nameof(Login), nameof(User));
            }

            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            var token = await _authStorage.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                TempData["errorMessage"] = "You must be logged in to change your password.";
                return RedirectToAction(nameof(Login), nameof(User));
            }

            if (!ModelState.IsValid)
            {
                return View(changePasswordViewModel);
            }

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var changePasswordDto = _mapper.Map<UserPasswordUpdateDto>(changePasswordViewModel);
            var response = await client.PutAsJsonAsync("user/update-password", changePasswordDto);
            if (!response.IsSuccessStatusCode)
            {
                TempData["errorMessage"] = await GetErrorMessageAsync(response);
                return View(changePasswordViewModel);
            }
            TempData["successMessage"] = "Password changed successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> MyInvites()
        {
            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync("user/my-invites");
            if (!response.IsSuccessStatusCode)
            {
                TempData["errorMessage"] = await GetErrorMessageAsync(response);
                return View(new List<PendingInviteViewModel>());
            }

            var json = await response.Content.ReadAsStreamAsync();
            var wrapper = await JsonSerializer.DeserializeAsync<CustomResponseDto<List<PendingInviteDto>>>(json, _jsonSerializerOptions);
            if (wrapper is null || !wrapper.IsSuccess || wrapper.Data is null)
            {
                TempData["errorMessage"] = "An error occurred while fetching team invitations. Please try again later.";
                return View(new List<PendingInviteViewModel>());
            }

            var inviteDtos = wrapper.Data;
            var inviteViewModels = _mapper.Map<List<PendingInviteViewModel>>(inviteDtos);
            return View(inviteViewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptInvite(int id)
        {
            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.PostAsync($"user/accept-invite/{id}", null);

            if (!response.IsSuccessStatusCode)
            {
                TempData["errorMessage"] = await GetErrorMessageAsync(response);
                return RedirectToAction(nameof(MyInvites));
            }

            TempData["successMessage"] = "You have successfully joined the team!";
            return RedirectToAction(nameof(MyInvites));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectInvite(int id)
        {
            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.PostAsync($"user/reject-invite/{id}", null);

            if (!response.IsSuccessStatusCode)
            {
                TempData["errorMessage"] = await GetErrorMessageAsync(response);
                return RedirectToAction(nameof(MyInvites));
            }

            TempData["successMessage"] = "Invitation rejected successfully.";
            return RedirectToAction(nameof(MyInvites));
        }
    }
}