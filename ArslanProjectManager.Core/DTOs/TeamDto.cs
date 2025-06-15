using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.DTOs;

public class TeamDto : BaseDto
{
    public string TeamName { get; set; } = null!;
    public int ManagerId { get; set; }

    public virtual UserDto Manager { get; set; } = null!;
    public virtual ICollection<ProjectDto> Projects { get; set; } = new List<ProjectDto>();
    public virtual ICollection<TeamInviteDto> TeamInvites { get; set; } = new List<TeamInviteDto>();
    public virtual ICollection<TeamUserDto> TeamUsers { get; set; } = new List<TeamUserDto>();
}