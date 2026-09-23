using ArslanProjectManager.API.Utilities;
using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
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
    /// Handles authentication: login, register, refresh token, and logout.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService, ITokenService tokenService, ITokenHandler tokenHandler, IMapper mapper, IRedisService redisService, IUserService userService) : CustomBaseController(tokenService)
    {
        /// <summary>
        /// Refreshes the access token using a refresh token
        /// </summary>
        /// <param name="dto">The refresh token request</param>
        /// <returns>New access token</returns>
        /// <response code="200">Returns the new access token</response>
        /// <response code="401">If the refresh token is invalid or expired</response>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequestDto dto)
        {
            var token = await TokenService.GetValidTokenByRefreshTokenAsync(dto.RefreshToken);
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.RefreshTokenMissing));
            }

            if (token.RefreshTokenExpiration < DateTime.UtcNow)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.RefreshTokenExpired));
            }

            var newToken = tokenHandler.CreateToken(token.User, []);
            if (newToken is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.TokenGenerationFailed));
            }

            // Update the new token with the existing token's refresh token and expiration
            // to allow user to not login more than refresh token's expiration time. Maximum 7 days authentication without login.
            newToken.RefreshToken = token.RefreshToken;
            newToken.RefreshTokenExpiration = token.RefreshTokenExpiration;

            var registeredToken = await TokenService.AddAsync(newToken);
            AuthCookieHelper.SetAuthCookies(Response, newToken);

            TokenService.ChangeStatus(token);

            var tokenDto = mapper.Map<TokenDto>(registeredToken);
            return CreateActionResult(CustomResponseDto<TokenDto>.Success(tokenDto, 200));
        }

        /// <summary>
        /// Logs in a user and returns an access token
        /// </summary>
        /// <param name="userLoginDto">User login credentials</param>
        /// <returns>Access token and refresh token</returns>
        /// <response code="200">Returns the access token and refresh token</response>
        /// <response code="404">If the user credentials are invalid</response>
        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            var token = await authService.LoginAsync(userLoginDto);
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.InvalidCredentials));
            }

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token.AccessToken))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.TokenGenerationFailed));
            }

            AuthCookieHelper.SetAuthCookies(Response, token);

            Token registeredToken = await TokenService.AddAsync(token);
            var tokenDto = mapper.Map<TokenDto>(registeredToken);
            return CreateActionResult(CustomResponseDto<TokenDto>.Success(tokenDto, 200));
        }

        /// <summary>
        /// Logs out the current user by invalidating their access token
        /// </summary>
        /// <returns>No content response</returns>
        /// <response code="204">User logged out successfully</response>
        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Revoke the current access token (jti) in Redis and invalidate the current refresh token row in DB
            var jti = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            AuthCookieHelper.ClearAuthCookies(Response);

            if (!string.IsNullOrEmpty(jti))
            {
                // Try to determine remaining lifetime from exp claim
                var expClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
                TimeSpan ttl = TimeSpan.FromMinutes(5);
                if (!string.IsNullOrEmpty(expClaim) && long.TryParse(expClaim, out var expSeconds))
                {
                    try
                    {
                        var exp = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
                        ttl = exp - DateTime.UtcNow;
                        if (ttl <= TimeSpan.Zero) ttl = TimeSpan.FromMinutes(1);
                    }
                    catch { }
                }

                await redisService.RevokeJtiAsync(jti, ttl);
            }

            var refreshToken = AuthCookieHelper.GetRefreshToken(Request);
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var token = await TokenService
                    .Where(t => t.RefreshToken == refreshToken)
                    .FirstOrDefaultAsync();
                if (token is not null)
                {
                    TokenService.ChangeStatus(token);
                }
            }

            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }

        /// <summary>
        /// Logs out all other devices for the current user while keeping the current device logged in.
        /// Rotates the SecurityStamp, invalidates cached stamps and revokes other refresh tokens.
        /// </summary>
        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll()
        {
            var jti = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out var userId))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Invalid user"));
            }

            var user = await userService.GetByIdAsync(userId);
            if (user == null) return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound));

            // Rotate security stamp
            user.SecurityStamp = Guid.NewGuid().ToString();
            userService.Update(user);

            // Invalidate cached stamp
            await redisService.InvalidateSecurityStampCacheAsync(user.Id);

            // Revoke other refresh tokens (except current one)
            var currentRefreshToken = AuthCookieHelper.GetRefreshToken(Request);
            await TokenService.RevokeTokensForUserAsync(user.Id, currentRefreshToken);

            // Re-issue token for current device
            if (!string.IsNullOrEmpty(currentRefreshToken))
            {
                var currentTokenRow = await TokenService.GetValidTokenByRefreshTokenAsync(currentRefreshToken);
                if (currentTokenRow != null)
                {
                    var newToken = tokenHandler.CreateToken(user, new List<Role>());
                    newToken.RefreshToken = currentTokenRow.RefreshToken;
                    newToken.RefreshTokenExpiration = currentTokenRow.RefreshTokenExpiration;
                    await TokenService.AddAsync(newToken);
                    TokenService.ChangeStatus(currentTokenRow);
                    AuthCookieHelper.SetAuthCookies(Response, newToken);
                    var tokenDto = mapper.Map<TokenDto>(newToken);
                    return CreateActionResult(CustomResponseDto<TokenDto>.Success(tokenDto, 200));
                }
            }

            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="userDto">User registration details</param>
        /// <returns>Created user information</returns>
        /// <response code="201">Returns the created user</response>
        /// <response code="400">If the email is already in use</response>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserCreateDto userDto)
        {
            var emailValidation = ValidateEmail(userDto.Email);
            if (emailValidation != null)
                return emailValidation;

            var passwordValidation = ValidatePassword(userDto.Password);
            if (passwordValidation != null)
                return passwordValidation;

            var responseDto = await authService.RegisterAsync(userDto);
            return CreateActionResult(responseDto);
        }
    }
}
