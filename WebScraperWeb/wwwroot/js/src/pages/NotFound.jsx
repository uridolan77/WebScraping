import React from 'react';
import { Link as RouterLink } from 'react-router-dom';
import { Box, Typography, Button, Paper } from '@mui/material';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';
import HomeIcon from '@mui/icons-material/Home';

const NotFound = () => {
  return (
    <Box sx={{ 
      display: 'flex', 
      flexDirection: 'column',
      alignItems: 'center', 
      justifyContent: 'center', 
      minHeight: '70vh',
      textAlign: 'center'
    }}>
      <Paper sx={{ p: 5, maxWidth: 500, width: '100%' }}>
        <ErrorOutlineIcon sx={{ fontSize: 80, color: 'error.main', mb: 2 }} />
        
        <Typography variant="h4" gutterBottom>
          404: Page Not Found
        </Typography>
        
        <Typography variant="body1" color="text.secondary" paragraph>
          Sorry, we couldn't find the page you're looking for. It might have been moved, 
          deleted, or perhaps it never existed.
        </Typography>
        
        <Button
          component={RouterLink}
          to="/scrapers"
          variant="contained"
          color="primary"
          startIcon={<HomeIcon />}
          sx={{ mt: 2 }}
        >
          Go to Home
        </Button>
      </Paper>
    </Box>
  );
};

export default NotFound;