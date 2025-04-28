import React, { Component } from 'react';
import { Box, Typography, Button, Paper, Accordion, AccordionSummary, AccordionDetails, Divider, Alert } from '@mui/material';
import { ErrorOutline, ExpandMore, BugReport, Refresh, Home } from '@mui/icons-material';

/**
 * ErrorBoundary component that catches JavaScript errors in its child component tree
 * and displays a fallback UI instead of crashing the whole app
 */
class ErrorBoundary extends Component {
  constructor(props) {
    super(props);
    this.state = {
      hasError: false,
      error: null,
      errorInfo: null,
      errorCount: 0,
      errorLog: []
    };
  }

  static getDerivedStateFromError(error) {
    // Update state so the next render will show the fallback UI
    return { hasError: true };
  }

  componentDidCatch(error, errorInfo) {
    // Update state with the error details
    this.setState(prevState => ({
      error: error,
      errorInfo: errorInfo,
      errorCount: prevState.errorCount + 1,
      errorLog: [
        ...prevState.errorLog,
        {
          timestamp: new Date().toISOString(),
          error: error.toString(),
          stack: error.stack,
          componentStack: errorInfo.componentStack
        }
      ]
    }));

    // Log the error to console
    console.error("Error caught by ErrorBoundary:", error, errorInfo);

    // Here you could send the error to an error reporting service
    // Example: sendToErrorReportingService(error, errorInfo);
  }

  // Function to send error to a reporting service (mock implementation)
  sendToErrorReportingService = (error, errorInfo) => {
    // This would be implemented to send errors to a service like Sentry, LogRocket, etc.
    console.log("Sending error to reporting service:", error, errorInfo);
  }

  handleReset = () => {
    this.setState(prevState => ({
      hasError: false,
      error: null,
      errorInfo: null,
      // Keep the error count and log for reference
    }));

    // Call the onReset prop if provided
    if (this.props.onReset) {
      this.props.onReset();
    }
  }

  handleGoHome = () => {
    // Reset the error state before navigating
    this.setState({
      hasError: false,
      error: null,
      errorInfo: null
    });

    // Navigate to home page
    window.location.href = '/';
  }

  render() {
    const { fallback, children } = this.props;
    const { hasError, error, errorInfo, errorCount } = this.state;

    // If there's no error, render children normally
    if (!hasError) {
      return children;
    }

    // If a custom fallback is provided, use it
    if (fallback) {
      return fallback(error, errorInfo, this.handleReset);
    }

    // Default fallback UI
    return (
      <Paper
        elevation={3}
        sx={{
          p: 4,
          m: 2,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          maxWidth: '800px',
          mx: 'auto'
        }}
      >
        <ErrorOutline color="error" sx={{ fontSize: 60, mb: 2 }} />

        <Typography variant="h5" gutterBottom>
          Something went wrong
        </Typography>

        <Alert severity="error" sx={{ width: '100%', mb: 3 }}>
          {error ? error.toString() : 'An unexpected error occurred'}
        </Alert>

        <Box sx={{ width: '100%', mb: 3 }}>
          <Accordion>
            <AccordionSummary
              expandIcon={<ExpandMore />}
              aria-controls="error-details-content"
              id="error-details-header"
            >
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <BugReport sx={{ mr: 1 }} />
                <Typography>Technical Details</Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <Typography variant="subtitle2" gutterBottom>
                Error Count: {errorCount}
              </Typography>

              <Divider sx={{ my: 1 }} />

              <Typography variant="subtitle2" gutterBottom>
                Error Stack:
              </Typography>
              <Box
                component="pre"
                sx={{
                  p: 2,
                  bgcolor: 'grey.100',
                  borderRadius: 1,
                  overflow: 'auto',
                  fontSize: '0.75rem',
                  maxHeight: '200px'
                }}
              >
                {error && error.stack}
              </Box>

              <Divider sx={{ my: 1 }} />

              <Typography variant="subtitle2" gutterBottom>
                Component Stack:
              </Typography>
              <Box
                component="pre"
                sx={{
                  p: 2,
                  bgcolor: 'grey.100',
                  borderRadius: 1,
                  overflow: 'auto',
                  fontSize: '0.75rem',
                  maxHeight: '200px'
                }}
              >
                {errorInfo && errorInfo.componentStack}
              </Box>
            </AccordionDetails>
          </Accordion>
        </Box>

        <Box sx={{ display: 'flex', gap: 2 }}>
          <Button
            variant="contained"
            color="primary"
            onClick={this.handleReset}
            startIcon={<Refresh />}
          >
            Try Again
          </Button>
          <Button
            variant="outlined"
            onClick={this.handleGoHome}
            startIcon={<Home />}
          >
            Go to Dashboard
          </Button>
        </Box>
      </Paper>
    );
  }
}

export default ErrorBoundary;
