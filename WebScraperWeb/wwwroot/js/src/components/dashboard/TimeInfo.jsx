import React from 'react';
import {
  Paper,
  Typography,
  Box,
  Grid
} from '@mui/material';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import StopIcon from '@mui/icons-material/Stop';
import UpdateIcon from '@mui/icons-material/Update';

/**
 * Component to display time-related information for a scraper
 */
const TimeInfo = ({ startTime, endTime, lastRun, nextRun }) => {
  /**
   * Format date to a readable string
   */
  const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    
    const date = new Date(dateString);
    
    // Check if date is valid
    if (isNaN(date.getTime())) return 'Invalid date';
    
    return date.toLocaleString();
  };
  
  return (
    <Paper sx={{ p: 2, mt: 2 }}>
      <Typography variant="h6" gutterBottom>
        Time Information
      </Typography>
      
      <Grid container spacing={2}>
        {startTime && (
          <Grid item xs={12}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
              <PlayArrowIcon fontSize="small" sx={{ mr: 1, color: 'success.main' }} />
              <Typography variant="body2" component="span" sx={{ fontWeight: 'bold', mr: 1 }}>
                Start Time:
              </Typography>
              <Typography variant="body2" component="span">
                {formatDate(startTime)}
              </Typography>
            </Box>
          </Grid>
        )}
        
        {endTime && (
          <Grid item xs={12}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
              <StopIcon fontSize="small" sx={{ mr: 1, color: 'error.main' }} />
              <Typography variant="body2" component="span" sx={{ fontWeight: 'bold', mr: 1 }}>
                End Time:
              </Typography>
              <Typography variant="body2" component="span">
                {formatDate(endTime)}
              </Typography>
            </Box>
          </Grid>
        )}
        
        {lastRun && (
          <Grid item xs={12}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
              <AccessTimeIcon fontSize="small" sx={{ mr: 1, color: 'text.secondary' }} />
              <Typography variant="body2" component="span" sx={{ fontWeight: 'bold', mr: 1 }}>
                Last Run:
              </Typography>
              <Typography variant="body2" component="span">
                {formatDate(lastRun)}
              </Typography>
            </Box>
          </Grid>
        )}
        
        {nextRun && (
          <Grid item xs={12}>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <UpdateIcon fontSize="small" sx={{ mr: 1, color: 'info.main' }} />
              <Typography variant="body2" component="span" sx={{ fontWeight: 'bold', mr: 1 }}>
                Next Scheduled Run:
              </Typography>
              <Typography variant="body2" component="span">
                {formatDate(nextRun)}
              </Typography>
            </Box>
          </Grid>
        )}
      </Grid>
    </Paper>
  );
};

export default TimeInfo;