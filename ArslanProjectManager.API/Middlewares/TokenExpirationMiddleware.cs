using ArslanProjectManager.API.Utilities;
using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ArslanProjectManager.API.Middlewares
{
    /// <summary>
    /// Middleware that silently refreshes expired access tokens for cookie-based sessions.
    /// Runs before authentication; real access-token validation (JWT signature, jti revocation,
    /// SecurityStamp match) happens later in the JwtBearer <c>OnTokenValidated</c> event.
    /// </summary>
    /// <remarks>
    /// <para><b>Token source</b></para>
    /// <list type="bullet">
    /// <item><b>Authorization header (Bearer)</b>: No action taken here; the request proceeds and is
    /// validated entirely by the JwtBearer authentication pipeline (stateless).</item>
    /// <item><b>AccessToken cookie</b>: The token's <c>exp</c> is inspected (without signature verification,
    /// used only to decide whether a silent refresh is needed — not as a trust boundary). If still valid,
    /// the request proceeds unchanged. If expired, the RefreshToken cookie is used to look up the refresh
    /// token in the database and, if valid, mint and persist a new access/refresh token pair, update cookies
    /// and the Authorization header for the current request, then proceed.</item>
    /// </list>
    /// <para>When no access token is present, the request proceeds to the next middleware unchanged.</para>
    /// <para><paramref name="next"/> is the next delegate in the pipeline; <paramref name="tokenService"/> for refresh-token persistence and lookup; <paramref name="tokenHandler"/> for creating new JWTs.</para>
    /// </remarks>
    public class TokenExpirationMiddleware(RequestDelegate next, ITokenService tokenService, ITokenHandler tokenHandler)
    {
        /// <summary>
        /// For cookie-based requests, silently refreshes the access token if expired using the refresh token.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <returns>A task that completes when the pipeline has been invoked or a 401 response has been sent.</returns>
        /// <remarks>
        /// Resolves the access token from the Authorization header (Bearer) first, then from the AccessToken cookie.
        /// Refresh token is read only from the RefreshToken cookie. On refresh failure, returns 401
        /// with a JSON body containing a "message" field; otherwise calls the next middleware.
        /// </remarks>
        public async Task InvokeAsync(HttpContext context)
        {
            // Resolve access token: Authorization header first, then cookie
            var authHeader = context.Request.Headers.Authorization.ToString();
            var accessTokenFromHeader = authHeader?.StartsWith("Bearer ") == true ? authHeader["Bearer ".Length..].Trim() : null;

            if (!string.IsNullOrEmpty(accessTokenFromHeader))
            {
                await next(context);
                return;
            }

            string? accessToken, refreshToken;
            accessToken = AuthCookieHelper.GetAccessToken(context.Request);

            if (string.IsNullOrWhiteSpace(accessToken) && string.IsNullOrEmpty(accessTokenFromHeader))
            {
                await next(context);
                return;
            }

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                var jsonWebTokenHandler = new JsonWebTokenHandler();
                var jwtToken = jsonWebTokenHandler.ReadToken(accessToken);

                if (jwtToken.ValidTo > DateTime.UtcNow)
                {
                    // Token is still valid, proceed with the request
                    await next(context);
                    return;
                }

                refreshToken = AuthCookieHelper.GetRefreshToken(context.Request);
                // Access token is expired, try to refresh using refresh token
                if (string.IsNullOrEmpty(refreshToken))
                {
                    // No refresh token available, clear cookies and return unauthorized
                    AuthCookieHelper.ClearAuthCookies(context.Response);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = ErrorMessages.RefreshTokenMissing });
                    return;
                }

                // Try to get a valid token using the refresh token
                var existingTokenByRefresh = await tokenService.GetValidTokenByRefreshTokenAsync(refreshToken);
                if (existingTokenByRefresh == null)
                {
                    // Invalid or expired refresh token
                    AuthCookieHelper.ClearAuthCookies(context.Response);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = ErrorMessages.RefreshTokenMissing });
                    return;
                }

                if (existingTokenByRefresh.RefreshTokenExpiration < DateTime.UtcNow)
                {
                    // Refresh token expired: marked inactive with GetValidTokenByRefreshTokenAsync() above,
                    // but just in case, clear cookies and return unauthorized
                    AuthCookieHelper.ClearAuthCookies(context.Response);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = ErrorMessages.RefreshTokenExpired });
                    return;
                }

                // Create new token
                var newToken = tokenHandler.CreateToken(existingTokenByRefresh.User, []);

                // Save new token to database
                newToken.RefreshToken = existingTokenByRefresh.RefreshToken; // Keep the same refresh token
                newToken.RefreshTokenExpiration = existingTokenByRefresh.RefreshTokenExpiration;
                await tokenService.AddAsync(newToken);

                // Mark old token as inactive
                tokenService.ChangeStatus(existingTokenByRefresh);

                // Set new cookies

                AuthCookieHelper.SetAuthCookies(context.Response, newToken);

                // Update the Authorization header with the new token
                context.Request.Headers.Authorization = $"Bearer {newToken.AccessToken}";
                await next(context);
            }
        }
    }
}