using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArslanProjectManager.Core.DTOs.CreateDTOs;

public class BoardTagCreateDto : BaseCreateDto
{
    public string BoardName { get; set; } = null!;

    public byte BoardOrder { get; set; }    
}
