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

        public async Task<CustomResponseDto<TokenDto>?> LoginAsync(UserLoginDto loginDto)
        {
            var client = base.GetClient();
            client.DefaultRequestHeaders.Add("SkipTokenRefresher", "true");
            var response = await client.PostAsJsonAsync("user/login", loginDto);
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<TokenDto>>();
                return errorWrapper ?? new CustomResponseDto<TokenDto> { IsSuccess = false, Errors = ["Login failed"] };
            }
            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<TokenDto>>();
            return wrapper;
        }

        public async Task<CustomResponseDto<UserDto>?> RegisterAsync(UserCreateDto registerDto)
        {
            var client = base.GetClient();
            client.DefaultRequestHeaders.Add("SkipTokenRefresher", "true");
            var response = await client.PostAsJsonAsync("user/register", registerDto);
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<UserDto>>();
                return errorWrapper ?? new CustomResponseDto<UserDto> { IsSuccess = false, Errors = ["Register failed"] };
            }
            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<UserDto>>();
            return wrapper;
        }

        public async Task<CustomResponseDto<UserUpdateDto>?> GetUpdateProfileAsync()
        {
            var client = base.GetClient();
            var response = await client.GetAsync("user/update");
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
            var response = await client.PutAsJsonAsync("user/update", dto);
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
            var response = await client.PutAsJsonAsync("user/update-password", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<NoContentDto>>();
                return errorWrapper ?? new CustomResponseDto<NoContentDto> { IsSuccess = false, Errors = ["Failed to change password"] };
            }
            return new CustomResponseDto<NoContentDto> { IsSuccess = true};
        }
    }
}
