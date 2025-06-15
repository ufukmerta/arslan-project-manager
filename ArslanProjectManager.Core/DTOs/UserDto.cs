using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.DTOs;

public class UserDto : BaseDto
{
    public string Name { get; set; } = null!;    
    public string Email { get; set; } = null!;
    public string? ProfilePictureUrl { get; set; } = "profile.png";

    public virtual ICollection<TeamInviteDto> TeamInvites { get; set; } = new List<TeamInviteDto>();
    public virtual ICollection<TeamUserDto> TeamUsers { get; set; } = new List<TeamUserDto>();
    public virtual ICollection<TeamDto> Teams { get; set; } = new List<TeamDto>();
}