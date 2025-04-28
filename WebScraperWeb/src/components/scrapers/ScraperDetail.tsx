import React, { useState, useEffect } from 'react';
import { 
  Container, Box, Typography, Paper, Tabs, Tab, Button, 
  Chip, Divider, Grid, Card, CardContent, CardHeader, 
  List, ListItem, ListItemText, ListItemIcon, IconButton,
  Tooltip, CircularProgress, Alert, LinearProgress
} from '@mui/material';
import { 
  PlayArrow as PlayIcon, 
  Stop as StopIcon, 
  Refresh as RefreshIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Schedule as ScheduleIcon,
  Notifications as NotificationsIcon,
  Settings as SettingsIcon,
  ContentCopy as ContentCopyIcon,
  Download as DownloadIcon,
  History as HistoryIcon,
  Link as LinkIcon,
  Error as ErrorIcon,
  CheckCircle as CheckCircleIcon,
  Warning as WarningIcon
} from '@mui/icons-material';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import { useScrapers } from '../../contexts/ScraperContext';
import { useAnalytics } from '../../hooks/useAnalytics';
import { formatDistanceToNow } from 'date-fns';
import ScraperLogs from './ScraperLogs';
import ScraperResults from './ScraperResults';
import ScraperSettings from './ScraperSettings';
import ScraperStatistics from './ScraperStatistics';
import { getUserFriendlyErrorMessage } from '../../utils/errorHandler';

// TabPanel component for tab content
function TabPanel(props) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`scraper-tabpanel-${index}`}
      aria-labelledby={`scraper-tab-${index}`}
      {...other}
      style={{ padding: '24px 0' }}
    >
      {value === index && children}
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

