using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs.UpdateDTOs
{
    public class TeamUpdateDto : BaseUpdateDto
    {
        public string TeamName { get; set; } = default!;
    }
}
