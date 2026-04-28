using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Services.Repositories.Common;
using System.Linq.Expressions;

namespace Persistence.Services.Repositories
{
    public class FamilyRepository(AppDbContext context, IRedisService redisService)
        : GenericRepository<Family>(context, redisService), IFamilyRepository
    {
        private IQueryable<Family> Query() => _dbSet.Include(x => x.Image);

        public override Task<Family?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        public override Task<Family?> GetAsync(Expression<Func<Family, bool>> predicate, CancellationToken cancellationToken = default)
            => Query().FirstOrDefaultAsync(predicate, cancellationToken);

        public override async Task<List<Family>> GetAllAsync(Expression<Func<Family, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            IQueryable<Family> q = Query();
            if (predicate != null) q = q.Where(predicate);
            return await q.ToListAsync(cancellationToken);
        }

        public override async Task<(List<Family> Data, int TotalItems)> GetPaginatedAsync(
            Expression<Func<Family, bool>>? predicate = null,
            int pageIndex = 0,
            int pageSize = 10,
            Func<IQueryable<Family>, IOrderedQueryable<Family>>? orderBy = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Family> q = Query();
            if (predicate != null) q = q.Where(predicate);

            int totalItems = await q.CountAsync(cancellationToken);
            if (orderBy != null) q = orderBy(q);

            var data = await q.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return (data, totalItems);
        }
    }
}
