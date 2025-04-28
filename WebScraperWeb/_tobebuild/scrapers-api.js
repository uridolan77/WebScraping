// src/api/scrapers.js
import apiClient from './index';

// Get all scrapers
export const getAllScrapers = async () => {
  try {
    const response = await apiClient.get('/Scraper');
    return response.data;
  } catch (error) {
    console.error('Error fetching scrapers:', error);
    throw error;
  }
};

// Get a single scraper by ID
export const getScraper = async (id) => {
  try {
    const response = await apiClient.get(`/Scraper/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching scraper with id ${id}:`, error);
    throw error;
  }
};

// Create a new scraper
export const createScraper = async (scraperData) => {
  try {
    const response = await apiClient.post('/Scraper', scraperData);
    return response.data;
  } catch (error) {
    console.error('Error creating scraper:', error);
    throw error;
  }
};

// Update an existing scraper
export const updateScraper = async (id, scraperData) => {
  try {
    const response = await apiClient.put(`/Scraper/${id}`, scraperData);
    return response.data;
  } catch (error) {
    console.error(`Error updating scraper with id ${id}:`, error);
    throw error;
  }
};

// Delete a scraper
export const deleteScraper = async (id) => {
  try {
    const response = await apiClient.delete(`/Scraper/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error deleting scraper with id ${id}:`, error);
    throw error;
  }
};

// Get scraper status
export const getScraperStatus = async (id) => {
  try {
    const response = await apiClient.get(`/Scraper/${id}/status`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching status for scraper with id ${id}:`, error);
    throw error;
  }
};

// Get scraper logs
export const getScraperLogs = async (id, limit = 100) => {
  try {
    const response = await apiClient.get(`/Scraper/${id}/logs`, {
      params: { limit }
    });
    return response.data;
  } catch (error) {
    console.error(`Error fetching logs for scraper with id ${id}:`, error);
    throw error;
  }
};

// Start a scraper
export const startScraper = async (id) => {
  try {
    const response = await apiClient.post(`/Scraper/${id}/start`);
    return response.data;
  } catch (error) {
    console.error(`Error starting scraper with id ${id}:`, error);
    throw error;
  }
};

// Stop a scraper
export const stopScraper = async (id) => {
  try {
    const response = await apiClient.post(`/Scraper/${id}/stop`);
    return response.data;
  } catch (error) {
    console.error(`Error stopping scraper with id ${id}:`, error);
    throw error;
  }
};

// Set monitoring settings
export const setMonitoring = async (id, settings) => {
  try {
    const response = await apiClient.post(`/Scraper/${id}/monitor`, settings);
    return response.data;
  } catch (error) {
    console.error(`Error setting monitoring for scraper with id ${id}:`, error);
    throw error;
  }
};

// Get scraper results
export const getScraperResults = async (page = 1, pageSize = 20, search = null, scraperId = null) => {
  try {
    const params = { page, pageSize };
    if (search) params.search = search;
    if (scraperId) params.scraperId = scraperId;
    
    const response = await apiClient.get('/Scraper/results', { params });
    return response.data;
  } catch (error) {
    console.error('Error fetching scraper results:', error);
    throw error;
  }
};

// Get detected changes for a scraper
export const getDetectedChanges = async (id, since = null, limit = 100) => {
  try {
    const params = { limit };
    if (since) params.since = since.toISOString();
    
    const response = await apiClient.get(`/Scraper/${id}/changes`, { params });
    return response.data;
  } catch (error) {
    console.error(`Error fetching detected changes for scraper with id ${id}:`, error);
    throw error;
  }
};

// Get processed documents
export const getProcessedDocuments = async (id, documentType = null, page = 1, pageSize = 20) => {
  try {
    const params = { page, pageSize };
    if (documentType) params.documentType = documentType;
    
    const response = await apiClient.get(`/Scraper/${id}/documents`, { params });
    return response.data;
  } catch (error) {
    console.error(`Error fetching processed documents for scraper with id ${id}:`, error);
    throw error;
  }
};

// Compress stored content
export const compressStoredContent = async (id) => {
  try {
    const response = await apiClient.post(`/Scraper/${id}/compress`);
    return response.data;
  } catch (error) {
    console.error(`Error compressing content for scraper with id ${id}:`, error);
    throw error;
  }
};