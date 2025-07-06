using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArslanProjectManager.Core.DTOs;

public class TeamDetailsDto : BaseDto
{
    public string TeamName { get; set; } = null!;
    public string? Description { get; set; }
    public int ManagerId { get; set; }
    public string ManagerName { get; set; } = null!;

    public List<TeamProjectDto> Projects { get; set; } = [];
    public List<TeamUserDto> Members { get; set; } = [];
}