using System;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.Services
{
    public interface IRedisService
    {
        Task<bool> IsJtiRevokedAsync(string jti);
        Task RevokeJtiAsync(string jti, TimeSpan ttl);

        Task<string?> GetCachedSecurityStampAsync(int userId);
        Task SetCachedSecurityStampAsync(int userId, string stamp, TimeSpan ttl);
        Task InvalidateSecurityStampCacheAsync(int userId);
    }
}
