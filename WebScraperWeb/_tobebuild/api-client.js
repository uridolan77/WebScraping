// src/api/index.js
import axios from 'axios';

// Create an axios instance with default config
const apiClient = axios.create({
  baseURL: process.env.REACT_APP_API_URL || 'http://localhost:5203/api',
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000, // 30 seconds
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
    const { response } = error;
    
    // Handle specific error status codes
    if (response) {
      if (response.status === 401) {
        // Redirect to login page or refresh token
        console.error('Unauthorized access');
        // Auth logic here
      }
      if (response.status === 403) {
        console.error('Forbidden access');
      }
      if (response.status === 500) {
        console.error('Server error');
      }
    } else {
      console.error('Network error or server is down');
    }
    
    return Promise.reject(error);
  }
);

export default apiClient;