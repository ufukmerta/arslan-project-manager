using ArslanProjectManager.Core.Services;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ArslanProjectManager.API.Middlewares
{
    public class TokenExpirationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITokenService _tokenService;
        private readonly ITokenHandler _tokenHandler;

        public TokenExpirationMiddleware(RequestDelegate next, ITokenService tokenService, ITokenHandler tokenHandler)
        {
            _next = next;
            _tokenService = tokenService;
            _tokenHandler = tokenHandler;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var accessToken = context.Request.Cookies["AccessToken"];
            var refreshToken = context.Request.Cookies["RefreshToken"];

            if (!string.IsNullOrEmpty(accessToken))
            {
                var tokenHandler = new JsonWebTokenHandler();
                var jwtToken = tokenHandler.ReadToken(accessToken);

                if (jwtToken.ValidTo > DateTime.UtcNow)
                {
                    // Token is still valid, proceed with the request
                    await _next(context);
                    return;
                }
                // Access token is expired, try to refresh using refresh token
                if (string.IsNullOrEmpty(refreshToken))
                {
                    // No refresh token available, clear cookies and return unauthorized
                    ClearTokenCookies(context);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = "Token expired and no refresh token available. Please login again." });
                    return;
                }

                // Try to get a valid token using the refresh token
                var existingToken = await _tokenService.GetValidTokenByRefreshTokenAsync(refreshToken);
                if (existingToken == null)
                {
                    // Invalid or expired refresh token
                    ClearTokenCookies(context);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = "Invalid or expired refresh token. Please login again." });
                    return;
                }

                if(existingToken.RefreshTokenExpiration < DateTime.UtcNow)
                {
                    // Refresh token is also expired, clear cookies and return unauthorized
                    ClearTokenCookies(context);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = "Refresh token expired. Please login again." });
                    return;
                }

                // Create new token
                var newToken = _tokenHandler.CreateToken(existingToken.User, new List<Core.Models.Role>());

                // Save new token to database
                newToken.RefreshToken = existingToken.RefreshToken; // Keep the same refresh token
                newToken.RefreshTokenExpiration = existingToken.RefreshTokenExpiration;
                await _tokenService.AddAsync(newToken);

                // Update old token status
                existingToken.IsActive = false;
                _tokenService.Update(existingToken);

                // Set new cookies
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = newToken.Expiration
                };

                var refreshCookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = newToken.RefreshTokenExpiration
                };

                context.Response.Cookies.Delete("AccessToken");
                context.Response.Cookies.Delete("RefreshToken");
                context.Response.Cookies.Append("AccessToken", newToken.AccessToken, cookieOptions);
                context.Response.Cookies.Append("RefreshToken", newToken.RefreshToken, refreshCookieOptions);

                // Update the Authorization header with the new token
                context.Request.Headers.Authorization = $"Bearer {newToken.AccessToken}";
            }                      
            await _next(context);
        }

        private static void ClearTokenCookies(HttpContext context)
        {
            context.Response.Cookies.Delete("AccessToken");
            context.Response.Cookies.Delete("RefreshToken");
        }
    }
} 