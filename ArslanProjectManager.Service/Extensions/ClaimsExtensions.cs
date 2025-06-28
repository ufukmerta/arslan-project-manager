using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Service.Extensions
{
    public static class ClaimsExtensions
    {
        public static void AddName(this ICollection<Claim> claims, string name, string email)
        {
            if (!string.IsNullOrEmpty(name))
            {
                claims.Add(new Claim(ClaimTypes.Name, name));
            }

            if (!string.IsNullOrEmpty(email))
            {
                claims.Add(new Claim(ClaimTypes.Email, email));
            }
        }
        public static void AddRoles(this ICollection<Claim> claims, IEnumerable<string> roles)
        {
            if (roles == null || !roles.Any()) return;
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }
    }
}
