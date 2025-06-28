using ArslanProjectManager.API.Filters;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
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
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : CustomBaseController
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public UserController(IUserService userService, ITokenService tokenService, IMapper mapper) : base(tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            var token = await _userService.Login(userLoginDto);
            if (token == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Invalid email or password."));
            }

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token.AccessToken))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, "Invalid token format generated"));
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
            var TokenDto = _mapper.Map<TokenDto>(registeredToken);
            return CreateActionResult(CustomResponseDto<TokenDto>.Success(TokenDto, 200));
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            var accessToken = GetToken()?.AccessToken ?? Request.Cookies["AccessToken"];
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
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "This email is already registered."));
            }
            var user = _mapper.Map<User>(userDto);
            if (userDto.ProfilePicture != null && userDto.ProfilePicture.Length > 0)
            {
                var byteArray = Convert.FromBase64String(userDto.ProfilePicture);
                user.ProfilePicture = byteArray;
            }
            user.Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            var savedUser = await _userService.AddAsync(user);
            var savedUserDto = _mapper.Map<UserDto>(savedUser);
            return CreateActionResult(CustomResponseDto<UserDto>.Success(savedUserDto, 201));
        }
        
        [HttpPut("[action]")]
        [Authorize]
        public async Task<IActionResult> Update(UserUpdateDto userDto)
        {
            var existingUser = await _userService.GetByIdAsync(userDto.Id);
            if (existingUser == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "User not found."));
            }
            if (string.IsNullOrEmpty(userDto.Password) && !string.IsNullOrEmpty(userDto.NewPassword))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Current password is required to set a new password."));
            }
            if (!string.IsNullOrEmpty(userDto.Password) && string.IsNullOrEmpty(userDto.NewPassword))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "New password is required to change the current password."));
            }
            if (!string.IsNullOrEmpty(userDto.NewPassword) && userDto.NewPassword.Length <= 8)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "New password must be at least 8 characters long."));
            }
            if (!string.IsNullOrEmpty(userDto.Password) && !BCrypt.Net.BCrypt.Verify(userDto.Password, existingUser.Password))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Current password is incorrect."));
            }

            existingUser.Name = userDto.Name;
            existingUser.Email = userDto.Email;
            existingUser.Password = string.IsNullOrEmpty(userDto.NewPassword) ? existingUser.Password : BCrypt.Net.BCrypt.HashPassword(userDto.NewPassword);
            if (userDto.ProfilePicture != null && userDto.ProfilePicture.Length > 0)
            {
                var byteArray = Convert.FromBase64String(userDto.ProfilePicture);
                existingUser.ProfilePicture = byteArray;
            }

            _userService.Update(existingUser);

            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }
        
        [HttpPut("remove-picture/{userId}")]
        [ServiceFilter(typeof(NotFoundFilter<User>))]
        [Authorize]
        public async Task<IActionResult> RemovePicture(int userId)
        {
            var token = GetToken();
            if (token == null || token.UserId != userId)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, "You are not authorized to remove this profile picture."));
            }
            var user = await _userService.GetByIdAsync(token.UserId);
            if (user == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "User not found."));
            }
            user.ProfilePicture = null;
            _userService.Update(user);
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }
    }
}
