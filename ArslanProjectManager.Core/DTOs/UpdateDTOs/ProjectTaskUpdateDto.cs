using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs.UpdateDTOs
{
    public class ProjectTaskUpdateDto : BaseUpdateDto
    {
        public string TaskName { get; set; } = default!;
        public string? Description { get; set; } = null;
        public DateTime CreatedDate { get; set; }
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public DateTime? ExpectedEndDate { get; set; } = null;        
        public ProjectTask.TaskPriority Priority { get; set; }
        public int? TaskCategoryId { get; set; } = null;
        public int? AppointeeId { get; set; } = null;
    }
}
