using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs.UpdateDTOs
{
    public class RoleUpdateDto : BaseUpdateDto
    {
        public string RoleName { get; set; } = default!;
        public bool? ViewPermission { get; set; } = null;
        public bool? EditPermission { get; set; } = null;
    }
}
