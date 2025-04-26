using System;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace WebScraperAPI.Data.Repositories.Redis
{
    /// <summary>
    /// Redis implementation of the ICacheRepository interface
    /// </summary>
    public class CacheRepository : ICacheRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        
        /// <summary>
        /// Initializes a new instance of the CacheRepository class
        /// </summary>
        /// <param name="redis">The Redis connection multiplexer</param>
        public CacheRepository(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = redis.GetDatabase();
        }
        
        /// <inheritdoc/>
        public async Task<T> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }
            
            return JsonSerializer.Deserialize<T>(value);
        }
        
        /// <inheritdoc/>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var serialized = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, serialized, expiry);
        }
        
        /// <inheritdoc/>
        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }
        
        /// <inheritdoc/>
        public async Task<bool> LockAsync(string key, TimeSpan timeout)
        {
            var lockKey = $"lock:{key}";
            return await _database.LockTakeAsync(lockKey, "locked", timeout);
        }
        
        /// <inheritdoc/>
        public async Task UnlockAsync(string key)
        {
            var lockKey = $"lock:{key}";
            await _database.LockReleaseAsync(lockKey, "locked");
        }
        
        /// <inheritdoc/>
        public async Task<long> IncrementAsync(string key, long value = 1)
        {
            return await _database.StringIncrementAsync(key, value);
        }
        
        /// <inheritdoc/>
        public async Task<long> DecrementAsync(string key, long value = 1)
        {
            return await _database.StringDecrementAsync(key, value);
        }
        
        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }
        
        /// <inheritdoc/>
        public async Task<bool> ExpireAsync(string key, TimeSpan expiry)
        {
            return await _database.KeyExpireAsync(key, expiry);
        }
    }
}
