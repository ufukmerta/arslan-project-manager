using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArslanProjectManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController(IHomeService homeService, IUserService userService, ITokenService tokenService) : CustomBaseController(tokenService)
    {
        private readonly IHomeService _homeService = homeService;
        private readonly IUserService _userService = userService;

        [HttpGet("")]
        public async Task<IActionResult> IndexAsync()
        {
            Token? token = base.GetToken();
            if (token == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in or access token is invalid"));
            }

            var user = await _userService.GetByIdAsync(token.UserId);
            if (user == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "User not found"));
            }

            HomeDto homeDto = await _homeService.GetHomeSummaryAsync(user.Id);
            if (homeDto == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "An error occurred while processing your request. Please try again later."));
            }

            return CreateActionResult(CustomResponseDto<HomeDto>.Success(homeDto, 200));
        }
    }
}
