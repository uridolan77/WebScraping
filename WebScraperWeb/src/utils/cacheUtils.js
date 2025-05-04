// src/utils/cacheUtils.js

/**
 * LRU (Least Recently Used) cache implementation
 * This implementation maintains a size limit and automatically removes
 * the least recently used items when the cache reaches capacity
 */
class LRUCache {
  /**
   * Create a new LRU cache
   * @param {number} maxSize - Maximum number of items to store in the cache
   */
  constructor(maxSize = 100) {
    this.cache = new Map();
    this.maxSize = maxSize;
    this.stats = {
      hits: 0,
      misses: 0,
      evictions: 0,
      expirations: 0
    };
  }

  /**
   * Get a value from the cache
   * @param {string} key - Cache key
   * @returns {any|null} Cached value or null if not found or expired
   */
  get(key) {
    if (!this.cache.has(key)) {
      this.stats.misses++;
      return null;
    }

    const entry = this.cache.get(key);

    // Check if the cached value has expired
    if (entry.expiry && entry.expiry < Date.now()) {
      this.delete(key);
      this.stats.expirations++;
      this.stats.misses++;
      return null;
    }

    // Update access order (delete and re-add to put at the end)
    this.cache.delete(key);
    this.cache.set(key, entry);

    this.stats.hits++;
    return entry.value;
  }

  /**
   * Set a value in the cache
   * @param {string} key - Cache key
   * @param {any} value - Value to cache
   * @param {number} ttl - Time to live in milliseconds (default: 5 minutes)
   */
  set(key, value, ttl = 5 * 60 * 1000) {
    // If the key already exists, just update it
    if (this.cache.has(key)) {
      this.cache.delete(key);
    }
    // If we're at capacity and adding a new key, remove the oldest item (first in map)
    else if (this.cache.size >= this.maxSize) {
      const oldestKey = this.cache.keys().next().value;
      this.cache.delete(oldestKey);
      this.stats.evictions++;
    }

    // Add the new item
    const expiry = ttl ? Date.now() + ttl : null;
    this.cache.set(key, { value, expiry, addedAt: Date.now() });

    return this;
  }

  /**
   * Delete a value from the cache
   * @param {string} key - Cache key
   */
  delete(key) {
    this.cache.delete(key);
  }

  /**
   * Clear the entire cache
   */
  clear() {
    this.cache.clear();
    // Reset stats when clearing the cache
    this.stats = {
      hits: 0,
      misses: 0,
      evictions: 0,
      expirations: 0
    };
  }

  /**
   * Get all keys in the cache
   * @returns {Array} Array of cache keys
   */
  keys() {
    return Array.from(this.cache.keys());
  }

  /**
   * Check if a key exists in the cache and is not expired
   * @param {string} key - Cache key
   * @returns {boolean} Whether the key exists and is not expired
   */
  has(key) {
    if (!this.cache.has(key)) {
      return false;
    }

    const { expiry } = this.cache.get(key);

    // Check if the cached value has expired
    if (expiry && expiry < Date.now()) {
      this.delete(key);
      this.stats.expirations++;
      return false;
    }

    return true;
  }

  /**
   * Get cache statistics
   * @returns {Object} Cache statistics
   */
  getStats() {
    return {
      ...this.stats,
      size: this.cache.size,
      maxSize: this.maxSize,
      hitRate: this.stats.hits / (this.stats.hits + this.stats.misses) || 0
    };
  }
}

// Create a singleton instance with a limit of 200 items
const memoryCache = new LRUCache(200);

/**
 * Generate a cache key from a request
 * @param {string} url - Request URL
 * @param {Object} params - Request parameters
 * @returns {string} Cache key
 */
export const generateCacheKey = (url, params = {}) => {
  const sortedParams = Object.keys(params)
    .sort()
    .reduce((acc, key) => {
      acc[key] = params[key];
      return acc;
    }, {});

  return `${url}:${JSON.stringify(sortedParams)}`;
};

/**
 * Cache decorator for API functions
 * @param {Function} fn - Function to decorate
 * @param {number} ttl - Time to live in milliseconds
 * @returns {Function} Decorated function with caching
 */
export const withCache = (fn, ttl = 5 * 60 * 1000) => {
  return async (...args) => {
    // Generate a cache key based on the function name and arguments
    const cacheKey = `${fn.name}:${JSON.stringify(args)}`;

    // Check if we have a cached value
    const cachedValue = memoryCache.get(cacheKey);
    if (cachedValue !== null) {
      return cachedValue;
    }

    // Call the original function
    const result = await fn(...args);

    // Cache the result
    memoryCache.set(cacheKey, result, ttl);

    return result;
  };
};

/**
 * Clear cache entries that match a prefix
 * @param {string} prefix - Cache key prefix
 */
export const clearCacheByPrefix = (prefix) => {
  const keys = memoryCache.keys();
  keys.forEach(key => {
    if (key.startsWith(prefix)) {
      memoryCache.delete(key);
    }
  });
};

export default memoryCache;
