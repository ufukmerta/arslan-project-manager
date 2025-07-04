using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.DTOs.CreateDTOs;

public class ProjectTaskCreateDto : BaseCreateDto
{
    public int ProjectId { get; set; }
    public string TaskName { get; set; } = null!;
    public DateOnly? StartDate { get; set; }
    public DateOnly? ExpectedEndDate { get; set; }
    public int TaskCategoryId { get; set; }
    public int BoardId { get; set; }
    public string? Description { get; set; }
    public int AppointeeId { get; set; }
    public int AppointerId { get; set; }
    public ProjectTask.TaskPriority Priority { get; set; } = ProjectTask.TaskPriority.Medium;
}
