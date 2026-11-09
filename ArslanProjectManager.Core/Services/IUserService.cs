using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.Services
{
    public interface IUserService : IGenericService<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<UserProfileDto?> GetUserProfileAsync(int userId);
        Task<CustomResponseDto<UserProfileDto>> GetProfileForUserAsync(int userId);
        Task<CustomResponseDto<TokenDto>> GetUserByTokenAsync(Token token);
        Task<CustomResponseDto<UserUpdateDto>> GetEditMetaAsync(int userId);
        Task<CustomResponseDto<NoContentDto>> UpdateUserProfileAsync(int userId, UserUpdateDto userDto);
        Task<CustomResponseDto<NoContentDto>> RemovePictureAsync(int userId);
        Task<CustomResponseDto<TokenDto?>> ChangePasswordAsync(int userId, UserPasswordUpdateDto dto, string? currentRefreshToken);
        Task<CustomResponseDto<List<PendingInviteDto>>> GetPendingInvitesAsync(int userId);
        Task<CustomResponseDto<NoContentDto>> AcceptInviteAsync(int userId, int inviteId);
        Task<CustomResponseDto<NoContentDto>> RejectInviteAsync(int userId, int inviteId);
    }
}
