using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Services;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ArslanProjectManager.WebUI.Services
{
    public class TokenRefresher(IAuthStorage authStorage, IHttpClientFactory httpClientFactory) : ITokenRefresher
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
                return null;

            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<TokenDto>>();
            if (wrapper == null || !wrapper.IsSuccess || wrapper.Data == null)
                return null;

            var tokenDto = wrapper.Data;
            if (tokenDto is null)
                return null;

            await authStorage.SaveTokensAsync(tokenDto.AccessToken, tokenDto.RefreshToken, tokenDto.Expiration, tokenDto.RefreshTokenExpiration);
            return tokenDto.AccessToken;
        }
    }

}
