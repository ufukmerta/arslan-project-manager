using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.Repositories
{
    public interface ITokenRepository : IGenericRepository<Token>
    {
        Task<Token?> GetByRefreshTokenAsync(string refreshToken);
        Task<List<Token>> GetActiveTokensByUserIdAsync(int userId);
    }
}
