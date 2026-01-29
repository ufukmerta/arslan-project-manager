using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArslanProjectManager.API.Controllers
{
    /// <summary>
    /// Manages home dashboard operations including user summary and statistics
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController(IHomeService homeService, ITokenService tokenService) : CustomBaseController(tokenService)
    {
        /// <summary>
        /// Retrieves the home dashboard summary for the authenticated user
        /// </summary>
        /// <returns>User's home dashboard summary including recent activities and statistics</returns>
        /// <response code="200">Returns the home dashboard summary</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If there's an error processing the home summary</response>
        [HttpGet("")]
        [Authorize]
        public async Task<IActionResult> IndexAsync()
        {
            var token = (await GetToken())!;
            HomeDto homeDto = await homeService.GetHomeSummaryAsync(token!.UserId);
            if (homeDto is null)
            {
                //every user has a home summary page regardless of whether the person is any team member, any project contributor or any task contributor
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.ProcessingError));
            }

            return CreateActionResult(CustomResponseDto<HomeDto>.Success(homeDto, 200));
        }
    }
}
