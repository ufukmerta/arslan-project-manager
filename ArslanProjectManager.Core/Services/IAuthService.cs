using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.Services
{
    public interface IAuthService
    {
        Task<Token?> LoginAsync(UserLoginDto dto);
        Task<CustomResponseDto<TokenDto>> LoginAndCreateSessionAsync(UserLoginDto dto);
        Task<CustomResponseDto<UserDto>> RegisterAsync(UserCreateDto userDto);
        Task<CustomResponseDto<TokenDto>> RefreshTokenAsync(string? refreshToken);
        Task<CustomResponseDto<NoContentDto>> LogoutAsync(string? jti, string? expClaim, string? refreshToken);
        Task<CustomResponseDto<TokenDto?>> LogoutAllAsync(int userId, string? currentRefreshToken);
    }
}
