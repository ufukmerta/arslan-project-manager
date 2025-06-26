using ArslanProjectManager.Core.Services;
using ArslanProjectManager.MobileUI.Services;
using Microsoft.Extensions.Logging;


namespace ArslanProjectManager.MobileUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<IAuthStorage, MobileAuthStorage>();
            builder.Services.AddSingleton<ITokenRefresher, TokenRefresher>();
            builder.Services.AddTransient<AuthenticatedHttpMessageHandler>();

            builder.Services.AddHttpClient("ArslanProjectManagerAPI", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ArslanProjectManagerAPI:BaseUrl"]!);
            })
            .AddHttpMessageHandler<AuthenticatedHttpMessageHandler>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
