// src/pages/ScraperDetail.jsx
import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import {
  Container, Typography, Box, Button, Tabs, Tab, Paper, Divider,
  Grid, Card, CardContent, Chip, IconButton, Tooltip, CircularProgress,
  Dialog, DialogTitle, DialogContent, DialogContentText, DialogActions,
  List, ListItem, ListItemText, Alert, Snackbar
} from '@mui/material';
import {
  PlayArrow as PlayIcon,
  Stop as StopIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Refresh as RefreshIcon,
  BarChart as BarChartIcon,
  Schedule as ScheduleIcon,
  Settings as SettingsIcon,
  TimerOff as TimerOffIcon,
  Timer as TimerIcon,
  ErrorOutline as ErrorIcon,
  Storage as StorageIcon
} from '@mui/icons-material';
import { format } from 'date-fns';
import {
  getScraper,
  getScraperStatus,
  getScraperLogs,
  startScraper,
  stopScraper,
  deleteScraper,
  getDetectedChanges,
  getProcessedDocuments,
  compressStoredContent
} from '../../api/scrapers';

// Tab panel component
function TabPanel(props) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`scraper-tabpanel-${index}`}
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

const ScraperDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [scraper, setScraper] = useState(null);
  const [status, setStatus] = useState(null);
  const [logs, setLogs] = useState([]);
  const [changes, setChanges] = useState([]);
  const [documents, setDocuments] = useState({ documents: [], totalCount: 0 });
  const [activeTab, setActiveTab] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [actionInProgress, setActionInProgress] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'info' });

  // Fetch scraper details
  const fetchScraperDetails = async () => {
    try {
      setIsLoading(true);

      // Fetch main scraper data
      const scraperData = await getScraper(id);
      setScraper(scraperData);

      // Fetch status
      const statusData = await getScraperStatus(id);
      setStatus(statusData);

      // Fetch logs
      const logsData = await getScraperLogs(id, 100); // Get last 100 logs
      setLogs(logsData.logs || []);

      // Don't load all tabs' data at once to improve performance
      if (activeTab === 2) {
        await fetchChanges();
      } else if (activeTab === 3) {
        await fetchDocuments();
      }

    } catch (error) {
      console.error('Error fetching scraper details:', error);
      setSnackbar({
        open: true,
        message: 'Error loading scraper details',
        severity: 'error'
      });
    } finally {
      setIsLoading(false);
    }
  };

  // Fetch content changes
  const fetchChanges = async () => {
    try {
      const changesData = await getDetectedChanges(id);
      setChanges(changesData.changes || []);
    } catch (error) {
      console.error('Error fetching content changes:', error);
    }
  };

  // Fetch documents
  const fetchDocuments = async () => {
    try {
      const documentsData = await getProcessedDocuments(id);
      setDocuments(documentsData);
    } catch (error) {
      console.error('Error fetching processed documents:', error);
    }
  };

  // Initial load
  useEffect(() => {
    fetchScraperDetails();

    // Auto-refresh status and logs every 10 seconds
    const interval = setInterval(() => {
      if (!actionInProgress) {
        getScraperStatus(id)
          .then(statusData => setStatus(statusData))
          .catch(error => console.error('Error fetching status:', error));

        getScraperLogs(id, 100)
          .then(logsData => setLogs(logsData.logs || []))
          .catch(error => console.error('Error fetching logs:', error));
      }
    }, 10000);

    return () => clearInterval(interval);
  }, [id]);

  // Load tab-specific data when tab changes
  useEffect(() => {
    if (activeTab === 2) {
      fetchChanges();
    } else if (activeTab === 3) {
      fetchDocuments();
    }
  }, [activeTab]);

  // Handle tab change
  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
  };

  // Handle scraper actions
  const handleStartScraper = async () => {
    try {
      setActionInProgress(true);
      await startScraper(id);
      setSnackbar({
        open: true,
        message: 'Scraper started successfully',
        severity: 'success'
      });
      fetchScraperDetails();
    } catch (error) {
      console.error('Error starting scraper:', error);
      setSnackbar({
        open: true,
        message: 'Error starting scraper',
        severity: 'error'
      });
    } finally {
      setActionInProgress(false);
    }
  };

  const handleStopScraper = async () => {
    try {
      setActionInProgress(true);
      await stopScraper(id);
      setSnackbar({
        open: true,
        message: 'Scraper stopped successfully',
        severity: 'success'
      });
      fetchScraperDetails();
    } catch (error) {
      console.error('Error stopping scraper:', error);
      setSnackbar({
        open: true,
        message: 'Error stopping scraper',
        severity: 'error'
      });
    } finally {
      setActionInProgress(false);
    }
  };

  const handleDeleteScraper = async () => {
    try {
      setActionInProgress(true);
      await deleteScraper(id);
      setSnackbar({
        open: true,
        message: 'Scraper deleted successfully',
        severity: 'success'
      });
      setDeleteDialogOpen(false);
      navigate('/scrapers');
    } catch (error) {
      console.error('Error deleting scraper:', error);
      setSnackbar({
        open: true,
        message: 'Error deleting scraper',
        severity: 'error'
      });
    } finally {
      setActionInProgress(false);
    }
  };

  const handleCompressContent = async () => {
    try {
      setActionInProgress(true);
      const result = await compressStoredContent(id);

      if (result.success) {
        setSnackbar({
          open: true,
          message: 'Content compressed successfully',
          severity: 'success'
        });
      } else {
        setSnackbar({
          open: true,
          message: result.message || 'Error compressing content',
          severity: 'error'
        });
      }
    } catch (error) {
      console.error('Error compressing content:', error);
      setSnackbar({
        open: true,
        message: 'Error compressing content',
        severity: 'error'
      });
    } finally {
      setActionInProgress(false);
    }
  };

  // Handle snackbar close
  const handleSnackbarClose = () => {
    setSnackbar({ ...snackbar, open: false });
  };

  // Render a status chip
  const renderStatusChip = () => {
    if (!status) return null;

    if (status.isRunning) {
      return <Chip label="Running" color="success" />;
    } else if (status.hasErrors) {
      return <Chip label="Error" color="error" />;
    } else {
      return <Chip label="Idle" color="default" />;
    }
  };

  if (isLoading && !scraper) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '80vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!scraper) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4 }}>
        <Alert severity="error">Scraper not found</Alert>
        <Button component={Link} to="/scrapers" sx={{ mt: 2 }}>
          Back to Scrapers
        </Button>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      {/* Header */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
          <Box>
            <Typography variant="h4" gutterBottom>
              {scraper.name}
            </Typography>
            <Typography variant="body1" color="textSecondary" gutterBottom>
              {scraper.baseUrl}
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mt: 1 }}>
              {renderStatusChip()}
              <Typography variant="body2">
                ID: {id}
              </Typography>
            </Box>
          </Box>
          <Box sx={{ display: 'flex', gap: 1 }}>
            {status?.isRunning ? (
              <Button
                variant="contained"
                color="warning"
                startIcon={<StopIcon />}
                onClick={handleStopScraper}
                disabled={actionInProgress}
              >
                {actionInProgress ? <CircularProgress size={24} color="inherit" /> : 'Stop'}
              </Button>
            ) : (
              <Button
                variant="contained"
                color="success"
                startIcon={<PlayIcon />}
                onClick={handleStartScraper}
                disabled={actionInProgress}
              >
                {actionInProgress ? <CircularProgress size={24} color="inherit" /> : 'Start'}
              </Button>
            )}
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
            <Button
              variant="outlined"
              color="error"
              startIcon={<DeleteIcon />}
              onClick={() => setDeleteDialogOpen(true)}
              disabled={actionInProgress || status?.isRunning}
            >
              Delete
            </Button>
            <IconButton
              color="primary"
              onClick={fetchScraperDetails}
              disabled={actionInProgress}
            >
              <RefreshIcon />
            </IconButton>
          </Box>
        </Box>
      </Paper>

      {/* Stats Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                URLs Processed
              </Typography>
              <Typography variant="h4">
                {status?.urlsProcessed || 0}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Documents Processed
              </Typography>
              <Typography variant="h4">
                {status?.documentsProcessed || 0}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Last Run
              </Typography>
              <Typography variant="h6">
                {scraper.lastRun ? format(new Date(scraper.lastRun), 'PPp') : 'Never'}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Monitoring
              </Typography>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                {scraper.enableContinuousMonitoring ? (
                  <>
                    <TimerIcon color="primary" sx={{ mr: 1 }} />
                    <Typography>
                      Every {scraper.monitoringIntervalMinutes} min
                    </Typography>
                  </>
                ) : (
                  <>
                    <TimerOffIcon color="action" sx={{ mr: 1 }} />
                    <Typography>Disabled</Typography>
                  </>
                )}
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Tabs */}
      <Paper sx={{ mb: 3 }}>
        <Tabs
          value={activeTab}
          onChange={handleTabChange}
          indicatorColor="primary"
          textColor="primary"
          variant="scrollable"
          scrollButtons="auto"
        >
          <Tab label="Overview" />
          <Tab label="Logs" />
          <Tab label="Content Changes" />
          <Tab label="Documents" />
          <Tab label="Settings" />
        </Tabs>

        <Divider />

        {/* Overview Tab */}
        <TabPanel value={activeTab} index={0}>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Typography variant="h6" gutterBottom>
                Scraper Configuration
              </Typography>
              <Card variant="outlined">
                <List dense>
                  <ListItem>
                    <ListItemText
                      primary="Start URL"
                      secondary={scraper.startUrl}
                    />
                  </ListItem>
                  <Divider component="li" />
                  <ListItem>
                    <ListItemText
                      primary="Max Depth"
                      secondary={scraper.maxDepth}
                    />
                  </ListItem>
                  <Divider component="li" />
                  <ListItem>
                    <ListItemText
                      primary="Max Concurrent Requests"
                      secondary={scraper.maxConcurrentRequests}
                    />
                  </ListItem>
                  <Divider component="li" />
                  <ListItem>
                    <ListItemText
                      primary="Follow External Links"
                      secondary={scraper.followExternalLinks ? 'Yes' : 'No'}
                    />
                  </ListItem>
                  <Divider component="li" />
                  <ListItem>
                    <ListItemText
                      primary="Respect Robots.txt"
                      secondary={scraper.respectRobotsTxt ? 'Yes' : 'No'}
                    />
                  </ListItem>
                  <Divider component="li" />
                  <ListItem>
                    <ListItemText
                      primary="Adaptive Crawling"
                      secondary={scraper.enableAdaptiveCrawling ? 'Enabled' : 'Disabled'}
                    />
                  </ListItem>
                  <Divider component="li" />
                  <ListItem>
                    <ListItemText
                      primary="Adaptive Rate Limiting"
                      secondary={scraper.enableAdaptiveRateLimiting ? 'Enabled' : 'Disabled'}
                    />
                  </ListItem>
                </List>
              </Card>

              <Box sx={{ mt: 3 }}>
                <Typography variant="h6" gutterBottom>
                  Advanced Features
                </Typography>
                <Card variant="outlined">
                  <List dense>
                    <ListItem>
                      <ListItemText
                        primary="Change Detection"
                        secondary={scraper.enableChangeDetection ? 'Enabled' : 'Disabled'}
                      />
                    </ListItem>
                    <Divider component="li" />
                    <ListItem>
                      <ListItemText
                        primary="Regulatory Content Analysis"
                        secondary={scraper.enableRegulatoryContentAnalysis ? 'Enabled' : 'Disabled'}
                      />
                    </ListItem>
                    <Divider component="li" />
                    <ListItem>
                      <ListItemText
                        primary="Process PDF Documents"
                        secondary={scraper.processPdfDocuments ? 'Enabled' : 'Disabled'}
                      />
                    </ListItem>
                    <Divider component="li" />
                    <ListItem>
                      <ListItemText
                        primary="Process Office Documents"
                        secondary={scraper.processOfficeDocuments ? 'Enabled' : 'Disabled'}
                      />
                    </ListItem>
                    <Divider component="li" />
                    <ListItem>
                      <ListItemText
                        primary="Extract Structured Content"
                        secondary={scraper.extractStructuredContent ? 'Enabled' : 'Disabled'}
                      />
                    </ListItem>
                  </List>
                </Card>
              </Box>
            </Grid>

            <Grid item xs={12} md={6}>
              <Box sx={{ mb: 3 }}>
                <Typography variant="h6" gutterBottom>
                  Execution Status
                </Typography>
                <Card variant="outlined">
                  <List dense>
                    <ListItem>
                      <ListItemText
                        primary="Current Status"
                        secondary={
                          <>
                            {renderStatusChip()} {status?.message && `(${status.message})`}
                          </>
                        }
                      />
                    </ListItem>
                    <Divider component="li" />
                    <ListItem>
                      <ListItemText
                        primary="Last Status Update"
                        secondary={status?.lastStatusUpdate
                          ? format(new Date(status.lastStatusUpdate), 'PPp')
                          : 'Never'}
                      />
                    </ListItem>
                    <Divider component="li" />
                    <ListItem>
                      <ListItemText
                        primary="Last Monitoring Check"
                        secondary={status?.lastMonitorCheck
                          ? format(new Date(status.lastMonitorCheck), 'PPp')
                          : 'Never'}
                      />
                    </ListItem>
                    {status?.isRunning && (
                      <>
                        <Divider component="li" />
                        <ListItem>
                          <ListItemText
                            primary="Start Time"
                            secondary={status.startTime
                              ? format(new Date(status.startTime), 'PPp')
                              : 'N/A'}
                          />
                        </ListItem>
                        <Divider component="li" />
                        <ListItem>
                          <ListItemText
                            primary="Elapsed Time"
                            secondary={status.elapsedTime || 'N/A'}
                          />
                        </ListItem>
                      </>
                    )}
                    {!status?.isRunning && status?.endTime && (
                      <>
                        <Divider component="li" />
                        <ListItem>
                          <ListItemText
                            primary="End Time"
                            secondary={format(new Date(status.endTime), 'PPp')}
                          />
                        </ListItem>
                      </>
                    )}
                  </List>
                </Card>
              </Box>

              <Box>
                <Typography variant="h6" gutterBottom>
                  Quick Actions
                </Typography>
                <Card variant="outlined">
                  <CardContent>
                    <Grid container spacing={2}>
                      <Grid item xs={12} sm={6}>
                        <Button
                          fullWidth
                          variant="outlined"
                          startIcon={<BarChartIcon />}
                          component={Link}
                          to={`/analytics/${id}`}
                        >
                          View Analytics
                        </Button>
                      </Grid>
                      <Grid item xs={12} sm={6}>
                        <Button
                          fullWidth
                          variant="outlined"
                          startIcon={<ScheduleIcon />}
                          component={Link}
                          to={`/scheduling/${id}`}
                        >
                          Manage Schedule
                        </Button>
                      </Grid>
                      <Grid item xs={12} sm={6}>
                        <Button
                          fullWidth
                          variant="outlined"
                          startIcon={<StorageIcon />}
                          onClick={handleCompressContent}
                          disabled={actionInProgress}
                        >
                          {actionInProgress ? (
                            <CircularProgress size={24} />
                          ) : (
                            'Compress Content'
                          )}
                        </Button>
                      </Grid>
                      <Grid item xs={12} sm={6}>
                        <Button
                          fullWidth
                          variant="outlined"
                          startIcon={<SettingsIcon />}
                          component={Link}
                          to={`/scrapers/${id}/settings`}
                        >
                          Advanced Settings
                        </Button>
                      </Grid>
                    </Grid>
                  </CardContent>
                </Card>
              </Box>

              {status?.hasErrors && (
                <Box sx={{ mt: 3 }}>
                  <Alert severity="error" sx={{ mb: 2 }}>
                    <Typography variant="subtitle1">
                      Last Error:
                    </Typography>
                    <Typography variant="body2">
                      {status.lastError || 'Unknown error occurred'}
                    </Typography>
                  </Alert>
                </Box>
              )}
            </Grid>
          </Grid>
        </TabPanel>

        {/* Logs Tab */}
        <TabPanel value={activeTab} index={1}>
          <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h6">
              Recent Logs
            </Typography>
            <Button
              startIcon={<RefreshIcon />}
              onClick={() => getScraperLogs(id, 100).then(data => setLogs(data.logs || []))}
              disabled={actionInProgress}
            >
              Refresh Logs
            </Button>
          </Box>

          <Paper variant="outlined" sx={{ p: 0 }}>
            {logs.length > 0 ? (
              <List dense>
                {logs.map((log, index) => (
                  <React.Fragment key={index}>
                    <ListItem alignItems="flex-start">
                      <ListItemText
                        primary={
                          <Box sx={{ display: 'flex', alignItems: 'center' }}>
                            <Typography
                              component="span"
                              variant="body2"
                              color="textSecondary"
                              sx={{ minWidth: 140 }}
                            >
                              {log.timestamp
                                ? format(new Date(log.timestamp), 'HH:mm:ss dd/MM/yyyy')
                                : 'No timestamp'}
                            </Typography>
                            <Box sx={{ ml: 2 }}>
                              {log.level === 'Error' && <ErrorIcon color="error" sx={{ mr: 1, fontSize: 16 }} />}
                              <Typography
                                component="span"
                                variant="body2"
                                color={log.level === 'Error' ? 'error' : 'textPrimary'}
                              >
                                {log.message}
                              </Typography>
                            </Box>
                          </Box>
                        }
                      />
                    </ListItem>
                    {index < logs.length - 1 && <Divider component="li" />}
                  </React.Fragment>
                ))}
              </List>
            ) : (
              <Box sx={{ p: 4, textAlign: 'center' }}>
                <Typography variant="body1" color="textSecondary">
                  No logs available
                </Typography>
              </Box>
            )}
          </Paper>
        </TabPanel>

        {/* Content Changes Tab */}
        <TabPanel value={activeTab} index={2}>
          <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h6">
              Detected Content Changes
            </Typography>
            <Button
              startIcon={<RefreshIcon />}
              onClick={fetchChanges}
              disabled={actionInProgress}
            >
              Refresh Changes
            </Button>
          </Box>

          <Paper variant="outlined">
            {changes.length > 0 ? (
              <List dense>
                {changes.map((change, index) => (
                  <React.Fragment key={index}>
                    <ListItem alignItems="flex-start">
                      <ListItemText
                        primary={
                          <Box sx={{ display: 'flex', alignItems: 'center' }}>
                            <Chip
                              label={change.changeType}
                              size="small"
                              color={
                                change.changeType === 'Addition' ? 'success' :
                                change.changeType === 'Removal' ? 'error' :
                                change.changeType === 'Modification' ? 'warning' :
                                'default'
                              }
                              sx={{ mr: 1 }}
                            />
                            <Typography
                              variant="body2"
                              component="a"
                              href={change.url}
                              target="_blank"
                              rel="noopener noreferrer"
                              sx={{ textDecoration: 'none' }}
                            >
                              {change.url}
                            </Typography>
                          </Box>
                        }
                        secondary={
                          <Box sx={{ mt: 1 }}>
                            <Typography variant="body2" color="textSecondary">
                              Detected at: {format(new Date(change.detectedAt), 'PPp')}
                            </Typography>
                            <Typography variant="body2" color="textSecondary">
                              Significance: {change.significance}/100
                            </Typography>
                            {change.changeDetails && (
                              <Typography variant="body2" sx={{ mt: 1 }}>
                                {change.changeDetails}
                              </Typography>
                            )}
                          </Box>
                        }
                      />
                    </ListItem>
                    {index < changes.length - 1 && <Divider component="li" />}
                  </React.Fragment>
                ))}
              </List>
            ) : (
              <Box sx={{ p: 4, textAlign: 'center' }}>
                <Typography variant="body1" color="textSecondary">
                  No content changes detected
                </Typography>
              </Box>
            )}
          </Paper>
        </TabPanel>

        {/* Documents Tab */}
        <TabPanel value={activeTab} index={3}>
          <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h6">
              Processed Documents
            </Typography>
            <Button
              startIcon={<RefreshIcon />}
              onClick={fetchDocuments}
              disabled={actionInProgress}
            >
              Refresh Documents
            </Button>
          </Box>

          <Paper variant="outlined">
            {documents.documents.length > 0 ? (
              <List dense>
                {documents.documents.map((doc, index) => (
                  <React.Fragment key={index}>
                    <ListItem alignItems="flex-start">
                      <ListItemText
                        primary={
                          <Box sx={{ display: 'flex', alignItems: 'center' }}>
                            <Chip
                              label={doc.documentType}
                              size="small"
                              color={
                                doc.documentType === 'PDF' ? 'error' :
                                doc.documentType === 'WORD' ? 'primary' :
                                doc.documentType === 'EXCEL' ? 'success' :
                                'default'
                              }
                              sx={{ mr: 1 }}
                            />
                            <Typography variant="body1">
                              {doc.title || 'Untitled Document'}
                            </Typography>
                          </Box>
                        }
                        secondary={
                          <Box sx={{ mt: 1 }}>
                            <Typography
                              variant="body2"
                              component="a"
                              href={doc.url}
                              target="_blank"
                              rel="noopener noreferrer"
                              sx={{ display: 'block', mb: 0.5 }}
                            >
                              {doc.url}
                            </Typography>
                            <Typography variant="body2" color="textSecondary">
                              Processed at: {format(new Date(doc.processedAt), 'PPp')}
                            </Typography>
                            <Typography variant="body2" color="textSecondary">
                              Size: {Math.round(doc.contentSizeBytes / 1024)} KB
                            </Typography>
                          </Box>
                        }
                      />
                    </ListItem>
                    {index < documents.documents.length - 1 && <Divider component="li" />}
                  </React.Fragment>
                ))}
              </List>
            ) : (
              <Box sx={{ p: 4, textAlign: 'center' }}>
                <Typography variant="body1" color="textSecondary">
                  No documents processed
                </Typography>
              </Box>
            )}
          </Paper>
        </TabPanel>

        {/* Settings Tab */}
        <TabPanel value={activeTab} index={4}>
          <Typography variant="h6" gutterBottom>
            Scraper Settings
          </Typography>
          <Typography variant="body1" paragraph>
            Manage advanced settings for this scraper using the buttons below:
          </Typography>

          <Grid container spacing={3}>
            <Grid item xs={12} sm={6} md={4}>
              <Card sx={{ height: '100%' }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Monitoring
                  </Typography>
                  <Typography variant="body2" sx={{ mb: 2 }}>
                    Configure continuous monitoring and change detection settings
                  </Typography>
                  <Button
                    fullWidth
                    variant="outlined"
                    component={Link}
                    to={`/scrapers/${id}/monitoring`}
                  >
                    Configure Monitoring
                  </Button>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} sm={6} md={4}>
              <Card sx={{ height: '100%' }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Scheduling
                  </Typography>
                  <Typography variant="body2" sx={{ mb: 2 }}>
                    Set up automatic execution schedules for this scraper
                  </Typography>
                  <Button
                    fullWidth
                    variant="outlined"
                    component={Link}
                    to={`/scheduling/${id}`}
                  >
                    Manage Schedules
                  </Button>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} sm={6} md={4}>
              <Card sx={{ height: '100%' }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Notifications
                  </Typography>
                  <Typography variant="body2" sx={{ mb: 2 }}>
                    Configure webhook notifications for scraper events
                  </Typography>
                  <Button
                    fullWidth
                    variant="outlined"
                    component={Link}
                    to={`/notifications/${id}`}
                  >
                    Configure Notifications
                  </Button>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} sm={6} md={4}>
              <Card sx={{ height: '100%' }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Rate Limiting
                  </Typography>
                  <Typography variant="body2" sx={{ mb: 2 }}>
                    Configure request rate limits and adaptive rate limiting
                  </Typography>
                  <Button
                    fullWidth
                    variant="outlined"
                    component={Link}
                    to={`/scrapers/${id}/rate-limiting`}
                  >
                    Configure Rate Limits
                  </Button>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} sm={6} md={4}>
              <Card sx={{ height: '100%' }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Content Processing
                  </Typography>
                  <Typography variant="body2" sx={{ mb: 2 }}>
                    Configure how content and documents are processed
                  </Typography>
                  <Button
                    fullWidth
                    variant="outlined"
                    component={Link}
                    to={`/scrapers/${id}/content-processing`}
                  >
                    Configure Processing
                  </Button>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} sm={6} md={4}>
              <Card sx={{ height: '100%' }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Data Export
                  </Typography>
                  <Typography variant="body2" sx={{ mb: 2 }}>
                    Export scraped data and configurations
                  </Typography>
                  <Button
                    fullWidth
                    variant="outlined"
                    component={Link}
                    to={`/scrapers/${id}/export`}
                  >
                    Export Data
                  </Button>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </TabPanel>
      </Paper>

      {/* Delete Dialog */}
      <Dialog
        open={deleteDialogOpen}
        onClose={() => setDeleteDialogOpen(false)}
      >
        <DialogTitle>Delete Scraper</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete the scraper "{scraper.name}"?
            This action cannot be undone and all associated data will be lost.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button
            onClick={handleDeleteScraper}
            color="error"
            variant="contained"
            disabled={actionInProgress}
          >
            {actionInProgress ? <CircularProgress size={24} color="inherit" /> : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Snackbar for feedback */}
      <Snackbar
        open={snackbar.open}
        autoHideDuration={6000}
        onClose={handleSnackbarClose}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert onClose={handleSnackbarClose} severity={snackbar.severity}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Container>
  );
};

export default ScraperDetail;