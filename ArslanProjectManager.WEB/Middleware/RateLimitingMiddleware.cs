using System.Collections.Concurrent;
using System.Net;

namespace ArslanProjectManager.WEB.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();
        private const int MaxTokens = 10; // Maximum number of requests
        private const int TokensPerRefill = 1; // Tokens added per refill
        private static readonly TimeSpan RefillInterval = TimeSpan.FromMinutes(1); // Refill interval

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only rate limit login and register endpoints
            if (IsAuthEndpoint(context.Request))
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var bucket = _buckets.GetOrAdd(ipAddress, _ => new TokenBucket(MaxTokens, TokensPerRefill, RefillInterval));

                if (!bucket.TryConsume(1))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    await context.Response.WriteAsync("Too many requests. Please try again later.");
                    return;
                }
            }

            await _next(context);
        }

        private bool IsAuthEndpoint(HttpRequest request)
        {
            return request.Path.StartsWithSegments("/User/Login", StringComparison.OrdinalIgnoreCase) ||
                   request.Path.StartsWithSegments("/User/Register", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class TokenBucket
    {
        private readonly int _maxTokens;
        private readonly int _tokensPerRefill;
        private readonly TimeSpan _refillInterval;
        private int _currentTokens;
        private DateTime _lastRefillTime;
        private readonly object _lock = new();

        public TokenBucket(int maxTokens, int tokensPerRefill, TimeSpan refillInterval)
        {
            _maxTokens = maxTokens;
            _tokensPerRefill = tokensPerRefill;
            _refillInterval = refillInterval;
            _currentTokens = maxTokens;
            _lastRefillTime = DateTime.UtcNow;
        }

        public bool TryConsume(int tokens)
        {
            lock (_lock)
            {
                RefillTokens();

                if (_currentTokens >= tokens)
                {
                    _currentTokens -= tokens;
                    return true;
                }

                return false;
            }
        }

        private void RefillTokens()
        {
            var now = DateTime.UtcNow;
            var timePassed = now - _lastRefillTime;
            var intervals = (int)(timePassed.TotalMilliseconds / _refillInterval.TotalMilliseconds);

            if (intervals > 0)
            {
                _currentTokens = Math.Min(_maxTokens, _currentTokens + (_tokensPerRefill * intervals));
                _lastRefillTime = now;
            }
        }
    }

    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
} 