using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArslanProjectManager.Core.DTOs.DeleteDTOs;

public class ProjectDeleteDto : BaseDeleteDto
{
    public string ProjectName { get; set; } = null!;
    public string TeamName { get; set; } = null!;
    public int TaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public string? ProjectDetail { get; set; }
    public DateOnly StartDate { get; set; }    
}
