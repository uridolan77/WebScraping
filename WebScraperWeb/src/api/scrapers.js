// src/api/scrapers.js
import apiClient, { handleResponse } from './index';
import { handleApiError } from '../utils/errorHandler';

/**
 * Get all scrapers
 * @returns {Promise<Array>} List of scrapers
 */
export const getAllScrapers = async () => {
  try {
    return handleResponse(apiClient.get('/Scraper'));
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch scrapers');
  }
};

/**
 * Get a single scraper by ID
 * @param {string} id - Scraper ID
 * @returns {Promise<Object>} Scraper details
 */
export const getScraper = async (id) => {
  try {
    return handleResponse(apiClient.get(`/Scraper/${id}`));
  } catch (error) {
    throw handleApiError(error, `Failed to fetch scraper with ID ${id}`);
  }
};

/**
 * Create a new scraper
 * @param {Object} scraperData - Scraper configuration
 * @returns {Promise<Object>} Created scraper
 */
export const createScraper = async (scraperData) => {
  try {
    // Ensure required fields are present
    const requiredFields = ['name', 'startUrl', 'baseUrl'];
    for (const field of requiredFields) {
      if (!scraperData[field]) {
        throw new Error(`Missing required field: ${field}`);
      }
    }

    // Ensure ID is a string if provided
    if (scraperData.id && typeof scraperData.id !== 'string') {
      scraperData.id = String(scraperData.id);
    }

    // Set default values for required fields if not provided
    const defaultData = {
      outputDirectory: 'ScrapedData',
      maxDepth: 5,
      maxPages: 1000,
      maxConcurrentRequests: 5,
      delayBetweenRequests: 1000,
      followLinks: true,
      followExternalLinks: false,
      respectRobotsTxt: true
    };

    const mergedData = { ...defaultData, ...scraperData };

    // Convert camelCase to PascalCase for .NET API
    const pascalCaseData = {};
    Object.keys(mergedData).forEach(key => {
      // Convert first character to uppercase
      const pascalKey = key.charAt(0).toUpperCase() + key.slice(1);
      pascalCaseData[pascalKey] = mergedData[key];
    });

    console.log('Sending scraper data to API:', pascalCaseData);
    return handleResponse(apiClient.post('/Scraper', pascalCaseData));
  } catch (error) {
    console.error('Error creating scraper:', error);
    throw handleApiError(error, 'Failed to create scraper');
  }
};

/**
 * Update an existing scraper
 * @param {string} id - Scraper ID
 * @param {Object} scraperData - Updated scraper configuration
 * @returns {Promise<Object>} Updated scraper
 */
export const updateScraper = async (id, scraperData) => {
  try {
    // Convert camelCase to PascalCase for .NET API
    const pascalCaseData = {};
    Object.keys(scraperData).forEach(key => {
      // Convert first character to uppercase
      const pascalKey = key.charAt(0).toUpperCase() + key.slice(1);
      pascalCaseData[pascalKey] = scraperData[key];
    });

    return handleResponse(apiClient.put(`/Scraper/${id}`, pascalCaseData));
  } catch (error) {
    throw handleApiError(error, `Failed to update scraper with ID ${id}`);
  }
};

/**
 * Delete a scraper
 * @param {string} id - Scraper ID
 * @returns {Promise<void>}
 */
export const deleteScraper = async (id) => {
  try {
    return handleResponse(apiClient.delete(`/Scraper/${id}`));
  } catch (error) {
    throw handleApiError(error, `Failed to delete scraper with ID ${id}`);
  }
};

/**
 * Get scraper status
 * @param {string} id - Scraper ID
 * @returns {Promise<Object>} Scraper status
 */
export const getScraperStatus = async (id) => {
  try {
    return handleResponse(apiClient.get(`/Scraper/${id}/status`));
  } catch (error) {
    throw handleApiError(error, `Failed to get status for scraper with ID ${id}`);
  }
};

