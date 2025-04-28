import React, { Component } from 'react';
import { Box, Typography, Button, Paper } from '@mui/material';
import { ErrorOutline } from '@mui/icons-material';

class ErrorBoundary extends Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false, error: null, errorInfo: null };
  }

  static getDerivedStateFromError(error) {
    // Update state so the next render will show the fallback UI
    return { hasError: true };
  }

  componentDidCatch(error, errorInfo) {
    // You can also log the error to an error reporting service
    this.setState({
      error: error,
      errorInfo: errorInfo
    });
    console.error("Error caught by ErrorBoundary:", error, errorInfo);
  }

  handleReset = () => {
    this.setState({ hasError: false, error: null, errorInfo: null });
  }

  render() {
    if (this.state.hasError) {
      // You can render any custom fallback UI
      return (
        <Paper 
          elevation={3} 
          sx={{ 
            p: 4, 
            m: 2, 
            display: 'flex', 
            flexDirection: 'column', 
            alignItems: 'center',
            textAlign: 'center'
          }}
        >
          <ErrorOutline color="error" sx={{ fontSize: 60, mb: 2 }} />
          <Typography variant="h5" gutterBottom>
            Something went wrong
          </Typography>
          <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
            {this.state.error && this.state.error.toString()}
          </Typography>
          <Box sx={{ mt: 2 }}>
            <Button 
              variant="contained" 
              color="primary" 
              onClick={this.handleReset}
              sx={{ mr: 2 }}
            >
              Try Again
            </Button>
            <Button 
              variant="outlined" 
              onClick={() => window.location.href = '/'}
            >
              Go to Dashboard
            </Button>
          </Box>
        </Paper>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
