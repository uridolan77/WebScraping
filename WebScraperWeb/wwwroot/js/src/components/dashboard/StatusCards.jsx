import React from 'react';
import {
  Grid,
  Card,
  CardContent,
  Typography,
  Box,
  Chip,
} from '@mui/material';
import {
  CheckCircle as CheckCircleIcon,
  Cancel as CancelIcon,
  Public as PublicIcon,
  AccessTime as AccessTimeIcon,
  Storage as StorageIcon,
  Timelapse as TimelapseIcon,
  DataUsage as DataUsageIcon,
} from '@mui/icons-material';

/**
 * Component to display the status of a scraper in card format
 */
const StatusCards = ({ isRunning, scraperStats }) => {
  // Default values if no stats are provided
  const stats = scraperStats || {
    urlsProcessed: 0,
    urlsQueued: 0,
    urlsSucceeded: 0,
    urlsFailed: 0,
    pagesPerSecond: 0,
    elapsedTime: '00:00:00',
    dataSize: '0',
  };

  return (
    <Grid container spacing={3}>
      {/* Running Status */}
      <Grid item xs={12} sm={6} md={3}>
        <Card>
          <CardContent sx={{ textAlign: 'center' }}>
            <Box sx={{ 
              display: 'flex', 
              justifyContent: 'center', 
              mb: 2, 
              color: isRunning ? 'success.main' : 'text.secondary' 
            }}>
              {isRunning ? 
                <CheckCircleIcon fontSize="large" /> : 
                <CancelIcon fontSize="large" />
              }
            </Box>
            <Typography variant="h6" component="div">
              Status
            </Typography>
            <Chip 
              label={isRunning ? "Running" : "Stopped"} 
              color={isRunning ? "success" : "default"}
              size="small"
              sx={{ mt: 1 }}
            />
          </CardContent>
        </Card>
      </Grid>

      {/* URLs Processed */}
      <Grid item xs={12} sm={6} md={3}>
        <Card>
          <CardContent sx={{ textAlign: 'center' }}>
            <Box sx={{ display: 'flex', justifyContent: 'center', mb: 2, color: 'primary.main' }}>
              <PublicIcon fontSize="large" />
            </Box>
            <Typography variant="h6" component="div">
              Pages
            </Typography>
            <Typography variant="h4" color="text.primary">
              {stats.urlsProcessed !== undefined ? stats.urlsProcessed.toLocaleString() : '0'}
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              {stats.urlsQueued !== undefined && 
                `${stats.urlsQueued.toLocaleString()} in queue`}
            </Typography>
          </CardContent>
        </Card>
      </Grid>

      {/* Data Collected */}
      <Grid item xs={12} sm={6} md={3}>
        <Card>
          <CardContent sx={{ textAlign: 'center' }}>
            <Box sx={{ display: 'flex', justifyContent: 'center', mb: 2, color: 'info.main' }}>
              <StorageIcon fontSize="large" />
            </Box>
            <Typography variant="h6" component="div">
              Data Collected
            </Typography>
            <Typography variant="h4" color="text.primary">
              {stats.dataSize || '0 KB'}
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              {stats.urlsSucceeded !== undefined && stats.urlsFailed !== undefined && 
                `${stats.urlsSucceeded.toLocaleString()} success / ${stats.urlsFailed.toLocaleString()} failed`}
            </Typography>
          </CardContent>
        </Card>
      </Grid>

      {/* Time Stats */}
      <Grid item xs={12} sm={6} md={3}>
        <Card>
          <CardContent sx={{ textAlign: 'center' }}>
            <Box sx={{ display: 'flex', justifyContent: 'center', mb: 2, color: 'warning.main' }}>
              <AccessTimeIcon fontSize="large" />
            </Box>
            <Typography variant="h6" component="div">
              Time
            </Typography>
            <Typography variant="h4" color="text.primary">
              {stats.elapsedTime || '00:00:00'}
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', mt: 1 }}>
              <TimelapseIcon fontSize="small" sx={{ mr: 0.5, color: 'text.secondary' }} />
              <Typography variant="body2" color="text.secondary">
                {stats.pagesPerSecond !== undefined ? 
                  `${stats.pagesPerSecond.toFixed(2)} pages/sec` : 
                  '0 pages/sec'}
              </Typography>
            </Box>
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );
};

export default StatusCards;