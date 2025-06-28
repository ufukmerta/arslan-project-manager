using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs.UpdateDTOs
{
    public class UserUpdateDto : BaseUpdateDto
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Password { get; set; }
        public string? NewPassword { get; set; }
        public string? ProfilePicture { get; set; }
    }
}
