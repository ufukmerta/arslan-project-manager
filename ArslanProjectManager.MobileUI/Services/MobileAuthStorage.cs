using ArslanProjectManager.Core.Services;

namespace ArslanProjectManager.MobileUI.Services
{
    public class MobileAuthStorage : IAuthStorage
    {
        public Task<string?> GetAccessTokenAsync() =>
            SecureStorage.Default.GetAsync("AccessToken");

        public Task<string?> GetRefreshTokenAsync() =>
            SecureStorage.Default.GetAsync("RefreshToken");

        public async Task SaveTokensAsync(string accessToken, string refreshToken, DateTime accessExpiration, DateTime refreshExpiration)
        {
            await SecureStorage.Default.SetAsync("AccessToken", accessToken);
            await SecureStorage.Default.SetAsync("RefreshToken", refreshToken);
        }

        public Task ClearTokensAsync()
        {
            SecureStorage.Default.Remove("AccessToken");
            SecureStorage.Default.Remove("RefreshToken");
            return Task.CompletedTask;
        }
    }
}
