using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArslanProjectManager.Core;

public class TaskCommentCreateDto : BaseCreateDto
{
    public int TaskId { get; set; }
    public int TeamUserId { get; set; }
    public string Comment { get; set; } = null!;    
}