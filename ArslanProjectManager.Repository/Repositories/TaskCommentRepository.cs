using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;

namespace ArslanProjectManager.Repository.Repositories
{
    public class TaskCommentRepository(ProjectManagerDbContext context): GenericRepository<TaskComment>(context), ITaskCommentRepository
    {
    }
}
