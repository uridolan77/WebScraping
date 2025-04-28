// src/api/monitoring.js
import apiClient, { handleResponse } from './index';
import { handleApiError } from '../utils/errorHandler';

/**
 * Get system health metrics
 * @returns {Promise<Object>} System health data
 */
export const getSystemHealth = async () => {
  try {
    return handleResponse(apiClient.get('/Monitoring/system-health'));
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch system health data');
  }
};

/**
 * Get active scrapers
 * @returns {Promise<Array>} List of active scrapers
 */
export const getActiveScrapers = async () => {
  try {
    return handleResponse(apiClient.get('/Monitoring/active-scrapers'));
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch active scrapers');
  }
};

/**
 * Get scraper status summary
 * @returns {Promise<Object>} Summary of scraper statuses
 */
export const getScraperStatusSummary = async () => {
  try {
    return handleResponse(apiClient.get('/Monitoring/status-summary'));
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch scraper status summary');
  }
};

/**
 * Get recent notifications
 * @param {number} limit - Maximum number of notifications to return
 * @param {boolean} includeRead - Whether to include read notifications
 * @returns {Promise<Array>} List of notifications
 */
export const getNotifications = async (limit = 10, includeRead = false) => {
  try {
    return handleResponse(apiClient.get('/Monitoring/notifications', {
      params: { limit, includeRead }
    }));
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch notifications');
  }
};

/**
 * Mark notification as read
 * @param {string} id - Notification ID
 * @returns {Promise<Object>} Updated notification
 */
export const markNotificationAsRead = async (id: string) => {
  try {
    return handleResponse(apiClient.post(`/Monitoring/notifications/${id}/read`));
  } catch (error) {
    throw handleApiError(error, 'Failed to mark notification as read');
  }
};

/**
 * Mark all notifications as read
 * @returns {Promise<Object>} Result of operation
 */
export const markAllNotificationsAsRead = async () => {
  try {
    return handleResponse(apiClient.post('/Monitoring/notifications/read-all'));
  } catch (error) {
    throw handleApiError(error, 'Failed to mark all notifications as read');
  }
};

/**
 * Get resource usage history
 * @param {string} resource - Resource type (cpu, memory, disk, network)
 * @param {string} timeframe - Time period (hour, day, week, month)
 * @returns {Promise<Array>} Resource usage history
 */
export const getResourceUsageHistory = async (resource: string, timeframe: string = 'day') => {
  try {
    return handleResponse(apiClient.get('/Monitoring/resource-usage', {
      params: { resource, timeframe }
    }));
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch resource usage history');
  }
};

/**
 * Get service status
 * @returns {Promise<Array>} Status of all services
 */
export const getServiceStatus = async () => {
  try {
    return handleResponse(apiClient.get('/Monitoring/services'));
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch service status');
  }
};

/**
 * Get system issues
 * @param {string} severity - Filter by severity (warning, error, critical)
 * @returns {Promise<Array>} List of system issues
 */
export const getSystemIssues = async (severity: string | null = null) => {
  try {
    const params: Record<string, string> = {};
    if (severity) params.severity = severity;

    return handleResponse(apiClient.get('/Monitoring/issues', { params }));
  } catch (error) {
    throw handleApiError(error, 'Failed to fetch system issues');
  }
};

export default {
  getSystemHealth,
  getActiveScrapers,
  getScraperStatusSummary,
  getNotifications,
  markNotificationAsRead,
  markAllNotificationsAsRead,
  getResourceUsageHistory,
  getServiceStatus,
  getSystemIssues
};
