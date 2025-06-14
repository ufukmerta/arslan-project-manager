using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);

        IQueryable<T> Where(Expression<Func<T, bool>> expression);

        int Count();

        Task<int> CountAsync();

        void Update(T entity);

        void ChangeStatus(T entity);

        Task AddAsync(T entity);

        Task<bool> AnyAsync(Expression<Func<T, bool>> expression);

    }
}
