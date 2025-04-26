// API service for the multi-scraper functionality

// Base API URL
const API_URL = '/api';

// Helper function to handle API responses
const handleResponse = async (response) => {
  if (!response.ok) {
    const errorData = await response.json().catch(() => ({}));
    throw new Error(errorData.message || `API error: ${response.status}`);
  }
  return response.json();
};

// Fetch all scrapers
export const fetchAllScrapers = async () => {
  const response = await fetch(`${API_URL}/scraper`);
  return handleResponse(response);
};

// Fetch a specific scraper by ID
export const fetchScraper = async (id) => {
  const response = await fetch(`${API_URL}/scraper/${id}`);
  return handleResponse(response);
};

// Create a new scraper
export const createScraper = async (scraperConfig) => {
  const response = await fetch(`${API_URL}/scraper`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(scraperConfig)
  });
  return handleResponse(response);
};

// Update an existing scraper
export const updateScraper = async (id, scraperConfig) => {
  const response = await fetch(`${API_URL}/scraper/${id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(scraperConfig)
  });
  return handleResponse(response);
};

// Delete a scraper
export const deleteScraper = async (id) => {
  const response = await fetch(`${API_URL}/scraper/${id}`, {
    method: 'DELETE'
  });
  return handleResponse(response);
};

// Start a scraper
export const startScraper = async (id) => {
  const response = await fetch(`${API_URL}/scraper/${id}/start`, {
    method: 'POST'
  });
  return handleResponse(response);
};

// Stop a scraper
export const stopScraper = async (id) => {
  const response = await fetch(`${API_URL}/scraper/${id}/stop`, {
    method: 'POST'
  });
  return handleResponse(response);
};

// Get scraper status
export const fetchScraperStatus = async (id) => {
  const response = await fetch(`${API_URL}/scraper/${id}/status`);
  return handleResponse(response);
};

// Get scraper logs
export const fetchScraperLogs = async (id) => {
  const response = await fetch(`${API_URL}/scraper/${id}/logs`);
  return handleResponse(response);
};

// Set monitoring settings
export const setMonitoring = async (id, settings) => {
  const response = await fetch(`${API_URL}/scraper/${id}/monitoring`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(settings)
  });
  return handleResponse(response);
};

// Fetch scraping results
export const fetchResults = async (params = {}) => {
  const queryParams = new URLSearchParams();
  if (params.scraperId) queryParams.append('scraperId', params.scraperId);
  if (params.page) queryParams.append('page', params.page);
  if (params.pageSize) queryParams.append('pageSize', params.pageSize);
  if (params.searchTerm) queryParams.append('searchTerm', params.searchTerm);
  
  const response = await fetch(`${API_URL}/results?${queryParams.toString()}`);
  return handleResponse(response);
};

// Fetch result detail
export const fetchResultDetail = async (url) => {
  const encodedUrl = encodeURIComponent(url);
  const response = await fetch(`${API_URL}/results/${encodedUrl}`);
  return handleResponse(response);
};

// For compatibility with the old API
export const fetchLogs = async () => {
  const response = await fetch(`${API_URL}/scraper/logs`);
  return handleResponse(response);
};

export const stopScraping = async () => {
  const response = await fetch(`${API_URL}/scraper/stop`, {
    method: 'POST'
  });
  return handleResponse(response);
};