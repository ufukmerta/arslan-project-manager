using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArslanProjectManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController(IHomeService homeService, ITokenService tokenService) : CustomBaseController(tokenService)
    {
        private readonly IHomeService _homeService = homeService;

        [HttpGet("")]
        [Authorize]
        public async Task<IActionResult> IndexAsync()
        {
            var tokenValidation = await ValidateToken();
            if (tokenValidation is not null)
            {
                return tokenValidation;
            }

            var token = await GetToken();
            HomeDto homeDto = await _homeService.GetHomeSummaryAsync(token!.UserId);
            if (homeDto is null)
            {
                //every user has a home summary page regardless of whether the person is any team member, any project contributor or any task contributor
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.ProcessingError));
            }

            return CreateActionResult(CustomResponseDto<HomeDto>.Success(homeDto, 200));
        }
    }
}
