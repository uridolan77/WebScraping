import { 
  useQuery, 
  useMutation, 
  useQueryClient,
  UseQueryOptions,
  UseMutationOptions
} from '@tanstack/react-query';
import axios from 'axios';
import { Scraper, ScraperStatus, ApiError } from '../../types';
import { 
  getAllScrapers, 
  getScraper, 
  getScraperStatus, 
  createScraper, 
  updateScraper, 
  deleteScraper, 
  startScraper, 
  stopScraper 
} from '../../api/scrapers';

// Query keys
export const scraperKeys = {
  all: ['scrapers'] as const,
  lists: () => [...scraperKeys.all, 'list'] as const,
  list: (filters: Record<string, any>) => [...scraperKeys.lists(), filters] as const,
  details: () => [...scraperKeys.all, 'detail'] as const,
  detail: (id: string) => [...scraperKeys.details(), id] as const,
  status: () => [...scraperKeys.all, 'status'] as const,
  scraperStatus: (id: string) => [...scraperKeys.status(), id] as const,
};

// Get all scrapers
export const useScrapers = (
  options?: UseQueryOptions<Scraper[], ApiError>
) => {
  return useQuery<Scraper[], ApiError>({
    queryKey: scraperKeys.lists(),
    queryFn: () => getAllScrapers(),
    ...options
  });
};

// Get a single scraper
export const useScraper = (
  id: string,
  options?: UseQueryOptions<Scraper, ApiError>
) => {
  return useQuery<Scraper, ApiError>({
    queryKey: scraperKeys.detail(id),
    queryFn: () => getScraper(id),
    enabled: !!id,
    ...options
  });
};

// Get scraper status
export const useScraperStatus = (
  id: string,
  options?: UseQueryOptions<ScraperStatus, ApiError>
) => {
  return useQuery<ScraperStatus, ApiError>({
    queryKey: scraperKeys.scraperStatus(id),
    queryFn: () => getScraperStatus(id),
    enabled: !!id,
    // Status data changes frequently, so we use a shorter stale time
    staleTime: 30 * 1000, // 30 seconds
    ...options
  });
};

// Create a new scraper
export const useCreateScraper = (
  options?: UseMutationOptions<Scraper, ApiError, Scraper>
) => {
  const queryClient = useQueryClient();
  
  return useMutation<Scraper, ApiError, Scraper>({
    mutationFn: (newScraper) => createScraper(newScraper),
    onSuccess: (data) => {
      // Invalidate scrapers list query to refetch
      queryClient.invalidateQueries({ queryKey: scraperKeys.lists() });
      // Add the new scraper to the cache
      queryClient.setQueryData(scraperKeys.detail(data.id), data);
    },
    ...options
  });
};

// Update a scraper
export const useUpdateScraper = (
  options?: UseMutationOptions<Scraper, ApiError, Scraper>
) => {
  const queryClient = useQueryClient();
  
  return useMutation<Scraper, ApiError, Scraper>({
    mutationFn: (updatedScraper) => updateScraper(updatedScraper),
    onSuccess: (data) => {
      // Invalidate scrapers list query to refetch
      queryClient.invalidateQueries({ queryKey: scraperKeys.lists() });
      // Update the scraper in the cache
      queryClient.setQueryData(scraperKeys.detail(data.id), data);
    },
    ...options
  });
};

// Delete a scraper
export const useDeleteScraper = (
  options?: UseMutationOptions<void, ApiError, string>
) => {
  const queryClient = useQueryClient();
  
  return useMutation<void, ApiError, string>({
    mutationFn: (id) => deleteScraper(id),
    onSuccess: (_, id) => {
      // Invalidate scrapers list query to refetch
      queryClient.invalidateQueries({ queryKey: scraperKeys.lists() });
      // Remove the scraper from the cache
      queryClient.removeQueries({ queryKey: scraperKeys.detail(id) });
      // Remove the scraper status from the cache
      queryClient.removeQueries({ queryKey: scraperKeys.scraperStatus(id) });
    },
    ...options
  });
};

// Start a scraper
export const useStartScraper = (
  options?: UseMutationOptions<void, ApiError, string>
) => {
  const queryClient = useQueryClient();
  
  return useMutation<void, ApiError, string>({
    mutationFn: (id) => startScraper(id),
    onSuccess: (_, id) => {
      // Invalidate scraper status query to refetch
      queryClient.invalidateQueries({ queryKey: scraperKeys.scraperStatus(id) });
    },
    ...options
  });
};

// Stop a scraper
export const useStopScraper = (
  options?: UseMutationOptions<void, ApiError, string>
) => {
  const queryClient = useQueryClient();
  
  return useMutation<void, ApiError, string>({
    mutationFn: (id) => stopScraper(id),
    onSuccess: (_, id) => {
      // Invalidate scraper status query to refetch
      queryClient.invalidateQueries({ queryKey: scraperKeys.scraperStatus(id) });
    },
    ...options
  });
};
