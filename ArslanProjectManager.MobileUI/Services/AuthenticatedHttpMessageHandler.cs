using ArslanProjectManager.Core.Services;
using System.Net.Http.Headers;

namespace ArslanProjectManager.MobileUI.Services
{
    public partial class AuthenticatedHttpMessageHandler(ITokenRefresher tokenRefresher) : DelegatingHandler
    {        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Contains("SkipTokenRefresher"))
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var token = await tokenRefresher.EnsureValidAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}

