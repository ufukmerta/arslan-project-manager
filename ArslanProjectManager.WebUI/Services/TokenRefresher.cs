using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.JsonWebTokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ArslanProjectManager.WebUI.Services
{
    public class TokenRefresher(IAuthStorage authStorage, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor) : ITokenRefresher
    {
        public async Task<string?> EnsureValidAccessTokenAsync()
        {
            var accessToken = await authStorage.GetAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                var handler = new JsonWebTokenHandler();
                var jwt = handler.ReadJsonWebToken(accessToken);
                if (jwt.ValidTo > DateTime.UtcNow)
                    return accessToken;
            }

            // Token expired, try refreshing it
            var refreshToken = await authStorage.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refreshToken))
                return null; // No refresh token available. User needs to log in again.

            var client = httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            client.DefaultRequestHeaders.Add("SkipTokenRefresher", "true");

            var response = await client.PostAsJsonAsync("auth/refresh-token", new { RefreshToken = refreshToken });

            if (!response.IsSuccessStatusCode)
            {
                await authStorage.ClearTokensAsync();
                return null;
            }

            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<TokenDto>>();
            if (wrapper is null || !wrapper.IsSuccess || wrapper.Data is null)
            {
                await authStorage.ClearTokensAsync();
                return null;
            }

            var tokenDto = wrapper.Data;
            await authStorage.SaveTokensAsync(tokenDto.AccessToken, tokenDto.RefreshToken, tokenDto.Expiration, tokenDto.RefreshTokenExpiration);

            // Re-establish the authentication principal with the new token
            await UpdateAuthenticationPrincipalAsync(tokenDto.AccessToken);

            return tokenDto.AccessToken;
        }

        /// <summary>
        /// Updates the authentication principal with claims from the new access token.
        /// This ensures the user remains authenticated after token refresh.
        /// </summary>
        private async Task UpdateAuthenticationPrincipalAsync(string accessToken)
        {
            try
            {
                var httpContext = httpContextAccessor.HttpContext;
                if (httpContext == null)
                    return; // No HttpContext available, skip re-authentication

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(accessToken);
                var claims = jwtToken.Claims;

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }
            catch
            {
            }
        }
    }
}