using ArslanProjectManager.API.Utilities;
using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.Services;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ArslanProjectManager.API.Middlewares
{
    /// <summary>
    /// Middleware that silently refreshes expired access tokens for cookie-based sessions.
    /// </summary>
    public class TokenExpirationMiddleware(RequestDelegate next, IAuthService authService)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers.Authorization.ToString();
            var accessTokenFromHeader = authHeader?.StartsWith("Bearer ") == true
                ? authHeader["Bearer ".Length..].Trim()
                : null;

            if (!string.IsNullOrEmpty(accessTokenFromHeader))
            {
                await next(context);
                return;
            }

            var accessToken = AuthCookieHelper.GetAccessToken(context.Request);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                await next(context);
                return;
            }

            var jsonWebTokenHandler = new JsonWebTokenHandler();
            var jwtToken = jsonWebTokenHandler.ReadToken(accessToken);
            if (jwtToken.ValidTo > DateTime.UtcNow)
            {
                await next(context);
                return;
            }

            var refreshToken = AuthCookieHelper.GetRefreshToken(context.Request);
            var refreshResponse = await authService.RefreshTokenAsync(refreshToken);

            if (refreshResponse.StatusCode != 200 || refreshResponse.Data is null)
            {
                AuthCookieHelper.ClearAuthCookies(context.Response);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                var errorMessage = refreshResponse.Errors?.FirstOrDefault() ?? ErrorMessages.RefreshTokenMissing;
                await context.Response.WriteAsJsonAsync(new { message = errorMessage });
                return;
            }

            AuthCookieHelper.SetAuthCookies(context.Response, refreshResponse.Data);
            context.Request.Headers.Authorization = $"Bearer {refreshResponse.Data.AccessToken}";
            await next(context);
        }
    }
}
