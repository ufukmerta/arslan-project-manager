using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Services;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Net.Http.Json;

namespace ArslanProjectManager.MobileUI.Services
{
    public class TokenRefresher : ITokenRefresher
    {
        private readonly IAuthStorage _authStorage;
        private readonly IHttpClientFactory _httpClientFactory;

        public TokenRefresher(IAuthStorage authStorage, IHttpClientFactory httpClientFactory)
        {
            _authStorage = authStorage;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string?> EnsureValidAccessTokenAsync()
        {
            var accessToken = await _authStorage.GetAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                var jwt = new JsonWebTokenHandler().ReadJsonWebToken(accessToken);
                if (jwt.ValidTo > DateTime.UtcNow)
                    return accessToken;
            }

            var refreshToken = await _authStorage.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refreshToken))
                return null;

            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            client.DefaultRequestHeaders.Add("SkipTokenRefresher", "true");

            var response = await client.PostAsJsonAsync("user/refresh-token", new { RefreshToken = refreshToken });

            if (!response.IsSuccessStatusCode)
                return null;

            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<TokenDto>>();
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
                return null;

            var dto = wrapper.Data;
            await _authStorage.SaveTokensAsync(dto.AccessToken, dto.RefreshToken, dto.Expiration, dto.RefreshTokenExpiration);
            return dto.AccessToken;
        }
    }
}

