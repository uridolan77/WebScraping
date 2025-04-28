// src/api/analytics.js
import apiClient, { handleResponse } from './index';
import { handleApiError } from '../utils/errorHandler';

/**
 * Get overall analytics data
 * @param {string} timeframe - Time period for analytics (day, week, month, year)
 * @returns {Promise<Object>} Overall analytics data
 */
export const getOverallAnalytics = async (timeframe = 'week') => {
  try {
    return handleResponse(apiClient.get('/Analytics/overall', {
      params: { timeframe }
    }));
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch overall analytics');
  }
};

/**
 * Get analytics data for a specific scraper
 * @param {string} id - Scraper ID
 * @param {string} timeframe - Time period for analytics (day, week, month, year)
 * @returns {Promise<Object>} Scraper-specific analytics data
 */
export const getScraperAnalytics = async (id, timeframe = 'week') => {
  try {
    return handleResponse(apiClient.get(`/Analytics/scraper/${id}`, {
      params: { timeframe }
    }));
  } catch (error) {
    throw handleApiError(error, `Failed to fetch analytics for scraper with ID ${id}`);
  }
};

/**
 * Get content change analytics
 * @param {string} timeframe - Time period for analytics (day, week, month, year)
 * @returns {Promise<Object>} Content change analytics data
 */
export const getContentChangeAnalytics = async (timeframe = 'month') => {
  try {
    return handleResponse(apiClient.get('/Analytics/changes', {
      params: { timeframe }
    }));
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch content change analytics');
  }
};

/**
 * Get performance metrics
 * @param {string} timeframe - Time period for analytics (day, week, month, year)
 * @returns {Promise<Object>} Performance metrics data
 */
export const getPerformanceMetrics = async (timeframe = 'week') => {
  try {
    return handleResponse(apiClient.get('/Analytics/performance', {
      params: { timeframe }
    }));
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch performance metrics');
  }
};

/**
 * Get content type distribution
 * @param {string} scraperId - Optional scraper ID to filter by
 * @returns {Promise<Object>} Content type distribution data
 */
export const getContentTypeDistribution = async (scraperId = null) => {
  try {
    const params = {};
    if (scraperId) params.scraperId = scraperId;

    return handleResponse(apiClient.get('/Analytics/content-types', { params }));
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch content type distribution');
  }
};

/**
 * Get regulatory impact analysis
 * @param {string} timeframe - Time period for analytics (day, week, month, year)
 * @returns {Promise<Object>} Regulatory impact analysis data
 */
export const getRegulatoryImpactAnalysis = async (timeframe = 'month') => {
  try {
    return handleResponse(apiClient.get('/Analytics/regulatory-impact', {
      params: { timeframe }
    }));
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch regulatory impact analysis');
  }
};

/**
 * Get trend analysis
 * @param {string} metric - Metric to analyze (pages, changes, errors)
 * @param {string} timeframe - Time period for analytics (day, week, month, year)
 * @returns {Promise<Object>} Trend analysis data
 */
export const getTrendAnalysis = async (metric = 'changes', timeframe = 'month') => {
  try {
    return handleResponse(apiClient.get('/Analytics/trends', {
      params: { metric, timeframe }
    }));
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
export const getScraperComparison = async (scraperIds, metric = 'performance', timeframe = 'month') => {
  try {
    return handleResponse(apiClient.post('/Analytics/compare', {
      scraperIds,
      metric,
      timeframe
    }));
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch scraper comparison data');
  }
};
