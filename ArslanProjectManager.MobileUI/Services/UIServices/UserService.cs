using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using System.Net.Http.Json;

namespace ArslanProjectManager.MobileUI.Services.UIServices
{
    public class UserService(IHttpClientFactory httpClientFactory)
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        private HttpClient GetClient()
        {
            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            return client;
        }

        public async Task<CustomResponseDto<TokenDto>?> LoginAsync(UserLoginDto loginDto)
        {
            var client = GetClient();
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
            var client = GetClient();
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
    }
}
