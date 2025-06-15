using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;

namespace ArslanProjectManager.Repository.Repositories
{
    public class TaskLogRepository(ProjectManagerDbContext context): GenericRepository<TaskLog>(context), ITaskLogRepository
    {
    }
}
