
using Application.Common.Interfaces.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Services.Repositories.Common
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity>
    {
        public virtual Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public virtual Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public virtual Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public virtual Task<(List<TEntity> Data, int TotalItems)> GetPaginatedAsync(Expression<Func<TEntity, bool>>? predicate = null, int pageIndex = 0, int pageSize = 10, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public virtual IQueryable<TEntity> Query(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public virtual Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
