using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.Services
{
    public interface IRoleService: IGenericService<Role>
    {
        Task<Role?> GetDefaultRoleAsync();
        Task<List<RoleDto>> GetRolesForTeamAsync(int teamId);
    }
}
