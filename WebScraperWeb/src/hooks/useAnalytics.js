// src/hooks/useAnalytics.js
import { useState, useCallback } from 'react';
import { 
  getOverallAnalytics, 
  getScraperAnalytics, 
  getContentChangeAnalytics,
  getPerformanceMetrics,
  getContentTypeDistribution,
  getRegulatoryImpactAnalysis
} from '../api/analytics';

const useAnalytics = () => {
  const [overallData, setOverallData] = useState(null);
  const [scraperData, setScraperData] = useState(null);
  const [changeData, setChangeData] = useState(null);
  const [performanceData, setPerformanceData] = useState(null);
  const [contentTypeData, setContentTypeData] = useState(null);
  const [regulatoryImpactData, setRegulatoryImpactData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  // Fetch overall analytics
  const fetchOverallAnalytics = useCallback(async (timeframe = 'week') => {
    try {
      setLoading(true);
      setError(null);
      const data = await getOverallAnalytics(timeframe);
      setOverallData(data);
      return data;
    } catch (err) {
      setError('Failed to fetch overall analytics');
      console.error(err);
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  // Fetch scraper-specific analytics
  const fetchScraperAnalytics = useCallback(async (id, timeframe = 'week') => {
    try {
      setLoading(true);
      setError(null);
      const data = await getScraperAnalytics(id, timeframe);
      setScraperData(data);
      return data;
    } catch (err) {
      setError(`Failed to fetch analytics for scraper with ID ${id}`);
      console.error(err);
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  // Fetch content change analytics
  const fetchContentChangeAnalytics = useCallback(async (timeframe = 'month') => {
    try {
      setLoading(true);
      setError(null);
      const data = await getContentChangeAnalytics(timeframe);
      setChangeData(data);
      return data;
    } catch (err) {
      setError('Failed to fetch content change analytics');
      console.error(err);
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  // Fetch performance metrics
  const fetchPerformanceMetrics = useCallback(async (timeframe = 'week') => {
    try {
      setLoading(true);
      setError(null);
      const data = await getPerformanceMetrics(timeframe);
      setPerformanceData(data);
      return data;
    } catch (err) {
      setError('Failed to fetch performance metrics');
      console.error(err);
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  // Fetch content type distribution
  const fetchContentTypeDistribution = useCallback(async (scraperId = null) => {
    try {
      setLoading(true);
      setError(null);
      const data = await getContentTypeDistribution(scraperId);
      setContentTypeData(data);
      return data;
    } catch (err) {
      setError('Failed to fetch content type distribution');
      console.error(err);
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  // Fetch regulatory impact analysis
  const fetchRegulatoryImpactAnalysis = useCallback(async (timeframe = 'month') => {
    try {
      setLoading(true);
      setError(null);
      const data = await getRegulatoryImpactAnalysis(timeframe);
      setRegulatoryImpactData(data);
      return data;
    } catch (err) {
      setError('Failed to fetch regulatory impact analysis');
      console.error(err);
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    overallData,
    scraperData,
    changeData,
    performanceData,
    contentTypeData,
    regulatoryImpactData,
    loading,
    error,
    fetchOverallAnalytics,
    fetchScraperAnalytics,
    fetchContentChangeAnalytics,
    fetchPerformanceMetrics,
    fetchContentTypeDistribution,
    fetchRegulatoryImpactAnalysis
  };
};

export default useAnalytics;
