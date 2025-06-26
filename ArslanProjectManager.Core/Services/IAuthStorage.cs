using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.Services
{
    public interface IAuthStorage
    {
        Task<string?> GetAccessTokenAsync();
        Task<string?> GetRefreshTokenAsync();
        Task SaveTokensAsync(string accessToken, string refreshToken, DateTime accessExpiration, DateTime refreshExpiration);
        Task ClearTokensAsync();
    }
}
