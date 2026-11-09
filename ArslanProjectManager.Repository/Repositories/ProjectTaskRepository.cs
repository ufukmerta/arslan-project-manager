using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Repository.Repositories
{
    public class ProjectTaskRepository(ProjectManagerDbContext context) : GenericRepository<ProjectTask>(context), IProjectTaskRepository
    {
        public async Task<List<ProjectTask>> GetUserTasksAsync(IReadOnlyCollection<int> teamUserIds, IReadOnlyCollection<int> viewableTeamIds)
        {
            return await _context.ProjectTasks
                .AsNoTracking()
                .AsSplitQuery()
                .Include(p => p.Project)
                .Include(c => c.TaskCategory)
                .Include(b => b.Board)
                .Include(a => a.Appointee).ThenInclude(u => u.User)
                .Include(a => a.Appointer).ThenInclude(u => u.User)
                .Where(t =>
                    (teamUserIds.Contains(t.AppointeeId) || teamUserIds.Contains(t.AppointerId)) &&
                    viewableTeamIds.Contains(t.Project.TeamId))
                .ToListAsync();
        }

        public async Task<ProjectTask?> GetTaskWithDetailsAsync(int id)
        {
            return await _context.ProjectTasks
                .AsNoTracking()
                .AsSplitQuery()
                .Include(p => p.Appointee).ThenInclude(p => p.User)
                .Include(p => p.Appointer).ThenInclude(p => p.User)
                .Include(p => p.Board)
                .Include(p => p.Project)
                .Include(p => p.TaskCategory)
                .Include(p => p.TaskComments).ThenInclude(c => c.TeamUser).ThenInclude(tu => tu.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<ProjectTask?> GetTaskForEditAsync(int id, bool asNoTracking = true)
        {
            IQueryable<ProjectTask> query = _context.ProjectTasks;
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query
                .Include(p => p.Project)
                .Include(p => p.TaskCategory)
                .Include(p => p.Board)
                .Include(p => p.Appointee).ThenInclude(a => a.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<ProjectTask?> GetTaskForDeleteConfirmAsync(int id)
        {
            return await _context.ProjectTasks
                .AsNoTracking()
                .Include(p => p.Appointee).ThenInclude(p => p.User)
                .Include(p => p.Appointer).ThenInclude(p => p.User)
                .Include(p => p.Board)
                .Include(p => p.Project)
                .Include(p => p.TaskCategory)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<(List<TaskUserDto> TeamMembers, List<BoardTagDto> Boards, List<TaskCategoryDto> TaskCategories)> GetTaskFormMetaAsync(int teamId)
        {
            var teamMembers = await _context.TeamUsers
                .AsNoTracking()
                .Where(x => x.TeamId == teamId)
                .Select(x => new TaskUserDto
                {
                    TeamUserId = x.Id,
                    Name = x.User.Name
                })
                .ToListAsync();

            var boards = await _context.BoardTags
                .AsNoTracking()
                .OrderBy(x => x.BoardOrder)
                .Select(x => new BoardTagDto
                {
                    Id = x.Id,
                    BoardName = x.BoardName
                })
                .ToListAsync();

            var taskCategories = await _context.TaskCategories
                .AsNoTracking()
                .Select(x => new TaskCategoryDto
                {
                    Id = x.Id,
                    Category = x.Category
                })
                .ToListAsync();

            return (teamMembers, boards, taskCategories);
        }
    }
}
