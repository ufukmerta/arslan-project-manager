using ArslanProjectManager.Core.DTOs;
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

        private static CookieOptions CreateCookieOptions(DateTime? expires = null) =>
            new()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = expires,
                Path = "/"
            };

        internal static void SetAuthCookies(HttpResponse response, Token token)
        {
            ClearAuthCookies(response);

            if (string.IsNullOrEmpty(token?.AccessToken))
                return;

            response.Cookies.Append(AccessTokenKey, token.AccessToken, CreateCookieOptions(token.Expiration));
            response.Cookies.Append(RefreshTokenKey, token.RefreshToken, CreateCookieOptions(token.RefreshTokenExpiration));
        }

        internal static void SetAuthCookies(HttpResponse response, TokenDto? token)
        {
            ClearAuthCookies(response);

            if (string.IsNullOrEmpty(token?.AccessToken))
                return;

            response.Cookies.Append(AccessTokenKey, token.AccessToken, CreateCookieOptions(token.Expiration));
            response.Cookies.Append(RefreshTokenKey, token.RefreshToken, CreateCookieOptions(token.RefreshTokenExpiration));
        }

        internal static string? GetAccessToken(HttpRequest request)
        {
            return request.Cookies.TryGetValue(AccessTokenKey, out var token) && !string.IsNullOrEmpty(token)
                ? token
                : null;
        }

        internal static string? GetRefreshToken(HttpRequest request)
        {
            return request.Cookies.TryGetValue(RefreshTokenKey, out var token) && !string.IsNullOrEmpty(token)
                ? token
                : null;
        }

        internal static void ClearAuthCookies(HttpResponse response)
        {
            response.Cookies.Delete(AccessTokenKey);
            response.Cookies.Delete(RefreshTokenKey);
        }
    }
}
