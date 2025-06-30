using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs
{
    public class UserProjectDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateOnly StartDate { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public int TeamId { get; set; }
        public int ManagerId { get; set; }
        public int TaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
    }
}
