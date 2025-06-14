using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArslanProjectManager.Core;

public class TaskTagCreateDto : BaseCreateDto
{
    public int TaskId { get; set; }
    public string Tag { get; set; } = null!;
    
}