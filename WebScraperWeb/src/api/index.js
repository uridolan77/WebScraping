// src/api/index.js
import axios from 'axios';
import { logError, isAuthError, isNetworkError } from '../utils/errorHandler';
import memoryCache, { generateCacheKey } from '../utils/cacheUtils';

// Create an axios instance with default config
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5203/api',
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json'
  },
  timeout: 30000, // 30 seconds
  withCredentials: false, // Set to true if your API uses cookies for auth
});

// Debug API calls in development
if (import.meta.env.DEV) {
  apiClient.interceptors.request.use(request => {
    console.log('API Request:', {
      url: request.url,
      method: request.method,
      baseURL: request.baseURL,
      data: request.data,
      params: request.params
    });
    return request;
  });

  apiClient.interceptors.response.use(
    response => {
      console.log('API Response:', {
        url: response.config.url,
        status: response.status,
        data: response.data
      });
      return response;
    },
    error => {
      console.error('API Error:', {
        url: error.config?.url,
        method: error.config?.method,
        message: error.message,
        response: error.response?.data
      });
      return Promise.reject(error);
    }
  );
}

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
export const handleResponse = (promise) => {
  return promise
    .then(response => response.data)
    .catch(error => {
      throw error;
    });
};

/**
 * Make a GET request with caching
 * @param {string} url - The URL to request
 * @param {Object} options - Request options
 * @param {Object} options.params - URL parameters
 * @param {number} options.cacheTTL - Cache TTL in milliseconds (default: 5 minutes)
 * @param {boolean} options.forceRefresh - Force a refresh of the cache
 * @returns {Promise<any>} The response data
 */
export const cachedGet = async (url, options = {}) => {
  const { params = {}, cacheTTL = 5 * 60 * 1000, forceRefresh = false } = options;

  // Generate a cache key
  const cacheKey = generateCacheKey(url, params);

  // Check if we have a cached response and it's not a forced refresh
  if (!forceRefresh) {
    const cachedResponse = memoryCache.get(cacheKey);
    if (cachedResponse !== null) {
      return cachedResponse;
    }
  }

  // Make the request
  const response = await apiClient.get(url, { params });
  const data = response.data;

  // Cache the response
  memoryCache.set(cacheKey, data, cacheTTL);

  return data;
};

/**
 * Clear cache for a specific URL pattern
 * @param {string} urlPattern - URL pattern to match
 */
export const clearCache = (urlPattern) => {
  const keys = memoryCache.keys();
  keys.forEach(key => {
    if (key.includes(urlPattern)) {
      memoryCache.delete(key);
    }
  });
};

// Export the API client
export default apiClient;
