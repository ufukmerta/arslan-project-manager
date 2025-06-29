using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs.UpdateDTOs
{
    public class UserPasswordUpdateDto : BaseUpdateDto
    {
        public string? Password { get; set; }
        public string? NewPassword { get; set; }
    }
}
