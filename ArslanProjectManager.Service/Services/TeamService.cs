using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Service.Services
{
    public class TeamService(IGenericRepository<Team> repository, ITeamUserRepository teamUserRepository, IUnitOfWork unitOfWork) : GenericService<Team>(repository, unitOfWork), ITeamService
    {
        private readonly ITeamUserRepository _teamUserRepository = teamUserRepository;

        public async Task<TeamUser?> GetTeamUserAsync(int teamId, int userId)
        {
            return await _teamUserRepository
                .Where(x => x.TeamId == teamId && x.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<TeamUser> AddTeamUserAsync(TeamUser teamUser)
        {
            await _teamUserRepository.AddAsync(teamUser);
            return teamUser;
        }
    }
}
