using ArslanProjectManager.Core.Services;
using ArslanProjectManager.MobileUI.Services;
using System.Globalization;

namespace ArslanProjectManager.MobileUI
{
    public partial class App : Application
    {
        private readonly ITokenRefresher _tokenRefresher;
        private readonly IAuthStorage _authStorage;
        public App(ITokenRefresher tokenRefresher, IAuthStorage authStorage)
        {
            InitializeComponent();
            _tokenRefresher = tokenRefresher;
            _authStorage = authStorage;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = new AppShell();
            var window = new Window(shell);
            
            shell.Dispatcher.Dispatch(async () =>
            {
                var accessToken = await SecureStorage.Default.GetAsync("AccessToken");
                var expirationStr = await SecureStorage.Default.GetAsync("AccessTokenExpiration");
                if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(expirationStr)
                    && DateTime.TryParse(expirationStr, null, DateTimeStyles.RoundtripKind, out var expiration)
                    && expiration > DateTime.UtcNow)
                {
                    await Shell.Current.GoToAsync("//home");
                }
                else
                {
                    var refreshToken = await SecureStorage.Default.GetAsync("RefreshToken");
                    var refreshExpirationStr = await SecureStorage.Default.GetAsync("RefreshTokenExpiration");
                    if (!string.IsNullOrEmpty(refreshToken) && !string.IsNullOrEmpty(refreshExpirationStr)
                        && DateTime.TryParse(refreshExpirationStr, null, DateTimeStyles.RoundtripKind, out var refreshExpiration)
                        && refreshExpiration > DateTime.UtcNow)
                    {
                        var newAccessToken = await _tokenRefresher.EnsureValidAccessTokenAsync();
                        if (!string.IsNullOrEmpty(newAccessToken))
                        {
                            await Shell.Current.GoToAsync("//home");
                        }
                    }                    
                }
                await _authStorage.ClearTokensAsync();
                await Shell.Current.GoToAsync("//login");
            });

            return window;
        }
    }
}