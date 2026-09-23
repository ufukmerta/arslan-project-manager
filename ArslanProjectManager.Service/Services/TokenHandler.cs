using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Service.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ArslanProjectManager.Service.Services
{
    public class TokenHandler(IConfiguration configuration) : ITokenHandler
    {
        public string CreateRefreshToken()
        {
            byte[] randomNumber = new byte[64];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public Token CreateToken(User user, List<Role> roles)
        {
            Token token = new();
            var securityKey = configuration["Jwt:SecurityKey"];
            if (string.IsNullOrEmpty(securityKey))
            {
                throw new InvalidOperationException("Security key is not configured");
            }

            SymmetricSecurityKey symmetricSecurityKey = new(Encoding.UTF8.GetBytes(securityKey));
            SigningCredentials signingCredentials = new(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var claims = SetClaims(user, roles).ToList();

            // Add jti and security stamp for revocation and stamp validation
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim("security_stamp", user.SecurityStamp));

            // Use UTC for token lifetime calculations

            var tokenExpire = configuration["Jwt:ExpirationInMinutes"];
            if (!int.TryParse(tokenExpire, out int expirationInMinutes))
            {
                expirationInMinutes = 60; // Default to 60 minutes if not configured
            }

            var refreshTokenExpire = configuration["RefreshToken:ExpirationInDays"];
            if (!int.TryParse(refreshTokenExpire, out int refreshTokenExpirationInDays))
            {
                refreshTokenExpirationInDays = 7; // Default to 7 days if not configured
            }

            token.Expiration = DateTime.UtcNow.AddMinutes(expirationInMinutes);
            token.RefreshTokenExpiration = DateTime.UtcNow.AddDays(refreshTokenExpirationInDays);
            token.UserId = user.Id;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = configuration["Jwt:Issuer"],
                Audience = configuration["Jwt:Audience"],
                Expires = token.Expiration,
                NotBefore = DateTime.UtcNow,
                SigningCredentials = signingCredentials,
                Subject = new ClaimsIdentity(claims)
            };

            var jsonWebTokenHandler = new JsonWebTokenHandler();
            token.AccessToken = jsonWebTokenHandler.CreateToken(tokenDescriptor);
            token.RefreshToken = CreateRefreshToken();

            // Validate the token can be read back
            try
            {
                var validatedToken = jsonWebTokenHandler.ReadToken(token.AccessToken) ?? throw new SecurityTokenException("Token validation failed");
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException($"Token creation failed validation: {ex.Message}");
            }

            return token;
        }
        public IEnumerable<Claim> SetClaims(User user, List<Role> roles)
        {
            List<Claim> claims =
            [
                new Claim("sub", user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            ];

            claims.AddName(user.Name, user.Email);
            claims.AddRoles([.. roles.Select(r => r.RoleName)]);
            return claims;
        }
    }
}
