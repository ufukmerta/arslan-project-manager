using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs.UpdateDTOs
{
    public class TeamInviteUpdateDto : BaseUpdateDto
    {
        public TeamInvite.InviteStatus Status { get; set; } = TeamInvite.InviteStatus.Pending;
        public string? StatusChangeNote { get; set; } = null!;
    }
}
