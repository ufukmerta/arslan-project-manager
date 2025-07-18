﻿using ArslanProjectManager.Core.Services;
using ArslanProjectManager.MobileUI.Services;
using ArslanProjectManager.Service.Mappings;
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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
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

            

            return builder.Build();
        }
    }
}
