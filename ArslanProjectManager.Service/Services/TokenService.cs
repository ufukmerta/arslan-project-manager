using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;

namespace ArslanProjectManager.Service.Services
{
    public class TokenService(ITokenRepository tokenRepository, IUnitOfWork unitOfWork)
        : GenericService<Token>(tokenRepository, unitOfWork), ITokenService
    {
        public async Task<Token?> GetValidTokenByRefreshTokenAsync(string refreshToken)
        {
            var token = await tokenRepository.GetByRefreshTokenAsync(refreshToken);

            if (token is null)
            {
                return null;
            }

            if (token.RefreshTokenExpiration < DateTime.UtcNow)
            {
                await ChangeStatusAsync(token);
            }

            return token;
        }

        public async Task<List<Token>> GetActiveTokensForUserAsync(int userId)
        {
            return await tokenRepository.GetActiveTokensByUserIdAsync(userId);
        }

        public async Task RevokeTokensForUserAsync(int userId, string? exceptRefreshToken = null)
        {
            var tokens = await tokenRepository.GetActiveTokensByUserIdAsync(userId);
            var hasChanges = false;

            foreach (var token in tokens)
            {
                if (!string.IsNullOrEmpty(exceptRefreshToken) && token.RefreshToken == exceptRefreshToken)
                {
                    continue;
                }

                token.IsActive = false;
                tokenRepository.ChangeStatus(token);
                hasChanges = true;
            }

            if (hasChanges)
            {
                await UnitOfWork.CommitAsync();
            }
        }
    }
}
