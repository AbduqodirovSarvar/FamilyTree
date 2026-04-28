using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Services.Repositories.Common;
using System.Linq.Expressions;

namespace Persistence.Services.Repositories
{
    public class MemberRepository(AppDbContext context, IRedisService redisService)
        : GenericRepository<Member>(context, redisService), IMemberRepository
    {
        private IQueryable<Member> Query() => _dbSet.Include(x => x.Image);

        public override Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        public override Task<Member?> GetAsync(Expression<Func<Member, bool>> predicate, CancellationToken cancellationToken = default)
            => Query().FirstOrDefaultAsync(predicate, cancellationToken);

        public override async Task<List<Member>> GetAllAsync(Expression<Func<Member, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            IQueryable<Member> q = Query();
            if (predicate != null) q = q.Where(predicate);
            return await q.ToListAsync(cancellationToken);
        }

        public override async Task<(List<Member> Data, int TotalItems)> GetPaginatedAsync(
            Expression<Func<Member, bool>>? predicate = null,
            int pageIndex = 0,
            int pageSize = 10,
            Func<IQueryable<Member>, IOrderedQueryable<Member>>? orderBy = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Member> q = Query();
            if (predicate != null) q = q.Where(predicate);

            int totalItems = await q.CountAsync(cancellationToken);
            if (orderBy != null) q = orderBy(q);

            var data = await q.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return (data, totalItems);
        }
    }
}
