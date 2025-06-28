using System;
using System.Collections.Generic;

namespace ArslanProjectManager.Core.Models;

public partial class User : BaseEntity
{
    public string Name { get; set; } = null!;    
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public byte[]? ProfilePicture { get; set; }

    public virtual ICollection<TeamInvite> TeamInvites { get; set; } = [];
    public virtual ICollection<TeamUser> TeamUsers { get; set; } = [];
    public virtual ICollection<Team> Teams { get; set; } = [];
    public virtual ICollection<Token> Tokens { get; set; } = [];
}
