using ArslanProjectManager.Core.Services;
using StackExchange.Redis;

namespace ArslanProjectManager.Service.Services
{
    public class RedisService(IConnectionMultiplexer multiplexer) : IRedisService
    {
        private readonly IDatabase db = multiplexer.GetDatabase();

        public async Task<bool> IsJtiRevokedAsync(string jti)
        {
            if (string.IsNullOrEmpty(jti)) 
                return false;

            return await db.KeyExistsAsync(GetJtiKey(jti));
        }

        public async Task RevokeJtiAsync(string jti, TimeSpan ttl)
        {
            if (string.IsNullOrEmpty(jti)) 
                return;

            await db.StringSetAsync(GetJtiKey(jti), "1", ttl);
        }

        public async Task<string?> GetCachedSecurityStampAsync(int userId)
        {
            var val = await db.StringGetAsync(GetSecurityStampKey(userId));
            return val.HasValue ? (string?)val.ToString() : null;
        }

        public async Task SetCachedSecurityStampAsync(int userId, string stamp, TimeSpan ttl)
        {
            if (stamp == null) 
                return;

            await db.StringSetAsync(GetSecurityStampKey(userId), stamp, ttl);
        }

        public async Task InvalidateSecurityStampCacheAsync(int userId)
        {
            await db.KeyDeleteAsync(GetSecurityStampKey(userId));
        }

        private static string GetJtiKey(string jti) => $"revoked:jti:{jti}";
        private static string GetSecurityStampKey(int userId) => $"securitystamp:{userId}";
    }
}
