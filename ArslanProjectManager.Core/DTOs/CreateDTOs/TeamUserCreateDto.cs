using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArslanProjectManager.Core;

public class TeamUserCreateDto : BaseCreateDto
{
    public int TeamId { get; set; }
    public int UserId { get; set; }
    public int RoleId { get; set; }
    
    public virtual Role Role { get; set; } = null!;
    public virtual Team Team { get; set; } = null!;
    public virtual User User { get; set; } = null!;    
}