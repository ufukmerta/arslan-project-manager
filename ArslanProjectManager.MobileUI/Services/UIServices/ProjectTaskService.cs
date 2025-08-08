using ArslanProjectManager.Core.DTOs;
using System.Net.Http.Json;

namespace ArslanProjectManager.MobileUI.Services.UIServices
{
    public class ProjectTaskService(IHttpClientFactory httpClientFactory) : GenericService(httpClientFactory)
    {
        public async Task<CustomResponseDto<List<ProjectTaskDto>>?> GetAllTasksAsync()
        {
            var client = base.GetClient();
            var response = await client.GetAsync($"tasks");
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<List<ProjectTaskDto>>>();
                return errorWrapper ?? new CustomResponseDto<List<ProjectTaskDto>> { IsSuccess = false, Errors = ["Failed to load tasks"] };
            }
            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<List<ProjectTaskDto>>>();
            return wrapper;
        }
    }
}
