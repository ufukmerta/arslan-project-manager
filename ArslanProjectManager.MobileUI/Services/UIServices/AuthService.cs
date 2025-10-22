using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using System.Net.Http.Json;

namespace ArslanProjectManager.MobileUI.Services.UIServices
{
    public class AuthService(IHttpClientFactory httpClientFactory) : GenericService(httpClientFactory)
    {
        public async Task<CustomResponseDto<TokenDto>?> LoginAsync(UserLoginDto loginDto)
        {
            var client = base.GetClient();
            client.DefaultRequestHeaders.Add("SkipTokenRefresher", "true");
            var response = await client.PostAsJsonAsync("auth/login", loginDto);
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<TokenDto>>();
                return errorWrapper ?? new CustomResponseDto<TokenDto> { IsSuccess = false, Errors = ["Login failed"] };
            }
            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<TokenDto>>();
            return wrapper;
        }

        public async Task<CustomResponseDto<bool>?> LogoutAsync()
        {
            var client = base.GetClient();
            var response = await client.PostAsync("auth/logout", null);
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<bool>>();
                return errorWrapper ?? new CustomResponseDto<bool> { IsSuccess = false, Errors = ["Logout failed"] };
            }
            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<bool>>();
            return wrapper;
        }

        public async Task<CustomResponseDto<UserDto>?> RegisterAsync(UserCreateDto registerDto)
        {
            var client = base.GetClient();
            client.DefaultRequestHeaders.Add("SkipTokenRefresher", "true");
            var response = await client.PostAsJsonAsync("auth/register", registerDto);
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<UserDto>>();
                return errorWrapper ?? new CustomResponseDto<UserDto> { IsSuccess = false, Errors = ["Register failed"] };
            }
            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<UserDto>>();
            return wrapper;
        }

    }
}
