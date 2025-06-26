using ArslanProjectManager.Core.Services;
using System.Net.Http.Headers;

namespace ArslanProjectManager.WebUI.Services
{
    public class AuthenticatedHttpClientHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenRefresher _tokenRefresher;

        public AuthenticatedHttpClientHandler(IHttpContextAccessor httpContextAccessor, ITokenRefresher tokenRefresher)
        {
            _httpContextAccessor = httpContextAccessor;
            _tokenRefresher = tokenRefresher;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Check if the request has a header to skip token refresher. Request that will refresh the token should not trigger refresh the token again.
            if (request.Headers.Contains("SkipTokenRefresher"))
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var accessToken = await _tokenRefresher.EnsureValidAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }

}
