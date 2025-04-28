// src/utils/cacheUtils.js

/**
 * Simple in-memory cache implementation
 */
class MemoryCache {
  constructor() {
    this.cache = new Map();
  }

  /**
   * Get a value from the cache
   * @param {string} key - Cache key
   * @returns {any|null} Cached value or null if not found or expired
   */
  get(key) {
    if (!this.cache.has(key)) {
      return null;
    }

    const { value, expiry } = this.cache.get(key);
    
    // Check if the cached value has expired
    if (expiry && expiry < Date.now()) {
      this.delete(key);
      return null;
    }

    return value;
  }

  /**
   * Set a value in the cache
   * @param {string} key - Cache key
   * @param {any} value - Value to cache
   * @param {number} ttl - Time to live in milliseconds (default: 5 minutes)
   */
  set(key, value, ttl = 5 * 60 * 1000) {
    const expiry = ttl ? Date.now() + ttl : null;
    this.cache.set(key, { value, expiry });
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
      return false;
    }

    return true;
  }
}

// Create a singleton instance
const memoryCache = new MemoryCache();

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
