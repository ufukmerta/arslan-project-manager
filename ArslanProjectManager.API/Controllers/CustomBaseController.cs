using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ArslanProjectManager.API.Controllers
{
    /// <summary>
    /// Base controller for API endpoints that use a consistent response format and shared validation helpers.
    /// </summary>
    public partial class CustomBaseController(ITokenService? tokenService = null) : ControllerBase
    {
        protected ITokenService? TokenService => tokenService;

        protected int CurrentUserId =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : 0;

        protected string? CurrentJti =>
            User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

        protected string? CurrentExpClaim =>
            User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;

        [NonAction]
        public IActionResult CreateActionResult<T>(CustomResponseDto<T> response)
        {
            if (response is null)
            {
                return NotFound();
            }
            if (response.StatusCode == 204)
            {
                return new ObjectResult(null) { StatusCode = response.StatusCode };
            }

            return new ObjectResult(response) { StatusCode = response.StatusCode };
        }

        [NonAction]
        protected async Task<Token?> GetToken()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return null;
            }

            return new Token
            {
                AccessToken = accessToken,
                UserId = CurrentUserId
            };
        }

        [NonAction]
        protected IActionResult? ValidateModel<T>(T model, Func<T, bool> validator, string errorMessage)
        {
            if (model is null || !validator(model))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, errorMessage));
            }

            return null;
        }

        [NonAction]
        protected IActionResult? ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !EmailFormatRegex().IsMatch(email))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvalidEmailFormat));
            }

            return null;
        }

        [NonAction]
        protected IActionResult? ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password) || !StrongPasswordRegex().IsMatch(password))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvalidPasswordFormat));
            }

            return null;
        }

        [NonAction]
        protected IActionResult? ValidateId(int id, string errorMessage)
        {
            if (id <= 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, errorMessage));
            }

            return null;
        }

        [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
        private partial Regex EmailFormatRegex();

        [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@#$!%*\/?&+\-_.])[A-Za-z\d@#$!%*\/?&+\-_.]{8,}$")]
        private partial Regex StrongPasswordRegex();
    }
}
