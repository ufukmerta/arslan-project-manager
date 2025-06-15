using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;

namespace ArslanProjectManager.Repository.Repositories
{
    public class ProjectTaskRepository(ProjectManagerDbContext context): GenericRepository<ProjectTask>(context), IProjectTaskRepository
    {
    }
}
