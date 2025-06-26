using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ArslanProjectManager.Core.DTOs;

public class ProjectTaskDto : BaseDto
{
    public string TaskName { get; set; } = string.Empty;
    public DateOnly? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateOnly? ExpectedEndDate { get; set; }
    
    public int TaskCategoryId { get; set; }
    public string TaskCategoryName { get; set; } = string.Empty;
    
    public int BoardId { get; set; }
    public string BoardName { get; set; } = string.Empty;
    
    public string? Description { get; set; }

    public int AppointerId { get; set; }
    public string AppointerName { get; set; } = string.Empty;

    public int AppointeeId { get; set; }
    public string AppointeeName { get; set; } = string.Empty;

    public ProjectTask.TaskPriority Priority { get; set; }
    
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;

    public List<TaskCommentDto> Comments { get; set; } = new();
}
