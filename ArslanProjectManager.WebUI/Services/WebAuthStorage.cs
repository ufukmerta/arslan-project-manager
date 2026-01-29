using ArslanProjectManager.Core.Services;

namespace ArslanProjectManager.WebUI.Services
{
    public class WebAuthStorage(IHttpContextAccessor httpContextAccessor) : IAuthStorage
    {
        public Task<string?> GetAccessTokenAsync()
        {
            return Task.FromResult(httpContextAccessor.HttpContext?.Request.Cookies["AccessToken"]);
        }

        public Task<string?> GetRefreshTokenAsync()
        {
            return Task.FromResult(httpContextAccessor.HttpContext?.Request.Cookies["RefreshToken"]);
        }

        public Task SaveTokensAsync(string accessToken, string refreshToken, DateTime accessExpiration, DateTime refreshExpiration)
        {
            var context = httpContextAccessor.HttpContext!;
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = accessExpiration
            };

            var refreshTokenOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = refreshExpiration
            };

            context.Response.Cookies.Append("AccessToken", accessToken, cookieOptions);
            context.Response.Cookies.Append("RefreshToken", refreshToken, refreshTokenOptions);

            return Task.CompletedTask;
        }

        public Task ClearTokensAsync()
        {
            var context = httpContextAccessor.HttpContext!;
            context.Response.Cookies.Delete("AccessToken");
            context.Response.Cookies.Delete("RefreshToken");
            return Task.CompletedTask;
        }
    }

}
