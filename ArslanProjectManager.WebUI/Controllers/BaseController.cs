using ArslanProjectManager.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ArslanProjectManager.WebUI.Controllers
{
    public class BaseController : Controller
    {
        private protected readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private protected async Task<string?> GetErrorMessageAsync(HttpResponseMessage response)
        {
            var jsonError = await response.Content.ReadAsStreamAsync();
            var errorResponse = await JsonSerializer.DeserializeAsync<CustomResponseDto<NoContentDto>>(jsonError, _jsonSerializerOptions);
            return errorResponse?.Errors?.FirstOrDefault() ?? "An error occurred while processing your request. Please try again later.";
            
        }
    }
}
