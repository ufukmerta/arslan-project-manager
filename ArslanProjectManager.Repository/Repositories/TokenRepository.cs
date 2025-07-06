using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Repository.Repositories
{
    public class TokenRepository : GenericRepository<Token>, ITokenRepository
    {
        private readonly ProjectManagerDbContext _context;

        public TokenRepository(ProjectManagerDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Token?> GetByAcessTokenAsync(string accessToken)
        {
            return await _context.Tokens
                .Include(t => t.User)
                .OrderBy(t => t.Id)
                .LastOrDefaultAsync(t => t.AccessToken == accessToken);
        }

        public async Task<Token?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Tokens
               .Include(t => t.User)
               .OrderBy(t => t.Id)
               .LastOrDefaultAsync(t => t.RefreshToken == refreshToken);
        }

        public async Task<List<Token>> GetActiveTokensByUserIdAsync(int userId)
        {
            return await _context.Tokens
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }
    }
}
