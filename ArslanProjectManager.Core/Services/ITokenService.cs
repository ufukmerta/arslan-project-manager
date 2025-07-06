using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.Services
{
    public interface ITokenService : IGenericService<Token>
    {
        Task<Token?> GetValidTokenByAccessTokenAsync(string accessToken);
        Task<Token?> GetValidTokenByRefreshTokenAsync(string refreshToken);
        Task<List<Token>> GetActiveTokensForUserAsync(int userId);
        Task RevokeTokensForUserAsync(int userId);
    }
}
