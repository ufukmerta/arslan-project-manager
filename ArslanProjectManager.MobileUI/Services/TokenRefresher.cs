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
                return null;

            var client = httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            client.DefaultRequestHeaders.Add("SkipTokenRefresher", "true");

            var response = await client.PostAsJsonAsync("auth/refresh-token", new { RefreshToken = refreshToken });

            if (!response.IsSuccessStatusCode)
                return null;

            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<TokenDto>>();
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
                return null;

            var dto = wrapper.Data;
            await authStorage.SaveTokensAsync(dto.AccessToken, dto.RefreshToken, dto.Expiration, dto.RefreshTokenExpiration);
            return dto.AccessToken;
        }
    }
}

