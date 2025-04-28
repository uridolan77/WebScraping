// src/api/scrapers.ts
import apiClient from './client';
import { handleResponse } from './index';
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
export const getScraper = async (id: string) => {
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
export const createScraper = async (scraperData: any) => {
  try {
    return handleResponse(apiClient.post('/Scraper', scraperData));
  } catch (error) {
    throw handleApiError(error, 'Failed to create scraper');
  }
};

/**
 * Update an existing scraper
 * @param {string} id - Scraper ID
 * @param {Object} scraperData - Updated scraper configuration
 * @returns {Promise<Object>} Updated scraper
 */
export const updateScraper = async (id: string, scraperData: any) => {
  try {
    return handleResponse(apiClient.put(`/Scraper/${id}`, scraperData));
  } catch (error) {
    throw handleApiError(error, `Failed to update scraper with ID ${id}`);
  }
};

/**
 * Delete a scraper
 * @param {string} id - Scraper ID
 * @returns {Promise<void>}
 */
export const deleteScraper = async (id: string) => {
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
export const getScraperStatus = async (id: string) => {
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
export const getScraperLogs = async (id: string, limit: number = 100) => {
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
export const startScraper = async (id: string) => {
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
export const stopScraper = async (id: string) => {
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
export const setMonitoring = async (id: string, settings: any) => {
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
export const getScraperResults = async (page: number = 1, pageSize: number = 20, search: string | null = null, scraperId: string | null = null) => {
  try {
    const params: Record<string, any> = { page, pageSize };
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
export const getDetectedChanges = async (id: string, since: Date | null = null, limit: number = 100) => {
  try {
    const params: Record<string, any> = { limit };
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
export const getProcessedDocuments = async (id: string, documentType: string | null = null, page: number = 1, pageSize: number = 20) => {
  try {
    const params: Record<string, any> = { page, pageSize };
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
export const compressStoredContent = async (id: string) => {
  try {
    return handleResponse(apiClient.post(`/Scraper/${id}/compress`));
  } catch (error) {
    throw handleApiError(error, `Failed to compress content for scraper with ID ${id}`);
  }
};
