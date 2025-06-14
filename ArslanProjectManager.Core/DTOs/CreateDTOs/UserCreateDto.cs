using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArslanProjectManager.Core;

public class UserCreateDto : BaseCreateDto
{
    public string Name { get; set; } = null!;    
    public string Email { get; set; } = null!;
    public string? ProfilePictureUrl { get; set; } = "profile.png";
    public string Password { get; set; } = null!;    
}
