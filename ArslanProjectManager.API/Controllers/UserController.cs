﻿using ArslanProjectManager.API.Filters;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.Constants;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;

namespace ArslanProjectManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService userService, ITokenService tokenService, ITokenHandler tokenHandler, ITeamInviteService teamInviteService, ITeamService teamService, IMapper mapper) : CustomBaseController(tokenService)
    {
        private readonly IUserService _userService = userService;
        private readonly ITokenService _tokenService = tokenService;
        private readonly ITokenHandler _tokenHandler = tokenHandler;
        private readonly ITeamInviteService _teamInviteService = teamInviteService;
        private readonly ITeamService _teamService = teamService;
        private readonly IMapper _mapper = mapper;

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var token = await GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.Unauthorized));
            }

            JwtSecurityToken jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.TokenExpired));
            }

            var userProfileDto = await _userService.GetUserProfileAsync(token.UserId);
            if (userProfileDto is null)
            {
                return CreateActionResult(CustomResponseDto<UserProfileDto>.Fail(404, ErrorMessages.UserNotFound));
            }

            return CreateActionResult(CustomResponseDto<UserProfileDto>.Success(userProfileDto, 200));
        }

        [Authorize]
        [HttpGet()]
        public async Task<IActionResult> GetByToken()
        {
            var token = await GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.Unauthorized));
            }

            var user = await _userService.GetByIdAsync(token.UserId);
            if (user is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound));
            }

            var tokenDto = _mapper.Map<TokenDto>(token);
            return CreateActionResult(CustomResponseDto<TokenDto>.Success(tokenDto, 200));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequestDto dto)
        {
            var token = await _tokenService.GetValidTokenByRefreshTokenAsync(dto.RefreshToken);
            if (token is null || !token.IsActive)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.RefreshTokenMissing));
            }

            if (token.RefreshTokenExpiration < DateTime.UtcNow)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.RefreshTokenExpired));
            }

            var newToken = _tokenHandler.CreateToken(token.User, []);
            if (newToken is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.TokenGenerationFailed));
            }

            // Update the new token with the existing token's refresh token and expiration
            // to allow user to not login more than refresh token's expiration time. Maximum 7 days authorization.
            newToken.RefreshToken = token.RefreshToken;
            newToken.RefreshTokenExpiration = token.RefreshTokenExpiration;

            var registeredToken = await _tokenService.AddAsync(newToken);

            token.IsActive = false;
            _tokenService.ChangeStatus(token);

            var accessCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = newToken.Expiration,
                Path = "/"
            };
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Append("AccessToken", newToken.AccessToken, accessCookieOptions);

            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = newToken.RefreshTokenExpiration,
                Path = "/"
            };
            Response.Cookies.Delete("RefreshToken");
            Response.Cookies.Append("RefreshToken", newToken.RefreshToken, refreshCookieOptions);

            var tokenDto = _mapper.Map<TokenDto>(registeredToken);
            return CreateActionResult(CustomResponseDto<TokenDto>.Success(tokenDto, 200));
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            var token = await _userService.Login(userLoginDto);
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.InvalidCredentials));
            }

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token.AccessToken))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.TokenGenerationFailed));
            }

            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = token.Expiration,
                Path = "/",
                Domain = null
            };

            Response.Cookies.Append("AccessToken", token.AccessToken, cookieOptions);

            var refreshTokenOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = token.RefreshTokenExpiration,
                Path = "/",
                Domain = null
            };
            Response.Cookies.Append("RefreshToken", token.RefreshToken, refreshTokenOptions);

            Token registeredToken = await _tokenService.AddAsync(token);
            var tokenDto = _mapper.Map<TokenDto>(registeredToken);
            return CreateActionResult(CustomResponseDto<TokenDto>.Success(tokenDto, 200));
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            var accessToken = (await GetToken())?.AccessToken ?? Request.Cookies["AccessToken"];
            // Firstly, clear the access and refresh tokens from cookies before processing the logout. So, error handling will be easier.
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");

            if (!string.IsNullOrEmpty(accessToken))
            {
                var token = await _tokenService
                    .Where(t => t.AccessToken == accessToken && t.IsActive)
                    .FirstOrDefaultAsync();
                if (token is not null)
                {
                    token.IsActive = false;
                    _tokenService.Update(token);
                }
            }

            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserCreateDto userDto)
        {
            var existingUser = await _userService.AnyAsync(x => x.Email == userDto.Email);
            if (existingUser)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.EmailAlreadyExists));
            }

            var user = _mapper.Map<User>(userDto);

            if (userDto.ProfilePicture is not null && userDto.ProfilePicture.Length > 0)
            {
                var byteArray = Convert.FromBase64String(userDto.ProfilePicture);
                user.ProfilePicture = byteArray;
            }

            var emailValidation = ValidateEmail(userDto.Email);
            if (emailValidation != null) return emailValidation;

            var passwordValidation = ValidatePassword(userDto.Password);
            if (passwordValidation != null) return passwordValidation;

            user.Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            var savedUser = await _userService.AddAsync(user);
            var savedUserDto = _mapper.Map<UserDto>(savedUser);
            return CreateActionResult(CustomResponseDto<UserDto>.Success(savedUserDto, 201));
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> Update()
        {
            var token = await GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.AccessDenied));
            }
            var user = await _userService.GetByIdAsync(token.UserId);
            if (user is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound));
            }
            var userUpdateDto = _mapper.Map<UserUpdateDto>(user);

            userUpdateDto.ProfilePicture = user.ProfilePicture is not null ?
                Convert.ToBase64String(user.ProfilePicture).Insert(0, "data:image/png;base64,") : "/img/profile.png";

            return CreateActionResult(CustomResponseDto<UserUpdateDto>.Success(userUpdateDto, 200));
        }

        [HttpPut("[action]")]
        [Authorize]
        public async Task<IActionResult> Update(UserUpdateDto userDto)
        {
            var token = await GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.AccessDenied));
            }

            var existingUser = await _userService.GetByIdAsync(userDto.Id);
            if (existingUser is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound));
            }

            existingUser.Name = userDto.Name;

            if (!string.IsNullOrEmpty(userDto.Email))
            {
                var existingEmailUser = await _userService.GetByEmail(userDto.Email);
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

            _userService.Update(existingUser);

            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }

        [HttpDelete("remove-picture")]
        [Authorize]
        public async Task<IActionResult> RemovePicture()
        {
            var token = await GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.AccessDenied));
            }

            var user = await _userService.GetByIdAsync(token.UserId);
            if (user is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound));
            }

            user!.ProfilePicture = null;
            _userService.Update(user);
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }

        [HttpPut("update-password")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword(UserPasswordUpdateDto userPasswordUpdateDto)
        {
            var token = await GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.AccessDenied));
            }

            var existingUser = await _userService.GetByIdAsync(token.UserId);
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
            _userService.Update(existingUser);
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }

        [HttpGet("my-invites")]
        [Authorize]
        public async Task<IActionResult> MyInvites()
        {
            var token = await GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.Unauthorized));
            }

            var user = await _userService.GetByIdAsync(token.UserId);
            if (user is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound));
            }

            var pendingInvites = await _teamInviteService
                .Where(x => x.InvitedEmail == user.Email && x.Status == TeamInvite.InviteStatus.Pending)
                .Include(x => x.Team)
                .Include(x => x.InvitedBy)
                .ToListAsync();

            var inviteDtos = _mapper.Map<List<PendingInviteDto>>(pendingInvites);

            return CreateActionResult(CustomResponseDto<List<PendingInviteDto>>.Success(inviteDtos, 200));
        }


        [HttpPost("accept-invite/{id}")]
        [Authorize]
        public async Task<IActionResult> AcceptInvite(int id)
        {
            var token = await GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.Unauthorized));
            }

            var user = await _userService.GetByIdAsync(token.UserId);
            if (user is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound));
            }

            var invite = await _teamInviteService
                .Where(x => x.Id == id && x.InvitedEmail == user.Email)
                .Include(x => x.Team)
                .FirstOrDefaultAsync();

            if (invite is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.InviteNotFound));
            }

            if (invite.Status != TeamInvite.InviteStatus.Pending)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InviteAlreadyProcessed));
            }

            var existingTeamUser = await _teamService.GetTeamUserAsync(invite.TeamId, user.Id);
            if (existingTeamUser != null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.UserAlreadyTeamMember));
            }

            try
            {
                // Create team user
                var teamUser = new TeamUser
                {
                    TeamId = invite.TeamId,
                    UserId = user.Id,
                    RoleId = 1 // Default member role
                };

                await _teamService.AddTeamUserAsync(teamUser);

                // Update invite status
                invite.Status = TeamInvite.InviteStatus.Accepted;
                invite.UpdatedDate = DateTime.UtcNow;
                invite.StatusChangeNote = $"Accepted by {user.Name}";

                _teamInviteService.Update(invite);

                return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
            }
            catch
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToAcceptInvite));
            }
        }

        [HttpPost("reject-invite/{id}")]
        [Authorize]
        public async Task<IActionResult> RejectInvite(int id)
        {
            var token = await GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.Unauthorized));
            }

            var user = await _userService.GetByIdAsync(token.UserId);
            if (user is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound));
            }

            var invite = await _teamInviteService
                .Where(x => x.Id == id && x.InvitedEmail == user.Email)
                .FirstOrDefaultAsync();

            if (invite is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.InviteNotFound));
            }

            if (invite.Status != TeamInvite.InviteStatus.Pending)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InviteAlreadyProcessed));
            }

            try
            {
                invite.Status = TeamInvite.InviteStatus.Rejected;
                invite.UpdatedDate = DateTime.UtcNow;
                invite.StatusChangeNote = $"Rejected by {user.Name}";

                _teamInviteService.Update(invite);

                return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
            }
            catch
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToRejectInvite));
            }
        }

    }
}
