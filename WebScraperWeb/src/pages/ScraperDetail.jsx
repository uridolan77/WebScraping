import React, { useState, useCallback, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import {
  Container,
  Typography,
  Box,
  Button,
  Paper,
  Grid,
  Tabs,
  Tab,
  Divider,
  Chip,
  CircularProgress,
  Alert,
  Card,
  CardContent,
  IconButton,
  Tooltip,
} from '@mui/material';
import {
  PlayArrow as PlayIcon,
  Stop as StopIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  ArrowBack as ArrowBackIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import { formatDistanceToNow } from 'date-fns';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getScraper,
  getScraperStatus,
  getScraperLogs,
  startScraper,
  stopScraper,
  deleteScraper
} from '../api/scrapers';

// TabPanel component for tab content
function TabPanel(props) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`scraper-tab-${index}`}
      aria-labelledby={`scraper-tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ p: 3 }}>
          {children}
        </Box>
      )}
    </div>
  );
}

// Helper function for tab accessibility
function a11yProps(index) {
  return {
    id: `scraper-tab-${index}`,
    'aria-controls': `scraper-tabpanel-${index}`,
  };
}

// Format date helper function
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

// Calculate time ago helper function
const timeAgo = (dateString) => {
  if (!dateString) return 'Never';
  return formatDistanceToNow(new Date(dateString), { addSuffix: true });
};

// Status chip component
const StatusChip = ({ status }) => {
  if (!status) return <Chip label="Unknown" color="default" />;

  if (status.isRunning) {
    return <Chip label="Running" color="success" />;
  } else if (status.hasErrors) {
    return <Chip label="Error" color="error" />;
  } else {
    return <Chip label="Idle" color="default" />;
  }
};

const ScraperDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  // Local state
  const [activeTab, setActiveTab] = useState(0);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [alert, setAlert] = useState({ show: false, message: '', severity: 'info' });

  // Fetch scraper data with React Query
  const {
    data: scraper,
    isLoading: isScraperLoading,
    error: scraperError,
    refetch: refetchScraper
  } = useQuery({
    queryKey: ['scraper', id],
    queryFn: () => getScraper(id),
    staleTime: 60 * 1000, // 1 minute
    refetchOnWindowFocus: false,
    retry: 1,
    enabled: !!id
  });

  // Fetch scraper status with React Query
  const {
    data: status,
    isLoading: isStatusLoading,
    error: statusError,
    refetch: refetchStatus
  } = useQuery({
    queryKey: ['scraperStatus', id],
    queryFn: () => getScraperStatus(id),
    staleTime: 10 * 1000, // 10 seconds
    refetchInterval: 30000, // Default polling interval
    refetchIntervalInBackground: false,
    retry: 1,
    enabled: !!id
  });

  // Fetch scraper logs with React Query
  const {
    data: logsData,
    isLoading: isLogsLoading,
    error: logsError,
    refetch: refetchLogs
  } = useQuery({
    queryKey: ['scraperLogs', id],
    queryFn: () => getScraperLogs(id, 100),
    staleTime: 60000, // Default stale time
    refetchInterval: 30000, // Default polling interval
    refetchIntervalInBackground: false,
    retry: 1,
    enabled: !!id
  });

  // Extract logs from response
  const logs = logsData?.logs || [];

  // Update refetch intervals based on status
  useEffect(() => {
    if (status) {
      // If scraper is running, update the refetch intervals
      if (status.isRunning) {
        // Set shorter intervals for status and logs when scraper is running
        queryClient.setQueryDefaults(['scraperStatus', id], {
          refetchInterval: 5000
        });

        queryClient.setQueryDefaults(['scraperLogs', id], {
          staleTime: 5000,
          refetchInterval: 10000
        });
      } else {
        // Set longer intervals when scraper is idle
        queryClient.setQueryDefaults(['scraperStatus', id], {
          refetchInterval: 30000
        });

        queryClient.setQueryDefaults(['scraperLogs', id], {
          staleTime: 60000,
          refetchInterval: 30000
        });
      }
    }
  }, [status, id, queryClient]);

  // Start scraper mutation
  const startScraperMutation = useMutation({
    mutationFn: () => startScraper(id),
    onSuccess: () => {
      // Invalidate status query to trigger refetch
      queryClient.invalidateQueries({ queryKey: ['scraperStatus', id] });

      setAlert({
        show: true,
        message: 'Scraper started successfully',
        severity: 'success'
      });
    },
    onError: (error) => {
      setAlert({
        show: true,
        message: `Error starting scraper: ${error.message || 'Unknown error'}`,
        severity: 'error'
      });
    }
  });

  // Stop scraper mutation
  const stopScraperMutation = useMutation({
    mutationFn: () => stopScraper(id),
    onSuccess: () => {
      // Invalidate status query to trigger refetch
      queryClient.invalidateQueries({ queryKey: ['scraperStatus', id] });

      setAlert({
        show: true,
        message: 'Scraper stopped successfully',
        severity: 'success'
      });
    },
    onError: (error) => {
      setAlert({
        show: true,
        message: `Error stopping scraper: ${error.message || 'Unknown error'}`,
        severity: 'error'
      });
    }
  });

  // Delete scraper mutation
  const deleteScraperMutation = useMutation({
    mutationFn: () => deleteScraper(id),
    onSuccess: () => {
      // Invalidate scrapers list query
      queryClient.invalidateQueries({ queryKey: ['scrapers'] });

      setAlert({
        show: true,
        message: 'Scraper deleted successfully',
        severity: 'success'
      });

      // Navigate back to scrapers list after a short delay
      setTimeout(() => {
        navigate('/scrapers');
      }, 1500);
    },
    onError: (error) => {
      setAlert({
        show: true,
        message: `Error deleting scraper: ${error.message || 'Unknown error'}`,
        severity: 'error'
      });
      setDeleteDialogOpen(false);
    }
  });

  // Check if any action is in progress
  const isActionInProgress =
    startScraperMutation.isPending ||
    stopScraperMutation.isPending ||
    deleteScraperMutation.isPending;

  // Handle tab change
  const handleTabChange = (_, newValue) => {
    setActiveTab(newValue);
  };

  // Handle refresh
  const handleRefresh = useCallback(() => {
    refetchScraper();
    refetchStatus();
    refetchLogs();

    setAlert({
      show: true,
      message: 'Data refreshed successfully',
      severity: 'success'
    });
  }, [refetchScraper, refetchStatus, refetchLogs]);

  // Handle start scraper
  const handleStartScraper = useCallback(() => {
    startScraperMutation.mutate();
  }, [startScraperMutation]);

  // Handle stop scraper
  const handleStopScraper = useCallback(() => {
    stopScraperMutation.mutate();
  }, [stopScraperMutation]);

  // Handle delete scraper
  const handleDeleteScraper = useCallback(() => {
    deleteScraperMutation.mutate();
  }, [deleteScraperMutation]);

  // Show loading state if scraper data is loading
  if (isScraperLoading && !scraper) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  // Show error state if there was an error fetching scraper data
  if (scraperError && !scraper) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Box sx={{ mb: 3 }}>
          <Button
            component={Link}
            to="/scrapers"
            startIcon={<ArrowBackIcon />}
            variant="outlined"
          >
            Back to Scrapers
          </Button>
        </Box>
        <Alert severity="error" sx={{ mb: 3 }}>
          Error loading scraper details: {scraperError.message || 'Unknown error'}
        </Alert>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      {/* Back button */}
      <Box sx={{ mb: 3 }}>
        <Button
          component={Link}
          to="/scrapers"
          startIcon={<ArrowBackIcon />}
          variant="outlined"
          sx={{ mb: 2 }}
        >
          Back to Scrapers
        </Button>
      </Box>

      {/* Alert for messages */}
      {alert.show && (
        <Alert
          severity={alert.severity}
          sx={{ mb: 3 }}
          onClose={() => setAlert({ ...alert, show: false })}
        >
          {alert.message}
        </Alert>
      )}

      {/* Scraper header */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Grid container spacing={2} alignItems="center">
          <Grid item xs={12} md={8}>
            <Typography variant="h4" gutterBottom>
              {scraper?.name || 'Loading...'}
            </Typography>
            <Typography variant="subtitle1" color="textSecondary" gutterBottom>
              {scraper?.baseUrl || ''}
            </Typography>
            <Box sx={{ mt: 1, display: 'flex', gap: 1, flexWrap: 'wrap' }}>
              {isStatusLoading ? (
                <CircularProgress size={24} />
              ) : (
                <>
                  <StatusChip status={status} />
                  {status?.lastRun && (
                    <Chip
                      label={`Last run: ${timeAgo(status.lastRun)}`}
                      variant="outlined"
                      size="small"
                    />
                  )}
                  {status?.urlsProcessed > 0 && (
                    <Chip
                      label={`URLs processed: ${status.urlsProcessed}`}
                      variant="outlined"
                      size="small"
                    />
                  )}
                </>
              )}
            </Box>
          </Grid>
          <Grid item xs={12} md={4}>
            <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1, flexWrap: 'wrap' }}>
              <Tooltip title="Refresh">
                <IconButton
                  onClick={handleRefresh}
                  disabled={isActionInProgress}
                >
                  <RefreshIcon />
                </IconButton>
              </Tooltip>

              {status?.isRunning ? (
                <Tooltip title="Stop Scraper">
                  <Button
                    variant="contained"
                    color="warning"
                    startIcon={stopScraperMutation.isPending ? <CircularProgress size={20} color="inherit" /> : <StopIcon />}
                    onClick={handleStopScraper}
                    disabled={isActionInProgress || isStatusLoading}
                  >
                    Stop
                  </Button>
                </Tooltip>
              ) : (
                <Tooltip title="Start Scraper">
                  <Button
                    variant="contained"
                    color="success"
                    startIcon={startScraperMutation.isPending ? <CircularProgress size={20} color="inherit" /> : <PlayIcon />}
                    onClick={handleStartScraper}
                    disabled={isActionInProgress || isStatusLoading}
                  >
                    Start
                  </Button>
                </Tooltip>
              )}

              <Tooltip title="Edit Scraper">
                <Button
                  variant="outlined"
                  color="primary"
                  startIcon={<EditIcon />}
                  component={Link}
                  to={`/scrapers/${id}/edit`}
                  disabled={isActionInProgress}
                >
                  Edit
                </Button>
              </Tooltip>

              <Tooltip title="Delete Scraper">
                <Button
                  variant="outlined"
                  color="error"
                  startIcon={<DeleteIcon />}
                  onClick={() => setDeleteDialogOpen(true)}
                  disabled={isActionInProgress || status?.isRunning}
                >
                  Delete
                </Button>
              </Tooltip>
            </Box>
          </Grid>
        </Grid>
      </Paper>

      {/* Tabs for different sections */}
      <Paper sx={{ mb: 3 }}>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={activeTab} onChange={handleTabChange} aria-label="scraper detail tabs">
            <Tab label="Overview" {...a11yProps(0)} />
            <Tab label="Configuration" {...a11yProps(1)} />
            <Tab label="Logs" {...a11yProps(2)} />
            <Tab label="Results" {...a11yProps(3)} />
          </Tabs>
        </Box>

        {/* Overview Tab */}
        <TabPanel value={activeTab} index={0}>
          <Grid container spacing={3}>
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
                        <Typography variant="body1">
                          {status?.isRunning ? 'Running' : (status?.hasErrors ? 'Error' : 'Idle')}
                        </Typography>
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
          </Grid>
        </TabPanel>

        {/* Configuration Tab */}
        <TabPanel value={activeTab} index={1}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Scraper Configuration
              </Typography>
              <Divider sx={{ mb: 2 }} />

              <Grid container spacing={2}>
                <Grid item xs={12} md={6}>
                  <Typography variant="subtitle2" color="textSecondary">
                    Name
                  </Typography>
                  <Typography variant="body1" sx={{ mb: 2 }}>
                    {scraper?.name || 'N/A'}
                  </Typography>

                  <Typography variant="subtitle2" color="textSecondary">
                    Base URL
                  </Typography>
                  <Typography variant="body1" sx={{ mb: 2 }}>
                    {scraper?.baseUrl || 'N/A'}
                  </Typography>

                  <Typography variant="subtitle2" color="textSecondary">
                    Start URL
                  </Typography>
                  <Typography variant="body1" sx={{ mb: 2 }}>
                    {scraper?.startUrl || 'N/A'}
                  </Typography>

                  <Typography variant="subtitle2" color="textSecondary">
                    Output Directory
                  </Typography>
                  <Typography variant="body1" sx={{ mb: 2 }}>
                    {scraper?.outputDirectory || 'Default'}
                  </Typography>
                </Grid>

                <Grid item xs={12} md={6}>
                  <Typography variant="subtitle2" color="textSecondary">
                    Max Depth
                  </Typography>
                  <Typography variant="body1" sx={{ mb: 2 }}>
                    {scraper?.maxDepth || 'N/A'}
                  </Typography>

                  <Typography variant="subtitle2" color="textSecondary">
                    Max Pages
                  </Typography>
                  <Typography variant="body1" sx={{ mb: 2 }}>
                    {scraper?.maxPages || 'N/A'}
                  </Typography>

                  <Typography variant="subtitle2" color="textSecondary">
                    Max Concurrent Requests
                  </Typography>
                  <Typography variant="body1" sx={{ mb: 2 }}>
                    {scraper?.maxConcurrentRequests || 'N/A'}
                  </Typography>

                  <Typography variant="subtitle2" color="textSecondary">
                    Delay Between Requests (ms)
                  </Typography>
                  <Typography variant="body1" sx={{ mb: 2 }}>
                    {scraper?.delayBetweenRequests || 'N/A'}
                  </Typography>
                </Grid>

                <Grid item xs={12}>
                  <Divider sx={{ my: 2 }} />
                  <Typography variant="h6" gutterBottom>
                    Features
                  </Typography>

                  <Grid container spacing={2}>
                    <Grid item xs={6} sm={4}>
                      <Typography variant="body2">
                        Follow Links:
                      </Typography>
                    </Grid>
                    <Grid item xs={6} sm={2}>
                      <Chip
                        label={scraper?.followLinks ? 'Enabled' : 'Disabled'}
                        color={scraper?.followLinks ? 'success' : 'default'}
                        size="small"
                      />
                    </Grid>

                    <Grid item xs={6} sm={4}>
                      <Typography variant="body2">
                        Follow External Links:
                      </Typography>
                    </Grid>
                    <Grid item xs={6} sm={2}>
                      <Chip
                        label={scraper?.followExternalLinks ? 'Enabled' : 'Disabled'}
                        color={scraper?.followExternalLinks ? 'success' : 'default'}
                        size="small"
                      />
                    </Grid>

                    <Grid item xs={6} sm={4}>
                      <Typography variant="body2">
                        Respect Robots.txt:
                      </Typography>
                    </Grid>
                    <Grid item xs={6} sm={2}>
                      <Chip
                        label={scraper?.respectRobotsTxt ? 'Enabled' : 'Disabled'}
                        color={scraper?.respectRobotsTxt ? 'success' : 'default'}
                        size="small"
                      />
                    </Grid>

                    <Grid item xs={6} sm={4}>
                      <Typography variant="body2">
                        Change Detection:
                      </Typography>
                    </Grid>
                    <Grid item xs={6} sm={2}>
                      <Chip
                        label={scraper?.enableChangeDetection ? 'Enabled' : 'Disabled'}
                        color={scraper?.enableChangeDetection ? 'success' : 'default'}
                        size="small"
                      />
                    </Grid>

                    <Grid item xs={6} sm={4}>
                      <Typography variant="body2">
                        Track Content Versions:
                      </Typography>
                    </Grid>
                    <Grid item xs={6} sm={2}>
                      <Chip
                        label={scraper?.trackContentVersions ? 'Enabled' : 'Disabled'}
                        color={scraper?.trackContentVersions ? 'success' : 'default'}
                        size="small"
                      />
                    </Grid>

                    <Grid item xs={6} sm={4}>
                      <Typography variant="body2">
                        Adaptive Crawling:
                      </Typography>
                    </Grid>
                    <Grid item xs={6} sm={2}>
                      <Chip
                        label={scraper?.enableAdaptiveCrawling ? 'Enabled' : 'Disabled'}
                        color={scraper?.enableAdaptiveCrawling ? 'success' : 'default'}
                        size="small"
                      />
                    </Grid>
                  </Grid>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        </TabPanel>

        {/* Logs Tab */}
        <TabPanel value={activeTab} index={2}>
          <Box sx={{ mb: 2, display: 'flex', justifyContent: 'flex-end' }}>
            <Button
              variant="outlined"
              startIcon={<RefreshIcon />}
              onClick={() => refetchLogs()}
              disabled={isActionInProgress || isLogsLoading}
            >
              Refresh Logs
            </Button>
          </Box>

          <Card>
            <CardContent>
              {isLogsLoading ? (
                <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
                  <CircularProgress />
                </Box>
              ) : (
                <Box sx={{ maxHeight: 500, overflow: 'auto' }}>
                  {logs && logs.length > 0 ? (
                    logs.map((log, index) => (
                      <Box
                        key={index}
                        sx={{
                          mb: 1,
                          p: 1,
                          bgcolor: 'background.default',
                          borderRadius: 1,
                          borderLeft: 4,
                          borderColor:
                            log.level === 'ERROR' ? 'error.main' :
                            log.level === 'WARNING' ? 'warning.main' :
                            log.level === 'INFO' ? 'info.main' :
                            'grey.400'
                        }}
                      >
                        <Typography variant="body2" color="textSecondary">
                          {new Date(log.timestamp).toLocaleString()} - {log.level || 'INFO'}
                        </Typography>
                        <Typography variant="body2">
                          {log.message}
                        </Typography>
                        {log.source && (
                          <Typography variant="caption" color="textSecondary">
                            Source: {log.source}
                          </Typography>
                        )}
                      </Box>
                    ))
                  ) : (
                    <Typography variant="body1" sx={{ p: 2, textAlign: 'center' }}>
                      No logs available
                    </Typography>
                  )}
                </Box>
              )}
            </CardContent>
          </Card>
        </TabPanel>

        {/* Results Tab */}
        <TabPanel value={activeTab} index={3}>
          <Typography variant="h6" gutterBottom>
            Scraped Results
          </Typography>

          <Alert severity="info" sx={{ mb: 3 }}>
            This section will display the most recent results from this scraper. You can view more detailed results and analysis in the Analytics section.
          </Alert>

          <Card>
            <CardContent>
              <Typography variant="body1" sx={{ p: 2, textAlign: 'center' }}>
                No results available yet
              </Typography>
            </CardContent>
          </Card>
        </TabPanel>
      </Paper>

      {/* Delete Confirmation Dialog */}
      <Box
        component="div"
        sx={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          backgroundColor: 'rgba(0, 0, 0, 0.5)',
          display: deleteDialogOpen ? 'flex' : 'none',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 9999,
        }}
        onClick={() => !isActionInProgress && setDeleteDialogOpen(false)}
      >
        <Paper
          sx={{
            p: 3,
            width: '100%',
            maxWidth: 500,
            mx: 2,
          }}
          onClick={(e) => e.stopPropagation()}
        >
          <Typography variant="h6" gutterBottom>
            Delete Scraper
          </Typography>

          <Typography variant="body1" sx={{ mb: 3 }}>
            Are you sure you want to delete this scraper? This action cannot be undone.
          </Typography>

          <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
            <Button
              variant="outlined"
              onClick={() => setDeleteDialogOpen(false)}
              disabled={isActionInProgress}
            >
              Cancel
            </Button>

            <Button
              variant="contained"
              color="error"
              onClick={handleDeleteScraper}
              disabled={isActionInProgress}
            >
              {deleteScraperMutation.isPending ? (
                <CircularProgress size={24} color="inherit" />
              ) : (
                'Delete'
              )}
            </Button>
          </Box>
        </Paper>
      </Box>
    </Container>
  );
};

export default ScraperDetail;
