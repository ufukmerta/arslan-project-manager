using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ArslanProjectManager.Service.Services
{
    public class AuthService(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        ITokenService tokenService,
        ITokenHandler tokenHandler,
        IRedisService redisService,
        IPasswordHasher passwordHasher,
        IMapper mapper) : IAuthService
    {
        public async Task<Token?> LoginAsync(UserLoginDto userLoginDto)
        {
            var user = await userRepository.Where(u => u.Email == userLoginDto.Email)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }

            if (!passwordHasher.VerifyPassword(userLoginDto.Password, user.Password))
            {
                return null;
            }

            return tokenHandler.CreateToken(user, []);
        }

        public async Task<CustomResponseDto<TokenDto>> LoginAndCreateSessionAsync(UserLoginDto dto)
        {
            var token = await LoginAsync(dto);
            if (token is null)
            {
                return CustomResponseDto<TokenDto>.Fail(404, ErrorMessages.InvalidCredentials);
            }

            var handler = new JsonWebTokenHandler();
            if (!handler.CanReadToken(token.AccessToken))
            {
                return CustomResponseDto<TokenDto>.Fail(500, ErrorMessages.TokenGenerationFailed);
            }

            var registeredToken = await tokenService.AddAsync(token);
            var tokenDto = mapper.Map<TokenDto>(registeredToken);
            return CustomResponseDto<TokenDto>.Success(tokenDto, 200);
        }

        public async Task<CustomResponseDto<TokenDto>> RefreshTokenAsync(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return CustomResponseDto<TokenDto>.Fail(401, ErrorMessages.RefreshTokenMissing);
            }

            var token = await tokenService.GetValidTokenByRefreshTokenAsync(refreshToken);
            if (token is null)
            {
                return CustomResponseDto<TokenDto>.Fail(401, ErrorMessages.RefreshTokenMissing);
            }

            if (token.RefreshTokenExpiration < DateTime.UtcNow)
            {
                return CustomResponseDto<TokenDto>.Fail(401, ErrorMessages.RefreshTokenExpired);
            }

            var newToken = tokenHandler.CreateToken(token.User, []);
            if (newToken is null)
            {
                return CustomResponseDto<TokenDto>.Fail(401, ErrorMessages.TokenGenerationFailed);
            }

            newToken.RefreshToken = token.RefreshToken;
            newToken.RefreshTokenExpiration = token.RefreshTokenExpiration;

            var registeredToken = await tokenService.AddAsync(newToken);
            await tokenService.ChangeStatusAsync(token);

            var tokenDto = mapper.Map<TokenDto>(registeredToken);
            return CustomResponseDto<TokenDto>.Success(tokenDto, 200);
        }

        public async Task<CustomResponseDto<NoContentDto>> LogoutAsync(string? jti, string? expClaim, string? refreshToken)
        {
            if (!string.IsNullOrEmpty(jti))
            {
                TimeSpan ttl = TimeSpan.FromMinutes(5);
                if (!string.IsNullOrEmpty(expClaim) && long.TryParse(expClaim, out var expSeconds))
                {
                    var exp = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
                    ttl = exp - DateTime.UtcNow;
                    if (ttl <= TimeSpan.Zero)
                    {
                        ttl = TimeSpan.FromMinutes(1);
                    }
                }

                await redisService.RevokeJtiAsync(jti, ttl);
            }

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var token = await tokenService
                    .Where(t => t.RefreshToken == refreshToken)
                    .FirstOrDefaultAsync();
                if (token is not null)
                {
                    await tokenService.ChangeStatusAsync(token);
                }
            }

            return CustomResponseDto<NoContentDto>.Success(204);
        }

        public async Task<CustomResponseDto<TokenDto?>> LogoutAllAsync(int userId, string? currentRefreshToken)
        {
            if (userId <= 0)
            {
                return CustomResponseDto<TokenDto?>.Fail(401, ErrorMessages.Unauthorized);
            }

            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return CustomResponseDto<TokenDto?>.Fail(404, ErrorMessages.UserNotFound);
            }

            user.SecurityStamp = Guid.NewGuid().ToString();
            userRepository.Update(user);
            await unitOfWork.CommitAsync();

            await redisService.InvalidateSecurityStampCacheAsync(user.Id);
            await tokenService.RevokeTokensForUserAsync(user.Id, currentRefreshToken);

            if (!string.IsNullOrEmpty(currentRefreshToken))
            {
                var currentTokenRow = await tokenService.GetValidTokenByRefreshTokenAsync(currentRefreshToken);
                if (currentTokenRow != null && currentTokenRow.RefreshTokenExpiration >= DateTime.UtcNow)
                {
                    var newToken = tokenHandler.CreateToken(user, []);
                    newToken.RefreshToken = currentTokenRow.RefreshToken;
                    newToken.RefreshTokenExpiration = currentTokenRow.RefreshTokenExpiration;
                    await tokenService.AddAsync(newToken);
                    await tokenService.ChangeStatusAsync(currentTokenRow);

                    var tokenDto = mapper.Map<TokenDto>(newToken);
                    return CustomResponseDto<TokenDto?>.Success(tokenDto, 200);
                }
            }

            return CustomResponseDto<TokenDto?>.Success(null, 204);
        }

        public async Task<CustomResponseDto<UserDto>> RegisterAsync(UserCreateDto userDto)
        {
            var existingUser = await userRepository.AnyAsync(x => x.Email == userDto.Email);
            if (existingUser)
            {
                return CustomResponseDto<UserDto>.Fail(400, ErrorMessages.EmailAlreadyExists);
            }

            var user = mapper.Map<User>(userDto);

            if (!string.IsNullOrEmpty(userDto.ProfilePicture))
            {
                user.ProfilePicture = Convert.FromBase64String(userDto.ProfilePicture);
            }

            user.Password = passwordHasher.HashPassword(user.Password);

            await userRepository.AddAsync(user);
            await unitOfWork.CommitAsync();

            var newUserDto = mapper.Map<UserDto>(user);
            return CustomResponseDto<UserDto>.Success(newUserDto, 201);
        }
    }
}
