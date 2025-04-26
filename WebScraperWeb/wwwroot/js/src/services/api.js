// API service for the multi-scraper functionality

// Base API URL - use relative URL to work with any host
const API_URL = '/api';

// Log the API URL for debugging
console.log('API URL configured as:', API_URL);

// Helper function to handle API responses
const handleResponse = async (response) => {
  if (!response.ok) {
    // Get the response as text first
    const errorText = await response.text();
    console.error('Server response:', response.status, errorText);

    try {
      // Try to parse as JSON if possible
      const errorData = JSON.parse(errorText);
      console.error('Error details:', errorData);
      throw new Error(errorData.message || `API error: ${response.status}`);
    } catch (e) {
      // If not JSON, use the text
      console.error('Error response (text):', errorText);
      throw new Error(`API error: ${response.status} - ${errorText}`);
    }
  }
  return response.json();
};

// Mock data for development/testing
const MOCK_SCRAPERS = [
  {
    id: 'mock-1',
    name: 'Example Blog Scraper',
    description: 'Scrapes example.com blog posts',
    baseUrl: 'https://example.com/blog',
    startUrl: 'https://example.com/blog',
    status: 'idle',
    lastRun: new Date(Date.now() - 86400000).toISOString(), // Yesterday
    pagesCrawled: 42,
    monitoring: {
      enabled: true,
      intervalMinutes: 1440
    }
  },
  {
    id: 'mock-2',
    name: 'News Website Scraper',
    description: 'Collects news articles',
    baseUrl: 'https://news-example.org',
    startUrl: 'https://news-example.org/latest',
    status: 'idle',
    lastRun: new Date(Date.now() - 172800000).toISOString(), // 2 days ago
    pagesCrawled: 128,
    monitoring: {
      enabled: false
    }
  }
];

// Flag to enable mock data (set to true for testing, false for production)
const USE_MOCK_DATA = false;

// Fetch all scrapers
export const fetchAllScrapers = async () => {
  console.log('Fetching all scrapers...');

  // Return mock data if enabled
  if (USE_MOCK_DATA) {
    console.log('Using mock data for scrapers');
    return MOCK_SCRAPERS;
  }

  // Log the full URL for debugging
  const url = `${API_URL}/scraper`;
  console.log('Fetching from URL:', url);

  try {
    console.log('Starting fetch request for all scrapers...');
    const response = await fetch(url);

    // If response is not ok, log the error details
    if (!response.ok) {
      const errorText = await response.text();
      console.error('Server response for fetchAllScrapers:', response.status, errorText);
      try {
        // Try to parse as JSON if possible
        const errorJson = JSON.parse(errorText);
        console.error('Error details:', errorJson);
      } catch (e) {
        // If not JSON, log as text
        console.error('Error response (text):', errorText);
      }

      // If we get a 404, it might mean the endpoint doesn't exist or the API is not running
      if (response.status === 404) {
        console.warn('API endpoint not found. The API might not be running or the endpoint is incorrect.');
        // Return mock data as fallback in development
        if (window.location.hostname === 'localhost') {
          console.log('Using mock data as fallback on localhost');
          return MOCK_SCRAPERS;
        }
      }

      throw new Error(`API error: ${response.status} - ${errorText}`);
    }

    const data = await response.json();
    console.log('Fetched all scrapers data (raw):', data);

    // Check if the data is an array
    if (!Array.isArray(data)) {
      console.warn('API returned non-array data for fetchAllScrapers:', data);
      // Try to handle different response formats
      if (data && typeof data === 'object') {
        // If it's an object with a results property that is an array
        if (Array.isArray(data.results)) {
          console.log('Using data.results as scrapers array');
          return data.results;
        }
        // If it's an object with a scrapers property that is an array
        if (Array.isArray(data.scrapers)) {
          console.log('Using data.scrapers as scrapers array');
          return data.scrapers;
        }
        // If it's just an object with an id, wrap it in an array
        if (data.id) {
          console.log('Wrapping object with ID in array');
          return [data];
        }
        // If it's just an object, wrap it in an array
        console.log('Wrapping object in array');
        return [data];
      }

      // Return empty array as fallback
      console.warn('Returning empty array as fallback');

      // Use mock data as fallback in development
      if (window.location.hostname === 'localhost') {
        console.log('Using mock data as fallback on localhost');
        return MOCK_SCRAPERS;
      }

      return [];
    }

    // If the array is empty and we're on localhost, use mock data
    if (data.length === 0 && window.location.hostname === 'localhost') {
      console.log('API returned empty array. Using mock data as fallback on localhost');
      return MOCK_SCRAPERS;
    }

    // Normalize each scraper in the array
    const normalizedScrapers = data.map(scraper => {
      const normalizedScraper = {};
      Object.keys(scraper).forEach(key => {
        // Convert PascalCase to camelCase
        const camelCaseKey = key.charAt(0).toLowerCase() + key.slice(1);
        normalizedScraper[camelCaseKey] = scraper[key];
      });
      return normalizedScraper;
    });

    console.log('Normalized scrapers:', normalizedScrapers);
    return normalizedScrapers;
  } catch (error) {
    console.error('Error in fetchAllScrapers:', error);

    // Use mock data as fallback in development
    if (window.location.hostname === 'localhost') {
      console.log('Error occurred. Using mock data as fallback on localhost');
      return MOCK_SCRAPERS;
    }

    throw error;
  }
};

