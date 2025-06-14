using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs
{
    internal class UserLogoutDto
    {
        public string RefreshToken { get; set; } = null!;
    }
}
