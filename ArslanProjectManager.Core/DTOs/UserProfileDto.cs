using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs
{
    public class UserProfileDto
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime RegisterDate { get; set; }
        public bool OwnProfile { get; set; }

        // Project Status Information
        public int TotalProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int ProjectCompletionRate => TotalProjects == 0 ? 0 : (CompletedProjects * 100 / TotalProjects);

        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int TaskCompletionRate => TotalTasks == 0 ? 0 : (CompletedTasks * 100 / TotalTasks);

        public string? CurrentTeam { get; set; }
        public string? Role { get; set; } = "Team Member";
    }
}
