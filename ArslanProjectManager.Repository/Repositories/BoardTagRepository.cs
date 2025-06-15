using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;

namespace ArslanProjectManager.Repository.Repositories
{
    public class BoardTagRepository(ProjectManagerDbContext context): GenericRepository<BoardTag>(context), IBoardTagRepository
    {
    }
}
