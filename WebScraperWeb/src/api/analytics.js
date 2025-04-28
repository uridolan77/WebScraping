// src/api/analytics.js
import apiClient from './index';

// Get overall analytics data
export const getOverallAnalytics = async (timeframe = 'week') => {
  try {
    const response = await apiClient.get('/Analytics/overall', {
      params: { timeframe }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching overall analytics:', error);
    throw error;
  }
};

// Get scraper-specific analytics
export const getScraperAnalytics = async (id, timeframe = 'week') => {
  try {
    const response = await apiClient.get(`/Analytics/scraper/${id}`, {
      params: { timeframe }
    });
    return response.data;
  } catch (error) {
    console.error(`Error fetching analytics for scraper with id ${id}:`, error);
    throw error;
  }
};

// Get content change analytics
export const getContentChangeAnalytics = async (timeframe = 'month') => {
  try {
    const response = await apiClient.get('/Analytics/changes', {
      params: { timeframe }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching content change analytics:', error);
    throw error;
  }
};

// Get performance metrics
export const getPerformanceMetrics = async (timeframe = 'week') => {
  try {
    const response = await apiClient.get('/Analytics/performance', {
      params: { timeframe }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching performance metrics:', error);
    throw error;
  }
};

// Get content type distribution
export const getContentTypeDistribution = async (scraperId = null) => {
  try {
    const params = {};
    if (scraperId) params.scraperId = scraperId;
    
    const response = await apiClient.get('/Analytics/content-types', { params });
    return response.data;
  } catch (error) {
    console.error('Error fetching content type distribution:', error);
    throw error;
  }
};

// Get regulatory impact analysis
export const getRegulatoryImpactAnalysis = async (timeframe = 'month') => {
  try {
    const response = await apiClient.get('/Analytics/regulatory-impact', {
      params: { timeframe }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching regulatory impact analysis:', error);
    throw error;
  }
};
