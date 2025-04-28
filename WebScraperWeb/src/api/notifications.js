// src/api/notifications.js
import apiClient from './index';

// Get all notifications
export const getAllNotifications = async (page = 1, pageSize = 20, read = null) => {
  try {
    const params = { page, pageSize };
    if (read !== null) params.read = read;
    
    const response = await apiClient.get('/Notifications', { params });
    return response.data;
  } catch (error) {
    console.error('Error fetching notifications:', error);
    throw error;
  }
};

// Get notification by ID
export const getNotification = async (id) => {
  try {
    const response = await apiClient.get(`/Notifications/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching notification with id ${id}:`, error);
    throw error;
  }
};

// Mark notification as read
export const markNotificationAsRead = async (id) => {
  try {
    const response = await apiClient.put(`/Notifications/${id}/read`);
    return response.data;
  } catch (error) {
    console.error(`Error marking notification with id ${id} as read:`, error);
    throw error;
  }
};

// Mark all notifications as read
export const markAllNotificationsAsRead = async () => {
  try {
    const response = await apiClient.put('/Notifications/read-all');
    return response.data;
  } catch (error) {
    console.error('Error marking all notifications as read:', error);
    throw error;
  }
};

// Delete notification
export const deleteNotification = async (id) => {
  try {
    const response = await apiClient.delete(`/Notifications/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error deleting notification with id ${id}:`, error);
    throw error;
  }
};

// Get notification settings
export const getNotificationSettings = async () => {
  try {
    const response = await apiClient.get('/Notifications/settings');
    return response.data;
  } catch (error) {
    console.error('Error fetching notification settings:', error);
    throw error;
  }
};

// Update notification settings
export const updateNotificationSettings = async (settings) => {
  try {
    const response = await apiClient.put('/Notifications/settings', settings);
    return response.data;
  } catch (error) {
    console.error('Error updating notification settings:', error);
    throw error;
  }
};

// Configure webhook notifications
export const configureWebhook = async (webhookConfig) => {
  try {
    const response = await apiClient.post('/Notifications/webhook', webhookConfig);
    return response.data;
  } catch (error) {
    console.error('Error configuring webhook:', error);
    throw error;
  }
};

// Test webhook notification
export const testWebhook = async (webhookUrl) => {
  try {
    const response = await apiClient.post('/Notifications/webhook/test', { url: webhookUrl });
    return response.data;
  } catch (error) {
    console.error('Error testing webhook:', error);
    throw error;
  }
};
