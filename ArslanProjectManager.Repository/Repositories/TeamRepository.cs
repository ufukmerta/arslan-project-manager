using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Repository.Repositories
{
    public class TeamRepository(ProjectManagerDbContext context) : GenericRepository<Team>(context), ITeamRepository
    {
        public async Task<Team?> GetTeamWithUsersAndRolesAsync(int teamId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(x => x.TeamUsers)
                    .ThenInclude(x => x.Role)
                .Include(x => x.TeamUsers)
                    .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == teamId);
        }

        public async Task<List<TeamDto>> GetUserTeamsAsync(int userId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.ManagerId == userId || x.TeamUsers.Any(tu => tu.UserId == userId))
                .OrderBy(x => x.Id)
                .Select(x => new TeamDto
                {
                    Id = x.Id,
                    TeamName = x.TeamName,
                    Description = x.Description,
                    ManagerName = x.Manager.Name,
                    MemberCount = x.TeamUsers.Count,
                    ProjectCount = x.Projects.Count
                })
                .ToListAsync();
        }

        public async Task<TeamDetailsDto?> GetTeamDetailsForUserAsync(int teamId, int userId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.Id == teamId && (x.ManagerId == userId || x.TeamUsers.Any(tu => tu.UserId == userId)))
                .Select(x => new TeamDetailsDto
                {
                    Id = x.Id,
                    TeamName = x.TeamName,
                    Description = x.Description,
                    ManagerId = x.ManagerId,
                    ManagerName = x.Manager.Name,
                    Members = x.TeamUsers.Select(u => new TeamUserDto
                    {
                        Id = u.Id,
                        UserId = u.UserId,
                        Name = u.User.Name,
                        Email = u.User.Email,
                        Role = u.Role.RoleName
                    }).ToList(),
                    Projects = x.Projects.Select(p => new TeamProjectDto
                    {
                        Id = p.Id,
                        ProjectName = p.ProjectName,
                        Description = p.ProjectDetail,
                        TaskCount = p.ProjectTasks.Count,
                        CompletedTaskCount = p.ProjectTasks.Count(t => t.BoardId == SystemBoards.DoneId)
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Team?> GetTeamWithUsersAsync(int teamId, bool asNoTracking = true)
        {
            IQueryable<Team> query = _dbSet;
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query
                .Include(x => x.Manager)
                .Include(x => x.TeamUsers)
                    .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == teamId);
        }

        public async Task<Team?> GetTeamWithUsersAndInvitesAsync(int teamId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(x => x.TeamUsers)
                    .ThenInclude(x => x.User)
                .Include(x => x.TeamInvites)
                .FirstOrDefaultAsync(x => x.Id == teamId);
        }

        public async Task<Team?> GetTeamWithInvitesAndInvitersAsync(int teamId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(x => x.TeamUsers)
                .Include(x => x.TeamInvites)
                    .ThenInclude(x => x.InvitedBy)
                .FirstOrDefaultAsync(x => x.Id == teamId);
        }
    }
}
