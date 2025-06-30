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
        private readonly IConfiguration Configuration = configuration;

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
            var securityKey = Configuration["Jwt:SecurityKey"];
            if (string.IsNullOrEmpty(securityKey))
            {
                throw new InvalidOperationException("Security key is not configured");
            }

            SymmetricSecurityKey symmetricSecurityKey = new(Encoding.UTF8.GetBytes(securityKey));
            SigningCredentials signingCredentials = new(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var claims = SetClaims(user, roles).ToList();
            
            token.Expiration = DateTime.Now.AddHours(1);
            token.RefreshTokenExpiration = DateTime.Now.AddDays(7);
            token.UserId = user.Id;
            token.User = user;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = Configuration["Jwt:Issuer"],
                Audience = Configuration["Jwt:Audience"],
                Expires = token.Expiration,
                NotBefore = DateTime.Now,
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
            claims.AddRoles(roles.Select(r => r.RoleName).ToList());
            return claims;
        }
    }
}
