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
        public static void AddName(this ICollection<Claim> claims, string name, string email, string image)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (string.IsNullOrEmpty(email)) return;
            if (string.IsNullOrEmpty(image)) image = "profile.png";
            claims.Add(new Claim(ClaimTypes.Name, name));
            claims.Add(new Claim(ClaimTypes.Email, email));
            claims.Add(new Claim(ClaimTypes.Uri, image));
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
