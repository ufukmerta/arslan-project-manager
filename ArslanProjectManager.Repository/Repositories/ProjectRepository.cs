using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;

namespace ArslanProjectManager.Repository.Repositories
{
    public class ProjectRepository(ProjectManagerDbContext context): GenericRepository<Project>(context), IProjectRepository
    {
    }
}
