using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Services.Repositories.Common;
using System.Linq.Expressions;

namespace Persistence.Services.Repositories
{
    public class UserRepository(AppDbContext context, IRedisService redisService)
        : GenericRepository<User>(context, redisService), IUserRepository
    {
        private IQueryable<User> Query() => _dbSet.Include(x => x.Image);

        public override Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        public override Task<User?> GetAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
            => Query().FirstOrDefaultAsync(predicate, cancellationToken);

        public override async Task<List<User>> GetAllAsync(Expression<Func<User, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            IQueryable<User> q = Query();
            if (predicate != null) q = q.Where(predicate);
            return await q.ToListAsync(cancellationToken);
        }

        public override async Task<(List<User> Data, int TotalItems)> GetPaginatedAsync(
            Expression<Func<User, bool>>? predicate = null,
            int pageIndex = 0,
            int pageSize = 10,
            Func<IQueryable<User>, IOrderedQueryable<User>>? orderBy = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<User> q = Query();
            if (predicate != null) q = q.Where(predicate);

            int totalItems = await q.CountAsync(cancellationToken);
            if (orderBy != null) q = orderBy(q);

            var data = await q.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return (data, totalItems);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => await Query().FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
            => await Query().FirstOrDefaultAsync(x => x.UserName == username, cancellationToken);
    }
}
