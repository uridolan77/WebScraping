import axios from 'axios';

// Create axios instance with default config
const api = axios.create({
  baseURL: '/api', // This should point to your API base URL
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add a request interceptor for handling auth tokens if needed
api.interceptors.request.use(
  (config) => {
    // You can add auth tokens here if needed
    // const token = localStorage.getItem('token');
    // if (token) {
    //   config.headers.Authorization = `Bearer ${token}`;
    // }
    return config;
  },
  (error) => Promise.reject(error)
);

// Add a response interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    // Handle common error scenarios
    const { response } = error;
    
    if (!response) {
      // Network error
      console.error('Network error - unable to connect to API');
      return Promise.reject({
        message: 'Unable to connect to server. Please check your connection.',
      });
    }

    if (response.status === 401) {
      // Handle unauthorized
      console.error('Unauthorized access');
      // You might want to redirect to login or refresh tokens
    }

    return Promise.reject(
      response.data || { message: 'An unknown error occurred' }
    );
  }
);

// Scrapers API
const scrapers = {
  getAll: () => api.get('/Scraper'),
  getById: (id) => api.get(`/Scraper/${id}`),
  create: (data) => api.post('/Scraper', data),
  update: (id, data) => api.put(`/Scraper/${id}`, data),
  delete: (id) => api.delete(`/Scraper/${id}`),
  getStatus: (id) => api.get(`/Scraper/${id}/status`),
  start: (id) => api.post(`/Scraper/${id}/start`),
  stop: (id) => api.post(`/Scraper/${id}/stop`),
  getLogs: (id, params) => api.get(`/Scraper/${id}/logs`, { params }),
  getResults: (id, params) => api.get('/Scraper/results', { params: { scraperId: id, ...params } }),
  getChanges: (id, params) => api.get(`/Scraper/${id}/changes`, { params }),
  getDocuments: (id, params) => api.get(`/Scraper/${id}/documents`, { params }),
  compressContent: (id) => api.post(`/Scraper/${id}/compress`),
  setupMonitoring: (id, config) => api.post(`/Scraper/${id}/monitor`, config),
};

// Scheduling API
const scheduling = {
  getAll: () => api.get('/Scheduling'),
  getForScraper: (scraperId) => api.get(`/Scheduling/scraper/${scraperId}`),
  create: (scraperId, data) => api.post(`/Scheduling/scraper/${scraperId}`, data),
  update: (id, data) => api.put(`/Scheduling/${id}`, data),
  delete: (id) => api.delete(`/Scheduling/${id}`),
  validateCron: (expression) => api.post('/Scheduling/validate-cron', { expression }),
};

// Analytics API
const analytics = {
  getSummary: () => api.get('/Analytics/summary'),
  getPopularDomains: () => api.get('/Analytics/popular-domains'),
  getContentChangeFrequency: () => api.get('/Analytics/content-change-frequency'),
  getUsageStatistics: (params) => api.get('/Analytics/usage-statistics', { params }),
  getErrorDistribution: () => api.get('/Analytics/error-distribution'),
  getScraperAnalytics: (scraperId) => api.get(`/Analytics/scrapers/${scraperId}`),
  getScraperPerformance: (scraperId) => api.get(`/Analytics/scrapers/${scraperId}/performance`),
  getScraperMetrics: (scraperId) => api.get(`/Analytics/scrapers/${scraperId}/metrics`),
};

// Notifications API
const notifications = {
  updateWebhookConfig: (scraperId, config) => api.put(`/Notifications/scraper/${scraperId}/webhook-config`, config),
  testWebhook: (data) => api.post('/Notifications/webhook-test', data),
  sendNotification: (scraperId, data) => api.post(`/Notifications/scraper/${scraperId}/notify`, data),
};

const apiClient = {
  scrapers,
  scheduling,
  analytics,
  notifications,
};

export default apiClient;