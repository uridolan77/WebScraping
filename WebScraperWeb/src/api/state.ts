// src/api/state.js
import apiClient from './index';

// Get all active scraper instances
export const getActiveScrapers = async () => {
  try {
    const response = await apiClient.get('/State/active');
    return response.data;
  } catch (error) {
    console.error('Error fetching active scrapers:', error);
    throw error;
  }
};

// Get state for a specific scraper
export const getScraperState = async (id: string) => {
  try {
    const response = await apiClient.get(`/State/scraper/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching state for scraper with id ${id}:`, error);
    throw error;
  }
};

// Get system state
export const getSystemState = async () => {
  try {
    const response = await apiClient.get('/State/system');
    return response.data;
  } catch (error) {
    console.error('Error fetching system state:', error);
    throw error;
  }
};

// Get resource usage
export const getResourceUsage = async () => {
  try {
    const response = await apiClient.get('/State/resources');
    return response.data;
  } catch (error) {
    console.error('Error fetching resource usage:', error);
    throw error;
  }
};

// Get queue status
export const getQueueStatus = async () => {
  try {
    const response = await apiClient.get('/State/queue');
    return response.data;
  } catch (error) {
    console.error('Error fetching queue status:', error);
    throw error;
  }
};

// Get database status
export const getDatabaseStatus = async () => {
  try {
    const response = await apiClient.get('/State/database');
    return response.data;
  } catch (error) {
    console.error('Error fetching database status:', error);
    throw error;
  }
};

// Reset scraper state
export const resetScraperState = async (id: string) => {
  try {
    const response = await apiClient.post(`/State/scraper/${id}/reset`);
    return response.data;
  } catch (error) {
    console.error(`Error resetting state for scraper with id ${id}:`, error);
    throw error;
  }
};

// Get error logs
export const getErrorLogs = async (limit: number = 100) => {
  try {
    const response = await apiClient.get('/State/errors', {
      params: { limit }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching error logs:', error);
    throw error;
  }
};

// Get system settings
export const getSystemSettings = async () => {
  try {
    const response = await apiClient.get('/State/settings');
    return response.data;
  } catch (error) {
    console.error('Error fetching system settings:', error);
    throw error;
  }
};

// Update system settings
export const updateSystemSettings = async (settings: any) => {
  try {
    const response = await apiClient.put('/State/settings', settings);
    return response.data;
  } catch (error) {
    console.error('Error updating system settings:', error);
    throw error;
  }
};
