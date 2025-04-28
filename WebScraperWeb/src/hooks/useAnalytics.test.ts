import { renderHook, act } from '@testing-library/react-hooks';
import useAnalytics from './useAnalytics';
import * as analyticsApi from '../api/analytics';

// Mock the analytics API
jest.mock('../api/analytics');

describe('useAnalytics Hook', () => {
  const mockOverallData = { totalScrapers: 5, totalPages: 1000 };
  const mockScraperData = { id: 'test-scraper', pagesScraped: 200 };
  const mockChangeData = { totalChanges: 50, changesPerDay: [10, 5, 15, 20] };
  const mockPerformanceData = { averageTime: 120, requestsPerMinute: 30 };
  const mockContentTypeData = { html: 80, pdf: 15, other: 5 };
  const mockRegulatoryImpactData = { highImpact: 10, mediumImpact: 20, lowImpact: 30 };
  const mockTrendData = { trend: 'increasing', data: [10, 15, 20, 25, 30] };
  const mockComparisonData = { scrapers: [{ id: 'scraper1', performance: 90 }, { id: 'scraper2', performance: 85 }] };
  
  beforeEach(() => {
    // Reset all mocks
    jest.clearAllMocks();
    
    // Setup mock implementations
    analyticsApi.getOverallAnalytics.mockResolvedValue(mockOverallData);
    analyticsApi.getScraperAnalytics.mockResolvedValue(mockScraperData);
    analyticsApi.getContentChangeAnalytics.mockResolvedValue(mockChangeData);
    analyticsApi.getPerformanceMetrics.mockResolvedValue(mockPerformanceData);
    analyticsApi.getContentTypeDistribution.mockResolvedValue(mockContentTypeData);
    analyticsApi.getRegulatoryImpactAnalysis.mockResolvedValue(mockRegulatoryImpactData);
    analyticsApi.getTrendAnalysis.mockResolvedValue(mockTrendData);
    analyticsApi.getScraperComparison.mockResolvedValue(mockComparisonData);
  });
  
  test('initializes with null data and no loading or error state', () => {
    const { result } = renderHook(() => useAnalytics());
    
    expect(result.current.overallData).toBeNull();
    expect(result.current.scraperData).toBeNull();
    expect(result.current.changeData).toBeNull();
    expect(result.current.performanceData).toBeNull();
    expect(result.current.contentTypeData).toBeNull();
    expect(result.current.regulatoryImpactData).toBeNull();
    expect(result.current.trendData).toBeNull();
    expect(result.current.comparisonData).toBeNull();
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBeNull();
  });
  
  test('fetches overall analytics data', async () => {
    const { result, waitForNextUpdate } = renderHook(() => useAnalytics());
    
    // Call the fetch function
    let fetchPromise;
    act(() => {
      fetchPromise = result.current.fetchOverallAnalytics();
    });
    
    // Check loading state
    expect(result.current.loading).toBe(true);
    
    // Wait for the fetch to complete
    await waitForNextUpdate();
    
    // Check the result of the promise
    const data = await fetchPromise;
    expect(data).toEqual(mockOverallData);
    
    // Check the updated state
    expect(result.current.overallData).toEqual(mockOverallData);
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBeNull();
    
    // Check that the API was called with the correct parameters
    expect(analyticsApi.getOverallAnalytics).toHaveBeenCalledWith('week');
  });
  
  test('fetches scraper-specific analytics data', async () => {
    const scraperId = 'test-scraper';
    const { result, waitForNextUpdate } = renderHook(() => useAnalytics(scraperId));
    
    // Call the fetch function
    let fetchPromise;
    act(() => {
      fetchPromise = result.current.fetchScraperAnalytics();
    });
    
    // Check loading state
    expect(result.current.loading).toBe(true);
    
    // Wait for the fetch to complete
    await waitForNextUpdate();
    
    // Check the result of the promise
    const data = await fetchPromise;
    expect(data).toEqual(mockScraperData);
    
    // Check the updated state
    expect(result.current.scraperData).toEqual(mockScraperData);
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBeNull();
    
    // Check that the API was called with the correct parameters
    expect(analyticsApi.getScraperAnalytics).toHaveBeenCalledWith(scraperId, 'week');
  });
  
  test('handles error when fetching data', async () => {
    // Setup mock to reject
    const errorMessage = 'API Error';
    analyticsApi.getOverallAnalytics.mockRejectedValue(new Error(errorMessage));
    
    const { result, waitForNextUpdate } = renderHook(() => useAnalytics());
    
    // Call the fetch function
    let fetchPromise;
    act(() => {
      fetchPromise = result.current.fetchOverallAnalytics();
    });
    
    // Wait for the fetch to complete
    await waitForNextUpdate();
    
    // Check the result of the promise
    const data = await fetchPromise;
    expect(data).toBeNull();
    
    // Check the updated state
    expect(result.current.overallData).toBeNull();
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBe(errorMessage);
  });
  
  test('fetches all analytics data', async () => {
    const scraperId = 'test-scraper';
    const { result, waitForNextUpdate } = renderHook(() => useAnalytics(scraperId));
    
    // Call the fetch function
    let fetchPromise;
    act(() => {
      fetchPromise = result.current.fetchAllAnalytics('month');
    });
    
    // Check loading state
    expect(result.current.loading).toBe(true);
    
    // Wait for the fetch to complete
    await waitForNextUpdate();
    
    // Check the result of the promise
    const data = await fetchPromise;
    expect(data).toEqual({
      overall: mockOverallData,
      changes: mockChangeData,
      performance: mockPerformanceData,
      contentTypes: mockContentTypeData,
      regulatoryImpact: mockRegulatoryImpactData,
      scraper: mockScraperData
    });
    
    // Check the updated state
    expect(result.current.overallData).toEqual(mockOverallData);
    expect(result.current.changeData).toEqual(mockChangeData);
    expect(result.current.performanceData).toEqual(mockPerformanceData);
    expect(result.current.contentTypeData).toEqual(mockContentTypeData);
    expect(result.current.regulatoryImpactData).toEqual(mockRegulatoryImpactData);
    expect(result.current.scraperData).toEqual(mockScraperData);
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBeNull();
    
    // Check that the APIs were called with the correct parameters
    expect(analyticsApi.getOverallAnalytics).toHaveBeenCalledWith('month');
    expect(analyticsApi.getContentChangeAnalytics).toHaveBeenCalledWith('month');
    expect(analyticsApi.getPerformanceMetrics).toHaveBeenCalledWith('month');
    expect(analyticsApi.getContentTypeDistribution).toHaveBeenCalledWith(scraperId);
    expect(analyticsApi.getRegulatoryImpactAnalysis).toHaveBeenCalledWith('month');
    expect(analyticsApi.getScraperAnalytics).toHaveBeenCalledWith(scraperId, 'month');
  });
  
  test('clears all data', async () => {
    const { result, waitForNextUpdate } = renderHook(() => useAnalytics());
    
    // First fetch some data
    act(() => {
      result.current.fetchOverallAnalytics();
    });
    
    await waitForNextUpdate();
    
    // Verify data was fetched
    expect(result.current.overallData).toEqual(mockOverallData);
    
    // Clear the data
    act(() => {
      result.current.clearData();
    });
    
    // Check that all data is cleared
    expect(result.current.overallData).toBeNull();
    expect(result.current.scraperData).toBeNull();
    expect(result.current.changeData).toBeNull();
    expect(result.current.performanceData).toBeNull();
    expect(result.current.contentTypeData).toBeNull();
    expect(result.current.regulatoryImpactData).toBeNull();
    expect(result.current.trendData).toBeNull();
    expect(result.current.comparisonData).toBeNull();
    expect(result.current.error).toBeNull();
  });
});
