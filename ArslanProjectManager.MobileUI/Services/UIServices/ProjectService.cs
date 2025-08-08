using ArslanProjectManager.Core.DTOs;
using System.Net.Http.Json;

namespace ArslanProjectManager.MobileUI.Services.UIServices
{
    public class ProjectService (IHttpClientFactory httpClientFactory) : GenericService(httpClientFactory)
    {
        public async Task<CustomResponseDto<IEnumerable<UserProjectDto>>?> GetAllProjectsAsync()
        {
            var client = base.GetClient();
            var response = await client.GetAsync("projects");
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<IEnumerable<UserProjectDto>>>();
                return errorWrapper ?? new CustomResponseDto<IEnumerable<UserProjectDto>> { IsSuccess = false, Errors = ["Failed to load projects"] };
            }
            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<IEnumerable<UserProjectDto>>>();
            return wrapper;
        }
    }
}
