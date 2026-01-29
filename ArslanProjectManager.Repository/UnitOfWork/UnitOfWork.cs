using ArslanProjectManager.Core.UnitOfWork;

namespace ArslanProjectManager.Repository.UnitOfWork
{
    public class UnitOfWork(ProjectManagerDbContext context) : IUnitOfWork
    {        
        public void Commit()
        {
            context.SaveChanges();
        }

        public async Task CommitAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
