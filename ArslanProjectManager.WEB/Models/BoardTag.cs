using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.WEB.Models;

[Table("board_tag")]
[Index("BoardName", Name = "UQ_board_tag", IsUnique = true)]
public partial class BoardTag
{
    [Key]
    [Column("board_id")]
    public int BoardId { get; set; }

    [Column("board_name")]
    [StringLength(50)]
    public string BoardName { get; set; } = null!;

    [Column("board_order")]
    public byte BoardOrder { get; set; }

    [InverseProperty("Board")]
    public virtual ICollection<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();
}
