import React, { useState, useEffect } from 'react';
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
import { useScrapers } from '../hooks';
import { formatDistanceToNow } from 'date-fns';

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

const ScraperDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { 
    getScrapers, 
    getScraper,
    startScraper, 
    stopScraper, 
    deleteScraper,
    getScraperStatus,
    getScraperLogs,
    loading, 
    error 
  } = useScrapers();
  
  const [scraper, setScraper] = useState(null);
  const [status, setStatus] = useState(null);
  const [logs, setLogs] = useState([]);
  const [activeTab, setActiveTab] = useState(0);
  const [actionInProgress, setActionInProgress] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [alert, setAlert] = useState({ show: false, message: '', severity: 'info' });

  // Handle tab change
  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
  };

  // Fetch scraper data and status
  useEffect(() => {
    const fetchScraperData = async () => {
      if (!id) return;
      
      try {
        const scraperData = await getScraper(id);
        setScraper(scraperData);
        
        // Fetch status
        const statusData = await getScraperStatus(id);
        setStatus(statusData);
        
        // Fetch logs
        const logsData = await getScraperLogs(id, 100);
        setLogs(logsData?.logs || []);
      } catch (err) {
        setAlert({
          show: true,
          message: `Error loading scraper details: ${err.message || 'Unknown error'}`,
          severity: 'error'
        });
        console.error('Error fetching scraper details:', err);
      }
    };
    
    fetchScraperData();
    
    // Set up polling for status updates
    const statusInterval = setInterval(() => {
      if (id) {
        getScraperStatus(id)
          .then(statusData => setStatus(statusData))
          .catch(err => console.error('Error fetching status:', err));
      }
    }, 5000);
    
    // Set up polling for logs when scraper is running
    const logsInterval = setInterval(() => {
      if (id && status?.isRunning) {
        getScraperLogs(id, 100)
          .then(logsData => setLogs(logsData?.logs || []))
          .catch(err => console.error('Error fetching logs:', err));
      }
    }, 10000);
    
    return () => {
      clearInterval(statusInterval);
      clearInterval(logsInterval);
    };
  }, [id, getScraper, getScraperStatus, getScraperLogs]);
  
  // Handle start scraper
  const handleStartScraper = async () => {
    try {
      setActionInProgress(true);
      await startScraper(id);
      
      const statusData = await getScraperStatus(id);
      setStatus(statusData);
      
      setAlert({
        show: true,
        message: 'Scraper started successfully',
        severity: 'success'
      });
    } catch (err) {
      setAlert({
        show: true,
        message: `Error starting scraper: ${err.message || 'Unknown error'}`,
        severity: 'error'
      });
      console.error('Error starting scraper:', err);
    } finally {
      setActionInProgress(false);
    }
  };
  
  // Handle stop scraper
  const handleStopScraper = async () => {
    try {
      setActionInProgress(true);
      await stopScraper(id);
      
      const statusData = await getScraperStatus(id);
      setStatus(statusData);
      
      setAlert({
        show: true,
        message: 'Scraper stopped successfully',
        severity: 'success'
      });
    } catch (err) {
      setAlert({
        show: true,
        message: `Error stopping scraper: ${err.message || 'Unknown error'}`,
        severity: 'error'
      });
      console.error('Error stopping scraper:', err);
    } finally {
      setActionInProgress(false);
    }
  };
  
  // Handle delete scraper
  const handleDeleteScraper = async () => {
    try {
      setActionInProgress(true);
      await deleteScraper(id);
      
      setAlert({
        show: true,
        message: 'Scraper deleted successfully',
        severity: 'success'
      });
      
      // Navigate back to scrapers list after a short delay
      setTimeout(() => {
        navigate('/scrapers');
      }, 1500);
    } catch (err) {
      setAlert({
        show: true,
        message: `Error deleting scraper: ${err.message || 'Unknown error'}`,
        severity: 'error'
      });
      console.error('Error deleting scraper:', err);
      setActionInProgress(false);
    }
  };
  
  // Handle refresh
  const handleRefresh = async () => {
    try {
      const scraperData = await getScraper(id);
      setScraper(scraperData);
      
      const statusData = await getScraperStatus(id);
      setStatus(statusData);
      
      const logsData = await getScraperLogs(id, 100);
      setLogs(logsData?.logs || []);
      
      setAlert({
        show: true,
        message: 'Data refreshed successfully',
        severity: 'success'
      });
    } catch (err) {
      setAlert({
        show: true,
        message: `Error refreshing data: ${err.message || 'Unknown error'}`,
        severity: 'error'
      });
      console.error('Error refreshing data:', err);
    }
  };
  
  // Format date
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
  
  // Calculate time ago
  const timeAgo = (dateString) => {
    if (!dateString) return 'Never';
    return formatDistanceToNow(new Date(dateString), { addSuffix: true });
  };

  if (loading && !scraper) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
          <CircularProgress />
        </Box>
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
            {status && (
              <Box sx={{ mt: 1 }}>
                <Chip 
                  label={status.isRunning ? 'Running' : (status.hasErrors ? 'Error' : 'Idle')} 
                  color={status.isRunning ? 'success' : (status.hasErrors ? 'error' : 'default')}
                  sx={{ mr: 1 }}
                />
                {status.lastRun && (
                  <Chip 
                    label={`Last run: ${timeAgo(status.lastRun)}`} 
                    variant="outlined" 
                    size="small"
                    sx={{ mr: 1 }}
                  />
                )}
                {status.urlsProcessed > 0 && (
                  <Chip 
                    label={`URLs processed: ${status.urlsProcessed}`} 
                    variant="outlined" 
                    size="small"
                  />
                )}
              </Box>
            )}
          </Grid>
          <Grid item xs={12} md={4}>
            <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
              <Tooltip title="Refresh">
                <IconButton
                  onClick={handleRefresh}
                  disabled={actionInProgress}
                >
                  <RefreshIcon />
                </IconButton>
              </Tooltip>
              
              {status?.isRunning ? (
                <Tooltip title="Stop Scraper">
                  <Button
                    variant="contained"
                    color="warning"
                    startIcon={<StopIcon />}
                    onClick={handleStopScraper}
                    disabled={actionInProgress}
                  >
                    Stop
                  </Button>
                </Tooltip>
              ) : (
                <Tooltip title="Start Scraper">
                  <Button
                    variant="contained"
                    color="success"
                    startIcon={<PlayIcon />}
                    onClick={handleStartScraper}
                    disabled={actionInProgress}
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
                  disabled={actionInProgress}
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
                  disabled={actionInProgress || status?.isRunning}
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
                  {logs && logs.length > 0 ? (
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
              onClick={handleRefresh}
              disabled={actionInProgress}
            >
              Refresh Logs
            </Button>
          </Box>
          
          <Card>
            <CardContent>
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
        onClick={() => !actionInProgress && setDeleteDialogOpen(false)}
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
              disabled={actionInProgress}
            >
              Cancel
            </Button>
            
            <Button
              variant="contained"
              color="error"
              onClick={handleDeleteScraper}
              disabled={actionInProgress}
            >
              {actionInProgress ? (
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