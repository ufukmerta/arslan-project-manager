using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArslanProjectManager.API.Controllers
{
    public partial class CustomBaseController(ITokenService tokenService) : ControllerBase
    {
        private readonly ITokenService _tokenService = tokenService;

        [NonAction]
        public IActionResult CreateActionResult<T>(CustomResponseDto<T> response)
        {
            if (response == null)
            {
                return NotFound();
            }
            else if (response.StatusCode == 204)
            {
                return new ObjectResult(null) { StatusCode = response.StatusCode };
            }
            else
            {
                return new ObjectResult(response) { StatusCode = response.StatusCode };
            }
        }

        [NonAction]
        protected async Task<Token?> GetToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null) return null;

            var userId = int.Parse(userIdClaim.Value);                        
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var accessToken = authHeader["Bearer ".Length..].Trim();
                Token? token = await _tokenService.GetValidTokenByAccessTokenAsync(accessToken);
                return token;
            }
            else
            {
                return null;
            }
        }

        [NonAction]
        protected async Task<IActionResult?> ValidateToken()
        {
            var token = await GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.Unauthorized));
            }
            else if (token.RefreshTokenExpiration < DateTime.UtcNow)
            {
                token.IsActive = false;
                _tokenService.ChangeStatus(token);
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.Unauthorized));
            }
            else
            {
                return null;
            }
        }

        [NonAction]
        protected IActionResult? ValidateModel<T>(T model, Func<T, bool> validator, string errorMessage)
        {
            if (model is null || !validator(model))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, errorMessage));
            }
            else
            {
                return null;
            }
        }

        [NonAction]
        protected IActionResult? ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvalidEmailFormat));
            }

            var emailRegex = EmailFormatRegex();
            if (!emailRegex.IsMatch(email))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvalidEmailFormat));
            }

            return null;
        }

        [NonAction]
        protected IActionResult? ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvalidPasswordFormat));
            }

            var passwordRegex = StrongPasswordRegex();
            if (!passwordRegex.IsMatch(password))
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
            else
            {
                return null;
            }
        }

        [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
        private partial Regex EmailFormatRegex();

        [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*\/?&+\-_.])[A-Za-z\d@$!%*\/?&+\-_.]{8,}$")]
        private partial Regex StrongPasswordRegex();
    }
}
