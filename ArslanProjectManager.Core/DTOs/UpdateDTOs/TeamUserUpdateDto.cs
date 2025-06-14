using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs.UpdateDTOs
{
    public class TeamUserUpdateDto : BaseUpdateDto
    {
        public int UserId { get; set; }
        public int TeamId { get; set; }
        public string Role { get; set; } = null!;
    }
}
