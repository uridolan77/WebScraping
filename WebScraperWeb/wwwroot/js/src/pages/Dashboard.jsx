import React, { useState, useEffect } from 'react';
import { useParams, Link as RouterLink } from 'react-router-dom';
import {
  Typography,
  Grid,
  Box,
  Breadcrumbs,
  Link,
  Paper,
  Alert,
  CircularProgress
} from '@mui/material';
import HomeIcon from '@mui/icons-material/Home';
import DashboardIcon from '@mui/icons-material/Dashboard';

// Import our components
import LogViewer from '../components/LogViewer';
import ScraperStatusCard from '../components/ScraperStatusCard';
import ActionPanel from '../components/ActionPanel';

// Import API services
import { fetchScraper, fetchScraperStatus, fetchScraperLogs } from '../services/api';

const Dashboard = () => {
  const { id } = useParams();
  const [scraper, setScraper] = useState(null);
  const [status, setStatus] = useState({});
  const [logs, setLogs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [logsLoading, setLogsLoading] = useState(true);
  const [error, setError] = useState(null);

  // Load scraper data
  const loadScraper = async () => {
    setLoading(true);
    try {
      const scraperData = await fetchScraper(id);
      console.log('Dashboard received scraper data:', scraperData);

      // Create a mapping of property names to ensure consistent casing
      const propertyMap = {
        'Id': 'id',
        'Name': 'name',
        'Description': 'description',
        'StartUrl': 'startUrl',
        'BaseUrl': 'baseUrl',
        'MaxDepth': 'maxDepth',
        'MaxConcurrentRequests': 'maxConcurrentRequests',
        'DelayBetweenRequests': 'delayBetweenRequests',
        'RespectRobotsTxt': 'respectRobotsTxt',
        'FollowExternalLinks': 'followExternalLinks',
        'NotificationEmail': 'notificationEmail',
        'EnableContinuousMonitoring': 'enableContinuousMonitoring',
        'MonitoringIntervalMinutes': 'monitoringIntervalMinutes',
        'NotifyOnChanges': 'notifyOnChanges',
        'TrackChangesHistory': 'trackChangesHistory'
      };

      // Normalize the data using the property map
      const normalizedData = {};
      Object.keys(scraperData).forEach(key => {
        // Use the mapped property name if available, otherwise convert first character to lowercase
        const normalizedKey = propertyMap[key] || key.charAt(0).toLowerCase() + key.slice(1);
        normalizedData[normalizedKey] = scraperData[key];

        // Log any property name changes
        if (normalizedKey !== key) {
          console.log(`Property name mapped: ${key} -> ${normalizedKey}`);
        }
      });

      console.log('Dashboard normalized data:', normalizedData);
      setScraper(normalizedData);

      // Also fetch status
      await loadStatus();

      setError(null);
    } catch (err) {
      console.error('Error loading scraper in Dashboard:', err);
      setError(`Failed to load scraper: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };

  // Load scraper status
  const loadStatus = async () => {
    try {
      const statusData = await fetchScraperStatus(id);
      console.log('Status data received:', statusData);

      // Create a mapping of property names to ensure consistent casing
      const propertyMap = {
        'IsRunning': 'isRunning',
        'StartTime': 'startTime',
        'EndTime': 'endTime',
        'ElapsedTime': 'elapsedTime',
        'UrlsProcessed': 'urlsProcessed',
        'LastMonitorCheck': 'lastMonitorCheck',
        'IsMonitoring': 'isMonitoring',
        'MonitoringInterval': 'monitoringInterval',
        'ScraperId': 'scraperId',
        'ScraperName': 'scraperName'
      };

      // Normalize the data using the property map
      const normalizedStatus = {};
      Object.keys(statusData).forEach(key => {
        // Use the mapped property name if available, otherwise convert first character to lowercase
        const normalizedKey = propertyMap[key] || key.charAt(0).toLowerCase() + key.slice(1);
        normalizedStatus[normalizedKey] = statusData[key];

        // Log any property name changes
        if (normalizedKey !== key) {
          console.log(`Status property mapped: ${key} -> ${normalizedKey}`);
        }
      });

      console.log('Normalized status data:', normalizedStatus);
      setStatus(normalizedStatus);
    } catch (err) {
      console.error('Error fetching status:', err);
      // Don't set the main error here, as it's a secondary operation
    }
  };

  // Load scraper logs
  const loadLogs = async () => {
    setLogsLoading(true);
    try {
      const logsData = await fetchScraperLogs(id);
      console.log('Logs data received:', logsData);

      // Check if logs are in a nested property (common API pattern)
      const logsArray = logsData.logs || logsData.Logs || logsData;

      // Normalize log entries if needed
      const normalizedLogs = Array.isArray(logsArray) ? logsArray.map(log => {
        // If log entries have inconsistent casing, normalize them here
        if (log.Timestamp || log.TimeStamp) {
          return {
            timestamp: log.Timestamp || log.TimeStamp,
            message: log.Message || log.message,
            level: log.Level || log.level,
            ...log
          };
        }
        return log;
      }) : [];

      console.log('Normalized logs:', normalizedLogs);
      setLogs(normalizedLogs);
    } catch (err) {
      console.error('Error fetching logs:', err);
    } finally {
      setLogsLoading(false);
    }
  };

  // Initial data load
  useEffect(() => {
    loadScraper();
    loadLogs();

    // Set up polling for status and logs if scraper is running
    const intervalId = setInterval(() => {
      if (status.isRunning) {
        loadStatus();
        loadLogs();
      }
    }, 5000); // Poll every 5 seconds

    return () => clearInterval(intervalId);
  }, [id, status.isRunning]);

  // When status changes, refresh logs
  const handleStatusChange = () => {
    loadStatus();
    loadLogs();
  };

  // If loading or error
  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  // Check if scraper data exists
  if (!scraper && !loading) {
    return (
      <Alert severity="error" sx={{ mt: 2 }}>
        Scraper not found or has been deleted.
      </Alert>
    );
  }

  // Calculate monitoring settings
  const monitoringSettings = {
    enabled: scraper?.enableContinuousMonitoring || false,
    intervalMinutes: scraper?.monitoringIntervalMinutes || 1440,
    notifyOnChanges: scraper?.notifyOnChanges || false,
    notificationEmail: scraper?.notificationEmail || '',
    trackChangesHistory: scraper?.trackChangesHistory || true
  };

  return (
    <Box>
      {/* Breadcrumbs navigation */}
      <Breadcrumbs sx={{ mb: 3 }}>
        <Link
          component={RouterLink}
          to="/"
          color="inherit"
          sx={{ display: 'flex', alignItems: 'center' }}
        >
          <HomeIcon sx={{ mr: 0.5 }} fontSize="inherit" />
          Home
        </Link>
        <Link
          component={RouterLink}
          to="/scrapers"
          color="inherit"
          sx={{ display: 'flex', alignItems: 'center' }}
        >
          Scrapers
        </Link>
        <Typography
          color="text.primary"
          sx={{ display: 'flex', alignItems: 'center' }}
        >
          <DashboardIcon sx={{ mr: 0.5 }} fontSize="inherit" />
          {scraper?.name}
        </Typography>
      </Breadcrumbs>

      {/* Main error alert */}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Header section */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" gutterBottom>
          {scraper?.name}
        </Typography>
        <Typography variant="body1" color="text.secondary" gutterBottom>
          {scraper?.description || 'No description provided'}
        </Typography>
        <Typography variant="body2" sx={{ wordBreak: 'break-all' }}>
          Starting URL: {scraper?.startUrl}
        </Typography>
      </Box>

      {/* Main content grid */}
      <Grid container spacing={3}>
        {/* Left side - Status and Actions */}
        <Grid item xs={12} md={4}>
          <ScraperStatusCard
            isRunning={status.isRunning}
            urlsProcessed={status.urlsProcessed || scraper?.urlsProcessed || 0}
            startTime={status.startTime}
            endTime={status.endTime}
            elapsedTime={status.elapsedTime}
            resultsCount={status.resultsCount || 0}
            monitoringEnabled={scraper?.enableContinuousMonitoring}
          />

          <ActionPanel
            scraperId={id}
            isRunning={status.isRunning}
            onStatusChange={handleStatusChange}
            monitoringSettings={monitoringSettings}
          />

          {/* Scraper configuration info */}
          <Paper sx={{ p: 2, mt: 3 }}>
            <Typography variant="h6" gutterBottom>
              Configuration
            </Typography>
            <Box sx={{ mb: 1 }}>
              <Typography variant="body2" color="text.secondary">
                Max Depth:
              </Typography>
              <Typography variant="body1">
                {scraper?.maxDepth || 'Unlimited'}
              </Typography>
            </Box>
            <Box sx={{ mb: 1 }}>
              <Typography variant="body2" color="text.secondary">
                URL Pattern:
              </Typography>
              <Typography variant="body1" sx={{ wordBreak: 'break-all' }}>
                {scraper?.urlPattern || 'No pattern (all URLs)'}
              </Typography>
            </Box>
            <Box sx={{ mb: 1 }}>
              <Typography variant="body2" color="text.secondary">
                Content Selectors:
              </Typography>
              <Typography variant="body1">
                {scraper?.selectors?.length ? scraper.selectors.join(', ') : 'None'}
              </Typography>
            </Box>
            <Box>
              <Typography variant="body2" color="text.secondary">
                Rate Limiting:
              </Typography>
              <Typography variant="body1">
                {scraper?.requestDelay ? `${scraper.requestDelay}ms between requests` : 'No delay'}
              </Typography>
            </Box>
          </Paper>
        </Grid>

        {/* Right side - Logs */}
        <Grid item xs={12} md={8}>
          <LogViewer
            logs={logs}
            loading={logsLoading}
            onRefresh={loadLogs}
            title="Scraper Logs"
            maxHeight={600}
          />
        </Grid>
      </Grid>
    </Box>
  );
};

export default Dashboard;