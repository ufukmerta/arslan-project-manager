using ArslanProjectManager.API.Utilities;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArslanProjectManager.API.Controllers
{
    /// <summary>
    /// Handles authentication: login, register, refresh token, and logout.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : CustomBaseController
    {
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequestDto dto)
        {
            var response = await authService.RefreshTokenAsync(dto?.RefreshToken);
            if (response.Data is not null)
            {
                AuthCookieHelper.SetAuthCookies(Response, response.Data);
            }

            return CreateActionResult(response);
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            var response = await authService.LoginAndCreateSessionAsync(userLoginDto);
            if (response.Data is not null)
            {
                AuthCookieHelper.SetAuthCookies(Response, response.Data);
            }

            return CreateActionResult(response);
        }

        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            AuthCookieHelper.ClearAuthCookies(Response);
            var refreshToken = AuthCookieHelper.GetRefreshToken(Request);
            var response = await authService.LogoutAsync(CurrentJti, CurrentExpClaim, refreshToken);
            return CreateActionResult(response);
        }

        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll()
        {
            var currentRefreshToken = AuthCookieHelper.GetRefreshToken(Request);
            var response = await authService.LogoutAllAsync(CurrentUserId, currentRefreshToken);
            if (response.Data is not null)
            {
                AuthCookieHelper.SetAuthCookies(Response, response.Data);
            }

            return CreateActionResult(response);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserCreateDto userDto)
        {
            var emailValidation = ValidateEmail(userDto.Email);
            if (emailValidation != null)
            {
                return emailValidation;
            }

            var passwordValidation = ValidatePassword(userDto.Password);
            if (passwordValidation != null)
            {
                return passwordValidation;
            }

            return CreateActionResult(await authService.RegisterAsync(userDto));
        }
    }
}
