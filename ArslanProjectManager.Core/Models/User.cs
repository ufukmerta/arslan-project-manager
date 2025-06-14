using System;
using System.Collections.Generic;

namespace ArslanProjectManager.Core.Models;

public partial class User : BaseEntity
{
    public string Name { get; set; } = null!;    
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? ProfilePictureUrl { get; set; } = "profile.png";

    public virtual ICollection<TeamInvite>? TeamInvites { get; set; } = new List<TeamInvite>();
    public virtual ICollection<TeamUser>? TeamUsers { get; set; } = new List<TeamUser>();
    public virtual ICollection<Team>? Teams { get; set; } = new List<Team>();
    public virtual ICollection<Token>? Tokens { get; set; } = new List<Token>();
}
