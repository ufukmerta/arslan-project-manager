using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;

namespace ArslanProjectManager.Repository.Repositories
{
    public class TaskTagRepository(ProjectManagerDbContext context): GenericRepository<TaskTag>(context), ITaskTagRepository
    {
    }
}
