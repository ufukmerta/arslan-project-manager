using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArslanProjectManager.Core;

public class TaskCategoryCreateDto : BaseCreateDto
{
    public string Category { get; set; } = null!;
        
}