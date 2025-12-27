using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using System.Net.Http.Json;

namespace ArslanProjectManager.MobileUI.Services.UIServices
{
    public class UserService(IHttpClientFactory httpClientFactory) : GenericService(httpClientFactory)
    {
        public async Task<CustomResponseDto<UserProfileDto>?> GetProfileAsync()
        {
            var client = base.GetClient();
            var response = await client.GetAsync("user/profile");
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<UserProfileDto>>();
                return errorWrapper ?? new CustomResponseDto<UserProfileDto> { IsSuccess = false, Errors = ["Failed to load profile"] };
            }
            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<UserProfileDto>>();
            return wrapper;
        }

        public async Task<CustomResponseDto<UserUpdateDto>?> GetUpdateProfileAsync()
        {
            var client = base.GetClient();
            var response = await client.GetAsync("user/edit-meta");
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<UserUpdateDto>>();
                return errorWrapper ?? new CustomResponseDto<UserUpdateDto> { IsSuccess = false, Errors = ["Failed to retrieve user data."] };
            }
            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<UserUpdateDto>>();
            return wrapper;
        }

        public async Task<CustomResponseDto<NoContentDto>?> UpdateProfileAsync(UserUpdateDto dto)
        {
            var client = base.GetClient();
            var response = await client.PutAsJsonAsync("user", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<NoContentDto>>();
                return errorWrapper ?? new CustomResponseDto<NoContentDto> { IsSuccess = false, Errors = ["Failed to update profile"] };
            }
            return new CustomResponseDto<NoContentDto> { IsSuccess = true};
        }

        public async Task<CustomResponseDto<NoContentDto>?> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            var client = base.GetClient();
            var dto = new UserPasswordUpdateDto{ Password = currentPassword, NewPassword = newPassword };
            var response = await client.PutAsJsonAsync("user/password", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<NoContentDto>>();
                return errorWrapper ?? new CustomResponseDto<NoContentDto> { IsSuccess = false, Errors = ["Failed to change password"] };
            }
            return new CustomResponseDto<NoContentDto> { IsSuccess = true};
        }


        public async Task<CustomResponseDto<List<PendingInviteDto>>?> GetMyInvitesAsync()
        {
            var client = base.GetClient();
            var response = await client.GetAsync("user/invites");
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<List<PendingInviteDto>>>();
                return errorWrapper ?? new CustomResponseDto<List<PendingInviteDto>> { IsSuccess = false, Errors = ["Failed to load invitations"] };
            }

            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<List<PendingInviteDto>>>();
            return wrapper;
        }

        public async Task<CustomResponseDto<NoContentDto>?> AcceptInviteAsync(int inviteId)
        {
            var client = base.GetClient();
            var response = await client.PostAsync($"user/invites/{inviteId}/accept", null);
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<NoContentDto>>();
                return errorWrapper ?? new CustomResponseDto<NoContentDto> { IsSuccess = false, Errors = ["Failed to accept invitation"] };
            }

            return new CustomResponseDto<NoContentDto> { IsSuccess = true };
        }

        public async Task<CustomResponseDto<NoContentDto>?> RejectInviteAsync(int inviteId)
        {
            var client = base.GetClient();
            var response = await client.PostAsync($"user/invites/{inviteId}/reject", null);
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<NoContentDto>>();
                return errorWrapper ?? new CustomResponseDto<NoContentDto> { IsSuccess = false, Errors = ["Failed to reject invitation"] };
            }

            return new CustomResponseDto<NoContentDto> { IsSuccess = true };
        }
    }
}
