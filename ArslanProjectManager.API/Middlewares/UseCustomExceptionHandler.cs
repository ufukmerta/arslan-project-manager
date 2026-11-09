using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Service.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace ArslanProjectManager.API.Middlewares
{
    public static class UseCustomExceptionHandler
    {
        public static void UseCustomException(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(configure =>
            {
               configure.Run(async context =>
                {
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();                    
                    var statusCode = exceptionFeature?.Error switch
                    {
                        ClientSideException => 400,
                        NotFoundException => 404,
                        _ => 500
                    };
                    if (statusCode == 500 && exceptionFeature?.Error != null)
                    {
                        var logger = context.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("UseCustomExceptionHandler");
                        logger?.LogError(exceptionFeature.Error, "An unhandled server exception occurred.");
                    }
                    context.Response.StatusCode = statusCode;
                    var errorMessage = statusCode >= 500
                        ? ErrorMessages.ProcessingError
                        : (exceptionFeature?.Error.Message ?? "An unexpected error occurred.");
                    var response = CustomResponseDto<NoContentDto>.Fail(statusCode, errorMessage);
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                });
            });
        }
    }
}
