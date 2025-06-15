using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;

namespace ArslanProjectManager.Repository.Repositories
{
    public class LogCategoryRepository(ProjectManagerDbContext context): GenericRepository<LogCategory>(context), ILogCategoryRepository
    {
    }
}
