using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.API.Utilities
{
    /// <summary>
    /// Helper class for managing authentication cookies in the API.
    /// Provides centralized cookie options and methods for reading/writing auth tokens.
    /// </summary>
    internal static class AuthCookieHelper
    {
        private const string AccessTokenKey = "AccessToken";
        private const string RefreshTokenKey = "RefreshToken";

        /// <summary>
        /// Creates standardized cookie options for authentication cookies.
        /// </summary>
        /// <param name="expires">Optional expiration time. If not provided, uses the value specified.</param>
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
        /// Writes access and refresh tokens to HTTP cookies from a Token object.
        /// Clears any existing tokens before writing new ones.
        /// </summary>
        /// <param name="response">The HTTP response object.</param>
        /// <param name="token">The token object containing access and refresh tokens with expiration times.</param>
        internal static void SetAuthCookies(HttpResponse response, Token token)
        {
			// Clear existing cookies before writing new ones
            response.Cookies.Delete(AccessTokenKey);
            response.Cookies.Delete(RefreshTokenKey);
			
            if (token?.AccessToken == null)
                return;

            response.Cookies.Append(AccessTokenKey, token.AccessToken, CreateCookieOptions(token.Expiration));
            response.Cookies.Append(RefreshTokenKey, token.RefreshToken, CreateCookieOptions(token.RefreshTokenExpiration));
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
        /// Clears (deletes) both authentication cookies from the HTTP response.
        /// </summary>
        /// <param name="response">The HTTP response object.</param>
        internal static void ClearAuthCookies(HttpResponse response)
        {
            response.Cookies.Delete(AccessTokenKey);
            response.Cookies.Delete(RefreshTokenKey);
        }
    }
}
