using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces.Repositories.Common
{
    public interface IGenericRepository<TEntity>
    {
        Task<TEntity> CreateAsync(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task<bool> DeleteAsync(TEntity entity);
        Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate);
        Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? predicate = null);
        Task<(List<TEntity> Data, int TotalItems)> GetPaginatedAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            int pageIndex = 0,
            int pageSize = 10,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null);
        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>>? predicate = null);
    }
}
