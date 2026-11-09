using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Repository.Repositories
{
    public class TokenRepository(ProjectManagerDbContext context) : GenericRepository<Token>(context), ITokenRepository
    {
        public async Task<Token?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _dbSet
                .Include(t => t.User)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync(t => t.RefreshToken == refreshToken);
        }

        public async Task<List<Token>> GetActiveTokensByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }
    }
}
