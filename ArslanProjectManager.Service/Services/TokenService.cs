using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;

namespace ArslanProjectManager.Service.Services
{
    public class TokenService(IGenericRepository<Token> repository, ITokenRepository tokenRepository, IUnitOfWork unitOfWork)
        : GenericService<Token>(repository, unitOfWork), ITokenService
    {
        public async Task<Token?> GetValidTokenByAccessTokenAsync(string accessToken)
        {
            var token = await tokenRepository.GetByAcessTokenAsync(accessToken);
            if (token == null || !token.IsActive || token.RefreshTokenExpiration <= System.DateTime.UtcNow)
            {
                return null;
            }
            return token;
        }

        public async Task<Token?> GetValidTokenByRefreshTokenAsync(string refreshToken)
        {
            var token = await tokenRepository.GetByRefreshTokenAsync(refreshToken);
            if (token == null || !token.IsActive || token.RefreshTokenExpiration <= System.DateTime.UtcNow)
            {
                return null;
            }
            return token;
        }

        public async Task<List<Token>> GetActiveTokensForUserAsync(int userId)
        {
            return await tokenRepository.GetActiveTokensByUserIdAsync(userId);
        }

        public async Task RevokeTokensForUserAsync(int userId)
        {
            var tokens = await tokenRepository.GetActiveTokensByUserIdAsync(userId);

            foreach (var token in tokens)
            {
                token.IsActive = false;
                tokenRepository.Update(token);
            }
        }
    }
}
