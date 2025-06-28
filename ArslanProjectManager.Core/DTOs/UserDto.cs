using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.DTOs;

public class UserDto : BaseDto
{
    public string Name { get; set; } = null!;    
    public string Email { get; set; } = null!;
    public string? ProfilePicture { get; set; }

    public virtual ICollection<TeamInviteDto> TeamInvites { get; set; } = [];
    public virtual ICollection<TeamUserDto> TeamUsers { get; set; } = [];
    public virtual ICollection<TeamDto> Teams { get; set; } = [];
}