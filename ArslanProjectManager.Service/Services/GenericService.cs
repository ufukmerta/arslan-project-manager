using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;
using System.Linq.Expressions;

namespace ArslanProjectManager.Service.Services
{
    public class GenericService<T>(IGenericRepository<T> repository, IUnitOfWork unitOfWork) : IGenericService<T> where T : BaseEntity
    {
        protected IUnitOfWork UnitOfWork => unitOfWork;

        public virtual async Task<T> AddAsync(T entity)
        {
            await repository.AddAsync(entity);
            await unitOfWork.CommitAsync();
            return entity;
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression)
        {
            return await repository.AnyAsync(expression);
        }

        public void ChangeStatus(T entity)
        {
            entity.IsActive = false;
            repository.ChangeStatus(entity);
            unitOfWork.Commit();
        }

        public async Task ChangeStatusAsync(T entity)
        {
            entity.IsActive = false;
            repository.ChangeStatus(entity);
            await unitOfWork.CommitAsync();
        }

        public int Count()
        {
            return repository.Count();
        }

        public async Task<int> CountAsync()
        {
            return await repository.CountAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return (await repository.GetByIdAsync(id))!;
        }

        public void Update(T entity)
        {
            repository.Update(entity);
            unitOfWork.Commit();
        }

        public async Task UpdateAsync(T entity)
        {
            repository.Update(entity);
            await unitOfWork.CommitAsync();
        }

        public IQueryable<T> Where(Expression<Func<T, bool>> expression)
        {
            return repository.Where(expression);
        }
    }
}
