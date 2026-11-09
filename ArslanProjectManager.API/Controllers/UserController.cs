using ArslanProjectManager.API.Filters;
using ArslanProjectManager.API.Utilities;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArslanProjectManager.API.Controllers
{
    /// <summary>
    /// Manages user operations including profile management and team invitations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService userService) : CustomBaseController
    {
        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            return CreateActionResult(await userService.GetProfileForUserAsync(CurrentUserId));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetByToken()
        {
            var token = (await GetToken())!;
            return CreateActionResult(await userService.GetUserByTokenAsync(token));
        }

        [HttpGet("[action]-meta")]
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            return CreateActionResult(await userService.GetEditMetaAsync(CurrentUserId));
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Edit(UserUpdateDto userDto)
        {
            var emailValidation = ValidateEmail(userDto.Email);
            if (emailValidation != null)
            {
                return emailValidation;
            }

            return CreateActionResult(await userService.UpdateUserProfileAsync(CurrentUserId, userDto));
        }

        [HttpDelete("picture")]
        [Authorize]
        public async Task<IActionResult> RemovePicture()
        {
            return CreateActionResult(await userService.RemovePictureAsync(CurrentUserId));
        }

        [HttpPut("password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(UserPasswordUpdateDto userPasswordUpdateDto)
        {
            if (!string.IsNullOrEmpty(userPasswordUpdateDto.NewPassword))
            {
                var passwordValidation = ValidatePassword(userPasswordUpdateDto.NewPassword);
                if (passwordValidation != null)
                {
                    return passwordValidation;
                }
            }

            var currentRefreshToken = AuthCookieHelper.GetRefreshToken(Request);
            var response = await userService.ChangePasswordAsync(CurrentUserId, userPasswordUpdateDto, currentRefreshToken);
            if (response.Data is not null)
            {
                AuthCookieHelper.SetAuthCookies(Response, response.Data);
            }

            return CreateActionResult(response);
        }

        [HttpGet("invites")]
        [Authorize]
        public async Task<IActionResult> MyInvites()
        {
            return CreateActionResult(await userService.GetPendingInvitesAsync(CurrentUserId));
        }

        [HttpPost("invites/{id:int}/accept")]
        [ServiceFilter(typeof(NotFoundFilter<TeamInvite>))]
        [Authorize]
        public async Task<IActionResult> AcceptInvite(int id)
        {
            return CreateActionResult(await userService.AcceptInviteAsync(CurrentUserId, id));
        }

        [HttpPost("invites/{id:int}/reject")]
        [ServiceFilter(typeof(NotFoundFilter<TeamInvite>))]
        [Authorize]
        public async Task<IActionResult> RejectInvite(int id)
        {
            return CreateActionResult(await userService.RejectInviteAsync(CurrentUserId, id));
        }
    }
}
