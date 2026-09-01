using ArslanProjectManager.Core.DTOs;

namespace ArslanProjectManager.WebUI.Services
{
    /// <summary>
    /// Helper class for managing authentication cookies in the WebUI application.
    /// Provides centralized cookie options and methods for reading/writing auth tokens.
    /// </summary>
    internal static class WebCookieHelper
    {
        private const string AccessTokenKey = "AccessToken";
        private const string RefreshTokenKey = "RefreshToken";

        /// <summary>
        /// Creates standardized cookie options for authentication cookies.
        /// </summary>
        /// <param name="expires">Optional expiration time. If not provided, browser session cookie is used.</param>
        /// <returns>CookieOptions with HttpOnly, Secure, and SameSite settings.</returns>
        private static CookieOptions CreateCookieOptions(DateTime? expires = null) =>
            new()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = expires,
                Path = "/"
            };

        /// <summary>
        /// Appends authentication cookies (AccessToken and RefreshToken) to the HTTP response.
        /// </summary>
        /// <param name="response">The HTTP response object.</param>
        /// <param name="token">The token DTO containing access and refresh tokens with expiration times.</param>
        internal static void SetAuthCookies(HttpResponse response, TokenDto token)
        {
            if (token?.AccessToken == null)
                return;

            var accessTokenOptions = CreateCookieOptions(token.Expiration);
            response.Cookies.Append(AccessTokenKey, token.AccessToken, accessTokenOptions);

            if (token.RefreshToken != null)
            {
                var refreshTokenOptions = CreateCookieOptions(token.RefreshTokenExpiration);
                response.Cookies.Append(RefreshTokenKey, token.RefreshToken, refreshTokenOptions);
            }
        }

        /// <summary>
        /// Reads the access token from HTTP request cookies.
        /// </summary>
        /// <param name="request">The HTTP request object.</param>
        /// <returns>The access token if present; otherwise null.</returns>
        internal static string? GetAccessToken(HttpRequest request)
        {
            return request.Cookies.TryGetValue(AccessTokenKey, out var token) && !string.IsNullOrEmpty(token)
                ? token
                : null;
        }

        /// <summary>
        /// Reads the refresh token from HTTP request cookies.
        /// </summary>
        /// <param name="request">The HTTP request object.</param>
        /// <returns>The refresh token if present; otherwise null.</returns>
        internal static string? GetRefreshToken(HttpRequest request)
        {
            return request.Cookies.TryGetValue(RefreshTokenKey, out var token) && !string.IsNullOrEmpty(token)
                ? token
                : null;
        }

        /// <summary>
        /// Clears (deletes) authentication cookies from the HTTP response.
        /// </summary>
        /// <param name="response">The HTTP response object.</param>
        internal static void ClearAuthCookies(HttpResponse response)
        {
            response.Cookies.Delete(AccessTokenKey);
            response.Cookies.Delete(RefreshTokenKey);
        }
    }
}