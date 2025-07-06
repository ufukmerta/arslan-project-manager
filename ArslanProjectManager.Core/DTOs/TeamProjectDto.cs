using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs
{
    public class TeamProjectDto:BaseDto
    {
        public string ProjectName { get; set; } = null!;
        public string? Description { get; set; }
        public int TaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
    }
}
