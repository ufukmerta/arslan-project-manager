using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs
{
    public class ProjectDetailsDto
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? ProjectDetail { get; set; }
        public DateOnly StartDate { get; set; }

        public List<MiniProjectTaskDto> Tasks { get; set; } = [];
    }
}
