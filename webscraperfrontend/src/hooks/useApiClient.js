import { useState, useCallback } from 'react';
import apiClient from '../services/apiClient';

/**
 * Custom hook for making API calls with loading and error states
 */
const useApiClient = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  /**
   * Execute an API call with automatic loading and error handling
   * @param {Function} apiCall - The API function to call (e.g., apiClient.scrapers.getAll)
   * @param {Object} options - Additional options
   * @returns {Promise} - The resolved data or error
   */
  const execute = useCallback(async (apiCall, options = {}) => {
    const { 
      showLoading = true, 
      onSuccess, 
      onError 
    } = options;
    
    try {
      if (showLoading) setLoading(true);
      setError(null);
      
      const response = await apiCall();
      const data = response.data;
      
      if (onSuccess) onSuccess(data);
      
      return data;
    } catch (err) {
      const errorMessage = err.message || 'An unexpected error occurred';
      setError(errorMessage);
      
      if (onError) onError(err);
      
      throw err;
    } finally {
      if (showLoading) setLoading(false);
    }
  }, []);

  return {
    api: apiClient,
    loading,
    error,
    execute,
    clearError: () => setError(null),
  };
};

export default useApiClient;