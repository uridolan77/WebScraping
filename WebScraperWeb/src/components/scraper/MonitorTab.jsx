import React, { useState, useEffect, useRef } from 'react';
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
  LinearProgress,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Paper,
  Tab,
  Tabs,
  TextField,
  InputAdornment
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  Link as LinkIcon,
  Error as ErrorIcon,
  Warning as WarningIcon,
  Info as InfoIcon,
  Search as SearchIcon
} from '@mui/icons-material';
import EnhancedMetricsCard from './EnhancedMetricsCard';

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
  const [activeTab, setActiveTab] = useState(0);
  const [searchQuery, setSearchQuery] = useState('');
  const [autoRefresh, setAutoRefresh] = useState(true);
  const refreshIntervalRef = useRef(null);

  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
  };

  // Setup auto-refresh for logs
  useEffect(() => {
    // Clear any existing interval
    if (refreshIntervalRef.current) {
      clearInterval(refreshIntervalRef.current);
      refreshIntervalRef.current = null;
    }

    // Only set up auto-refresh when the scraper is running and auto-refresh is enabled
    if (status?.isRunning && autoRefresh) {
      refreshIntervalRef.current = setInterval(() => {
        onRefreshMonitor();
      }, 5000); // Update every 5 seconds
    }

    // Cleanup on unmount
    return () => {
      if (refreshIntervalRef.current) {
        clearInterval(refreshIntervalRef.current);
      }
    };
  }, [status?.isRunning, autoRefresh, onRefreshMonitor]);

  // Extract logs from monitorData and filter for URLs and errors
  const logs = getArrayFromResponse(monitorData?.logs || []);
  const processedUrls = logs.filter(log =>
    log.message && (log.message.includes('Processed URL:') || log.message.includes('Processing URL:'))
  );
  const errorLogs = logs.filter(log =>
    log.logLevel === 'Error' || log.logLevel === 'Warning'
  );

  // Filter based on search query
  const filteredLogs = searchQuery
    ? logs.filter(log => log.message && log.message.toLowerCase().includes(searchQuery.toLowerCase()))
    : logs;

  const filteredUrls = searchQuery
    ? processedUrls.filter(log => log.message && log.message.toLowerCase().includes(searchQuery.toLowerCase()))
    : processedUrls;

  const filteredErrors = searchQuery
    ? errorLogs.filter(log => log.message && log.message.toLowerCase().includes(searchQuery.toLowerCase()))
    : errorLogs;

  return (
    <>
      <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h6">
          Real-time Scraper Monitoring
        </Typography>
        <Box>
          <Button
            variant={autoRefresh ? "contained" : "outlined"}
            color={autoRefresh ? "success" : "primary"}
            onClick={() => setAutoRefresh(!autoRefresh)}
            sx={{ mr: 1 }}
            disabled={!status?.isRunning}
          >
            {autoRefresh ? 'Auto-refresh: On' : 'Auto-refresh: Off'}
          </Button>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={onRefreshMonitor}
            disabled={isActionInProgress || isMonitorLoading || !status?.isRunning}
          >
            Refresh
          </Button>
        </Box>
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
        <>
          <Grid container spacing={3} sx={{ mb: 3 }}>
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

            {/* Enhanced Metrics Card - only shown if enhanced features are enabled */}
            {monitorData?.enhancedFeatures?.enabled && (
              <Grid item xs={12}>
                <EnhancedMetricsCard monitorData={monitorData} />
              </Grid>
            )}
          </Grid>

          {/* Processed URLs and Errors Section */}
          <Typography variant="h6" gutterBottom>
            Detailed Processing Logs
          </Typography>
          <Paper sx={{ mb: 3, p: 0 }}>
            <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
              <Tabs value={activeTab} onChange={handleTabChange} aria-label="log tabs">
                <Tab label={`All Logs (${logs.length})`} id="tab-0" />
                <Tab
                  label={`Processed URLs (${processedUrls.length})`}
                  id="tab-1"
                  iconPosition="start"
                />
                <Tab
                  label={`Errors & Warnings (${errorLogs.length})`}
                  id="tab-2"
                  iconPosition="start"
                  sx={{ color: errorLogs.length > 0 ? 'error.main' : 'inherit' }}
                />
              </Tabs>
            </Box>

            <Box sx={{ p: 2 }}>
              <TextField
                fullWidth
                placeholder="Search logs..."
                size="small"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <SearchIcon />
                    </InputAdornment>
                  ),
                }}
                sx={{ mb: 2 }}
              />

              {/* All Logs Tab */}
              {activeTab === 0 && (
                <LogsList
                  logs={filteredLogs}
                  emptyMessage="No logs found"
                  maxHeight={400}
                />
              )}

              {/* Processed URLs Tab */}
              {activeTab === 1 && (
                <LogsList
                  logs={filteredUrls}
                  emptyMessage="No processed URLs found"
                  maxHeight={400}
                  highlightUrls
                />
              )}

              {/* Errors & Warnings Tab */}
              {activeTab === 2 && (
                <LogsList
                  logs={filteredErrors}
                  emptyMessage="No errors or warnings found"
                  maxHeight={400}
                  highlightErrors
                />
              )}
            </Box>
          </Paper>
        </>
      )}
    </>
  );
};

