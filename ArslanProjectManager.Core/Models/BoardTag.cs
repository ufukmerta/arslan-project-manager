using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Core.Models;

public partial class BoardTag : BaseEntity
{
    public string BoardName { get; set; } = null!;

    public byte BoardOrder { get; set; }
    
    public virtual ICollection<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();
}
