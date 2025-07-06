using System;
using System.Linq;
using System.Collections.Generic;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.DTOs;

public class TeamUserDto : BaseDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
}