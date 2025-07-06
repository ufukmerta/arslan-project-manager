using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.Services
{
    public interface ITeamService: IGenericService<Team>
    {
        Task<TeamUser?> GetTeamUserAsync(int teamId, int userId);
        Task<TeamUser> AddTeamUserAsync(TeamUser teamUser);
    }
}
