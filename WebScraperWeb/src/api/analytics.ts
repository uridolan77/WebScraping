// src/api/analytics.js
import apiClient, { handleResponse, cachedGet, clearCache } from './index';
import { handleApiError } from '../utils/errorHandler';

// Cache TTL constants (in milliseconds)
const CACHE_TTL = {
  SHORT: 2 * 60 * 1000,    // 2 minutes
  MEDIUM: 10 * 60 * 1000,  // 10 minutes
  LONG: 30 * 60 * 1000     // 30 minutes
};

/**
 * Get overall analytics data
 * @param {string} timeframe - Time period for analytics (day, week, month, year)
 * @param {boolean} forceRefresh - Force a refresh of the cache
 * @returns {Promise<Object>} Overall analytics data
 */
export const getOverallAnalytics = async (timeframe = 'week', forceRefresh = false) => {
  try {
    return await cachedGet('/Analytics/overall', {
      params: { timeframe },
      cacheTTL: CACHE_TTL.MEDIUM,
      forceRefresh
    });
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch overall analytics');
  }
};

/**
 * Get analytics data for a specific scraper
 * @param {string} id - Scraper ID
 * @param {string} timeframe - Time period for analytics (day, week, month, year)
 * @param {boolean} forceRefresh - Force a refresh of the cache
 * @returns {Promise<Object>} Scraper-specific analytics data
 */
export const getScraperAnalytics = async (id: string, timeframe: string = 'week', forceRefresh: boolean = false) => {
  try {
    return await cachedGet(`/Analytics/scraper/${id}`, {
      params: { timeframe },
      cacheTTL: CACHE_TTL.MEDIUM,
      forceRefresh
    });
  } catch (error) {
    throw handleApiError(error, `Failed to fetch analytics for scraper with ID ${id}`);
  }
};

/**
 * Get content change analytics
 * @param {string} timeframe - Time period for analytics (day, week, month, year)
 * @param {boolean} forceRefresh - Force a refresh of the cache
 * @returns {Promise<Object>} Content change analytics data
 */
export const getContentChangeAnalytics = async (timeframe: string = 'month', forceRefresh: boolean = false) => {
  try {
    return await cachedGet('/Analytics/changes', {
      params: { timeframe },
      cacheTTL: CACHE_TTL.MEDIUM,
      forceRefresh
    });
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch content change analytics');
  }
};

/**
 * Get performance metrics
 * @param {string} timeframe - Time period for analytics (day, week, month, year)
 * @param {boolean} forceRefresh - Force a refresh of the cache
 * @returns {Promise<Object>} Performance metrics data
 */
export const getPerformanceMetrics = async (timeframe: string = 'week', forceRefresh: boolean = false) => {
  try {
    return await cachedGet('/Analytics/performance', {
      params: { timeframe },
      cacheTTL: CACHE_TTL.SHORT, // Performance data changes more frequently
      forceRefresh
    });
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch performance metrics');
  }
};

/**
 * Get content type distribution
 * @param {string} scraperId - Optional scraper ID to filter by
 * @param {boolean} forceRefresh - Force a refresh of the cache
 * @returns {Promise<Object>} Content type distribution data
 */
export const getContentTypeDistribution = async (scraperId: string | null = null, forceRefresh: boolean = false) => {
  try {
    const params: Record<string, string> = {};
    if (scraperId) params.scraperId = scraperId;

    return await cachedGet('/Analytics/content-types', {
      params,
      cacheTTL: CACHE_TTL.LONG, // Content type distribution changes infrequently
      forceRefresh
    });
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch content type distribution');
  }
};

/**
 * Get regulatory impact analysis
 * @param {string} timeframe - Time period for analytics (day, week, month, year)
 * @param {boolean} forceRefresh - Force a refresh of the cache
 * @returns {Promise<Object>} Regulatory impact analysis data
 */
export const getRegulatoryImpactAnalysis = async (timeframe: string = 'month', forceRefresh: boolean = false) => {
  try {
    return await cachedGet('/Analytics/regulatory-impact', {
      params: { timeframe },
      cacheTTL: CACHE_TTL.MEDIUM,
      forceRefresh
    });
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch regulatory impact analysis');
  }
};

/**
 * Get trend analysis
 * @param {string} metric - Metric to analyze (pages, changes, errors)
 * @param {string} timeframe - Time period for analytics (day, week, month, year)
 * @param {boolean} forceRefresh - Force a refresh of the cache
 * @returns {Promise<Object>} Trend analysis data
 */
export const getTrendAnalysis = async (metric: string = 'changes', timeframe: string = 'month', forceRefresh: boolean = false) => {
  try {
    return await cachedGet('/Analytics/trends', {
      params: { metric, timeframe },
      cacheTTL: CACHE_TTL.MEDIUM,
      forceRefresh
    });
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch trend analysis');
  }
};

/**
 * Get comparison data between scrapers
 * @param {Array<string>} scraperIds - Array of scraper IDs to compare
 * @param {string} metric - Metric to compare (pages, changes, errors, performance)
 * @param {string} timeframe - Time period for comparison (day, week, month, year)
 * @returns {Promise<Object>} Comparison data
 */
export const getScraperComparison = async (scraperIds: string[], metric: string = 'performance', timeframe: string = 'month') => {
  try {
    // Note: We don't cache POST requests
    return handleResponse(apiClient.post('/Analytics/compare', {
      scraperIds,
      metric,
      timeframe
    }));
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch scraper comparison data');
  }
};

/**
 * Clear all analytics cache
 */
export const clearAnalyticsCache = () => {
  clearCache('/Analytics');
};

/**
 * Clear cache for a specific analytics endpoint
 * @param {string} endpoint - Analytics endpoint (e.g., 'overall', 'changes', 'performance')
 */
export const clearAnalyticsEndpointCache = (endpoint: string) => {
  clearCache(`/Analytics/${endpoint}`);
};

/**
 * Get analytics summary
 * @returns {Promise<Object>} Analytics summary data
 */
export const getAnalyticsSummary = async () => {
  try {
    return await cachedGet('/Analytics/summary', {
      cacheTTL: CACHE_TTL.SHORT
    });
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch analytics summary');
  }
};

/**
 * Get popular domains
 * @param {number} limit - Maximum number of domains to return
 * @returns {Promise<Object>} Popular domains data
 */
export const getPopularDomains = async (limit: number = 10) => {
  try {
    return await cachedGet('/Analytics/popular-domains', {
      params: { limit },
      cacheTTL: CACHE_TTL.MEDIUM
    });
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch popular domains');
  }
};

/**
 * Get content change frequency
 * @param {Object} options - Options
 * @param {Date} options.since - Date to get changes since
 * @returns {Promise<Object>} Content change frequency data
 */
export const getContentChangeFrequency = async (options: { since?: Date } = {}) => {
  try {
    const params: Record<string, string> = {};
    if (options.since) params.since = options.since.toISOString();

    return await cachedGet('/Analytics/change-frequency', {
      params,
      cacheTTL: CACHE_TTL.MEDIUM
    });
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch content change frequency');
  }
};
