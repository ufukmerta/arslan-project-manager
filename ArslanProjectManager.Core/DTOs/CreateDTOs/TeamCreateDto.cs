using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArslanProjectManager.Core;

public class TeamCreateDto : BaseCreateDto
{
    public string TeamName { get; set; } = null!;
    public int ManagerId { get; set; }

    public virtual User Manager { get; set; } = null!;    
}