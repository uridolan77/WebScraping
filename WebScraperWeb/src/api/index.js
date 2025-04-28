// src/api/index.js
import axios from 'axios';
import { logError, isAuthError, isNetworkError } from '../utils/errorHandler';

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
export const handleResponse = (promise) => {
  return promise
    .then(response => response.data)
    .catch(error => {
      throw error;
    });
};

// Export the API client
export default apiClient;
