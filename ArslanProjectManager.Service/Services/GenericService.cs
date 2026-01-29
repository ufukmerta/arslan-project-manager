using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Service.Services
{
    public class GenericService<T>(IGenericRepository<T> repository, IUnitOfWork unitOfWork) : IGenericService<T> where T : BaseEntity
    {
        public virtual async Task<T> AddAsync(T entity)
        {
            entity.CreatedDate = DateTime.Now;
            entity.UpdatedDate = DateTime.Now;
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
            entity.UpdatedDate = DateTime.Now;
            repository.ChangeStatus(entity);
            unitOfWork.Commit();
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
            return await repository.GetByIdAsync(id);
        }

        public void Update(T entity)
        {
            entity.UpdatedDate = DateTime.Now;
            repository.Update(entity);
            unitOfWork.Commit();
        }

        public IQueryable<T> Where(Expression<Func<T, bool>> expression)
        {
            return repository.Where(expression);
        }
    }
}
