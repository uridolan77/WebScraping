import React from 'react';
import { Box, CircularProgress, Alert, Typography, Button } from '@mui/material';
import { ErrorBoundary } from 'react-error-boundary';

interface AsyncWrapperProps {
  loading: boolean;
  error: any;
  children: React.ReactNode;
  loadingComponent?: React.ReactNode;
  errorComponent?: React.ReactNode;
  onRetry?: () => void;
  loadingMessage?: string;
  errorMessage?: string;
  minHeight?: string | number;
}

/**
 * A wrapper component for handling async operations with loading and error states
 */
const AsyncWrapper: React.FC<AsyncWrapperProps> = ({
  loading,
  error,
  children,
  loadingComponent,
  errorComponent,
  onRetry,
  loadingMessage = 'Loading...',
  errorMessage = 'An error occurred',
  minHeight = 200
}) => {
  // Default loading component
  const defaultLoadingComponent = (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight,
        width: '100%',
        p: 3
      }}
    >
      <CircularProgress size={40} />
      <Typography variant="body1" sx={{ mt: 2 }}>
        {loadingMessage}
      </Typography>
    </Box>
  );

  // Default error component
  const defaultErrorComponent = (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        width: '100%',
        p: 3
      }}
    >
      <Alert severity="error" sx={{ width: '100%', mb: 2 }}>
        {error?.message || errorMessage}
      </Alert>
      {onRetry && (
        <Button variant="contained" color="primary" onClick={onRetry}>
          Retry
        </Button>
      )}
    </Box>
  );

  // Error fallback component for ErrorBoundary
  const ErrorFallback = ({ error, resetErrorBoundary }: { error: Error; resetErrorBoundary: () => void }) => (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        width: '100%',
        p: 3
      }}
    >
      <Alert severity="error" sx={{ width: '100%', mb: 2 }}>
        <Typography variant="h6" gutterBottom>
          Something went wrong
        </Typography>
        <Typography variant="body2">{error.message}</Typography>
      </Alert>
      <Button variant="contained" color="primary" onClick={resetErrorBoundary}>
        Try again
      </Button>
    </Box>
  );

  // Render loading state
  if (loading) {
    return <>{loadingComponent || defaultLoadingComponent}</>;
  }

  // Render error state
  if (error) {
    return <>{errorComponent || defaultErrorComponent}</>;
  }

  // Render children wrapped in ErrorBoundary
  return (
    <ErrorBoundary FallbackComponent={ErrorFallback} onReset={onRetry}>
      {children}
    </ErrorBoundary>
  );
};

export default React.memo(AsyncWrapper);