/**
 * Get scraper logs
 * @param {string} id - Scraper ID
 * @param {number} limit - Maximum number of log entries to return
 * @returns {Promise<Array>} Scraper logs
 */
export const getScraperLogs = async (id, limit = 100) => {
  try {
    return handleResponse(apiClient.get(`/Scraper/${id}/logs`, {
      params: { limit }
    }));
  } catch (error) {
    throw handleApiError(error, `Failed to get logs for scraper with ID ${id}`);
  }
};

/**
 * Start a scraper
 * @param {string} id - Scraper ID
 * @returns {Promise<Object>} Start result
 */
export const startScraper = async (id) => {
  try {
    return handleResponse(apiClient.post(`/Scraper/${id}/start`));
  } catch (error) {
    throw handleApiError(error, `Failed to start scraper with ID ${id}`);
  }
};

/**
 * Stop a scraper
 * @param {string} id - Scraper ID
 * @returns {Promise<Object>} Stop result
 */
export const stopScraper = async (id) => {
  try {
    return handleResponse(apiClient.post(`/Scraper/${id}/stop`));
  } catch (error) {
    throw handleApiError(error, `Failed to stop scraper with ID ${id}`);
  }
};

/**
 * Set monitoring settings
 * @param {string} id - Scraper ID
 * @param {Object} settings - Monitoring settings
 * @returns {Promise<Object>} Updated monitoring settings
 */
export const setMonitoring = async (id, settings) => {
  try {
    return handleResponse(apiClient.post(`/Scraper/${id}/monitor`, settings));
  } catch (error) {
    throw handleApiError(error, `Failed to set monitoring for scraper with ID ${id}`);
  }
};

/**
 * Get scraper results
 * @param {number} page - Page number
 * @param {number} pageSize - Page size
 * @param {string} search - Search term
 * @param {string} scraperId - Scraper ID
 * @returns {Promise<Object>} Scraper results with pagination
 */
export const getScraperResults = async (page = 1, pageSize = 20, search = null, scraperId = null) => {
  try {
    const params = { page, pageSize };
    if (search) params.search = search;
    if (scraperId) params.scraperId = scraperId;

    return handleResponse(apiClient.get('/Scraper/results', { params }));
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch scraper results');
  }
};

/**
 * Get detected changes for a scraper
 * @param {string} id - Scraper ID
 * @param {Date} since - Date to get changes since
 * @param {number} limit - Maximum number of changes to return
 * @returns {Promise<Array>} Detected changes
 */
export const getDetectedChanges = async (id, since = null, limit = 100) => {
  try {
    const params = { limit };
    if (since) params.since = since.toISOString();

    return handleResponse(apiClient.get(`/Scraper/${id}/changes`, { params }));
  } catch (error) {
    throw handleApiError(error, `Failed to fetch detected changes for scraper with ID ${id}`);
  }
};

/**
 * Get processed documents
 * @param {string} id - Scraper ID
 * @param {string} documentType - Document type filter
 * @param {number} page - Page number
 * @param {number} pageSize - Page size
 * @returns {Promise<Object>} Processed documents with pagination
 */
export const getProcessedDocuments = async (id, documentType = null, page = 1, pageSize = 20) => {
  try {
    const params = { page, pageSize };
    if (documentType) params.documentType = documentType;

    return handleResponse(apiClient.get(`/Scraper/${id}/documents`, { params }));
  } catch (error) {
    throw handleApiError(error, `Failed to fetch processed documents for scraper with ID ${id}`);
  }
};

/**
 * Compress stored content
 * @param {string} id - Scraper ID
 * @returns {Promise<Object>} Compression result
 */
export const compressStoredContent = async (id) => {
  try {
    return handleResponse(apiClient.post(`/Scraper/${id}/compress`));
  } catch (error) {
    throw handleApiError(error, `Failed to compress content for scraper with ID ${id}`);
  }
};
