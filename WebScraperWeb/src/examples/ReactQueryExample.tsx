import React from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import ScraperListExample from '../components/examples/ScraperListExample';
import { Container, Typography, Box, Paper } from '@mui/material';

// Create a client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      gcTime: 10 * 60 * 1000, // 10 minutes
      retry: 1,
      refetchOnWindowFocus: true,
    },
  },
});

/**
 * Example component demonstrating React Query with virtualized tables
 */
const ReactQueryExample: React.FC = () => {
  return (
    <QueryClientProvider client={queryClient}>
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Typography variant="h4" gutterBottom>
          React Query with Virtualized Tables Example
        </Typography>
        
        <Paper sx={{ p: 3, mb: 4 }}>
          <Typography variant="h6" gutterBottom>
            Key Features
          </Typography>
          
          <Box component="ul" sx={{ pl: 2 }}>
            <Box component="li" sx={{ mb: 1 }}>
              <Typography variant="body1">
                <strong>Efficient Data Fetching:</strong> React Query handles caching, background updates, and stale data management
              </Typography>
            </Box>
            <Box component="li" sx={{ mb: 1 }}>
              <Typography variant="body1">
                <strong>Virtualized Rendering:</strong> Only visible rows are rendered, improving performance with large datasets
              </Typography>
            </Box>
            <Box component="li" sx={{ mb: 1 }}>
              <Typography variant="body1">
                <strong>Optimized UI:</strong> Components are memoized to prevent unnecessary re-renders
              </Typography>
            </Box>
            <Box component="li">
              <Typography variant="body1">
                <strong>TypeScript Integration:</strong> Full type safety for API responses and component props
              </Typography>
            </Box>
          </Box>
        </Paper>
        
        <ScraperListExample />
      </Container>
      
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
};

export default ReactQueryExample;
