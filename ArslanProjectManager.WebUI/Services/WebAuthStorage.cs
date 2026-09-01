using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Services;

namespace ArslanProjectManager.WebUI.Services
{
    public class WebAuthStorage(IHttpContextAccessor httpContextAccessor) : IAuthStorage
    {
        /// <summary>
        /// Gets the access token from cookies.
        /// </summary>
        public Task<string?> GetAccessTokenAsync()
        {
            var accessToken = WebCookieHelper.GetAccessToken(httpContextAccessor.HttpContext?.Request!);
            return Task.FromResult(accessToken);
        }

        /// <summary>
        /// Gets the refresh token from cookies.
        /// </summary>
        public Task<string?> GetRefreshTokenAsync()
        {
            var refreshToken = WebCookieHelper.GetRefreshToken(httpContextAccessor.HttpContext?.Request!);
            return Task.FromResult(refreshToken);
        }

        /// <summary>
        /// Saves both access and refresh tokens to cookies.
        /// </summary>
        public Task SaveTokensAsync(string accessToken, string refreshToken, DateTime accessExpiration, DateTime refreshExpiration)
        {
            var context = httpContextAccessor.HttpContext!;
            
            // Create a TokenDto to use with the centralized helper
            var tokenDto = new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiration = accessExpiration,
                RefreshTokenExpiration = refreshExpiration
            };

            WebCookieHelper.SetAuthCookies(context.Response, tokenDto);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Clears both authentication cookies.
        /// </summary>
        public Task ClearTokensAsync()
        {
            var context = httpContextAccessor.HttpContext!;
            WebCookieHelper.ClearAuthCookies(context.Response);
            return Task.CompletedTask;
        }
    }
}
