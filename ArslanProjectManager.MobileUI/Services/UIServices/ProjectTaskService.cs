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

        public async Task<CustomResponseDto<ProjectTaskDto>?> GetTaskDetailsAsync(int id)
        {
            var client = GetClient();
            var response = await client.GetAsync($"tasks/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<ProjectTaskDto>>();
                return errorWrapper ?? new CustomResponseDto<ProjectTaskDto> { IsSuccess = false, Errors = ["Failed to load task details"] };
            }
            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<ProjectTaskDto>>();
            return wrapper;
        }

        public async Task<CustomResponseDto<NoContentDto>?> AddCommentAsync(int taskId, string comment)
        {
            var client = GetClient();
            var response = await client.PostAsJsonAsync("tasks/comment", new { TaskId = taskId, Comment = comment });
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<NoContentDto>>();
                return errorWrapper ?? new CustomResponseDto<NoContentDto> { IsSuccess = false, Errors = ["Failed to add comment"] };
            }
            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<NoContentDto>>();
            return wrapper;
        }
    }
}
