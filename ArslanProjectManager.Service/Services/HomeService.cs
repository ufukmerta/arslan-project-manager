using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Service.Utilities;

namespace ArslanProjectManager.Service.Services
{
    public class HomeService(IUserRepository userRepository) : IHomeService
    {
        public async Task<HomeDto> GetHomeSummaryAsync(int userId)
        {
            var user = await userRepository.GetUserWithTeamsProjectsTasksAsync(userId);
            var userTeamUsers = user?.TeamUsers.ToList();
            if (userTeamUsers is null || userTeamUsers.Count == 0)
            {
                return new HomeDto();
            }

            var teamPermissions = userTeamUsers.ToDictionary(
                tu => tu.TeamId,
                tu => PermissionResolver.GetEffectivePermissions(tu, tu.Role)
            );

            // All teams the user has joined (used for team counts)
            var allTeams = userTeamUsers
                .Select(tu => tu.Team)
                .Distinct()
                .ToList();

            // Teams where the user can view projects or tasks (used for project/task visibility)
            var visibleTeams = allTeams
                .Where(t =>
                    teamPermissions.TryGetValue(t.Id, out var perms) &&
                    (perms.CanViewProjects || perms.CanViewTasks))
                .ToList();

            // Projects the user is allowed to view
            var projects = visibleTeams
                .Where(t => teamPermissions[t.Id].CanViewProjects)
                .SelectMany(t => t.Projects)
                .ToList();

            // Tasks the user is allowed to view
            var tasks = visibleTeams
                .Where(t => teamPermissions[t.Id].CanViewTasks)
                .SelectMany(t => t.Projects)
                .SelectMany(p => p.ProjectTasks)
                .ToList();

            var recentTasks = tasks
                .OrderByDescending(t => t.UpdatedDate)
                .Take(5)
                .Select(t => new RecentTaskDto
                {
                    Id = t.Id,
                    TaskName = t.TaskName,
                    Description = t.Description,
                    CreatedDate = t.CreatedDate,
                    UpdatedDate = t.UpdatedDate,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    BoardId = t.BoardId,
                    ProjectName = t.Project.ProjectName,
                    AppointeeName = t.Appointee.User.Name
                }).ToList();

            var recentProjects = projects
                .OrderByDescending(p => p.CreatedDate)
                .Take(5)
                .Select(p => new RecentProjectDto
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName,
                    Description = p.ProjectDetail,
                    TeamName = p.Team.TeamName,
                    CreatedDate = p.CreatedDate,
                    StartDate = p.StartDate,
                    TotalTasks = p.ProjectTasks.Count,
                    CompletedTasks = p.ProjectTasks.Count(t => t.BoardId == 3)
                }).ToList();

            return new HomeDto
            {
                TotalProjects = projects.Count,
                CompletedProjects = projects.Count(p => p.ProjectTasks.All(t => t.BoardId == 3)),
                TotalTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.BoardId == 3),
                TotalTeams = allTeams.Count,
                TotalMembers = allTeams.Sum(t => t.TeamUsers.Count),
                RecentTasks = recentTasks,
                RecentProjects = recentProjects
            };
        }
    }
}
