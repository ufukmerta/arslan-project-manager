using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs.UpdateDTOs
{
    public class ProjectUpdateDto: BaseUpdateDto
    {
        public string ProjectName { get; set; } = default!;
        public string? ProjectDetail { get; set; } = null;
    }
}
