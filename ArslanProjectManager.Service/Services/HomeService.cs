using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;

namespace ArslanProjectManager.Service.Services
{
    public class HomeService(IUserRepository userRepository) : IHomeService
    {
        public async Task<HomeDto> GetHomeSummaryAsync(int userId)
        {
            var user = await userRepository.GetUserWithTeamsProjectsTasksAsync(userId);
            var teams = user!.TeamUsers.Where(tu => tu.UserId == userId).Select(tu => tu.Team).ToList();
            var projects = teams.SelectMany(t => t.Projects).ToList();
            var tasks = projects.SelectMany(p => p.ProjectTasks).ToList();

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
                TotalTeams = teams.Count,
                TotalMembers = teams.Sum(t => t.TeamUsers.Count),
                RecentTasks = recentTasks,
                RecentProjects = recentProjects
            };
        }
    }
}
