using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.Services
{
    public interface IAuthService
    {
        Task<Token?> LoginAsync(UserLoginDto dto);
        Task<CustomResponseDto<UserDto>> RegisterAsync(UserCreateDto userDto);
        Task<TokenDto?> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync();
    }
}
