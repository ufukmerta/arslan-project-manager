using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.WEB.Models;

[Table("role")]
[Index("RoleName", Name = "UQ__role__783254B16F397384", IsUnique = true)]
public partial class Role
{
    [Key]
    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("role_name")]
    [StringLength(100)]
    public string RoleName { get; set; } = null!;

    [Column("view_permission")]
    public bool? ViewPermission { get; set; }

    [Column("edit_permission")]
    public bool? EditPermission { get; set; }

    [InverseProperty("Role")]
    public virtual ICollection<TeamUser> TeamUsers { get; set; } = new List<TeamUser>();
}
