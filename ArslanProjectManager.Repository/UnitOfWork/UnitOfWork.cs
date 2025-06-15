using ArslanProjectManager.Core.UnitOfWork;

namespace ArslanProjectManager.Repository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ProjectManagerDbContext _context;

        public UnitOfWork(ProjectManagerDbContext context)
        {
            _context = context;
        }
        public void Commit()
        {
            _context.SaveChanges();
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
