import { 
  useQuery, 
  UseQueryOptions,
  useQueryClient
} from '@tanstack/react-query';
import { 
  AnalyticsData, 
  ContentChange, 
  PerformanceMetric, 
  ContentTypeDistribution,
  ApiError
} from '../../types';
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
} from '../../api/analytics';

// Query keys
export const analyticsKeys = {
  all: ['analytics'] as const,
  overall: (timeframe: string) => [...analyticsKeys.all, 'overall', timeframe] as const,
  scraper: (id: string, timeframe: string) => [...analyticsKeys.all, 'scraper', id, timeframe] as const,
  changes: (timeframe: string) => [...analyticsKeys.all, 'changes', timeframe] as const,
  performance: (timeframe: string) => [...analyticsKeys.all, 'performance', timeframe] as const,
  contentTypes: (scraperId?: string) => [...analyticsKeys.all, 'contentTypes', scraperId] as const,
  regulatoryImpact: (timeframe: string) => [...analyticsKeys.all, 'regulatoryImpact', timeframe] as const,
  trends: (metric: string, timeframe: string) => [...analyticsKeys.all, 'trends', metric, timeframe] as const,
  comparison: (scraperIds: string[], metric: string, timeframe: string) => 
    [...analyticsKeys.all, 'comparison', scraperIds.join(','), metric, timeframe] as const,
};

// Get overall analytics
export const useOverallAnalytics = (
  timeframe: string = 'week',
  options?: UseQueryOptions<AnalyticsData, ApiError>
) => {
  return useQuery<AnalyticsData, ApiError>({
    queryKey: analyticsKeys.overall(timeframe),
    queryFn: () => getOverallAnalytics(timeframe),
    ...options
  });
};

// Get scraper-specific analytics
export const useScraperAnalytics = (
  id: string,
  timeframe: string = 'week',
  options?: UseQueryOptions<AnalyticsData, ApiError>
) => {
  return useQuery<AnalyticsData, ApiError>({
    queryKey: analyticsKeys.scraper(id, timeframe),
    queryFn: () => getScraperAnalytics(id, timeframe),
    enabled: !!id,
    ...options
  });
};

// Get content change analytics
export const useContentChangeAnalytics = (
  timeframe: string = 'month',
  options?: UseQueryOptions<ContentChange[], ApiError>
) => {
  return useQuery<ContentChange[], ApiError>({
    queryKey: analyticsKeys.changes(timeframe),
    queryFn: () => getContentChangeAnalytics(timeframe),
    ...options
  });
};

// Get performance metrics
export const usePerformanceMetrics = (
  timeframe: string = 'week',
  options?: UseQueryOptions<PerformanceMetric, ApiError>
) => {
  return useQuery<PerformanceMetric, ApiError>({
    queryKey: analyticsKeys.performance(timeframe),
    queryFn: () => getPerformanceMetrics(timeframe),
    // Performance data changes more frequently
    staleTime: 2 * 60 * 1000, // 2 minutes
    ...options
  });
};

// Get content type distribution
export const useContentTypeDistribution = (
  scraperId?: string,
  options?: UseQueryOptions<ContentTypeDistribution, ApiError>
) => {
  return useQuery<ContentTypeDistribution, ApiError>({
    queryKey: analyticsKeys.contentTypes(scraperId),
    queryFn: () => getContentTypeDistribution(scraperId),
    // Content type distribution changes infrequently
    staleTime: 30 * 60 * 1000, // 30 minutes
    ...options
  });
};

// Get regulatory impact analysis
export const useRegulatoryImpactAnalysis = (
  timeframe: string = 'month',
  options?: UseQueryOptions<any, ApiError>
) => {
  return useQuery<any, ApiError>({
    queryKey: analyticsKeys.regulatoryImpact(timeframe),
    queryFn: () => getRegulatoryImpactAnalysis(timeframe),
    ...options
  });
};

// Get trend analysis
export const useTrendAnalysis = (
  metric: string = 'changes',
  timeframe: string = 'month',
  options?: UseQueryOptions<any, ApiError>
) => {
  return useQuery<any, ApiError>({
    queryKey: analyticsKeys.trends(metric, timeframe),
    queryFn: () => getTrendAnalysis(metric, timeframe),
    ...options
  });
};

// Get scraper comparison
export const useScraperComparison = (
  scraperIds: string[],
  metric: string = 'performance',
  timeframe: string = 'month',
  options?: UseQueryOptions<any, ApiError>
) => {
  return useQuery<any, ApiError>({
    queryKey: analyticsKeys.comparison(scraperIds, metric, timeframe),
    queryFn: () => getScraperComparison(scraperIds, metric, timeframe),
    enabled: scraperIds.length > 0,
    ...options
  });
};

// Custom hook to refresh analytics cache
export const useRefreshAnalyticsCache = () => {
  const queryClient = useQueryClient();
  
  return {
    refreshCache: async (timeframe: string = 'week') => {
      // Clear the cache first
      clearAnalyticsCache();
      
      // Invalidate all analytics queries
      queryClient.invalidateQueries({ queryKey: analyticsKeys.all });
      
      // Refetch key analytics data
      await Promise.all([
        queryClient.refetchQueries({ queryKey: analyticsKeys.overall(timeframe) }),
        queryClient.refetchQueries({ queryKey: analyticsKeys.changes(timeframe) }),
        queryClient.refetchQueries({ queryKey: analyticsKeys.performance(timeframe) }),
        queryClient.refetchQueries({ queryKey: analyticsKeys.contentTypes() })
      ]);
    }
  };
};
