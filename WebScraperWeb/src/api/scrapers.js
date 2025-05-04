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
    // Pass the promise directly to handleResponse instead of awaiting it first
    return handleResponse(apiClient.get(`/Scraper/${id}`));
  } catch (error) {
    // Special handling for 404 errors (scraper not found)
    if (error.response && error.response.status === 404) {
      console.warn(`Scraper with ID ${id} not found. It may have been deleted.`);
      // Return a default object indicating the scraper doesn't exist
      return { 
        id, 
        notFound: true, 
        name: 'Scraper not found', 
        message: 'This scraper may have been deleted or never existed.'
      };
    }
    
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
    console.log('Original scraper data:', scraperData);

    // Create a deep copy to avoid modifying the original object
    const processedData = JSON.parse(JSON.stringify(scraperData));

    // Validate required fields but don't set defaults
    const requiredFields = ['name', 'startUrl', 'baseUrl'];
    for (const field of requiredFields) {
      if (!processedData[field] || processedData[field] === '') {
        throw new Error(`Missing required field: ${field}`);
      }
    }

    // Ensure ID is a string if provided
    if (processedData.id && typeof processedData.id !== 'string') {
      processedData.id = String(processedData.id);
    }

    // Only set runCount default if not provided
    if (processedData.runCount === undefined) {
      processedData.runCount = 0;
    }

    // Handle arrays properly
    // Convert startUrls to an empty array if it's not an array
    if (!processedData.startUrls || !Array.isArray(processedData.startUrls)) {
      processedData.startUrls = [];
    }

    // Add the main startUrl to startUrls if not already there
    if (processedData.startUrl && !processedData.startUrls.some(url => url === processedData.startUrl)) {
      processedData.startUrls.push(processedData.startUrl);
    }

    // Convert contentExtractorSelectors to an empty array if it's not an array
    if (!processedData.contentExtractorSelectors || !Array.isArray(processedData.contentExtractorSelectors)) {
      processedData.contentExtractorSelectors = [];
    }

    // Convert contentExtractorExcludeSelectors to an empty array if it's not an array
    if (!processedData.contentExtractorExcludeSelectors || !Array.isArray(processedData.contentExtractorExcludeSelectors)) {
      processedData.contentExtractorExcludeSelectors = [];
    }

    // Use the processed data directly without merging with defaults
    const mergedData = processedData;

    // Convert camelCase to PascalCase for .NET API
    const pascalCaseData = {};
    Object.keys(mergedData).forEach(key => {
      // Convert first character to uppercase
      const pascalKey = key.charAt(0).toUpperCase() + key.slice(1);

      // Handle arrays specially to ensure they're properly formatted
      if (Array.isArray(mergedData[key])) {
        pascalCaseData[pascalKey] = [...mergedData[key]]; // Create a new array to avoid reference issues
      } else {
        pascalCaseData[pascalKey] = mergedData[key];
      }
    });

    // Explicitly set the Id property to ensure it's correct
    pascalCaseData.Id = id;

    // Also ensure the id property is set (some APIs might use lowercase)
    pascalCaseData.id = id;

    // Ensure StartUrl is set (this is a required field)
    if (!pascalCaseData.StartUrl || pascalCaseData.StartUrl.trim() === '') {
      // Try to get StartUrl from StartUrls if available
      if (Array.isArray(pascalCaseData.StartUrls) && pascalCaseData.StartUrls.length > 0) {
        pascalCaseData.StartUrl = pascalCaseData.StartUrls[0];
      } else {
        // Don't set a default value, let the validation handle it
        throw new Error('StartUrl is required');
      }
    }

    // Ensure StartUrls is an array
    if (!Array.isArray(pascalCaseData.StartUrls)) {
      pascalCaseData.StartUrls = [];
    }

    // Add StartUrl to StartUrls if not already there
    if (pascalCaseData.StartUrl && !pascalCaseData.StartUrls.includes(pascalCaseData.StartUrl)) {
      pascalCaseData.StartUrls.push(pascalCaseData.StartUrl);
    }

    // Ensure ContentExtractorSelectors is an array
    if (!Array.isArray(pascalCaseData.ContentExtractorSelectors)) {
      pascalCaseData.ContentExtractorSelectors = [];
    }

    // Ensure ContentExtractorExcludeSelectors is an array
    if (!Array.isArray(pascalCaseData.ContentExtractorExcludeSelectors)) {
      pascalCaseData.ContentExtractorExcludeSelectors = [];
    }

    // Ensure KeywordAlertList is an array
    if (!Array.isArray(pascalCaseData.KeywordAlertList)) {
      pascalCaseData.KeywordAlertList = [];
    }

    // Ensure WebhookTriggers is an array
    if (!Array.isArray(pascalCaseData.WebhookTriggers)) {
      pascalCaseData.WebhookTriggers = [];
    }

    // Ensure Schedules is an array
    if (!Array.isArray(pascalCaseData.Schedules)) {
      pascalCaseData.Schedules = [];
    }

    console.log('Sending scraper data to API:', pascalCaseData);
    console.log('StartUrls:', pascalCaseData.StartUrls);
    console.log('ContentExtractorSelectors:', pascalCaseData.ContentExtractorSelectors);
    console.log('ContentExtractorExcludeSelectors:', pascalCaseData.ContentExtractorExcludeSelectors);

    // Final validation for StartUrl
    if (!pascalCaseData.StartUrl || pascalCaseData.StartUrl.trim() === '') {
      console.error('StartUrl is still empty after all checks.');
      throw new Error('StartUrl is required');
    }

    // Log the StartUrl value for debugging
    console.log('Final StartUrl value:', pascalCaseData.StartUrl);

    // Send the data directly without wrapping it in a 'model' property
    console.log('Final request data:', pascalCaseData);

    return handleResponse(apiClient.put(`/Scraper/${id}`, pascalCaseData));
  } catch (error) {
    console.error('Error updating scraper:', error);
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
 * Get detailed scraper status
 * @param {string} id - Scraper ID
 * @returns {Promise<Object>} Detailed scraper status including metrics and error details
 */
export const getScraperDetailedStatus = async (id) => {
  try {
    return handleResponse(apiClient.get(`/Scraper/${id}/detailed-status`));
  } catch (error) {
    throw handleApiError(error, `Failed to get detailed status for scraper with ID ${id}`);
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
 * Get real-time monitoring data for a running scraper
 * @param {string} id - Scraper ID
 * @returns {Promise<Object>} Monitoring data
 */
export const getScraperMonitor = async (id) => {
  try {
    return handleResponse(apiClient.get(`/Scraper/${id}/monitor`));
  } catch (error) {
    // Check if this is a 400 error indicating the scraper is not running
    if (error.response && error.response.status === 400) {
      // Extract the error message from the response if available
      const errorMessage = error.response.data?.error ||
                          "Scraper is not currently running. Start the scraper to see real-time monitoring data.";

      // Create a more specific error
      const customError = new Error(errorMessage);
      customError.isScraperNotRunningError = true;
      throw customError;
    }

    // For other errors, use the standard error handler
    throw handleApiError(error, `Failed to get monitoring data for scraper with ID ${id}`);
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

/**
 * Get scraper metrics
 * @param {string} id - Scraper ID
 * @returns {Promise<Object>} Scraper performance metrics
 */
export const getScraperMetrics = async (id) => {
  try {
    const response = await handleResponse(apiClient.get(`/Scraper/${id}/metrics`));
    
    // Process the metrics to make them easier to use in the UI
    const metrics = response.metrics || [];
    const metricSummaries = response.metricSummaries || {};
    
    // Calculate success and failure rates
    const successfulUrls = metricSummaries.successfulUrls || 0;
    const failedUrls = metricSummaries.failedUrls || 0;
    const totalUrls = successfulUrls + failedUrls;
    
    const successRate = totalUrls > 0 
      ? Math.round((successfulUrls / totalUrls) * 100) 
      : 100;
    
    const failureRate = totalUrls > 0 
      ? Math.round((failedUrls / totalUrls) * 100) 
      : 0;
    
    // Return a simplified metrics object for the UI
    return {
      status: response.status,
      metrics: metrics,
      metricSummaries: metricSummaries,
      successRate: successRate,
      failureRate: failureRate,
      performance: metricSummaries.performance || 0,
      urlsProcessed: metricSummaries.urlsProcessed || 0,
      documentsProcessed: metricSummaries.documentsProcessed || 0,
      bytesDownloaded: metricSummaries.bytesDownloaded || 0,
      clientErrors: metricSummaries.clientErrors || 0,
      serverErrors: metricSummaries.serverErrors || 0,
      memoryUsage: metricSummaries.memoryUsage || 0,
      averageResponseTime: metricSummaries.performance || 0
    };
  } catch (error) {
    throw handleApiError(error, `Failed to get metrics for scraper with ID ${id}`);
  }
};
