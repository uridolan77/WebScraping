// src/api/notifications.ts
import apiClient from './index';

// Get all notifications
export const getAllNotifications = async (page: number = 1, pageSize: number = 20, read: boolean | null = null) => {
  try {
    const params: Record<string, any> = { page, pageSize };
    if (read !== null) params.read = read;

    const response = await apiClient.get('/Notifications', { params });
    return response.data;
  } catch (error) {
    console.error('Error fetching notifications:', error);
    throw error;
  }
};

// Get notification by ID
export const getNotification = async (id: string) => {
  try {
    const response = await apiClient.get(`/Notifications/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching notification with id ${id}:`, error);
    throw error;
  }
};

// Mark notification as read
export const markNotificationAsRead = async (id: string) => {
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
export const deleteNotification = async (id: string) => {
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
export const updateNotificationSettings = async (settings: any) => {
  try {
    const response = await apiClient.put('/Notifications/settings', settings);
    return response.data;
  } catch (error) {
    console.error('Error updating notification settings:', error);
    throw error;
  }
};

// Configure webhook notifications
export const configureWebhook = async (webhookConfig: any) => {
  try {
    const response = await apiClient.post('/Notifications/webhook', webhookConfig);
    return response.data;
  } catch (error) {
    console.error('Error configuring webhook:', error);
    throw error;
  }
};

// Test webhook notification
export const testWebhook = async (webhookUrl: string, scraperId?: string) => {
  try {
    const endpoint = scraperId
      ? `/Scraper/${scraperId}/webhooks/test`
      : '/Notifications/webhook/test';
    const response = await apiClient.post(endpoint, { url: webhookUrl });
    return response.data;
  } catch (error) {
    console.error('Error testing webhook:', error);
    throw error;
  }
};

// Get webhook configuration for a scraper
export const getWebhookConfig = async (scraperId: string) => {
  try {
    const response = await apiClient.get(`/Scraper/${scraperId}/webhooks`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching webhook configuration for scraper ${scraperId}:`, error);
    throw error;
  }
};

// Update webhook configuration for a scraper
export const updateWebhookConfig = async (scraperId: string, config: any) => {
  try {
    const response = await apiClient.put(`/Scraper/${scraperId}/webhooks`, config);
    return response.data;
  } catch (error) {
    console.error(`Error updating webhook configuration for scraper ${scraperId}:`, error);
    throw error;
  }
};

// Get scraper by ID (this is a duplicate of the function in scrapers.ts, included here for convenience)
export const getScraper = async (id: string) => {
  try {
    const response = await apiClient.get(`/Scraper/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching scraper with ID ${id}:`, error);
    throw error;
  }
};
