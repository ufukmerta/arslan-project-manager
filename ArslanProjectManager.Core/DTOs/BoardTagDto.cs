using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.DTOs;

public class BoardTagDto : BaseDto
{
    public string BoardName { get; set; } = null!;

    public byte BoardOrder { get; set; }
}