/**
 * Component to display a list of logs
 */
const LogsList = ({ logs, emptyMessage, maxHeight = 300, highlightUrls = false, highlightErrors = false }) => {
  const endRef = useRef(null);
  
  // Auto-scroll to bottom when new logs arrive
  useEffect(() => {
    if (logs.length > 0 && endRef.current) {
      endRef.current.scrollIntoView({ behavior: 'smooth' });
    }
  }, [logs]);

  if (!logs || logs.length === 0) {
    return (
      <Box sx={{ p: 2, textAlign: 'center' }}>
        <Typography variant="body1" color="textSecondary">
          {emptyMessage}
        </Typography>
      </Box>
    );
  }

  return (
    <List
      sx={{
        maxHeight,
        overflow: 'auto',
        bgcolor: 'background.paper',
        borderRadius: 1,
        '& .MuiListItem-root': {
          borderBottom: '1px solid',
          borderColor: 'divider',
          py: 1
        }
      }}
      dense
    >
      {logs.map((log, index) => {
        const isUrlLog = log.message && (log.message.includes('Processed URL:') || log.message.includes('Processing URL:'));
        const isError = log.logLevel === 'Error';
        const isWarning = log.logLevel === 'Warning';

        let url = '';
        if (isUrlLog) {
          const urlMatch = log.message.match(/URL: (.+?)( |$)/);
          if (urlMatch && urlMatch.length > 1) {
            url = urlMatch[1];
          }
        }

        return (
          <ListItem
            key={index}
            sx={{
              bgcolor: isError ? 'rgba(255, 0, 0, 0.05)' :
                     isWarning ? 'rgba(255, 165, 0, 0.05)' :
                     isUrlLog ? 'rgba(0, 0, 255, 0.05)' : 'inherit'
            }}
          >
            <ListItemIcon sx={{ minWidth: 36 }}>
              {isError && <ErrorIcon color="error" fontSize="small" />}
              {isWarning && <WarningIcon color="warning" fontSize="small" />}
              {isUrlLog && <LinkIcon color="primary" fontSize="small" />}
              {!isError && !isWarning && !isUrlLog && <InfoIcon color="action" fontSize="small" />}
            </ListItemIcon>
            <ListItemText
              primary={
                <Box>
                  <Typography
                    variant="body2"
                    component="span"
                    sx={{
                      fontWeight: (isError || isWarning) ? 'bold' : 'normal',
                      fontFamily: 'monospace',
                      wordBreak: 'break-word'
                    }}
                  >
                    {log.message}
                  </Typography>
                  {url && (
                    <Chip
                      label={url.length > 40 ? url.substring(0, 37) + '...' : url}
                      size="small"
                      variant="outlined"
                      color="primary"
                      component="a"
                      href={url}
                      target="_blank"
                      clickable
                      sx={{ ml: 1, maxWidth: '200px' }}
                    />
                  )}
                </Box>
              }
              secondary={
                <Typography variant="caption" color="textSecondary" sx={{ fontFamily: 'monospace' }}>
                  {new Date(log.timestamp).toLocaleString()} - {log.logLevel}
                </Typography>
              }
            />
          </ListItem>
        );
      })}
      <div ref={endRef} />
    </List>
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