const ScraperDetail = ({ scraperId }) => {
  const navigate = useNavigate();
  const { 
    selectedScraper, 
    loading, 
    error, 
    status, 
    logs,
    fetchScraper, 
    fetchScraperStatus, 
    fetchScraperLogs,
    start, 
    stop
  } = useScrapers();
  
  const { 
    scraperData: analyticsData, 
    loading: analyticsLoading,
    fetchScraperAnalytics
  } = useAnalytics(scraperId);

  const [tabValue, setTabValue] = useState(0);
  const [isActionInProgress, setIsActionInProgress] = useState(false);
  const [notification, setNotification] = useState({ open: false, message: '', severity: 'info' });

  // Fetch scraper data on component mount
  useEffect(() => {
    const loadData = async () => {
      await fetchScraper(scraperId);
      await fetchScraperStatus(scraperId);
      await fetchScraperLogs(scraperId, 100);
      await fetchScraperAnalytics(scraperId);
    };
    
    loadData();
    
    // Set up polling for status updates
    const statusInterval = setInterval(() => {
      fetchScraperStatus(scraperId);
    }, 5000);
    
    // Set up polling for logs when scraper is running
    const logsInterval = setInterval(() => {
      if (status?.isRunning) {
        fetchScraperLogs(scraperId, 100);
      }
    }, 10000);
    
    return () => {
      clearInterval(statusInterval);
      clearInterval(logsInterval);
    };
  }, [scraperId, fetchScraper, fetchScraperStatus, fetchScraperLogs, fetchScraperAnalytics, status?.isRunning]);

  // Handle tab change
  const handleTabChange = (event, newValue) => {
    setTabValue(newValue);
  };

  // Show notification
  const showNotification = (message, severity = 'info') => {
    setNotification({
      open: true,
      message,
      severity
    });
  };

  // Handle notification close
  const handleNotificationClose = () => {
    setNotification(prev => ({ ...prev, open: false }));
  };

  // Handle start scraper
  const handleStartScraper = async () => {
    try {
      setIsActionInProgress(true);
      await start(scraperId);
      showNotification('Scraper started successfully', 'success');
    } catch (err) {
      showNotification(getUserFriendlyErrorMessage(err, 'Failed to start scraper'), 'error');
    } finally {
      setIsActionInProgress(false);
    }
  };

  // Handle stop scraper
  const handleStopScraper = async () => {
    try {
      setIsActionInProgress(true);
      await stop(scraperId);
      showNotification('Scraper stopped successfully', 'success');
    } catch (err) {
      showNotification(getUserFriendlyErrorMessage(err, 'Failed to stop scraper'), 'error');
    } finally {
      setIsActionInProgress(false);
    }
  };

  // Render status chip
  const renderStatusChip = () => {
    if (!status) {
      return <Chip label="Unknown" color="default" />;
    }

    if (status.isRunning) {
      return (
        <Chip 
          label="Running" 
          color="success" 
          icon={<PlayIcon />}
        />
      );
    } else if (status.hasErrors) {
      return (
        <Tooltip title={status.errorMessage || 'An error occurred'}>
          <Chip 
            label="Error" 
            color="error" 
            icon={<ErrorIcon />}
          />
        </Tooltip>
      );
    } else {
      return <Chip label="Idle" color="default" />;
    }
  };

  // If loading, show loading indicator
  if (loading && !selectedScraper) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh', flexDirection: 'column' }}>
          <CircularProgress size={60} sx={{ mb: 3 }} />
          <Typography variant="h6">Loading scraper details...</Typography>
        </Box>
      </Container>
    );
  }

  // If error, show error message
  if (error && !selectedScraper) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Alert 
          severity="error" 
          sx={{ mb: 3 }}
          action={
            <Button color="inherit" size="small" onClick={() => navigate('/scrapers')}>
              Back to Scrapers
            </Button>
          }
        >
          {getUserFriendlyErrorMessage(error, 'Failed to load scraper details')}
        </Alert>
      </Container>
    );
  }

  // If scraper not found, show not found message
  if (!selectedScraper) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Alert 
          severity="warning" 
          sx={{ mb: 3 }}
          action={
            <Button color="inherit" size="small" onClick={() => navigate('/scrapers')}>
              Back to Scrapers
            </Button>
          }
        >
          Scraper not found. It may have been deleted or you don't have access to it.
        </Alert>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      {/* Header */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
          <Box>
            <Typography variant="h4" gutterBottom>{selectedScraper.name}</Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
              <Typography variant="body2" color="text.secondary" sx={{ mr: 1 }}>
                ID: {selectedScraper.id}
              </Typography>
              <Tooltip title="Copy ID">
                <IconButton 
                  size="small"
                  onClick={() => {
                    navigator.clipboard.writeText(selectedScraper.id);
                    showNotification('ID copied to clipboard', 'success');
                  }}
                >
                  <ContentCopyIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
              <Tooltip title="Visit website">
                <Button 
                  size="small" 
                  startIcon={<LinkIcon />}
                  component="a"
                  href={selectedScraper.baseUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  {selectedScraper.baseUrl}
                </Button>
              </Tooltip>
              {renderStatusChip()}
              {status?.isRunning && (
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  <LinearProgress 
                    sx={{ width: 100, mr: 1 }} 
                    variant={status.progress ? "determinate" : "indeterminate"}
                    value={status.progress || 0}
                  />
                  {status.progress ? `${Math.round(status.progress)}%` : 'Processing...'}
                </Box>
              )}
            </Box>
          </Box>
          <Box sx={{ display: 'flex', gap: 1 }}>
            {status?.isRunning ? (
              <Button
                variant="contained"
                color="warning"
                startIcon={isActionInProgress ? <CircularProgress size={24} color="inherit" /> : <StopIcon />}
                onClick={handleStopScraper}
                disabled={isActionInProgress}
              >
                Stop
              </Button>
            ) : (
              <Button
                variant="contained"
                color="success"
                startIcon={isActionInProgress ? <CircularProgress size={24} color="inherit" /> : <PlayIcon />}
                onClick={handleStartScraper}
                disabled={isActionInProgress}
              >
                Start
              </Button>
            )}
            <Button
              variant="outlined"
              startIcon={<EditIcon />}
              component={RouterLink}
              to={`/scrapers/${scraperId}/edit`}
            >
              Edit
            </Button>
            <Button
              variant="outlined"
              color="primary"
              startIcon={<ScheduleIcon />}
              component={RouterLink}
              to={`/scrapers/${scraperId}/schedule`}
            >
              Schedule
            </Button>
          </Box>
        </Box>
        
        <Divider sx={{ my: 2 }} />
        
        <Grid container spacing={3}>
          <Grid item xs={12} md={3}>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">Last Run</Typography>
              <Typography variant="body1">
                {selectedScraper.lastRun 
                  ? formatDistanceToNow(new Date(selectedScraper.lastRun), { addSuffix: true })
                  : 'Never'}
              </Typography>
            </Box>
          </Grid>
          <Grid item xs={12} md={3}>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">URLs Processed</Typography>
              <Typography variant="body1">
                {status?.urlsProcessed || selectedScraper.urlsProcessed || 0}
              </Typography>
            </Box>
          </Grid>
          <Grid item xs={12} md={3}>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">Content Changes</Typography>
              <Typography variant="body1">
                {analyticsData?.changesDetected || 0}
              </Typography>
            </Box>
          </Grid>
          <Grid item xs={12} md={3}>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">Created</Typography>
              <Typography variant="body1">
                {selectedScraper.createdAt 
                  ? formatDistanceToNow(new Date(selectedScraper.createdAt), { addSuffix: true })
                  : 'Unknown'}
              </Typography>
            </Box>
          </Grid>
        </Grid>
      </Paper>
      
      {/* Tabs */}
      <Paper sx={{ mb: 3 }}>
        <Tabs 
          value={tabValue} 
          onChange={handleTabChange} 
          aria-label="scraper tabs"
          variant="scrollable"
          scrollButtons="auto"
          sx={{ borderBottom: 1, borderColor: 'divider' }}
        >
          <Tab label="Overview" {...a11yProps(0)} />
          <Tab label="Results" {...a11yProps(1)} />
          <Tab label="Logs" {...a11yProps(2)} />
          <Tab label="Settings" {...a11yProps(3)} />
        </Tabs>
        
        <Box sx={{ p: 3 }}>
          <TabPanel value={tabValue} index={0}>
            <ScraperStatistics 
              scraper={selectedScraper} 
              status={status} 
              analyticsData={analyticsData}
              isLoading={analyticsLoading}
            />
          </TabPanel>
          
          <TabPanel value={tabValue} index={1}>
            <ScraperResults scraperId={scraperId} />
          </TabPanel>
          
          <TabPanel value={tabValue} index={2}>
            <ScraperLogs logs={logs} isRunning={status?.isRunning} />
          </TabPanel>
          
          <TabPanel value={tabValue} index={3}>
            <ScraperSettings scraper={selectedScraper} />
          </TabPanel>
        </Box>
      </Paper>
    </Container>
  );
};

export default ScraperDetail;
