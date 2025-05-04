// src/api/enhancedFeatures.js
import apiClient from './index';
import { handleResponse, handleApiError } from '../utils/apiUtils';

/**
 * Get enhanced features metrics for a scraper
 * @param {string} id - Scraper ID
 * @returns {Promise<Object>} Enhanced features metrics
 */
export const getEnhancedMetrics = async (id) => {
  try {
    return handleResponse(apiClient.get(`/Scraper/${id}/enhanced-metrics`));
  } catch (error) {
    throw handleApiError(error, `Failed to fetch enhanced metrics for scraper with ID ${id}`);
  }
};

/**
 * Update enhanced features configuration for a scraper
 * @param {string} id - Scraper ID
 * @param {Object} config - Enhanced features configuration
 * @returns {Promise<Object>} Updated scraper configuration
 */
export const updateEnhancedFeatures = async (id, config) => {
  try {
    return handleResponse(apiClient.put(`/Scraper/${id}/enhanced-features`, config));
  } catch (error) {
    throw handleApiError(error, `Failed to update enhanced features for scraper with ID ${id}`);
  }
};

/**
 * Enable or disable enhanced content extraction for a scraper
 * @param {string} id - Scraper ID
 * @param {boolean} enabled - Whether to enable or disable enhanced content extraction
 * @returns {Promise<Object>} Updated scraper configuration
 */
export const toggleEnhancedContentExtraction = async (id, enabled) => {
  try {
    return handleResponse(apiClient.put(`/Scraper/${id}/enhanced-content-extraction`, { enabled }));
  } catch (error) {
    throw handleApiError(error, `Failed to ${enabled ? 'enable' : 'disable'} enhanced content extraction for scraper with ID ${id}`);
  }
};

/**
 * Enable or disable circuit breaker for a scraper
 * @param {string} id - Scraper ID
 * @param {boolean} enabled - Whether to enable or disable circuit breaker
 * @returns {Promise<Object>} Updated scraper configuration
 */
export const toggleCircuitBreaker = async (id, enabled) => {
  try {
    return handleResponse(apiClient.put(`/Scraper/${id}/circuit-breaker`, { enabled }));
  } catch (error) {
    throw handleApiError(error, `Failed to ${enabled ? 'enable' : 'disable'} circuit breaker for scraper with ID ${id}`);
  }
};

/**
 * Enable or disable security validation for a scraper
 * @param {string} id - Scraper ID
 * @param {boolean} enabled - Whether to enable or disable security validation
 * @returns {Promise<Object>} Updated scraper configuration
 */
export const toggleSecurityValidation = async (id, enabled) => {
  try {
    return handleResponse(apiClient.put(`/Scraper/${id}/security-validation`, { enabled }));
  } catch (error) {
    throw handleApiError(error, `Failed to ${enabled ? 'enable' : 'disable'} security validation for scraper with ID ${id}`);
  }
};

/**
 * Enable or disable ML content classification for a scraper
 * @param {string} id - Scraper ID
 * @param {boolean} enabled - Whether to enable or disable ML content classification
 * @returns {Promise<Object>} Updated scraper configuration
 */
export const toggleMachineLearningClassification = async (id, enabled) => {
  try {
    return handleResponse(apiClient.put(`/Scraper/${id}/ml-classification`, { enabled }));
  } catch (error) {
    throw handleApiError(error, `Failed to ${enabled ? 'enable' : 'disable'} ML content classification for scraper with ID ${id}`);
  }
};

/**
 * Reset circuit breaker for a scraper
 * @param {string} id - Scraper ID
 * @returns {Promise<Object>} Success status
 */
export const resetCircuitBreaker = async (id) => {
  try {
    return handleResponse(apiClient.post(`/Scraper/${id}/reset-circuit-breaker`));
  } catch (error) {
    throw handleApiError(error, `Failed to reset circuit breaker for scraper with ID ${id}`);
  }
};

export default {
  getEnhancedMetrics,
  updateEnhancedFeatures,
  toggleEnhancedContentExtraction,
  toggleCircuitBreaker,
  toggleSecurityValidation,
  toggleMachineLearningClassification,
  resetCircuitBreaker
};
