using ArslanProjectManager.Core.DTOs;
using System.Net.Http.Json;

namespace ArslanProjectManager.MobileUI.Services.UIServices
{
    public class HomeService(IHttpClientFactory httpClientFactory)
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public async Task<CustomResponseDto<HomeDto>?> GetHomeSummaryAsync()
        {
            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            var response = await client.GetAsync("home");
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<HomeDto>>();
                return errorWrapper ?? new CustomResponseDto<HomeDto> { IsSuccess = false, Errors = ["Failed to load home data"] };
            }

            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<HomeDto>>();
            return wrapper;
        }
    }
}