// Fetch a specific scraper by ID
export const fetchScraper = async (id) => {
  console.log('🔍🔍🔍 FUNCTION CALLED: fetchScraper 🔍🔍🔍');
  console.log('📌 Fetching scraper with ID:', id);
  console.trace('Call stack for fetchScraper');

  try {
    // First try to get all scrapers to find the one with matching name or ID
    console.log('Getting all scrapers to find the matching one...');
    const allScrapersUrl = `${API_URL}/scraper`;
    console.log('Fetching all scrapers from URL:', allScrapersUrl);

    const allScrapersResponse = await fetch(allScrapersUrl, {
      headers: {
        'Accept': 'application/json'
      }
    });

    if (!allScrapersResponse.ok) {
      const errorText = await allScrapersResponse.text();
      console.error('Server response when getting all scrapers:', allScrapersResponse.status, errorText);
      throw new Error(`API error when getting all scrapers: ${allScrapersResponse.status} - ${errorText}`);
    }

    const allScrapersText = await allScrapersResponse.text();

    // Check if we got HTML instead of JSON
    if (allScrapersText.includes('<!DOCTYPE html>')) {
      console.error('Received HTML instead of JSON when getting all scrapers. API proxy issue detected.');
      throw new Error('API proxy error: Received HTML instead of JSON when getting all scrapers');
    }

    let allScrapers;
    try {
      allScrapers = JSON.parse(allScrapersText);
      console.log('All scrapers:', allScrapers);
    } catch (e) {
      console.error('Error parsing all scrapers response as JSON:', e);
      throw new Error(`Failed to parse all scrapers API response as JSON: ${e.message}`);
    }

    // Find the scraper with matching name or ID
    const matchingScraper = allScrapers.find(s =>
      s.id === id ||
      s.Id === id ||
      s.name === id ||
      s.Name === id
    );

    if (!matchingScraper) {
      console.error('No matching scraper found for ID or name:', id);
      throw new Error(`No scraper found with ID or name: ${id}`);
    }

    console.log('Found matching scraper:', matchingScraper);

    // Get the actual ID
    const actualId = matchingScraper.id || matchingScraper.Id;
    console.log('Actual scraper ID:', actualId);

    // Now fetch the specific scraper with the actual ID
    const url = `${API_URL}/scraper/${actualId}`;
    console.log('Fetching specific scraper from URL:', url);

    // Add Accept header to ensure we get JSON back
    const response = await fetch(url, {
      headers: {
        'Accept': 'application/json'
      }
    });

    // If response is not ok, log the error details
    if (!response.ok) {
      const errorText = await response.text();
      console.error('Server response:', response.status, errorText);

      // Check if we got HTML instead of JSON
      if (errorText.includes('<!DOCTYPE html>')) {
        console.error('Received HTML instead of JSON. API proxy issue detected.');
        throw new Error('API proxy error: Received HTML instead of JSON');
      }

      try {
        // Try to parse as JSON if possible
        const errorJson = JSON.parse(errorText);
        console.error('Error details:', errorJson);
      } catch (e) {
        // If not JSON, log as text
        console.error('Error response (text):', errorText);
      }
      throw new Error(`API error: ${response.status} - ${errorText}`);
    }

    // Get the response as text first to check for HTML
    const responseText = await response.text();
    console.log('Response text:', responseText.substring(0, 200) + '...'); // Log first 200 chars

    // Check if we got HTML instead of JSON
    if (responseText.includes('<!DOCTYPE html>')) {
      console.error('Received HTML instead of JSON. API proxy issue detected.');
      throw new Error('API proxy error: Received HTML instead of JSON');
    }

    // Parse the response as JSON
    let data;
    try {
      data = JSON.parse(responseText);
    } catch (e) {
      console.error('Error parsing response as JSON:', e);
      throw new Error(`Failed to parse API response as JSON: ${e.message}`);
    }

    console.log('Fetched scraper data (raw):', data);

    // Handle case where data is null or undefined
    if (!data) {
      console.error('API returned null or undefined data');
      throw new Error('API returned no data for the requested scraper');
    }

    // Normalize the data - convert all property names to camelCase
    const normalizedData = {};
    Object.keys(data).forEach(key => {
      // Convert PascalCase to camelCase (e.g., "StartUrl" to "startUrl")
      const camelCaseKey = key.charAt(0).toLowerCase() + key.slice(1);
      normalizedData[camelCaseKey] = data[key];
    });

    // Special handling for array properties
    if (normalizedData.includePatterns && Array.isArray(normalizedData.includePatterns)) {
      normalizedData.includePatterns = normalizedData.includePatterns.join('\n');
    }

    if (normalizedData.excludePatterns && Array.isArray(normalizedData.excludePatterns)) {
      normalizedData.excludePatterns = normalizedData.excludePatterns.join('\n');
    }

    // Special handling for object properties
    if (normalizedData.customHeaders && typeof normalizedData.customHeaders === 'object') {
      normalizedData.customHeaders = JSON.stringify(normalizedData.customHeaders, null, 2);
    }

    // Ensure required properties exist
    normalizedData.id = normalizedData.id || actualId;
    normalizedData.name = normalizedData.name || data.Name || normalizedData.baseUrl || data.BaseUrl || 'Unnamed Scraper';
    normalizedData.startUrl = normalizedData.startUrl || data.StartUrl || '';
    normalizedData.baseUrl = normalizedData.baseUrl || data.BaseUrl || '';

    console.log('Fetched scraper data (normalized):', normalizedData);
    return normalizedData;
  } catch (error) {
    console.error('Error fetching scraper:', error);
    throw error;
  }
};

