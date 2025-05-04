import React from 'react';
import { 
  Typography, 
  Alert,
  Box,
  Button,
  Card, 
  CardContent,
  CircularProgress,
  Divider,
  Grid,
  Chip,
  LinearProgress
} from '@mui/material';
import { Refresh as RefreshIcon } from '@mui/icons-material';

/**
 * Helper function to handle .NET-style response format with $values
 * @param {any} data - The data to extract array from
 * @returns {Array} - Array of items
 */
const getArrayFromResponse = (data) => {
  if (!data) return [];
  if (Array.isArray(data)) return data;
  if (data.$values && Array.isArray(data.$values)) return data.$values;
  return [];
};

/**
 * Component for the Monitor tab in the scraper details page
 */
const MonitorTab = ({ 
  status, 
  monitorData, 
  monitorError, 
  isMonitorLoading, 
  isActionInProgress,
  onRefreshMonitor 
}) => {
  return (
    <>
      <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h6">
          Real-time Scraper Monitoring
        </Typography>
        <Button
          variant="outlined"
          startIcon={<RefreshIcon />}
          onClick={onRefreshMonitor}
          disabled={isActionInProgress || isMonitorLoading || !status?.isRunning}
        >
          Refresh
        </Button>
      </Box>

      {!status?.isRunning ? (
        <Alert severity="info" sx={{ mb: 3 }}>
          The scraper is not currently running. Start the scraper to see real-time monitoring data.
        </Alert>
      ) : monitorError ? (
        <Alert severity={monitorError.isScraperNotRunningError ? "info" : "error"} sx={{ mb: 3 }}>
          {monitorError.message || "Error loading monitoring data. The scraper may have stopped running."}
        </Alert>
      ) : isMonitorLoading && !monitorData ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
          <CircularProgress />
        </Box>
      ) : (
        <Grid container spacing={3}>
          {/* Status Card */}
          <Grid item xs={12} md={6}>
            <StatusCard monitorData={monitorData} />
          </Grid>

          {/* Progress Card */}
          <Grid item xs={12} md={6}>
            <ProgressCard monitorData={monitorData} />
          </Grid>

          {/* Performance Card */}
          <Grid item xs={12} md={6}>
            <PerformanceCard monitorData={monitorData} />
          </Grid>

          {/* Recent Activity Card */}
          <Grid item xs={12} md={6}>
            <RecentActivityCard monitorData={monitorData} getArrayFromResponse={getArrayFromResponse} />
          </Grid>
        </Grid>
      )}
    </>
  );
};

/**
 * Component for the status section of the monitor tab
 */
const StatusCard = ({ monitorData }) => {
  return (
    <Card>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Status
        </Typography>
        <Divider sx={{ mb: 2 }} />

        <Grid container spacing={2}>
          <Grid item xs={6}>
            <Typography variant="subtitle2" color="textSecondary">
              State
            </Typography>
            <Box sx={{ mb: 1 }}>
              {monitorData?.status?.isRunning ? (
                <Chip label="Running" color="success" size="small" />
              ) : (
                <Chip label="Stopped" color="default" size="small" />
              )}
            </Box>
          </Grid>

          <Grid item xs={6}>
            <Typography variant="subtitle2" color="textSecondary">
              URLs Processed
            </Typography>
            <Typography variant="body1" sx={{ mb: 1 }}>
              {monitorData?.status?.urlsProcessed || 0}
            </Typography>
          </Grid>

          <Grid item xs={6}>
            <Typography variant="subtitle2" color="textSecondary">
              Start Time
            </Typography>
            <Typography variant="body1" sx={{ mb: 1 }}>
              {monitorData?.status?.startTime ? new Date(monitorData.status.startTime).toLocaleTimeString() : 'N/A'}
            </Typography>
          </Grid>

          <Grid item xs={6}>
            <Typography variant="subtitle2" color="textSecondary">
              Elapsed Time
            </Typography>
            <Typography variant="body1" sx={{ mb: 1 }}>
              {monitorData?.status?.elapsedTime || '00:00:00'}
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="subtitle2" color="textSecondary">
              Message
            </Typography>
            <Typography variant="body1" sx={{ mb: 1 }}>
              {monitorData?.status?.message || 'No message'}
            </Typography>
          </Grid>
        </Grid>
      </CardContent>
    </Card>
  );
};

/**
 * Component for the progress section of the monitor tab
 */
