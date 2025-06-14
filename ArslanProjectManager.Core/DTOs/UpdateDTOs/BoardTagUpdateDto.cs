using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs.UpdateDTOs
{
    public class BoardTagUpdateDto : BaseUpdateDto
    {
        public string BoardName { get; set; } = default!;
        public byte BoardOrder { get; set; }
    }
}