// Create a new scraper
export const createScraper = async (scraperConfig) => {
  // Ensure all required fields are present with default values if not provided
  const completeConfig = {
    // Required fields with defaults
    name: scraperConfig.name || 'New Scraper',
    startUrl: scraperConfig.startUrl,
    baseUrl: scraperConfig.baseUrl,

    // Optional fields with defaults
    outputDirectory: scraperConfig.outputDirectory || 'ScrapedData',
    delayBetweenRequests: scraperConfig.delayBetweenRequests || 1000,
    maxConcurrentRequests: scraperConfig.maxConcurrentRequests || 5,
    maxDepth: scraperConfig.maxDepth || 5,
    followExternalLinks: scraperConfig.followExternalLinks || false,
    respectRobotsTxt: scraperConfig.respectRobotsTxt !== undefined ? scraperConfig.respectRobotsTxt : true,

    // Header/footer pattern learning
    autoLearnHeaderFooter: scraperConfig.autoLearnHeaderFooter !== undefined ? scraperConfig.autoLearnHeaderFooter : true,
    learningPagesCount: scraperConfig.learningPagesCount || 5,

    // Content Change Detection
    enableChangeDetection: scraperConfig.enableChangeDetection !== undefined ? scraperConfig.enableChangeDetection : true,
    trackContentVersions: scraperConfig.trackContentVersions !== undefined ? scraperConfig.trackContentVersions : true,
    maxVersionsToKeep: scraperConfig.maxVersionsToKeep || 5,

    // Adaptive Crawling
    enableAdaptiveCrawling: scraperConfig.enableAdaptiveCrawling !== undefined ? scraperConfig.enableAdaptiveCrawling : true,
    priorityQueueSize: scraperConfig.priorityQueueSize || 100,
    adjustDepthBasedOnQuality: scraperConfig.adjustDepthBasedOnQuality !== undefined ? scraperConfig.adjustDepthBasedOnQuality : true,

    // Smart Rate Limiting
    enableAdaptiveRateLimiting: scraperConfig.enableAdaptiveRateLimiting !== undefined ? scraperConfig.enableAdaptiveRateLimiting : true,
    minDelayBetweenRequests: scraperConfig.minDelayBetweenRequests || 500,
    maxDelayBetweenRequests: scraperConfig.maxDelayBetweenRequests || 5000,
    monitorResponseTimes: scraperConfig.monitorResponseTimes !== undefined ? scraperConfig.monitorResponseTimes : true,

    // Monitoring settings
    enableContinuousMonitoring: scraperConfig.enableContinuousMonitoring !== undefined ? scraperConfig.enableContinuousMonitoring : false,
    monitoringIntervalMinutes: scraperConfig.monitoringIntervalMinutes || 1440, // Default: 24 hours
    notifyOnChanges: scraperConfig.notifyOnChanges !== undefined ? scraperConfig.notifyOnChanges : false,
    notificationEmail: scraperConfig.notificationEmail || 'no-reply@example.com', // Default email address
    trackChangesHistory: scraperConfig.trackChangesHistory !== undefined ? scraperConfig.trackChangesHistory : true,

    // Any other fields from the original config
    ...scraperConfig
  };

  // Log the request payload for debugging
  console.log('Creating scraper with payload:', completeConfig);

  try {
    const response = await fetch(`${API_URL}/scraper`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(completeConfig)
    });

    // If response is not ok, log the error details
    if (!response.ok) {
      const errorText = await response.text();
      console.error('Server response:', response.status, errorText);
      try {
        // Try to parse as JSON if possible
        const errorJson = JSON.parse(errorText);
        console.error('Error details:', errorJson);
      } catch (e) {
        // If not JSON, log as text
        console.error('Error response (text):', errorText);
      }
      throw new Error(`API error: ${response.status} - ${errorText}`);
    }

    return response.json();
  } catch (error) {
    console.error('Error creating scraper:', error);
    throw error;
  }
};

