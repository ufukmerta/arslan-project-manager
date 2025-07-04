using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs
{
    public class TaskUserDto
    {
        public required int TeamUserId { get; set; }
        public required string Name { get; set; }
    }
}
