import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  Chip,
  CircularProgress,
  Grid,
  Divider
} from '@mui/material';
import CheckIcon from '@mui/icons-material/Check';
import QueryStatsIcon from '@mui/icons-material/QueryStats';
import LinkIcon from '@mui/icons-material/Link';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import StorageIcon from '@mui/icons-material/Storage';
import MonitorHeartIcon from '@mui/icons-material/MonitorHeart';

/**
 * ScraperStatusCard component to display the current status and metrics of a scraper
 * 
 * @param {Object} props - Component properties
 * @param {boolean} props.isRunning - Whether the scraper is currently running
 * @param {number} props.urlsProcessed - Number of URLs processed
 * @param {string} props.startTime - ISO string of when the scraper started
 * @param {string} props.endTime - ISO string of when the scraper finished
 * @param {string} props.elapsedTime - Formatted elapsed time (e.g., "00:05:23")
 * @param {number} props.resultsCount - Number of results collected
 * @param {boolean} props.monitoringEnabled - Whether monitoring is enabled
 */
const ScraperStatusCard = ({
  isRunning,
  urlsProcessed = 0,
  startTime,
  endTime,
  elapsedTime = '00:00:00',
  resultsCount = 0,
  monitoringEnabled = false
}) => {
  // Format date helper function
  const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleString();
  };

  return (
    <Card sx={{ mb: 3 }}>
      <CardContent>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>
            Scraper Status
          </Typography>
          
          {isRunning ? (
            <Chip
              size="small"
              color="secondary"
              icon={<CircularProgress size={16} color="inherit" />}
              label="Running"
            />
          ) : endTime ? (
            <Chip
              size="small"
              color="success"
              icon={<CheckIcon />}
              label="Completed"
            />
          ) : (
            <Chip
              size="small"
              color="default"
              icon={<QueryStatsIcon />}
              label="Idle"
            />
          )}
        </Box>
        
        <Grid container spacing={2}>
          <Grid item xs={6} sm={4}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
              <LinkIcon fontSize="small" sx={{ mr: 1, color: 'text.secondary' }} />
              <Typography variant="body2" color="text.secondary">
                URLs Processed
              </Typography>
            </Box>
            <Typography variant="h6">
              {urlsProcessed}
            </Typography>
          </Grid>
          
          <Grid item xs={6} sm={4}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
              <StorageIcon fontSize="small" sx={{ mr: 1, color: 'text.secondary' }} />
              <Typography variant="body2" color="text.secondary">
                Results Collected
              </Typography>
            </Box>
            <Typography variant="h6">
              {resultsCount}
            </Typography>
          </Grid>
          
          <Grid item xs={6} sm={4}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
              <AccessTimeIcon fontSize="small" sx={{ mr: 1, color: 'text.secondary' }} />
              <Typography variant="body2" color="text.secondary">
                Duration
              </Typography>
            </Box>
            <Typography variant="h6">
              {elapsedTime || '00:00:00'}
            </Typography>
          </Grid>
          
          {(startTime || endTime) && (
            <Grid item xs={12}>
              <Divider sx={{ my: 1 }} />
              <Grid container spacing={2}>
                {startTime && (
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Started
                    </Typography>
                    <Typography variant="body2">
                      {formatDate(startTime)}
                    </Typography>
                  </Grid>
                )}
                
                {endTime && (
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Completed
                    </Typography>
                    <Typography variant="body2">
                      {formatDate(endTime)}
                    </Typography>
                  </Grid>
                )}
              </Grid>
            </Grid>
          )}
          
          <Grid item xs={12}>
            <Divider sx={{ my: 1 }} />
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <MonitorHeartIcon 
                fontSize="small" 
                sx={{ mr: 1, color: monitoringEnabled ? 'primary.main' : 'text.disabled' }} 
              />
              <Typography variant="body2">
                {monitoringEnabled ? 'Continuous Monitoring Enabled' : 'Continuous Monitoring Disabled'}
              </Typography>
            </Box>
          </Grid>
        </Grid>
      </CardContent>
    </Card>
  );
};

export default ScraperStatusCard;