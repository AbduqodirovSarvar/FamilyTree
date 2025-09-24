using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Services.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Services.Repositories
{
    public class UserRepository(AppDbContext context, IRedisService redisService)
        : GenericRepository<User>(context, redisService), IUserRepository
    {
        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var result = await _redisService.GetAsync<User>(email);
            if (result != null)
                return result;

            return await _dbSet.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            var result = await _redisService.GetAsync<User>(username);
            if (result != null)
                return result;

            return await _dbSet.FirstOrDefaultAsync(x => x.UserName == username, cancellationToken);
        }
    }
}
