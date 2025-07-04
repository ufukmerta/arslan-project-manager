using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs.DeleteDTOs
{
    public class ProjectTaskDeleteDto: BaseDeleteDto
    {
        public string TaskName { get; set; }= string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BoardName {  get; set; } = string.Empty;
        public string TaskCategoryName { get; set; } = string.Empty;
        public string AppointeeName { get; set;} = string.Empty;
        public string AppointerName { get;set; } = string.Empty;
        public ProjectTask.TaskPriority Priority { get; set; }
    }
}
