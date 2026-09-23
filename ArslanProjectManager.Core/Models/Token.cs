using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.Models
{
    public class Token:BaseEntity
    {
        /// <summary>
        /// Signed JWT access token. Not persisted: access tokens are stateless and validated via
        /// JWT signature + Redis jti revocation + SecurityStamp comparison (see JwtBearer OnTokenValidated).
        /// This property only carries the token in-memory for the current request/response (cookies, response DTOs).
        /// </summary>
        [NotMapped]
        public string AccessToken { get; set; } = null!;

        /// <summary>
        /// Access token expiration. Not persisted for the same reason as <see cref="AccessToken"/>.
        /// </summary>
        [NotMapped]
        public DateTime Expiration { get; set; }
        public string RefreshToken { get; set; } = null!;
        public DateTime RefreshTokenExpiration { get; set; }
        public int UserId { get; set; }

        public User User { get; set; } = null!;
    }
}
