using ArslanProjectManager.Core.Services;
using System.Net.Http.Headers;

namespace ArslanProjectManager.MobileUI.Services
{
    public class AuthenticatedHttpMessageHandler : DelegatingHandler
    {
        private readonly ITokenRefresher _tokenRefresher;

        public AuthenticatedHttpMessageHandler(ITokenRefresher tokenRefresher)
        {
            _tokenRefresher = tokenRefresher;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Contains("SkipTokenRefresher"))
                return await base.SendAsync(request, cancellationToken);

            var token = await _tokenRefresher.EnsureValidAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}

