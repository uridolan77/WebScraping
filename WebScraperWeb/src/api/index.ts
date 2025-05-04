// src/api/index.js
import axios from 'axios';
import { logError, isAuthError, isNetworkError } from '../utils/errorHandler';
import memoryCache, { generateCacheKey } from '../utils/cacheUtils';

// Create an axios instance with default config
const apiClient = axios.create({
  baseURL: process.env.REACT_APP_API_URL || 'https://localhost:7143/api',
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000, // 30 seconds
  withCredentials: false, // Set to true if your API uses cookies for auth
});

// Add request interceptor for auth
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('auth_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Add response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    // Log the error with context
    logError(error, 'API Request', {
      url: error.config?.url,
      method: error.config?.method,
      status: error.response?.status
    });

    // Handle authentication errors
    if (isAuthError(error)) {
      // Clear auth token
      localStorage.removeItem('auth_token');

      // Redirect to login page if not already there
      const currentPath = window.location.pathname;
      if (currentPath !== '/login') {
        // Store the current location to redirect back after login
        sessionStorage.setItem('redirectAfterLogin', currentPath);

        // Redirect to login page
        window.location.href = '/login';
      }
    }

    // Handle network errors
    if (isNetworkError(error)) {
      // You could show a global notification here
      console.error('Network error: Unable to connect to the server');
    }

    // Return the rejected promise for the caller to handle
    return Promise.reject(error);
  }
);

// Helper function to handle API responses
export const handleResponse = (promise: Promise<any>) => {
  return promise
    .then(response => response.data)
    .catch(error => {
      throw error;
    });
};

/**
 * Make a GET request with caching and conditional requests
 * @param {string} url - The URL to request
 * @param {Object} options - Request options
 * @param {Object} options.params - URL parameters
 * @param {number} options.cacheTTL - Cache TTL in milliseconds (default: 5 minutes)
 * @param {boolean} options.forceRefresh - Force a refresh of the cache
 * @param {boolean} options.useConditionalRequests - Use HTTP conditional requests (default: true)
 * @returns {Promise<any>} The response data
 */
export const cachedGet = async (url: string, options: any = {}) => {
  const {
    params = {},
    cacheTTL = 5 * 60 * 1000,
    forceRefresh = false,
    useConditionalRequests = true
  } = options;

  // Generate a cache key
  const cacheKey = generateCacheKey(url, params);

  // Check if we have a cached response and it's not a forced refresh
  let cachedData = null;
  let etag = null;
  let lastModified = null;

  if (!forceRefresh) {
    const cachedEntry = memoryCache.get(cacheKey);
    if (cachedEntry !== null) {
      cachedData = cachedEntry.data;
      etag = cachedEntry.etag;
      lastModified = cachedEntry.lastModified;

      // If we're not using conditional requests, return the cached data immediately
      if (!useConditionalRequests) {
        return cachedData;
      }
    }
  }

  // Set up headers for conditional request if we have cached data
  const headers: Record<string, string> = {};
  if (useConditionalRequests && cachedData) {
    if (etag) {
      headers['If-None-Match'] = etag;
    }
    if (lastModified) {
      headers['If-Modified-Since'] = lastModified;
    }
  }

  try {
    // Make the request with conditional headers if available
    const response = await apiClient.get(url, {
      params,
      headers
    });

    // Get ETag and Last-Modified from response headers
    const newEtag = response.headers['etag'];
    const newLastModified = response.headers['last-modified'];
    const data = response.data;

    // Cache the response with metadata
    memoryCache.set(cacheKey, {
      data,
      etag: newEtag || etag,
      lastModified: newLastModified || lastModified,
      timestamp: Date.now()
    }, cacheTTL);

    return data;
  } catch (error) {
    // If we get a 304 Not Modified, use the cached data
    if (error.response && error.response.status === 304 && cachedData) {
      // Update the timestamp in the cache to extend TTL
      memoryCache.set(cacheKey, {
        data: cachedData,
        etag,
        lastModified,
        timestamp: Date.now()
      }, cacheTTL);

      return cachedData;
    }

    // For other errors, throw them to be handled by the caller
    throw error;
  }
};

/**
 * Clear cache for a specific URL pattern
 * @param {string} urlPattern - URL pattern to match
 */
export const clearCache = (urlPattern: string) => {
  const keys = memoryCache.keys();
  keys.forEach(key => {
    if (typeof key === 'string' && key.includes(urlPattern)) {
      memoryCache.delete(key);
    }
  });
};

// Export the API client
export default apiClient;
