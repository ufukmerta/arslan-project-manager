﻿using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArslanProjectManager.Core.DTOs.CreateDTOs;

public class ProjectCreateDto : BaseCreateDto
{
    public string ProjectName { get; set; } = null!;
    public int TeamId { get; set; }
    public string? ProjectDetail { get; set; }
    public DateOnly StartDate { get; set; }    
}
