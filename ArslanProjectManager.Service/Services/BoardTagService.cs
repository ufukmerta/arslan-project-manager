using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;

namespace ArslanProjectManager.Service.Services
{
    public class BoardTagService(IBoardTagRepository repository, IUnitOfWork unitOfWork)
        : GenericService<BoardTag>(repository, unitOfWork), IBoardTagService
    {
    }
}
