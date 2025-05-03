import axios from 'axios';

// Create an axios instance with default config
const apiClient = axios.create({
  baseURL: process.env.REACT_APP_API_URL || 'https://localhost:7143/api',
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
  (error) => {
    console.error('Request error:', error);
    return Promise.reject(error);
  }
);

// Add response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    // Handle network errors
    if (error.message === 'Network Error') {
      console.error('Network error - unable to connect to the server:', error);
      // You could dispatch an action to show a global network error notification
    }
    // Handle timeout errors
    else if (error.code === 'ECONNABORTED') {
      console.error('Request timeout:', error);
    }
    // Handle server errors
    else if (error.response) {
      console.error(`API Error (${error.response.status}):`, error.response.data);
    }
    // Handle other errors
    else {
      console.error('API Error:', error);
    }

    return Promise.reject(error);
  }
);

// Add retry mechanism for failed requests
apiClient.interceptors.response.use(undefined, async (error) => {
  const { config, message } = error;

  // Only retry GET requests with network errors
  if (!config || !config.method || config.method.toLowerCase() !== 'get' || message !== 'Network Error') {
    return Promise.reject(error);
  }

  // Don't retry if we've already retried
  if (config._retryCount >= 2) {
    return Promise.reject(error);
  }

  // Set retry count
  config._retryCount = config._retryCount || 0;
  config._retryCount += 1;

  // Exponential backoff
  const delay = config._retryCount * 1000;

  console.log(`Retrying request (attempt ${config._retryCount}) after ${delay}ms...`);

  return new Promise(resolve => setTimeout(() => resolve(apiClient(config)), delay));
});

export default apiClient;