const ProgressCard = ({ monitorData }) => {
  return (
    <Card>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Progress
        </Typography>
        <Divider sx={{ mb: 2 }} />

        <Box sx={{ mb: 2 }}>
          <Typography variant="subtitle2" color="textSecondary" gutterBottom>
            Completion
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <Box sx={{ width: '100%', mr: 1 }}>
              <LinearProgress
                variant="determinate"
                value={monitorData?.progress?.percentComplete || 0}
                color="success"
                sx={{ height: 10, borderRadius: 5 }}
              />
            </Box>
            <Box sx={{ minWidth: 35 }}>
              <Typography variant="body2" color="textSecondary">
                {monitorData?.progress?.percentComplete || 0}%
              </Typography>
            </Box>
          </Box>
        </Box>

        <Grid container spacing={2}>
          <Grid item xs={12}>
            <Typography variant="subtitle2" color="textSecondary">
              Current URL
            </Typography>
            <Typography variant="body1" sx={{ mb: 1, wordBreak: 'break-all' }}>
              {monitorData?.progress?.currentUrl || 'N/A'}
            </Typography>
          </Grid>

          <Grid item xs={6}>
            <Typography variant="subtitle2" color="textSecondary">
              Current Depth
            </Typography>
            <Typography variant="body1" sx={{ mb: 1 }}>
              {monitorData?.progress?.currentDepth || 0} / {monitorData?.progress?.maxDepth || 0}
            </Typography>
          </Grid>

          <Grid item xs={6}>
            <Typography variant="subtitle2" color="textSecondary">
              Est. Time Remaining
            </Typography>
            <Typography variant="body1" sx={{ mb: 1 }}>
              {monitorData?.progress?.estimatedTimeRemaining || 'Unknown'}
            </Typography>
          </Grid>
        </Grid>
      </CardContent>
    </Card>
  );
};

/**
 * Component for the performance section of the monitor tab
 */
const PerformanceCard = ({ monitorData }) => {
  return (
    <Card>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Performance
        </Typography>
        <Divider sx={{ mb: 2 }} />

        <Grid container spacing={2}>
          <Grid item xs={6}>
            <Typography variant="subtitle2" color="textSecondary">
              Requests/Second
            </Typography>
            <Typography variant="body1" sx={{ mb: 1 }}>
              {monitorData?.performance?.requestsPerSecond || 0}
            </Typography>
          </Grid>

          <Grid item xs={6}>
            <Typography variant="subtitle2" color="textSecondary">
              Active Threads
            </Typography>
            <Typography variant="body1" sx={{ mb: 1 }}>
              {monitorData?.performance?.activeThreads || 0}
            </Typography>
          </Grid>

          <Grid item xs={6}>
            <Typography variant="subtitle2" color="textSecondary">
              Memory Usage
            </Typography>
            <Typography variant="body1" sx={{ mb: 1 }}>
              {monitorData?.performance?.memoryUsage || 'N/A'}
            </Typography>
          </Grid>

          <Grid item xs={6}>
            <Typography variant="subtitle2" color="textSecondary">
              CPU Usage
            </Typography>
            <Typography variant="body1" sx={{ mb: 1 }}>
              {monitorData?.performance?.cpuUsage || 'N/A'}
            </Typography>
          </Grid>
        </Grid>
      </CardContent>
    </Card>
  );
};

/**
 * Component for the recent activity section of the monitor tab
 */
const RecentActivityCard = ({ monitorData, getArrayFromResponse }) => {
  return (
    <Card>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Recent Activity
        </Typography>
        <Divider sx={{ mb: 2 }} />

        {monitorData?.recentActivity && getArrayFromResponse(monitorData.recentActivity).length > 0 ? (
          <Box sx={{ maxHeight: 300, overflow: 'auto' }}>
            {getArrayFromResponse(monitorData.recentActivity).map((activity, index) => (
              <Box
                key={index}
                sx={{
                  mb: 1,
                  p: 1,
                  bgcolor: 'background.default',
                  borderRadius: 1,
                  borderLeft: 4,
                  borderColor: 'success.main'
                }}
              >
                <Typography variant="body2" color="textSecondary">
                  {new Date(activity.timestamp).toLocaleTimeString()} - {activity.action}
                </Typography>
                <Typography variant="body2">
                  {activity.url}
                </Typography>
                <Typography variant="caption" color="textSecondary">
                  {activity.details}
                </Typography>
              </Box>
            ))}
          </Box>
        ) : (
          <Typography variant="body1" sx={{ p: 2, textAlign: 'center' }}>
            No recent activity
          </Typography>
        )}
      </CardContent>
    </Card>
  );
};

export default MonitorTab;