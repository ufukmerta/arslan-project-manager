using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;

namespace ArslanProjectManager.Repository.Repositories
{
    public class RoleRepository(ProjectManagerDbContext context): GenericRepository<Role>(context), IRoleRepository
    {
    }
}
