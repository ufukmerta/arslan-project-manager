using ArslanProjectManager.Core.Services;

namespace ArslanProjectManager.MobileUI.Services
{
    public class MobileAuthStorage : IAuthStorage
    {
        public async Task<string?> GetAccessTokenAsync() =>
            await SecureStorage.Default.GetAsync("AccessToken");

        public async Task<string?> GetRefreshTokenAsync() =>
            await SecureStorage.Default.GetAsync("RefreshToken");

        public async Task SaveTokensAsync(string accessToken, string refreshToken, DateTime accessExpiration, DateTime refreshExpiration)
        {
            await SecureStorage.Default.SetAsync("AccessToken", accessToken);
            await SecureStorage.Default.SetAsync("RefreshToken", refreshToken);
            await SecureStorage.Default.SetAsync("AccessTokenExpiration", accessExpiration.ToString("o"));
            await SecureStorage.Default.SetAsync("RefreshTokenExpiration", refreshExpiration.ToString("o"));
        }

        public static async Task<string?> GetAccessTokenExpirationAsync() =>
            await SecureStorage.Default.GetAsync("AccessTokenExpiration");

        public static async Task<string?> GetRefreshTokenExpirationAsync() =>
            await SecureStorage.Default.GetAsync("RefreshTokenExpiration");

        public Task ClearTokensAsync()
        {
            SecureStorage.Default.Remove("AccessToken");
            SecureStorage.Default.Remove("RefreshToken");
            SecureStorage.Default.Remove("AccessTokenExpiration");
            SecureStorage.Default.Remove("RefreshTokenExpiration");
            return Task.CompletedTask;
        }
    }
}
