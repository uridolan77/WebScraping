import { QueryClient } from '@tanstack/react-query';
import { AxiosError } from 'axios';

// Create a client
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // Default stale time of 5 minutes
      staleTime: 5 * 60 * 1000,
      // Default cache time of 10 minutes
      gcTime: 10 * 60 * 1000,
      // Retry failed queries 3 times
      retry: (failureCount, error) => {
        // Don't retry on 404s or 401s
        if (error instanceof AxiosError) {
          const status = error.response?.status;
          if (status === 404 || status === 401 || status === 403) {
            return false;
          }
        }
        return failureCount < 3;
      },
      // Refetch on window focus
      refetchOnWindowFocus: true,
      // Don't refetch on reconnect
      refetchOnReconnect: false,
    },
    mutations: {
      // Retry failed mutations once
      retry: 1,
    },
  },
});

export default queryClient;
