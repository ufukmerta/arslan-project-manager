using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs
{
    public class MiniUserDto: BaseDto
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public byte[]? ProfilePicture { get; set; }
    }
}
