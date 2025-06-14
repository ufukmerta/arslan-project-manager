using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs.UpdateDTOs
{
    public class TaskCommentUpdateDto: BaseUpdateDto
    {
        public string Comment { get; set; } = default!;        
    }
}
