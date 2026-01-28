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
    public class AuthController(IAuthService authService, ITokenService tokenService, ITokenHandler tokenHandler, IMapper mapper) : CustomBaseController(tokenService)
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
            if (token is null || !token.IsActive)
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
            // to allow user to not login more than refresh token's expiration time. Maximum 7 days authorization.
            newToken.RefreshToken = token.RefreshToken;
            newToken.RefreshTokenExpiration = token.RefreshTokenExpiration;

            var registeredToken = await TokenService.AddAsync(newToken);

            token.IsActive = false;
            TokenService.ChangeStatus(token);

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
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            var accessToken = (await GetToken())?.AccessToken ?? Request.Cookies["AccessToken"];
            // Firstly, clear the access and refresh tokens from cookies before processing the logout. So, error handling will be easier.
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");

            if (!string.IsNullOrEmpty(accessToken))
            {
                var token = await TokenService
                    .Where(t => t.AccessToken == accessToken && t.IsActive)
                    .FirstOrDefaultAsync();
                if (token is not null)
                {
                    token.IsActive = false;
                    TokenService.Update(token);
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
            if (emailValidation != null) return emailValidation;

            var passwordValidation = ValidatePassword(userDto.Password);
            if (passwordValidation != null) return passwordValidation;            

            var responseDto = await authService.RegisterAsync(userDto);
            return CreateActionResult(responseDto);
        }
    }
}
