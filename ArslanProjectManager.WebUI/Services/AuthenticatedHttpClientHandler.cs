using ArslanProjectManager.Core.Services;
using System.Net.Http.Headers;

namespace ArslanProjectManager.WebUI.Services
{
    public class AuthenticatedHttpClientHandler(ITokenRefresher tokenRefresher) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Check if the request has a header to skip token refresher. Request that will refresh the token should not trigger refresh the token again.
            if (request.Headers.Contains("SkipTokenRefresher"))
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var accessToken = await tokenRefresher.EnsureValidAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }

}
