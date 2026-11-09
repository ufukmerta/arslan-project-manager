using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.Services
{
    public interface IGenericService<T> where T : class
    {
        Task<T> GetByIdAsync(int id);

        IQueryable<T> Where(Expression<Func<T, bool>> expression);

        int Count();

        Task<int> CountAsync();

        void Update(T entity);

        Task UpdateAsync(T entity);

        void ChangeStatus(T entity);

        Task ChangeStatusAsync(T entity);

        Task<T> AddAsync(T entity);

        Task<bool> AnyAsync(Expression<Func<T, bool>> expression);
    }
}
