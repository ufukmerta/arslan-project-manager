using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;

namespace ArslanProjectManager.Repository.Repositories
{
    public class TaskCategoryRepository(ProjectManagerDbContext context): GenericRepository<TaskCategory>(context), ITaskCategoryRepository
    {
    }
}
