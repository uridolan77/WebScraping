// src/hooks/useEnhancedFeatures.js
import { useState, useCallback } from 'react';
import { 
  getEnhancedMetrics, 
  updateEnhancedFeatures,
  toggleEnhancedContentExtraction,
  toggleCircuitBreaker,
  toggleSecurityValidation,
  toggleMachineLearningClassification,
  resetCircuitBreaker
} from '../api/enhancedFeatures';

/**
 * Custom hook for managing enhanced features
 * @param {string} scraperId - The ID of the scraper
 * @returns {Object} Enhanced features state and methods
 */
const useEnhancedFeatures = (scraperId) => {
  const [metrics, setMetrics] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  /**
   * Fetch enhanced metrics for the scraper
   */
  const fetchMetrics = useCallback(async () => {
    if (!scraperId) return;
    
    try {
      setIsLoading(true);
      setError(null);
      
      const data = await getEnhancedMetrics(scraperId);
      setMetrics(data);
      
      return data;
    } catch (err) {
      setError(err);
      console.error('Error fetching enhanced metrics:', err);
      return null;
    } finally {
      setIsLoading(false);
    }
  }, [scraperId]);

  /**
   * Update enhanced features configuration
   * @param {Object} config - The configuration to update
   */
  const updateFeatures = useCallback(async (config) => {
    if (!scraperId) return;
    
    try {
      setIsLoading(true);
      setError(null);
      
      const data = await updateEnhancedFeatures(scraperId, config);
      
      // Refresh metrics after update
      await fetchMetrics();
      
      return data;
    } catch (err) {
      setError(err);
      console.error('Error updating enhanced features:', err);
      return null;
    } finally {
      setIsLoading(false);
    }
  }, [scraperId, fetchMetrics]);

  /**
   * Toggle enhanced content extraction
   * @param {boolean} enabled - Whether to enable or disable
   */
  const toggleContentExtraction = useCallback(async (enabled) => {
    if (!scraperId) return;
    
    try {
      setIsLoading(true);
      setError(null);
      
      const data = await toggleEnhancedContentExtraction(scraperId, enabled);
      
      // Refresh metrics after update
      await fetchMetrics();
      
      return data;
    } catch (err) {
      setError(err);
      console.error('Error toggling enhanced content extraction:', err);
      return null;
    } finally {
      setIsLoading(false);
    }
  }, [scraperId, fetchMetrics]);

  /**
   * Toggle circuit breaker
   * @param {boolean} enabled - Whether to enable or disable
   */
  const toggleCircuitBreakerFeature = useCallback(async (enabled) => {
    if (!scraperId) return;
    
    try {
      setIsLoading(true);
      setError(null);
      
      const data = await toggleCircuitBreaker(scraperId, enabled);
      
      // Refresh metrics after update
      await fetchMetrics();
      
      return data;
    } catch (err) {
      setError(err);
      console.error('Error toggling circuit breaker:', err);
      return null;
    } finally {
      setIsLoading(false);
    }
  }, [scraperId, fetchMetrics]);

  /**
   * Toggle security validation
   * @param {boolean} enabled - Whether to enable or disable
   */
  const toggleSecurity = useCallback(async (enabled) => {
    if (!scraperId) return;
    
    try {
      setIsLoading(true);
      setError(null);
      
      const data = await toggleSecurityValidation(scraperId, enabled);
      
      // Refresh metrics after update
      await fetchMetrics();
      
      return data;
    } catch (err) {
      setError(err);
      console.error('Error toggling security validation:', err);
      return null;
    } finally {
      setIsLoading(false);
    }
  }, [scraperId, fetchMetrics]);

  /**
   * Toggle ML content classification
   * @param {boolean} enabled - Whether to enable or disable
   */
  const toggleMlClassification = useCallback(async (enabled) => {
    if (!scraperId) return;
    
    try {
      setIsLoading(true);
      setError(null);
      
      const data = await toggleMachineLearningClassification(scraperId, enabled);
      
      // Refresh metrics after update
      await fetchMetrics();
      
      return data;
    } catch (err) {
      setError(err);
      console.error('Error toggling ML classification:', err);
      return null;
    } finally {
      setIsLoading(false);
    }
  }, [scraperId, fetchMetrics]);

  /**
   * Reset circuit breaker
   */
  const resetCircuitBreakerState = useCallback(async () => {
    if (!scraperId) return;
    
    try {
      setIsLoading(true);
      setError(null);
      
      const data = await resetCircuitBreaker(scraperId);
      
      // Refresh metrics after reset
      await fetchMetrics();
      
      return data;
    } catch (err) {
      setError(err);
      console.error('Error resetting circuit breaker:', err);
      return null;
    } finally {
      setIsLoading(false);
    }
  }, [scraperId, fetchMetrics]);

  return {
    metrics,
    isLoading,
    error,
    fetchMetrics,
    updateFeatures,
    toggleContentExtraction,
    toggleCircuitBreaker: toggleCircuitBreakerFeature,
    toggleSecurity,
    toggleMlClassification,
    resetCircuitBreaker: resetCircuitBreakerState
  };
};

export default useEnhancedFeatures;
