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
    }
}
