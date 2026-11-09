using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArslanProjectManager.API.Controllers
{
    /// <summary>
    /// Manages home dashboard operations including user summary and statistics
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController(IHomeService homeService) : CustomBaseController
    {
        [HttpGet("")]
        [Authorize]
        public async Task<IActionResult> IndexAsync()
        {
            var homeDto = await homeService.GetHomeSummaryAsync(CurrentUserId);
            if (homeDto is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.ProcessingError));
            }

            return CreateActionResult(CustomResponseDto<HomeDto>.Success(homeDto, 200));
        }
    }
}
