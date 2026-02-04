using ArslanProjectManager.API.Filters;
using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace ArslanProjectManager.API.Controllers
{
    /// <summary>
    /// Manages user operations including authentication, profile management, and team invitations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService userService, ITokenService tokenService, ITeamInviteService teamInviteService, ITeamService teamService, IMapper mapper, IRoleService roleService) : CustomBaseController(tokenService)
    {
        /// <summary>
        /// Retrieves the profile information of the authenticated user
        /// </summary>
        /// <returns>User profile information</returns>
        /// <response code="200">Returns the user profile</response>
        /// <response code="401">If the user is not authenticated or token is expired</response>
        /// <response code="404">If the user is not found</response>
        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var token = (await GetToken())!;
            JwtSecurityToken jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.TokenExpired));
            }

            var userProfileDto = await userService.GetUserProfileAsync(token.UserId);
            if (userProfileDto is null)
            {
                return CreateActionResult(CustomResponseDto<UserProfileDto>.Fail(404, ErrorMessages.UserNotFound));
            }

            return CreateActionResult(CustomResponseDto<UserProfileDto>.Success(userProfileDto, 200));
        }

        /// <summary>
        /// Retrieves the current user's token information
        /// </summary>
        /// <returns>User token information including access token and refresh token details</returns>
        /// <response code="200">Returns the user token information</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the user is not found</response>
        [Authorize]
        [HttpGet()]
        public async Task<IActionResult> GetByToken()
        {
            var token = (await GetToken())!;
            var user = await userService.GetByIdAsync(token.UserId);
            if (user is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound));
            }

            var tokenDto = mapper.Map<TokenDto>(token);
            return CreateActionResult(CustomResponseDto<TokenDto>.Success(tokenDto, 200));
        }



        /// <summary>
        /// Retrieves the edit profile form metadata for the authenticated user
        /// </summary>
        /// <returns>User edit form metadata</returns>
        /// <response code="200">Returns the user edit form metadata</response>
        /// <response code="403">If the user is not authenticated</response>
        /// <response code="404">If the user is not found</response>
        [HttpGet("[action]-meta")]
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var token = (await GetToken())!;
            var user = await userService.GetByIdAsync(token.UserId);
            if (user is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound));
            }
            var userUpdateDto = mapper.Map<UserUpdateDto>(user);

            userUpdateDto.ProfilePicture = user.ProfilePicture is not null ?
                Convert.ToBase64String(user.ProfilePicture).Insert(0, "data:image/png;base64,") : "/img/profile.png";

            return CreateActionResult(CustomResponseDto<UserUpdateDto>.Success(userUpdateDto, 200));
        }

        /// <summary>
        /// Updates the profile information of the authenticated user
        /// </summary>
        /// <param name="userDto">Updated user information including name, email, and profile picture</param>
        /// <returns>No content response</returns>
        /// <response code="204">User profile updated successfully</response>
        /// <response code="403">If the user is not authenticated</response>
        /// <response code="404">If the user is not found</response>
        /// <response code="409">If the email is already in use by another user</response>
        [HttpPut()]
        [Authorize]
        public async Task<IActionResult> Edit(UserUpdateDto userDto)
        {            
            var existingUser = await userService.GetByIdAsync(userDto.Id);
            if (existingUser is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound));
            }

            existingUser.Name = userDto.Name;

            if (!string.IsNullOrEmpty(userDto.Email))
            {
                var existingEmailUser = await userService.GetByEmailAsync(userDto.Email);
                if (existingEmailUser != null && existingEmailUser.Id != userDto.Id)
                {
                    return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(409, ErrorMessages.EmailAlreadyExistsForUpdate));
                }
            }

            var emailValidation = ValidateEmail(userDto.Email);
            if (emailValidation != null) return emailValidation;

            existingUser.Email = userDto.Email;

            if (userDto.ProfilePicture is not null && userDto.ProfilePicture.Length > 0)
            {
                var byteArray = Convert.FromBase64String(userDto.ProfilePicture);
                existingUser.ProfilePicture = byteArray;
            }

            userService.Update(existingUser);

            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }

        /// <summary>
        /// Removes the profile picture of the authenticated user
        /// </summary>
        /// <returns>No content response</returns>
        /// <response code="204">Profile picture removed successfully</response>
        /// <response code="403">If the user is not authenticated</response>
        /// <response code="404">If the user is not found</response>
        [HttpDelete("picture")]
        [Authorize]
        public async Task<IActionResult> RemovePicture()
        {
            var token = (await GetToken())!;
            var user = await userService.GetByIdAsync(token.UserId);
            if (user is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound));
            }

            user!.ProfilePicture = null;
            userService.Update(user);
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }

        /// <summary>
        /// Updates the password for the authenticated user
        /// </summary>
        /// <param name="userPasswordUpdateDto">Password update data including current password and new password</param>
        /// <returns>No content response</returns>
        /// <response code="204">Password updated successfully</response>
        /// <response code="400">If new password or current password is empty or current password is incorrect</response>
        /// <response code="403">If the user is not authenticated</response>
        /// <response code="404">If the user is not found</response>
        [HttpPut("password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(UserPasswordUpdateDto userPasswordUpdateDto)
        {
            var token = (await GetToken())!;
            var existingUser = await userService.GetByIdAsync(token.UserId);
            if (existingUser is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound));
            }

            if (string.IsNullOrEmpty(userPasswordUpdateDto.Password) || string.IsNullOrEmpty(userPasswordUpdateDto.NewPassword))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.PasswordsRequired));
            }

            if (!string.IsNullOrEmpty(userPasswordUpdateDto.Password) && !BCrypt.Net.BCrypt.Verify(userPasswordUpdateDto.Password, existingUser.Password))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.CurrentPasswordIncorrect));
            }

            var passwordValidation = ValidatePassword(userPasswordUpdateDto.NewPassword);
            if (passwordValidation != null) return passwordValidation;

            existingUser.Password = BCrypt.Net.BCrypt.HashPassword(userPasswordUpdateDto.NewPassword);
            userService.Update(existingUser);
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }

        /// <summary>
        /// Retrieves the list of pending team invitations for the authenticated user
        /// </summary>
        /// <returns>List of pending invitations</returns>
        /// <response code="200">Returns the list of pending invitations</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the user is not found</response>
        [HttpGet("invites")]
        [Authorize]
        public async Task<IActionResult> MyInvites()
        {
            var token = (await GetToken())!;
            var user = await userService.GetByIdAsync(token.UserId);
            if (user is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound));
            }

            var pendingInvites = await teamInviteService
                .Where(x => x.InvitedEmail == user.Email && x.Status == TeamInvite.InviteStatus.Pending)
                .Include(x => x.Team)
                .Include(x => x.InvitedBy)
                .ToListAsync();

            var inviteDtos = mapper.Map<List<PendingInviteDto>>(pendingInvites);

            return CreateActionResult(CustomResponseDto<List<PendingInviteDto>>.Success(inviteDtos, 200));
        }


        /// <summary>
        /// Accepts a pending team invitation
        /// </summary>
        /// <param name="id">The ID of the invitation to accept</param>
        /// <returns>No content response</returns>
        /// <response code="204">Invitation accepted successfully</response>
        /// <response code="400">If the invitation is already processed</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the invitation is not found</response>
        [HttpPost("invites/{id:int}/accept")]
        [ServiceFilter(typeof(NotFoundFilter<TeamInvite>))]
        [Authorize]
        public async Task<IActionResult> AcceptInvite(int id)
        {
            var token = (await GetToken())!;
            var user = await userService.GetByIdAsync(token.UserId);
            if (user is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound));
            }

            var invite = await teamInviteService
                .Where(x => x.Id == id && x.InvitedEmail == user.Email)
                .Include(x => x.Team)
                .FirstOrDefaultAsync();

            if (invite!.Status != TeamInvite.InviteStatus.Pending)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InviteAlreadyProcessed));
            }

            var existingTeamUser = await teamService.GetTeamUserAsync(invite.TeamId, user.Id);
            if (existingTeamUser != null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.UserAlreadyTeamMember));
            }

            try
            {
                // Find default role (Member or first non-admin system role)
                var defaultRole = await roleService.GetDefaultRoleAsync();
                if (defaultRole == null)
                {
                    return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.DefaultRolesNotFound));
                }

                // Create team user
                var teamUser = new TeamUser
                {
                    TeamId = invite.TeamId,
                    UserId = user.Id,
                    RoleId = defaultRole.Id
                };

                await teamService.AddTeamUserAsync(teamUser);

                // Update invite status
                invite.Status = TeamInvite.InviteStatus.Accepted;
                invite.UpdatedDate = DateTime.UtcNow;
                invite.StatusChangeNote = $"Accepted by {user.Name}";

                teamInviteService.Update(invite);

                return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
            }
            catch
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToAcceptInvite));
            }
        }

        /// <summary>
        /// Rejects a pending team invitation
        /// </summary>
        /// <param name="id">The ID of the invitation to reject</param>
        /// <returns>No content response</returns>
        /// <response code="204">Invitation rejected successfully</response>
        /// <response code="400">If the invitation is already processed</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the invitation is not found</response>
        [HttpPost("invites/{id:int}/reject")]
        [ServiceFilter(typeof(NotFoundFilter<TeamInvite>))]
        [Authorize]
        public async Task<IActionResult> RejectInvite(int id)
        {
            var token = (await GetToken())!;
            var user = await userService.GetByIdAsync(token.UserId);
            if (user is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound));
            }

            var invite = await teamInviteService
                .Where(x => x.Id == id && x.InvitedEmail == user.Email)
                .FirstOrDefaultAsync();

            if (invite!.Status != TeamInvite.InviteStatus.Pending)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InviteAlreadyProcessed));
            }

            try
            {
                invite.Status = TeamInvite.InviteStatus.Rejected;
                invite.UpdatedDate = DateTime.UtcNow;
                invite.StatusChangeNote = $"Rejected by {user.Name}";

                teamInviteService.Update(invite);

                return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
            }
            catch
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToRejectInvite));
            }
        }

    }
}
