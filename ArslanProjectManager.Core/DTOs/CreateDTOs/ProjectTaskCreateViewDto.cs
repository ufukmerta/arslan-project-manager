using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs.CreateDTOs
{
    public class ProjectTaskCreateViewDto
    {
        public int ProjectId { get; set; }
        public List<TaskUserDto> TeamMembers { get; set; } = new List<TaskUserDto>();
        public List<BoardTagDto> BoardTags { get; set; } = new List<BoardTagDto>();
        public List<TaskCategoryDto> TaskCategories { get; set; } = new List<TaskCategoryDto>();

    }
}
