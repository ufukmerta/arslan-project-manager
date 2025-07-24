using ArslanProjectManager.Core.Services;
using ArslanProjectManager.MobileUI.Services;
using ArslanProjectManager.MobileUI.Services.UIServices;
using ArslanProjectManager.MobileUI.ViewModels;
using ArslanProjectManager.MobileUI.Views;
using ArslanProjectManager.Service.Mappings;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace ArslanProjectManager.MobileUI
{
    public static class MauiProgram
    {
        public const string BaseAddress = "https://192.168.1.5:7069";
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Font-Awesome-6-Free-Solid-900.otf", "FA");
                });

            // Services
            builder.Services.AddSingleton<IAuthStorage, MobileAuthStorage>();
            builder.Services.AddSingleton<ITokenRefresher, TokenRefresher>();
            builder.Services.AddTransient<AuthenticatedHttpMessageHandler>();

            builder.Services.AddAutoMapper(typeof(MapProfile));
#if DEBUG
            builder.Logging.AddDebug();            
            builder.Services.AddHttpClient("ArslanProjectManagerAPI", client =>
            {
                client.BaseAddress = new Uri($"{BaseAddress}/api/");
            })
            //next line is for debugging purposes only, remove in production. Otherwise, it will be security risk.
            //if you want to use self-signed certificates in production, you should implement a proper certificate validation handler.
            .ConfigurePrimaryHttpMessageHandler(() => new IgnoreSslHandler())
            .AddHttpMessageHandler<AuthenticatedHttpMessageHandler>();
#elif RELEASE
            builder.Services.AddHttpClient("ArslanProjectManagerAPI", client =>
            {
                client.BaseAddress = new Uri($"{BaseAddress}/api/");
            })
            .AddHttpMessageHandler<AuthenticatedHttpMessageHandler>();
#endif

            // UI Services
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<HomeService>();

            // ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<HomeViewModel>();

            // Views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<HomePage>();

            return builder.Build();
        }
    }
}
