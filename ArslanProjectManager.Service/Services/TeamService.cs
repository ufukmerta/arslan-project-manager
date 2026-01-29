using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Service.Services
{
    public class TeamService(ITeamRepository repository, ITeamUserRepository teamUserRepository, IUnitOfWork unitOfWork) : GenericService<Team>(repository, unitOfWork), ITeamService
    {
        public async Task<TeamUser?> GetTeamUserAsync(int teamId, int userId)
        {
            return await teamUserRepository
                .Where(x => x.TeamId == teamId && x.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<TeamUser> AddTeamUserAsync(TeamUser teamUser)
        {
            await teamUserRepository.AddAsync(teamUser);
            return teamUser;
        }
    }
}
