import { useState, useCallback } from 'react';
import { 
  getSystemHealth, 
  getActiveScrapers, 
  getScraperStatusSummary,
  getNotifications,
  markNotificationAsRead,
  markAllNotificationsAsRead,
  getResourceUsageHistory,
  getServiceStatus,
  getSystemIssues
} from '../api/monitoring';
import { getUserFriendlyErrorMessage } from '../utils/errorHandler';

/**
 * Custom hook for monitoring data and operations
 * @returns {Object} Monitoring data and functions
 */
export const useMonitoring = () => {
  const [systemHealth, setSystemHealth] = useState(null);
  const [activeScrapers, setActiveScrapers] = useState([]);
  const [statusSummary, setStatusSummary] = useState(null);
  const [notifications, setNotifications] = useState([]);
  const [resourceUsage, setResourceUsage] = useState({});
  const [serviceStatus, setServiceStatus] = useState([]);
  const [systemIssues, setSystemIssues] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  /**
   * Fetch system health data
   */
  const fetchSystemHealth = useCallback(async () => {
    setLoading(true);
    setError(null);
    
    try {
      const data = await getSystemHealth();
      setSystemHealth(data);
      return data;
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to fetch system health data');
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Fetch active scrapers
   */
  const fetchActiveScrapers = useCallback(async () => {
    setLoading(true);
    setError(null);
    
    try {
      const data = await getActiveScrapers();
      setActiveScrapers(data);
      return data;
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to fetch active scrapers');
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Fetch scraper status summary
   */
  const fetchStatusSummary = useCallback(async () => {
    setLoading(true);
    setError(null);
    
    try {
      const data = await getScraperStatusSummary();
      setStatusSummary(data);
      return data;
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to fetch status summary');
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Fetch notifications
   * @param {number} limit - Maximum number of notifications to return
   * @param {boolean} includeRead - Whether to include read notifications
   */
  const fetchNotifications = useCallback(async (limit = 10, includeRead = false) => {
    setLoading(true);
    setError(null);
    
    try {
      const data = await getNotifications(limit, includeRead);
      setNotifications(data);
      return data;
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to fetch notifications');
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Mark a notification as read
   * @param {string} id - Notification ID
   */
  const handleMarkAsRead = useCallback(async (id) => {
    try {
      await markNotificationAsRead(id);
      
      // Update local state
      setNotifications(prev => 
        prev.map(notification => 
          notification.id === id ? { ...notification, read: true } : notification
        )
      );
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to mark notification as read');
      setError(errorMessage);
      throw new Error(errorMessage);
    }
  }, []);

  /**
   * Mark all notifications as read
   */
  const handleMarkAllAsRead = useCallback(async () => {
    try {
      await markAllNotificationsAsRead();
      
      // Update local state
      setNotifications(prev => 
        prev.map(notification => ({ ...notification, read: true }))
      );
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to mark all notifications as read');
      setError(errorMessage);
      throw new Error(errorMessage);
    }
  }, []);

  /**
   * Fetch resource usage history
   * @param {string} resource - Resource type (cpu, memory, disk, network)
   * @param {string} timeframe - Time period (hour, day, week, month)
   */
  const fetchResourceUsage = useCallback(async (resource, timeframe = 'day') => {
    setLoading(true);
    setError(null);
    
    try {
      const data = await getResourceUsageHistory(resource, timeframe);
      setResourceUsage(prev => ({
        ...prev,
        [resource]: data
      }));
      return data;
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to fetch resource usage history');
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Fetch service status
   */
  const fetchServiceStatus = useCallback(async () => {
    setLoading(true);
    setError(null);
    
    try {
      const data = await getServiceStatus();
      setServiceStatus(data);
      return data;
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to fetch service status');
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Fetch system issues
   * @param {string} severity - Filter by severity (warning, error, critical)
   */
  const fetchSystemIssues = useCallback(async (severity = null) => {
    setLoading(true);
    setError(null);
    
    try {
      const data = await getSystemIssues(severity);
      setSystemIssues(data);
      return data;
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to fetch system issues');
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Fetch all monitoring data
   */
  const fetchAllMonitoringData = useCallback(async () => {
    setLoading(true);
    setError(null);
    
    try {
      const [
        healthData,
        activeScrapersData,
        summaryData,
        notificationsData,
        serviceStatusData,
        issuesData
      ] = await Promise.all([
        getSystemHealth(),
        getActiveScrapers(),
        getScraperStatusSummary(),
        getNotifications(),
        getServiceStatus(),
        getSystemIssues()
      ]);
      
      setSystemHealth(healthData);
      setActiveScrapers(activeScrapersData);
      setStatusSummary(summaryData);
      setNotifications(notificationsData);
      setServiceStatus(serviceStatusData);
      setSystemIssues(issuesData);
      
      return {
        systemHealth: healthData,
        activeScrapers: activeScrapersData,
        statusSummary: summaryData,
        notifications: notificationsData,
        serviceStatus: serviceStatusData,
        systemIssues: issuesData
      };
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to fetch monitoring data');
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    systemHealth,
    activeScrapers,
    statusSummary,
    notifications,
    resourceUsage,
    serviceStatus,
    systemIssues,
    loading,
    error,
    fetchSystemHealth,
    fetchActiveScrapers,
    fetchStatusSummary,
    fetchNotifications,
    handleMarkAsRead,
    handleMarkAllAsRead,
    fetchResourceUsage,
    fetchServiceStatus,
    fetchSystemIssues,
    fetchAllMonitoringData
  };
};

export default useMonitoring;
