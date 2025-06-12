using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.Models
{
    public class Token:BaseEntity
    {
        public string AccessToken { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public string RefreshToken { get; set; } = null!;
        public DateTime RefreshTokenExpiration { get; set; }
        public int UserId { get; set; }

        public User User { get; set; } = null!;
    }
}
