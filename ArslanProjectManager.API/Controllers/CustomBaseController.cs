using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArslanProjectManager.API.Controllers
{
    public class CustomBaseController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        public CustomBaseController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

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
        public Token? GetToken()
        {
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var accessToken = authHeader.Substring("Bearer ".Length).Trim();
                Token? token = _tokenService.Where(x => x.AccessToken == accessToken).FirstOrDefault();
                return token;
            }
            else
            {
                return null;
            }
        }
    }
}
