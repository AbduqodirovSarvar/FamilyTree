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
            throw new NotImplementedException();
        }

        public Task<T?> GetAsync<T>(string key)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            throw new NotImplementedException();
        }
    }
}
