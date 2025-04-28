import React, { createContext, useContext, ReactNode } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useCreateScraper, useUpdateScraper, useDeleteScraper, useStartScraper, useStopScraper } from '../hooks/queries/useScraperQueries';
import { getAllScrapers } from '../api/scrapers';
import { Scraper, ScraperStatus } from '../types';

interface ScraperContextType {
  // Queries
  scrapers: Scraper[];
  selectedScraper: Scraper | null;
  scraperStatus: Record<string, ScraperStatus>;
  loading: boolean;
  error: string | null;

  // Actions
  fetchScraper: (id: string) => Promise<Scraper | null>;
  refreshScrapers: () => void;
  addScraper: (scraper: Scraper) => Promise<Scraper | null>;
  editScraper: (scraper: Scraper) => Promise<Scraper | null>;
  removeScraper: (id: string) => Promise<void>;
  start: (id: string) => Promise<void>;
  stop: (id: string) => Promise<void>;
  fetchScraperStatus: (id: string) => Promise<ScraperStatus | null>;
}

// Create the context with a default value
const ScraperContext = createContext<ScraperContextType>({
  scrapers: [],
  selectedScraper: null,
  scraperStatus: {},
  loading: false,
  error: null,

  fetchScraper: async () => null,
  refreshScrapers: () => {},
  addScraper: async () => null,
  editScraper: async () => null,
  removeScraper: async () => {},
  start: async () => {},
  stop: async () => {},
  fetchScraperStatus: async () => null
});

// Custom hook to use the scraper context
export const useScraperContext = () => {
  return useContext(ScraperContext);
};

interface ScraperProviderProps {
  children: ReactNode;
}

// Provider component
export const ScraperProvider: React.FC<ScraperProviderProps> = ({ children }) => {
  // Use React Query hooks
  const {
    data: scrapers = [],
    isLoading: isScrapersLoading,
    error: scrapersError,
    refetch: refetchScrapers
  } = useQuery({
    queryKey: ['scrapers'],
    queryFn: getAllScrapers
  });

  const createScraperMutation = useCreateScraper();
  const updateScraperMutation = useUpdateScraper();
  const deleteScraperMutation = useDeleteScraper();
  const startScraperMutation = useStartScraper();
  const stopScraperMutation = useStopScraper();

  // Fetch a single scraper
  const fetchScraper = async (id: string): Promise<Scraper | null> => {
    try {
      // Use direct API call instead of React Query hook
      const response = await fetch(`/api/scrapers/${id}`);
      if (!response.ok) {
        throw new Error(`Failed to fetch scraper: ${response.statusText}`);
      }
      const data = await response.json();
      return data || null;
    } catch (error) {
      console.error(`Error fetching scraper with id ${id}:`, error);
      return null;
    }
  };

  // Fetch scraper status
  const fetchScraperStatus = async (id: string): Promise<ScraperStatus | null> => {
    try {
      // Use direct API call instead of React Query hook
      const response = await fetch(`/api/scrapers/${id}/status`);
      if (!response.ok) {
        throw new Error(`Failed to fetch scraper status: ${response.statusText}`);
      }
      const data = await response.json();
      return data || null;
    } catch (error) {
      console.error(`Error fetching status for scraper with id ${id}:`, error);
      return null;
    }
  };

  // Add a new scraper
  const addScraper = async (scraper: Scraper): Promise<Scraper | null> => {
    try {
      const result = await createScraperMutation.mutateAsync(scraper);
      return result;
    } catch (error) {
      console.error('Error creating scraper:', error);
      return null;
    }
  };

  // Edit an existing scraper
  const editScraper = async (scraper: Scraper): Promise<Scraper | null> => {
    try {
      const result = await updateScraperMutation.mutateAsync(scraper);
      return result;
    } catch (error) {
      console.error(`Error updating scraper with id ${scraper.id}:`, error);
      return null;
    }
  };

  // Remove a scraper
  const removeScraper = async (id: string): Promise<void> => {
    try {
      await deleteScraperMutation.mutateAsync(id);
    } catch (error) {
      console.error(`Error deleting scraper with id ${id}:`, error);
      throw error;
    }
  };

  // Start a scraper
  const start = async (id: string): Promise<void> => {
    try {
      await startScraperMutation.mutateAsync(id);
    } catch (error) {
      console.error(`Error starting scraper with id ${id}:`, error);
      throw error;
    }
  };

  // Stop a scraper
  const stop = async (id: string): Promise<void> => {
    try {
      await stopScraperMutation.mutateAsync(id);
    } catch (error) {
      console.error(`Error stopping scraper with id ${id}:`, error);
      throw error;
    }
  };

  // Compute derived state
  const loading = isScrapersLoading ||
    createScraperMutation.isPending ||
    updateScraperMutation.isPending ||
    deleteScraperMutation.isPending ||
    startScraperMutation.isPending ||
    stopScraperMutation.isPending;

  const error = scrapersError?.message ||
    createScraperMutation.error?.message ||
    updateScraperMutation.error?.message ||
    deleteScraperMutation.error?.message ||
    startScraperMutation.error?.message ||
    stopScraperMutation.error?.message ||
    null;

  // Build the context value
  const value: ScraperContextType = {
    scrapers,
    selectedScraper: null, // This will be set by individual components
    scraperStatus: {}, // This will be populated by individual components
    loading,
    error,

    fetchScraper,
    refreshScrapers: refetchScrapers,
    addScraper,
    editScraper,
    removeScraper,
    start,
    stop,
    fetchScraperStatus
  };

  return (
    <ScraperContext.Provider value={value}>
      {children}
    </ScraperContext.Provider>
  );
};

export default ScraperContext;
