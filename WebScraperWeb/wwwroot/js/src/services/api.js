// API service for the multi-scraper functionality

// Base API URL
const API_URL = 'https://localhost:7143/api';

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