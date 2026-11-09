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

        /// <summary>
        /// Redirects to the 429 Too Many Requests page when the API returns rate limit exceeded.
        /// </summary>
        protected IActionResult RedirectToTooManyRequests() => RedirectToAction("TooManyRequests", "Home");

        /// <summary>
        /// Redirects to the 500 Server Error page when the API returns a 5xx server error.
        /// </summary>
        protected IActionResult RedirectToServerError() => RedirectToAction("ServerError", "Home");

        private protected async Task<string?> GetErrorMessageAsync(HttpResponseMessage response)
        {
            const string defaultErrorMessage = "An error occurred while processing your request. Please try again later.";
            if ((int)response.StatusCode >= 500)
            {
                return defaultErrorMessage;
            }

            try
            {
                var jsonError = await response.Content.ReadAsStreamAsync();
                var errorResponse = await JsonSerializer.DeserializeAsync<CustomResponseDto<NoContentDto>>(jsonError, _jsonSerializerOptions);
                return errorResponse?.Errors?.FirstOrDefault() ?? defaultErrorMessage;
            }
            catch
            {
                return defaultErrorMessage;
            }
        }
    }
}
