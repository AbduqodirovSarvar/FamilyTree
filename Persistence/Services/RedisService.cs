using Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Services
{
    public class RedisService : IRedisService
    {
        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(false);
        }

        public Task<T?> GetAsync<T>(string key)
        {
            return Task.FromResult<T?>(default);
        }

        public Task<bool> RemoveAsync(string key)
        {
            return Task.FromResult(false);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            return Task.CompletedTask;
        }
    }
}
