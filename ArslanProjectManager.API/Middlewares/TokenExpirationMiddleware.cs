using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ArslanProjectManager.API.Middlewares
{
    /// <summary>
    /// Middleware that validates access tokens and refreshes them when sent via cookies.
    /// Runs before authentication; enforces token presence in the database and refresh-token expiry.
    /// </summary>
    /// <remarks>
    /// <para><b>Token source</b></para>
    /// <list type="bullet">
    /// <item><b>Authorization header (Bearer)</b>: Validation only. Ensures token exists in DB and refresh token is not expired.
    /// No automatic refresh; clients (e.g. mobile) are expected to refresh via a dedicated endpoint.</item>
    /// <item><b>AccessToken cookie</b>: Same DB validation (token exists, refresh not expired). If the access token is expired, a new one is issued using the
    /// RefreshToken cookie, and both cookies plus the Authorization header are updated for the current request.</item>
    /// </list>
    /// <para>When no access token is present, the request proceeds to the next middleware unchanged.</para>
    /// <para><paramref name="next"/> is the next delegate in the pipeline; <paramref name="tokenService"/> for token persistence and lookup; <paramref name="tokenHandler"/> for creating new JWTs.</para>
    /// </remarks>
    public class TokenExpirationMiddleware(RequestDelegate next, ITokenService tokenService, ITokenHandler tokenHandler)
    {
        /// <summary>
        /// Validates the access token and, for cookie-based requests, refreshes it if expired.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <returns>A task that completes when the pipeline has been invoked or a 401 response has been sent.</returns>
        /// <remarks>
        /// Resolves the access token from the Authorization header (Bearer) first, then from the AccessToken cookie.
        /// Refresh token is read only from the RefreshToken cookie. On validation or refresh failure, returns 401
        /// with a JSON body containing a "message" field; otherwise calls the next middleware.
        /// </remarks>
        public async Task InvokeAsync(HttpContext context)
        {
            // Resolve access token: Authorization header first, then cookie
            var authHeader = context.Request.Headers.Authorization.ToString();
            string? accessToken = null;
            var accessTokenFromHeader = false;

            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                accessToken = authHeader["Bearer ".Length..].Trim();
                accessTokenFromHeader = true;
            }
            else if (context.Request.Cookies.ContainsKey("AccessToken"))
            {
                accessToken = context.Request.Cookies["AccessToken"];
            }

            var refreshToken = context.Request.Cookies["RefreshToken"];

            if (!string.IsNullOrEmpty(accessToken))
            {
                if (accessTokenFromHeader)
                {
                    // Header path: validate only (token in DB, refresh not expired); no refresh
                    var existingToken = await tokenService.GetValidTokenByAccessTokenAsync(accessToken);
                    if (existingToken == null)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new { message = ErrorMessages.Unauthorized });
                        return;
                    }
                    if (existingToken.RefreshTokenExpiration < DateTime.UtcNow)
                    {
                        existingToken.IsActive = false;
                        tokenService.ChangeStatus(existingToken);
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new { message = ErrorMessages.Unauthorized });
                        return;
                    }
                    await next(context);
                    return;
                }

                // Cookie path: validate token in DB and JWT expiry; if expired, refresh using RefreshToken cookie
                var existingTokenFromDb = await tokenService.GetValidTokenByAccessTokenAsync(accessToken);
                if (existingTokenFromDb == null)
                {
                    ClearTokenCookies(context);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = ErrorMessages.Unauthorized });
                    return;
                }
                if (existingTokenFromDb.RefreshTokenExpiration < DateTime.UtcNow)
                {
                    existingTokenFromDb.IsActive = false;
                    tokenService.ChangeStatus(existingTokenFromDb);
                    ClearTokenCookies(context);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = ErrorMessages.Unauthorized });
                    return;
                }

                var jsonWebTokenHandler = new JsonWebTokenHandler();
                var jwtToken = jsonWebTokenHandler.ReadToken(accessToken);

                if (jwtToken.ValidTo > DateTime.UtcNow)
                {
                    // Token is still valid, proceed with the request
                    await next(context);
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
                var existingTokenByRefresh = await tokenService.GetValidTokenByRefreshTokenAsync(refreshToken);
                if (existingTokenByRefresh == null)
                {
                    // Invalid or expired refresh token
                    ClearTokenCookies(context);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = "Invalid or expired refresh token. Please login again." });
                    return;
                }

                if (existingTokenByRefresh.RefreshTokenExpiration < DateTime.UtcNow)
                {
                    // Refresh token expired: mark inactive (like CustomBaseController.ValidateToken)
                    existingTokenByRefresh.IsActive = false;
                    tokenService.ChangeStatus(existingTokenByRefresh);
                    ClearTokenCookies(context);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = "Refresh token expired. Please login again." });
                    return;
                }

                // Create new token
                var newToken = tokenHandler.CreateToken(existingTokenByRefresh.User, []);

                // Save new token to database
                newToken.RefreshToken = existingTokenByRefresh.RefreshToken; // Keep the same refresh token
                newToken.RefreshTokenExpiration = existingTokenByRefresh.RefreshTokenExpiration;
                await tokenService.AddAsync(newToken);

                // Update old token status
                existingTokenByRefresh.IsActive = false;
                tokenService.Update(existingTokenByRefresh);

                // Set new cookies

                ClearTokenCookies(context);
                AppendTokenToCookie(context, newToken);

                // Update the Authorization header with the new token
                context.Request.Headers.Authorization = $"Bearer {newToken.AccessToken}";
            }

            await next(context);
        }

        /// <summary>
        /// Removes the AccessToken and RefreshToken cookies from the response.
        /// </summary>
        /// <param name="context">The HTTP context; cookies are deleted on the response.</param>
        private static void ClearTokenCookies(HttpContext context)
        {
            context.Response.Cookies.Delete("AccessToken");
            context.Response.Cookies.Delete("RefreshToken");
        }

        /// <summary>
        /// Appends the access and refresh tokens to the response cookies with appropriate options.
        /// </summary>
        /// <param name="context">The HTTP context; cookies are added to the response.</param>
        private static void AppendTokenToCookie(HttpContext context, Token token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = token.Expiration
            };

            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = token.RefreshTokenExpiration
            };

            context.Response.Cookies.Append("AccessToken", token.AccessToken, cookieOptions);
            context.Response.Cookies.Append("RefreshToken", token.RefreshToken, refreshCookieOptions);
        }
    }
}