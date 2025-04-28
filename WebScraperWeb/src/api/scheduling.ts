// src/api/scheduling.js
import apiClient from './index';

// Get all scheduled tasks
export const getAllScheduledTasks = async () => {
  try {
    const response = await apiClient.get('/Scheduling');
    return response.data;
  } catch (error) {
    console.error('Error fetching scheduled tasks:', error);
    throw error;
  }
};

// Get scheduled task by ID
export const getScheduledTask = async (id: string) => {
  try {
    const response = await apiClient.get(`/Scheduling/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching scheduled task with id ${id}:`, error);
    throw error;
  }
};

// Create a new scheduled task
export const createScheduledTask = async (taskData: any) => {
  try {
    const response = await apiClient.post('/Scheduling', taskData);
    return response.data;
  } catch (error) {
    console.error('Error creating scheduled task:', error);
    throw error;
  }
};

// Update an existing scheduled task
export const updateScheduledTask = async (id: string, taskData: any) => {
  try {
    const response = await apiClient.put(`/Scheduling/${id}`, taskData);
    return response.data;
  } catch (error) {
    console.error(`Error updating scheduled task with id ${id}:`, error);
    throw error;
  }
};

// Delete a scheduled task
export const deleteScheduledTask = async (id: string) => {
  try {
    const response = await apiClient.delete(`/Scheduling/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error deleting scheduled task with id ${id}:`, error);
    throw error;
  }
};

// Get scheduled tasks for a specific scraper
export const getScraperScheduledTasks = async (scraperId: string) => {
  try {
    const response = await apiClient.get(`/Scheduling/scraper/${scraperId}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching scheduled tasks for scraper with id ${scraperId}:`, error);
    throw error;
  }
};

// Run a scheduled task immediately
export const runScheduledTaskNow = async (id: string) => {
  try {
    const response = await apiClient.post(`/Scheduling/${id}/run`);
    return response.data;
  } catch (error) {
    console.error(`Error running scheduled task with id ${id}:`, error);
    throw error;
  }
};

// Pause a scheduled task
export const pauseScheduledTask = async (id: string) => {
  try {
    const response = await apiClient.post(`/Scheduling/${id}/pause`);
    return response.data;
  } catch (error) {
    console.error(`Error pausing scheduled task with id ${id}:`, error);
    throw error;
  }
};

// Resume a scheduled task
export const resumeScheduledTask = async (id: string) => {
  try {
    const response = await apiClient.post(`/Scheduling/${id}/resume`);
    return response.data;
  } catch (error) {
    console.error(`Error resuming scheduled task with id ${id}:`, error);
    throw error;
  }
};

// Get execution history for a scheduled task
export const getTaskExecutionHistory = async (id: string, limit: number = 10) => {
  try {
    const response = await apiClient.get(`/Scheduling/${id}/history`, {
      params: { limit }
    });
    return response.data;
  } catch (error) {
    console.error(`Error fetching execution history for task with id ${id}:`, error);
    throw error;
  }
};
