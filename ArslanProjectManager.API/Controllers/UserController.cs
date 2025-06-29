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
using System.Text.RegularExpressions;

namespace ArslanProjectManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService userService, ITokenService tokenService, IMapper mapper) : CustomBaseController(tokenService)
    {
        private readonly IUserService _userService = userService;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IMapper _mapper = mapper;

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var token = base.GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in"));
            }

            JwtSecurityToken jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Token is expired"));
            }

            var userProfileDto = await _userService.GetUserProfileAsync(token.UserId);
            if (userProfileDto is null)
            {
                return CreateActionResult(CustomResponseDto<UserProfileDto>.Fail(404, "User not found"));
            }

            return CreateActionResult(CustomResponseDto<UserProfileDto>.Success(userProfileDto, 200));
        }

        [Authorize]
        [HttpGet()]
        public async Task<IActionResult> GetByToken()
        {
            var token = GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, "Not logged in"));
            }

            var user = await _userService.GetByIdAsync(token.UserId);
            if (user is null)
        {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "User not found"));
            }

            var tokenDto = _mapper.Map<TokenDto>(token);
            return CreateActionResult(CustomResponseDto<TokenDto>.Success(tokenDto, 200));
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            var token = await _userService.Login(userLoginDto);
            if (token is null)
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

            if (userDto.ProfilePicture is not null && userDto.ProfilePicture.Length > 0)
            {
                var byteArray = Convert.FromBase64String(userDto.ProfilePicture);
                user.ProfilePicture = byteArray;
            }

            var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            if (string.IsNullOrEmpty(userDto.Email) || !emailRegex.IsMatch(userDto.Email))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Invalid email format."));
            }

            var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*\/?&+\-_.])[A-Za-z\d@$@$!%*\/?&+\-_.]{8,}$");
            if (!passwordRegex.IsMatch(userDto.Password))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one digit, and one special character (@$!%*/?&+-_.)."));
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            var savedUser = await _userService.AddAsync(user);
            var savedUserDto = _mapper.Map<UserDto>(savedUser);
            return CreateActionResult(CustomResponseDto<UserDto>.Success(savedUserDto, 201));
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> Update()
        {
            var token = GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, "Not authorized"));
            }
            var user = await _userService.GetByIdAsync(token.UserId);
            if (user is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "User not found."));
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
            var token = GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, "Not authorized"));
            }

            var existingUser = await _userService.GetByIdAsync(userDto.Id);
            if (existingUser is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "User not found."));
            }

            existingUser.Name = userDto.Name;

            if (!string.IsNullOrEmpty(userDto.Email))
            {
                var existingEmailUser = await _userService.GetByEmail(userDto.Email);
                if (existingEmailUser != null && existingEmailUser.Id != userDto.Id)
            {
                    return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(409, "This email already exists. You cannot change your email with given email address."));
            }
            }

            var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            if (string.IsNullOrEmpty(userDto.Email) || !emailRegex.IsMatch(userDto.Email))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Invalid email format."));
            }

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
            var token = GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, "Not authorized"));
            }

            var user = await _userService.GetByIdAsync(token.UserId);
            if (user is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "User not found."));
            }

            user!.ProfilePicture = null;
            _userService.Update(user);
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }

        [HttpPut("update-password")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword(UserPasswordUpdateDto userPasswordUpdateDto)
        {
            var token = GetToken();
            if (token is null || token.UserId != userPasswordUpdateDto.Id)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, "Not authorized"));
            }

            var existingUser = await _userService.GetByIdAsync(userPasswordUpdateDto.Id);
            if (existingUser is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "User not found."));
            }

            if (string.IsNullOrEmpty(userPasswordUpdateDto.Password) || string.IsNullOrEmpty(userPasswordUpdateDto.NewPassword))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Current and new password are required to set a new password."));
            }

            if (!string.IsNullOrEmpty(userPasswordUpdateDto.Password) && !BCrypt.Net.BCrypt.Verify(userPasswordUpdateDto.Password, existingUser.Password))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Current password is incorrect."));
            }

            var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*\/?&+\-_.])[A-Za-z\d@$!%*\/?&+\-_.]{8,}$");
            if (!passwordRegex.IsMatch(userPasswordUpdateDto.Password))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character."));
            }

            existingUser.Password = BCrypt.Net.BCrypt.HashPassword(userPasswordUpdateDto.NewPassword);
            _userService.Update(existingUser);
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }
    }
}
