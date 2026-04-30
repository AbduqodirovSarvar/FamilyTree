using Application.Common.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Persistence.Services
{
    public class RedisService(IConnectionMultiplexer connectionMultiplexer) : IRedisService
    {
        private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

        /// <summary>
        /// EF-tracked entities have circular navigation properties
        /// (Family.Owner.Family.Owner...). Without IgnoreCycles the serializer
        /// recurses to MaxDepth=64 and throws JsonException_SerializerCycleDetected,
        /// which surfaces as a 500 on every update that touches the cache.
        /// IgnoreNullValues keeps stored payloads compact; PropertyNameCaseInsensitive
        /// makes the cache survive minor casing changes during deserialization.
        /// </summary>
        private static readonly JsonSerializerOptions _options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>(value!, _options);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await _database.KeyDeleteAsync(key);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var json = JsonSerializer.Serialize(value, _options);
            await _database.StringSetAsync(key, json, expiration);
        }
    }
}
