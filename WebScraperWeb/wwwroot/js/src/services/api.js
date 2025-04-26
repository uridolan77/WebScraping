// Base API URL
const API_URL = 'https://localhost:7143/api';

// Helper function for handling API errors
const handleResponse = async (response) => {
  if (!response.ok) {
    const errorData = await response.json().catch(() => null);
    const errorMessage = errorData?.message || `Error: ${response.status} ${response.statusText}`;
    throw new Error(errorMessage);
  }
  return response.json();
};

/**
 * Fetch the current status of the scraper
 */
export const fetchScraperStatus = async () => {
  try {
    const response = await fetch(`${API_URL}/scraper/status`);
    return handleResponse(response);
  } catch (error) {
    console.error('Error fetching scraper status:', error);
    throw error;
  }
};

/**
 * Start the scraper with the provided configuration
 */
export const startScraping = async (config) => {
  try {
    const response = await fetch(`${API_URL}/scraper/start`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(config),
    });
    return handleResponse(response);
  } catch (error) {
    console.error('Error starting scraper:', error);
    throw error;
  }
};

/**
 * Stop the currently running scraper
 */
export const stopScraping = async () => {
  try {
    const response = await fetch(`${API_URL}/scraper/stop`, {
      method: 'POST',
    });
    return handleResponse(response);
  } catch (error) {
    console.error('Error stopping scraper:', error);
    throw error;
  }
};

/**
 * Fetch the logs from the scraper
 */
export const fetchLogs = async (limit = 100) => {
  try {
    const response = await fetch(`${API_URL}/scraper/logs?limit=${limit}`);
    return handleResponse(response);
  } catch (error) {
    console.error('Error fetching logs:', error);
    throw error;
  }
};

/**
 * Fetch the results of scraping (paginated)
 */
export const fetchResults = async (page = 1, pageSize = 20, search = '') => {
  try {
    const url = new URL(`${API_URL}/scraper/results`);
    url.searchParams.append('page', page);
    url.searchParams.append('pageSize', pageSize);
    if (search) {
      url.searchParams.append('search', search);
    }
    
    const response = await fetch(url);
    return handleResponse(response);
  } catch (error) {
    console.error('Error fetching results:', error);
    throw error;
  }
};

/**
 * Fetch details for a specific scraped URL
 */
export const fetchResultDetail = async (url) => {
  try {
    const response = await fetch(`${API_URL}/scraper/result/${encodeURIComponent(url)}`);
    return handleResponse(response);
  } catch (error) {
    console.error('Error fetching result detail:', error);
    throw error;
  }
};