// Update an existing scraper
export const updateScraper = async (id, scraperConfig) => {
  // Ensure NotificationEmail is set
  const updatedConfig = {
    ...scraperConfig,
    notificationEmail: scraperConfig.notificationEmail || 'no-reply@example.com'
  };

  console.log('Updating scraper with payload:', updatedConfig);

  try {
    const response = await fetch(`${API_URL}/scraper/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(updatedConfig)
    });

    // If response is not ok, log the error details
    if (!response.ok) {
      const errorText = await response.text();
      console.error('Server response:', response.status, errorText);
      try {
        // Try to parse as JSON if possible
        const errorJson = JSON.parse(errorText);
        console.error('Error details:', errorJson);
      } catch (e) {
        // If not JSON, log as text
        console.error('Error response (text):', errorText);
      }
      throw new Error(`API error: ${response.status} - ${errorText}`);
    }

    return response.json();
  } catch (error) {
    console.error('Error updating scraper:', error);
    throw error;
  }
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
  try {
    const response = await fetch(`${API_URL}/scraper/${id}/status`);

    if (!response.ok) {
      const errorText = await response.text();
      console.error('Status response error:', response.status, errorText);
      throw new Error(`API error: ${response.status} - ${errorText}`);
    }

    const data = await response.json();
    console.log('Fetched status data (raw):', data);

    // Normalize the data - convert all property names to camelCase
    const normalizedData = {};
    Object.keys(data).forEach(key => {
      // Convert PascalCase to camelCase (e.g., "IsRunning" to "isRunning")
      const camelCaseKey = key.charAt(0).toLowerCase() + key.slice(1);
      normalizedData[camelCaseKey] = data[key];
    });

    console.log('Fetched status data (normalized):', normalizedData);
    return normalizedData;
  } catch (error) {
    console.error('Error fetching scraper status:', error);
    throw error;
  }
};

// Get scraper logs
export const fetchScraperLogs = async (id) => {
  try {
    const response = await fetch(`${API_URL}/scraper/${id}/logs`);

    if (!response.ok) {
      const errorText = await response.text();
      console.error('Logs response error:', response.status, errorText);
      throw new Error(`API error: ${response.status} - ${errorText}`);
    }

    const data = await response.json();
    console.log('Fetched logs data (raw):', data);

    // Normalize the data structure
    let logs = [];

    // Check if logs are in a nested property (common API pattern)
    if (data.Logs) {
      logs = data.Logs;
    } else if (data.logs) {
      logs = data.logs;
    } else if (Array.isArray(data)) {
      logs = data;
    }

    // Normalize each log entry
    const normalizedLogs = logs.map(log => {
      const normalizedLog = {};
      Object.keys(log).forEach(key => {
        // Convert PascalCase to camelCase
        const camelCaseKey = key.charAt(0).toLowerCase() + key.slice(1);
        normalizedLog[camelCaseKey] = log[key];
      });
      return normalizedLog;
    });

    console.log('Fetched logs data (normalized):', normalizedLogs);
    return normalizedLogs;
  } catch (error) {
    console.error('Error fetching scraper logs:', error);
    return []; // Return empty array on error to avoid breaking the UI
  }
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

// Save scraper (create or update)
export const saveScraper = async (scraperConfig, id = null) => {
  if (id) {
    return updateScraper(id, scraperConfig);
  } else {
    return createScraper(scraperConfig);
  }
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