import { handleResponse, handleApiError, apiClient, CACHE_TTL, cachedGet } from './apiUtils';

/**
 * Get content classifications for a scraper
 * @param {string} scraperId - Scraper ID
 * @param {number} limit - Maximum number of results to return
 * @param {boolean} forceRefresh - Force a refresh of the cache
 * @returns {Promise<Array>} Content classifications
 */
export const getContentClassifications = async (scraperId, limit = 50, forceRefresh = false) => {
  try {
    return await cachedGet(`/ContentClassification/scraper/${scraperId}`, {
      params: { limit },
      cacheTTL: CACHE_TTL.SHORT,
      forceRefresh
    });
  } catch (error) {
    throw handleApiError(error, `Failed to fetch content classifications for scraper with ID ${scraperId}`);
  }
};

/**
 * Get content classification for a URL
 * @param {string} scraperId - Scraper ID
 * @param {string} url - URL to get classification for
 * @returns {Promise<Object>} Content classification
 */
export const getContentClassification = async (scraperId, url) => {
  try {
    return handleResponse(apiClient.get(`/ContentClassification/scraper/${scraperId}/url`, {
      params: { url }
    }));
  } catch (error) {
    throw handleApiError(error, `Failed to fetch content classification for URL ${url}`);
  }
};

/**
 * Get classification statistics for a scraper
 * @param {string} scraperId - Scraper ID
 * @param {boolean} forceRefresh - Force a refresh of the cache
 * @returns {Promise<Object>} Classification statistics
 */
export const getClassificationStatistics = async (scraperId, forceRefresh = false) => {
  try {
    return await cachedGet(`/ContentClassification/scraper/${scraperId}/statistics`, {
      cacheTTL: CACHE_TTL.MEDIUM,
      forceRefresh
    });
  } catch (error) {
    throw handleApiError(error, `Failed to fetch classification statistics for scraper with ID ${scraperId}`);
  }
};

/**
 * Classify content
 * @param {string} scraperId - Scraper ID
 * @param {string} url - URL of the content
 * @param {string} content - Content to classify
 * @returns {Promise<Object>} Classification result
 */
export const classifyContent = async (scraperId, url, content) => {
  try {
    return handleResponse(apiClient.post(`/ContentClassification/scraper/${scraperId}/classify`, {
      url,
      content
    }));
  } catch (error) {
    throw handleApiError(error, 'Failed to classify content');
  }
};
