// src/hooks/useAnalytics.js
import { useState, useCallback, useEffect, useRef, useMemo } from 'react';
import {
  getOverallAnalytics,
  getScraperAnalytics,
  getContentChangeAnalytics,
  getPerformanceMetrics,
  getContentTypeDistribution,
  getRegulatoryImpactAnalysis,
  getTrendAnalysis,
  getScraperComparison,
  clearAnalyticsCache
} from '../api/analytics';
import { getUserFriendlyErrorMessage } from '../utils/errorHandler';

const useAnalytics = (scraperId = null) => {
  const [overallData, setOverallData] = useState(null);
  const [scraperData, setScraperData] = useState(null);
  const [changeData, setChangeData] = useState(null);
  const [performanceData, setPerformanceData] = useState(null);
  const [contentTypeData, setContentTypeData] = useState(null);
  const [regulatoryImpactData, setRegulatoryImpactData] = useState(null);
  const [trendData, setTrendData] = useState(null);
  const [comparisonData, setComparisonData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  // Use a ref to track if the component is mounted
  const isMounted = useRef(true);

  // Set isMounted to false when the component unmounts
  useEffect(() => {
    return () => {
      isMounted.current = false;
    };
  }, []);

  // Fetch overall analytics
  const fetchOverallAnalytics = useCallback(async (timeframe = 'week') => {
    try {
      setLoading(true);
      setError(null);
      const data = await getOverallAnalytics(timeframe);

      if (isMounted.current) {
        setOverallData(data);
      }

      return data;
    } catch (err) {
      if (isMounted.current) {
        setError(getUserFriendlyErrorMessage(err, 'Failed to fetch overall analytics'));
        console.error('Error fetching overall analytics:', err);
      }
      return null;
    } finally {
      if (isMounted.current) {
        setLoading(false);
      }
    }
  }, []);

  // Fetch scraper-specific analytics
  const fetchScraperAnalytics = useCallback(async (id = scraperId, timeframe = 'week') => {
    if (!id) return null;

    try {
      setLoading(true);
      setError(null);
      const data = await getScraperAnalytics(id, timeframe);

      if (isMounted.current) {
        setScraperData(data);
      }

      return data;
    } catch (err) {
      if (isMounted.current) {
        setError(getUserFriendlyErrorMessage(err, `Failed to fetch analytics for scraper with ID ${id}`));
        console.error(`Error fetching analytics for scraper with ID ${id}:`, err);
      }
      return null;
    } finally {
      if (isMounted.current) {
        setLoading(false);
      }
    }
  }, [scraperId]);

  // Fetch content change analytics
  const fetchContentChangeAnalytics = useCallback(async (timeframe = 'month') => {
    try {
      setLoading(true);
      setError(null);
      const data = await getContentChangeAnalytics(timeframe);

      if (isMounted.current) {
        setChangeData(data);
      }

      return data;
    } catch (err) {
      if (isMounted.current) {
        setError(getUserFriendlyErrorMessage(err, 'Failed to fetch content change analytics'));
        console.error('Error fetching content change analytics:', err);
      }
      return null;
    } finally {
      if (isMounted.current) {
        setLoading(false);
      }
    }
  }, []);

  // Fetch performance metrics
  const fetchPerformanceMetrics = useCallback(async (timeframe = 'week') => {
    try {
      setLoading(true);
      setError(null);
      const data = await getPerformanceMetrics(timeframe);

      if (isMounted.current) {
        setPerformanceData(data);
      }

      return data;
    } catch (err) {
      if (isMounted.current) {
        setError(getUserFriendlyErrorMessage(err, 'Failed to fetch performance metrics'));
        console.error('Error fetching performance metrics:', err);
      }
      return null;
    } finally {
      if (isMounted.current) {
        setLoading(false);
      }
    }
  }, []);

  // Fetch content type distribution
  const fetchContentTypeDistribution = useCallback(async (id = scraperId) => {
    try {
      setLoading(true);
      setError(null);
      const data = await getContentTypeDistribution(id);

      if (isMounted.current) {
        setContentTypeData(data);
      }

      return data;
    } catch (err) {
      if (isMounted.current) {
        setError(getUserFriendlyErrorMessage(err, 'Failed to fetch content type distribution'));
        console.error('Error fetching content type distribution:', err);
      }
      return null;
    } finally {
      if (isMounted.current) {
        setLoading(false);
      }
    }
  }, [scraperId]);

  // Fetch regulatory impact analysis
  const fetchRegulatoryImpactAnalysis = useCallback(async (timeframe = 'month') => {
    try {
      setLoading(true);
      setError(null);
      const data = await getRegulatoryImpactAnalysis(timeframe);

      if (isMounted.current) {
        setRegulatoryImpactData(data);
      }

      return data;
    } catch (err) {
      if (isMounted.current) {
        setError(getUserFriendlyErrorMessage(err, 'Failed to fetch regulatory impact analysis'));
        console.error('Error fetching regulatory impact analysis:', err);
      }
      return null;
    } finally {
      if (isMounted.current) {
        setLoading(false);
      }
    }
  }, []);

  // Fetch trend analysis
  const fetchTrendAnalysis = useCallback(async (metric = 'changes', timeframe = 'month') => {
    try {
      setLoading(true);
      setError(null);
      const data = await getTrendAnalysis(metric, timeframe);

      if (isMounted.current) {
        setTrendData(data);
      }

      return data;
    } catch (err) {
      if (isMounted.current) {
        setError(getUserFriendlyErrorMessage(err, 'Failed to fetch trend analysis'));
        console.error('Error fetching trend analysis:', err);
      }
      return null;
    } finally {
      if (isMounted.current) {
        setLoading(false);
      }
    }
  }, []);

  // Fetch scraper comparison
  const fetchScraperComparison = useCallback(async (scraperIds, metric = 'performance', timeframe = 'month') => {
    try {
      setLoading(true);
      setError(null);
      const data = await getScraperComparison(scraperIds, metric, timeframe);

      if (isMounted.current) {
        setComparisonData(data);
      }

      return data;
    } catch (err) {
      if (isMounted.current) {
        setError(getUserFriendlyErrorMessage(err, 'Failed to fetch scraper comparison data'));
        console.error('Error fetching scraper comparison data:', err);
      }
      return null;
    } finally {
      if (isMounted.current) {
        setLoading(false);
      }
    }
  }, []);

  // Fetch all analytics data
  const fetchAllAnalytics = useCallback(async (timeframe = 'week', forceRefresh = false) => {
    try {
      setLoading(true);
      setError(null);

      const promises = [
        getOverallAnalytics(timeframe, forceRefresh),
        getContentChangeAnalytics(timeframe, forceRefresh),
        getPerformanceMetrics(timeframe, forceRefresh),
        getContentTypeDistribution(scraperId, forceRefresh),
        getRegulatoryImpactAnalysis(timeframe, forceRefresh)
      ];

      // Add scraper-specific analytics if scraperId is provided
      if (scraperId) {
        promises.push(getScraperAnalytics(scraperId, timeframe, forceRefresh));
      }

      const [
        overallResult,
        changeResult,
        performanceResult,
        contentTypeResult,
        regulatoryResult,
        scraperResult
      ] = await Promise.all(promises);

      if (isMounted.current) {
        setOverallData(overallResult);
        setChangeData(changeResult);
        setPerformanceData(performanceResult);
        setContentTypeData(contentTypeResult);
        setRegulatoryImpactData(regulatoryResult);

        if (scraperResult) {
          setScraperData(scraperResult);
        }
      }

      return {
        overall: overallResult,
        changes: changeResult,
        performance: performanceResult,
        contentTypes: contentTypeResult,
        regulatoryImpact: regulatoryResult,
        scraper: scraperResult
      };
    } catch (err) {
      if (isMounted.current) {
        setError(getUserFriendlyErrorMessage(err, 'Failed to fetch analytics data'));
        console.error('Error fetching all analytics data:', err);
      }
      return null;
    } finally {
      if (isMounted.current) {
        setLoading(false);
      }
    }
  }, [scraperId]);

  // Clear all data
  const clearData = useCallback(() => {
    setOverallData(null);
    setScraperData(null);
    setChangeData(null);
    setPerformanceData(null);
    setContentTypeData(null);
    setRegulatoryImpactData(null);
    setTrendData(null);
    setComparisonData(null);
    setError(null);
  }, []);

  // Refresh analytics cache
  const refreshCache = useCallback(async (timeframe = 'week') => {
    // Clear the cache first
    clearAnalyticsCache();

    // Then fetch fresh data
    return await fetchAllAnalytics(timeframe, true);
  }, [fetchAllAnalytics]);

  // Memoized selectors
  const totalPagesScrapped = useMemo(() => {
    return overallData?.totalPages || 0;
  }, [overallData]);

  const totalChangesDetected = useMemo(() => {
    return changeData?.totalChanges || 0;
  }, [changeData]);

  const topPerformingScrapers = useMemo(() => {
    if (!overallData?.scraperPerformance) return [];

    // Sort scrapers by pages scraped
    return [...overallData.scraperPerformance]
      .sort((a, b) => b.pagesScraped - a.pagesScraped)
      .slice(0, 5);
  }, [overallData]);

  const contentTypeBreakdown = useMemo(() => {
    return contentTypeData?.breakdown || [];
  }, [contentTypeData]);

  const performanceMetrics = useMemo(() => {
    if (!performanceData) return null;

    return {
      avgCrawlTime: performanceData.avgCrawlTime,
      avgProcessingTime: performanceData.avgProcessingTime,
      avgMemoryUsage: performanceData.avgMemoryUsage,
      totalErrors: performanceData.totalErrors
    };
  }, [performanceData]);

  return {
    // Raw data
    overallData,
    scraperData,
    changeData,
    performanceData,
    contentTypeData,
    regulatoryImpactData,
    trendData,
    comparisonData,

    // Derived data (memoized)
    totalPagesScrapped,
    totalChangesDetected,
    topPerformingScrapers,
    contentTypeBreakdown,
    performanceMetrics,

    // Status
    loading,
    error,

    // Actions
    fetchOverallAnalytics,
    fetchScraperAnalytics,
    fetchContentChangeAnalytics,
    fetchPerformanceMetrics,
    fetchContentTypeDistribution,
    fetchRegulatoryImpactAnalysis,
    fetchTrendAnalysis,
    fetchScraperComparison,
    fetchAllAnalytics,
    clearData,
    refreshCache
  };
};

export default useAnalytics;
