using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.DTOs;

public class TeamDto : BaseDto
{
    public string TeamName { get; set; } = null!;
    public string? Description { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int ProjectCount { get; set; }
}