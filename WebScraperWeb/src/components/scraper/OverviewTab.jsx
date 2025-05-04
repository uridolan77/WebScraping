import React from 'react';
import { 
  Grid, 
  Card, 
  CardContent, 
  Typography, 
  Divider, 
  Box, 
  CircularProgress, 
  Alert 
} from '@mui/material';

/**
 * Helper function to format dates
 * @param {string} dateString - The date string to format
 * @returns {string} - Formatted date string
 */
const formatDate = (dateString) => {
  if (!dateString) return 'Never';

  const date = new Date(dateString);
  return new Intl.DateTimeFormat('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(date);
};

/**
 * Component for displaying scraper errors
 */
const ErrorsSection = ({ status }) => {
  if (!status?.hasErrors) return null;

  return (
    <Grid item xs={12}>
      <Card sx={{ bgcolor: 'error.light' }}>
        <CardContent>
          <Typography variant="h6" gutterBottom color="error">
            Errors
          </Typography>
          <Divider sx={{ mb: 2 }} />
          
          {status.errors && status.errors.length > 0 ? (
            <Box sx={{ maxHeight: 300, overflow: 'auto' }}>
              {status.errors.map((error, index) => (
                <Box key={index} sx={{ mb: 1, p: 1, bgcolor: 'error.lighter', borderRadius: 1, borderLeft: '4px solid', borderColor: 'error.main' }}>
                  <Typography variant="body2" color="textSecondary">
                    {new Date(error.timestamp).toLocaleString()}
                  </Typography>
                  <Typography variant="body2" fontWeight="bold">
                    URL: {error.url}
                  </Typography>
                  <Typography variant="body2">
                    {error.message}
                  </Typography>
                </Box>
              ))}
            </Box>
          ) : status.recentErrorLogs && status.recentErrorLogs.length > 0 ? (
            <Box sx={{ maxHeight: 300, overflow: 'auto' }}>
              {status.recentErrorLogs.map((log, index) => (
                <Box key={index} sx={{ mb: 1, p: 1, bgcolor: 'error.lighter', borderRadius: 1, borderLeft: '4px solid', borderColor: 'error.main' }}>
                  <Typography variant="body2" color="textSecondary">
                    {new Date(log.timestamp).toLocaleString()}
                  </Typography>
                  <Typography variant="body2">
                    {log.message}
                  </Typography>
                </Box>
              ))}
            </Box>
          ) : (
            <Alert severity="error">
              The scraper has encountered errors. Check the logs tab for more details.
            </Alert>
          )}
        </CardContent>
      </Card>
    </Grid>
  );
};

/**
 * Component for the Overview tab in the scraper details page
 */
const OverviewTab = ({ status, scraper, logs, isStatusLoading, isLogsLoading }) => {
  return (
    <Grid container spacing={3}>
      {/* Status Card */}
      <Grid item xs={12} md={6}>
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Scraper Status
            </Typography>
            <Divider sx={{ mb: 2 }} />
            {isStatusLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', p: 2 }}>
                <CircularProgress />
              </Box>
            ) : (
              <Grid container spacing={2}>
                <Grid item xs={6}>
                  <Typography variant="subtitle2" color="textSecondary">
                    Status
                  </Typography>
                  <Box>
                    {status?.isRunning ? 'Running' : (status?.hasErrors ? 'Error' : 'Idle')}
                  </Box>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="subtitle2" color="textSecondary">
                    Last Run
                  </Typography>
                  <Typography variant="body1">
                    {formatDate(status?.lastRun || scraper?.lastRun)}
                  </Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="subtitle2" color="textSecondary">
                    URLs Processed
                  </Typography>
                  <Typography variant="body1">
                    {status?.urlsProcessed || scraper?.urlsProcessed || 0}
                  </Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="subtitle2" color="textSecondary">
                    Error Count
                  </Typography>
                  <Typography variant="body1">
                    {status?.errorCount || 0}
                  </Typography>
                </Grid>
              </Grid>
            )}
          </CardContent>
        </Card>
      </Grid>

      {/* Details Card */}
      <Grid item xs={12} md={6}>
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Scraper Details
            </Typography>
            <Divider sx={{ mb: 2 }} />
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <Typography variant="subtitle2" color="textSecondary">
                  ID
                </Typography>
                <Typography variant="body1" noWrap>
                  {scraper?.id || 'N/A'}
                </Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2" color="textSecondary">
                  Created
                </Typography>
                <Typography variant="body1">
                  {formatDate(scraper?.createdAt)}
                </Typography>
              </Grid>
              <Grid item xs={12}>
                <Typography variant="subtitle2" color="textSecondary">
                  Base URL
                </Typography>
                <Typography variant="body1">
                  {scraper?.baseUrl || 'N/A'}
                </Typography>
              </Grid>
              <Grid item xs={12}>
                <Typography variant="subtitle2" color="textSecondary">
                  Start URL
                </Typography>
                <Typography variant="body1">
                  {scraper?.startUrl || 'N/A'}
                </Typography>
              </Grid>
            </Grid>
          </CardContent>
        </Card>
      </Grid>

      {/* Recent Activity Card */}
      <Grid item xs={12}>
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Recent Activity
            </Typography>
            <Divider sx={{ mb: 2 }} />
            {isLogsLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', p: 2 }}>
                <CircularProgress />
              </Box>
            ) : logs && logs.length > 0 ? (
              <Box sx={{ maxHeight: 300, overflow: 'auto' }}>
                {logs.slice(0, 5).map((log, index) => (
                  <Box key={index} sx={{ mb: 1, p: 1, bgcolor: 'background.default', borderRadius: 1 }}>
                    <Typography variant="body2" color="textSecondary">
                      {new Date(log.timestamp).toLocaleString()}
                    </Typography>
                    <Typography variant="body2">
                      {log.message}
                    </Typography>
                  </Box>
                ))}
              </Box>
            ) : (
              <Typography variant="body1">
                No recent activity
              </Typography>
            )}
          </CardContent>
        </Card>
      </Grid>

      {/* Errors Section */}
      <ErrorsSection status={status} />
    </Grid>
  );
};

export default OverviewTab;