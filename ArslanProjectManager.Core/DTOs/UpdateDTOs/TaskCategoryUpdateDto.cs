using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs.UpdateDTOs
{
    public class TaskCategoryUpdateDto : BaseUpdateDto
    {
        public string CategoryName { get; set; } = default!;
        public string? Description { get; set; } = null;
    }
}
