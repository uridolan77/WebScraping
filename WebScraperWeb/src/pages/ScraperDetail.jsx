import React, { useState, useCallback, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import {
  Container,
  Box,
  Button,
  Paper,
  Tabs,
  Tab,
  CircularProgress,
  Alert,
} from '@mui/material';
import { ArrowBack as ArrowBackIcon } from '@mui/icons-material';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getScraper,
  getScraperStatus,
  getScraperLogs,
  getScraperMonitor,
  startScraper,
  stopScraper,
  deleteScraper
} from '../api/scrapers';

// Import custom components
import TabPanel, { a11yProps } from '../components/scraper/TabPanel';
import ScraperHeader from '../components/scraper/ScraperHeader';
import OverviewTab from '../components/scraper/OverviewTab';
import ConfigurationTab from '../components/scraper/ConfigurationTab';
import LogsTab from '../components/scraper/LogsTab';
import ResultsTab from '../components/scraper/ResultsTab';
import MonitorTab from '../components/scraper/MonitorTab';
import DeleteConfirmationDialog from '../components/scraper/DeleteConfirmationDialog';
import EnhancedFeaturesManager from '../components/scraper/EnhancedFeaturesManager';

// Helper function to handle .NET-style response format with $values
const getArrayFromResponse = (data) => {
  if (!data) return [];
  if (Array.isArray(data)) return data;
  if (data.$values && Array.isArray(data.$values)) return data.$values;
  return [];
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

  // Fetch scraper monitor data with React Query
  const {
    data: monitorData,
    isLoading: isMonitorLoading,
    error: monitorError,
    refetch: refetchMonitor
  } = useQuery({
    queryKey: ['scraperMonitor', id],
    queryFn: () => getScraperMonitor(id),
    staleTime: 5000, // Short stale time for real-time monitoring
    refetchInterval: 5000, // Frequent polling for real-time updates
    refetchIntervalInBackground: false,
    retry: 0, // Don't retry on error - we want to show the error message
    enabled: !!id && status?.isRunning, // Only fetch when scraper is running
    onError: (error) => {
      console.error('Error fetching monitor data:', error);
      // We'll handle the error in the UI, no need to show a toast or alert here
      // as we're displaying the error directly in the Monitor tab
    }
  });

  // Extract logs from response
  const logs = logsData?.logs ? getArrayFromResponse(logsData.logs) : [];

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
    if (status?.isRunning) {
      refetchMonitor();
    }

    setAlert({
      show: true,
      message: 'Data refreshed successfully',
      severity: 'success'
    });
  }, [refetchScraper, refetchStatus, refetchLogs, refetchMonitor, status]);

  // Handle start scraper
  const handleStartScraper = useCallback(() => {
    startScraperMutation.mutate();
    // Switch to Monitor tab when starting the scraper
    setActiveTab(5); // Index 5 will be the Monitor tab
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

  // Check if scraper is not found
  if (scraper?.notFound) {
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
        <Alert severity="warning" sx={{ mb: 3 }}>
          Scraper not found: This scraper may have been deleted or never existed.
        </Alert>
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Box sx={{ mb: 3 }}>
            The scraper with ID {id} could not be found in the system.
          </Box>
          <Button
            variant="contained"
            component={Link}
            to="/scrapers"
          >
            View All Scrapers
          </Button>
        </Paper>
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
      <ScraperHeader
        scraper={scraper}
        status={status}
        isStatusLoading={isStatusLoading}
        isActionInProgress={isActionInProgress}
        onRefresh={handleRefresh}
        onStart={handleStartScraper}
        onStop={handleStopScraper}
        onDelete={() => setDeleteDialogOpen(true)}
      />

      {/* Tabs for different sections */}
      <Paper sx={{ mb: 3 }}>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={activeTab} onChange={handleTabChange} aria-label="scraper detail tabs">
            <Tab label="Overview" {...a11yProps(0)} />
            <Tab label="Configuration" {...a11yProps(1)} />
            <Tab label="Logs" {...a11yProps(2)} />
            <Tab label="Results" {...a11yProps(3)} />
            <Tab label="Enhanced Features" {...a11yProps(4)} />
            <Tab
              label="Monitor"
              {...a11yProps(5)}
              sx={{
                display: status?.isRunning ? 'flex' : 'none',
                color: status?.isRunning ? 'success.main' : 'inherit'
              }}
            />
          </Tabs>
        </Box>

        {/* Overview Tab */}
        <TabPanel value={activeTab} index={0}>
          <OverviewTab
            status={status}
            scraper={scraper}
            logs={logs}
            isStatusLoading={isStatusLoading}
            isLogsLoading={isLogsLoading}
          />
        </TabPanel>

        {/* Configuration Tab */}
        <TabPanel value={activeTab} index={1}>
          <ConfigurationTab scraper={scraper} />
        </TabPanel>

        {/* Logs Tab */}
        <TabPanel value={activeTab} index={2}>
          <LogsTab
            logs={logs}
            isLogsLoading={isLogsLoading}
            isActionInProgress={isActionInProgress}
            onRefreshLogs={refetchLogs}
          />
        </TabPanel>

        {/* Results Tab */}
        <TabPanel value={activeTab} index={3}>
          <ResultsTab />
        </TabPanel>

        {/* Enhanced Features Tab */}
        <TabPanel value={activeTab} index={4}>
          <EnhancedFeaturesManager scraper={scraper} />
        </TabPanel>

        {/* Monitor Tab */}
        <TabPanel value={activeTab} index={5}>
          <MonitorTab
            status={status}
            monitorData={monitorData}
            monitorError={monitorError}
            isMonitorLoading={isMonitorLoading}
            isActionInProgress={isActionInProgress}
            onRefreshMonitor={refetchMonitor}
          />
        </TabPanel>
      </Paper>

      {/* Delete Confirmation Dialog */}
      <DeleteConfirmationDialog
        open={deleteDialogOpen}
        onClose={() => setDeleteDialogOpen(false)}
        onConfirm={handleDeleteScraper}
        isDeleting={deleteScraperMutation.isPending}
      />
    </Container>
  );
};

export default ScraperDetail;
