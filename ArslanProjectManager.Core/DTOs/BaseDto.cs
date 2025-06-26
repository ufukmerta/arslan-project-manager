using System;
using System.Linq;
using System.Collections.Generic;

namespace ArslanProjectManager.Core.DTOs;

public class BaseDto
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; } = null;
}
