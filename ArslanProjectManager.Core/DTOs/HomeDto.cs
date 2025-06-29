using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs
{
    public class HomeDto
    {
        public int TotalProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalTeams { get; set; }
        public int TotalMembers { get; set; }

        public List<RecentTaskDto> RecentTasks { get; set; } = [];
        public List<RecentProjectDto> RecentProjects { get; set; } = [];
    }
}
