using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArslanProjectManager.Core.DTOs.CreateDTOs;

public class UserCreateDto : BaseCreateDto
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? ProfilePicture { get; set; }
    public string Password { get; set; } = null!;
}
