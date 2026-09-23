using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Services;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Net.Http.Json;

namespace ArslanProjectManager.MobileUI.Services
{
    public class TokenRefresher(IAuthStorage authStorage, IHttpClientFactory httpClientFactory) : ITokenRefresher
    {
        public async Task<string?> EnsureValidAccessTokenAsync()
        {
            var accessToken = await authStorage.GetAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                var jwt = new JsonWebTokenHandler().ReadJsonWebToken(accessToken);
                if (jwt.ValidTo > DateTime.UtcNow)
                    return accessToken;
            }

            var refreshToken = await authStorage.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                await authStorage.ClearTokensAsync();
                return null;
            }

            // Check if refresh token is already expired before making API call
            var refreshTokenExpirationStr = await MobileAuthStorage.GetRefreshTokenExpirationAsync();
            if (!string.IsNullOrWhiteSpace(refreshTokenExpirationStr) && 
                DateTime.TryParseExact(refreshTokenExpirationStr, "o", null, System.Globalization.DateTimeStyles.RoundtripKind, out var refreshTokenExpiration))
            {
                if (refreshTokenExpiration <= DateTime.UtcNow)
                {
                    await authStorage.ClearTokensAsync();
                    return null;
                }
            }

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

            var dto = wrapper.Data;
            await authStorage.SaveTokensAsync(dto.AccessToken, dto.RefreshToken, dto.Expiration, dto.RefreshTokenExpiration);
            return dto.AccessToken;
        }
    }
}

