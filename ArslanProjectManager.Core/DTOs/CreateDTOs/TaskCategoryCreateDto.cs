using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArslanProjectManager.Core.DTOs.CreateDTOs;

public class TaskCategoryCreateDto : BaseCreateDto
{
    public string Category { get; set; } = null!;
        
}