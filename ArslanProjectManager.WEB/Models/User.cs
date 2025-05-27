using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.WEB.Models;

[Table("user")]
[Index("Email", Name = "UQ__user__AB6E61641DE67B62", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Column("surname")]
    [StringLength(50)]
    public string Surname { get; set; } = null!;

    [Column("email")]
    [StringLength(50)]
    public string Email { get; set; } = null!;

    [Column("password")]
    [StringLength(100)]
    public string Password { get; set; } = null!;

    [Column("registerDate", TypeName = "datetime")]
    public DateTime RegisterDate { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<TeamUser> TeamUsers { get; set; } = new List<TeamUser>();

    [InverseProperty("Manager")]
    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
}
