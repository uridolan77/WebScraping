using System;
using System.Threading.Tasks;

namespace WebScraperAPI.Data.Repositories
{
    /// <summary>
    /// Repository interface for caching operations using Redis
    /// </summary>
    public interface ICacheRepository
    {
        /// <summary>
        /// Gets a value from the cache
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="key">The cache key</param>
        /// <returns>The cached value if found, or default</returns>
        Task<T> GetAsync<T>(string key);
        
        /// <summary>
        /// Sets a value in the cache
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="key">The cache key</param>
        /// <param name="value">The value to cache</param>
        /// <param name="expiry">Optional expiration time</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        
        /// <summary>
        /// Removes a value from the cache
        /// </summary>
        /// <param name="key">The cache key</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task RemoveAsync(string key);
        
        /// <summary>
        /// Acquires a distributed lock
        /// </summary>
        /// <param name="key">The lock key</param>
        /// <param name="timeout">The lock timeout</param>
        /// <returns>True if the lock was acquired, false otherwise</returns>
        Task<bool> LockAsync(string key, TimeSpan timeout);
        
        /// <summary>
        /// Releases a distributed lock
        /// </summary>
        /// <param name="key">The lock key</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task UnlockAsync(string key);
        
        /// <summary>
        /// Increments a counter in the cache
        /// </summary>
        /// <param name="key">The counter key</param>
        /// <param name="value">The increment value</param>
        /// <returns>The new counter value</returns>
        Task<long> IncrementAsync(string key, long value = 1);
        
        /// <summary>
        /// Decrements a counter in the cache
        /// </summary>
        /// <param name="key">The counter key</param>
        /// <param name="value">The decrement value</param>
        /// <returns>The new counter value</returns>
        Task<long> DecrementAsync(string key, long value = 1);
        
        /// <summary>
        /// Checks if a key exists in the cache
        /// </summary>
        /// <param name="key">The cache key</param>
        /// <returns>True if the key exists, false otherwise</returns>
        Task<bool> ExistsAsync(string key);
        
        /// <summary>
        /// Sets a key expiration time
        /// </summary>
        /// <param name="key">The cache key</param>
        /// <param name="expiry">The expiration time</param>
        /// <returns>True if the expiration was set, false otherwise</returns>
        Task<bool> ExpireAsync(string key, TimeSpan expiry);
    }
}
