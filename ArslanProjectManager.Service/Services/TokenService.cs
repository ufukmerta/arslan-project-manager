using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;

namespace ArslanProjectManager.Service.Services
{
    public class TokenService : GenericService<Token>, ITokenService
    {
        private readonly ITokenRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public TokenService(IGenericRepository<Token> repository, ITokenRepository tokenRepository, IUnitOfWork unitOfWork)
            : base(repository, unitOfWork)
        {
            _repository = tokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Token?> GetValidTokenByAccessTokenAsync(string accessToken)
        {
            var token = await _repository.GetByAcessTokenAsync(accessToken);
            if (token == null || !token.IsActive || token.RefreshTokenExpiration <= System.DateTime.UtcNow)
            {
                return null;
            }
            return token;
        }

        public async Task<Token?> GetValidTokenByRefreshTokenAsync(string refreshToken)
        {
            var token = await _repository.GetByRefreshTokenAsync(refreshToken);
            if (token == null || !token.IsActive || token.RefreshTokenExpiration <= System.DateTime.UtcNow)
            {
                return null;
            }
            return token;
        }

        public async Task<List<Token>> GetActiveTokensForUserAsync(int userId)
        {
            return await _repository.GetActiveTokensByUserIdAsync(userId);
        }

        public async Task RevokeTokensForUserAsync(int userId)
        {
            var tokens = await _repository.GetActiveTokensByUserIdAsync(userId);

            foreach (var token in tokens)
            {
                token.IsActive = false;
                _repository.Update(token);
            }
        }
    }
}
