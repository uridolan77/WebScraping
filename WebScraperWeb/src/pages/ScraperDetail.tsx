import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Container,
  Box,
  Typography,
  Button,
  Tabs,
  Tab,
  Paper,
  CircularProgress,
  Alert,
  Divider
} from '@mui/material';
import {
  Edit as EditIcon,
  PlayArrow as PlayIcon,
  Stop as StopIcon,
  Delete as DeleteIcon,
  ArrowBack as ArrowBackIcon
} from '@mui/icons-material';
import { useScraperContext } from '../contexts/ScraperContext';
import PageHeader from '../components/common/PageHeader';
import LoadingSpinner from '../components/common/LoadingSpinner';

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
  const {
    fetchScraper,
    selectedScraper,
    loading,
    error,
    start,
    stop,
    fetchScraperStatus,
    fetchScraperLogs,
    status,
    logs
  } = useScraperContext();

  const [tabValue, setTabValue] = useState(0);
  const [actionInProgress, setActionInProgress] = useState(false);

  // Fetch scraper data
  useEffect(() => {
    if (id) {
      fetchScraper(id);
      fetchScraperStatus(id);
      fetchScraperLogs(id);
    }
  }, [id, fetchScraper, fetchScraperStatus, fetchScraperLogs]);

  // Set up polling for status updates
  useEffect(() => {
    if (!id) return;

    const statusInterval = setInterval(() => {
      fetchScraperStatus(id);
    }, 5000);

    const logsInterval = setInterval(() => {
      if (status?.isRunning) {
        fetchScraperLogs(id);
      }
    }, 10000);

    return () => {
      clearInterval(statusInterval);
      clearInterval(logsInterval);
    };
  }, [id, status, fetchScraperStatus, fetchScraperLogs]);

  const handleTabChange = (event, newValue) => {
    setTabValue(newValue);
  };

  const handleStartScraper = async () => {
    if (!id) return;

    try {
      setActionInProgress(true);
      await start(id);
      fetchScraperStatus(id);
    } catch (error) {
      console.error('Error starting scraper:', error);
    } finally {
      setActionInProgress(false);
    }
  };

  const handleStopScraper = async () => {
    if (!id) return;

    try {
      setActionInProgress(true);
      await stop(id);
      fetchScraperStatus(id);
    } catch (error) {
      console.error('Error stopping scraper:', error);
    } finally {
      setActionInProgress(false);
    }
  };

  if (loading && !selectedScraper) {
    return <LoadingSpinner message="Loading scraper details..." />;
  }

  if (error) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Alert severity="error" sx={{ mb: 2 }}>
          Error loading scraper: {error}
        </Alert>
        <Button
          variant="contained"
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/scrapers')}
        >
          Back to Scrapers
        </Button>
      </Container>
    );
  }

  if (!selectedScraper) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Alert severity="warning" sx={{ mb: 2 }}>
          Scraper not found
        </Alert>
        <Button
          variant="contained"
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/scrapers')}
        >
          Back to Scrapers
        </Button>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      {/* Header */}
      <PageHeader
        title={selectedScraper.name}
        subtitle={`Scraper ID: ${selectedScraper.id}`}
        breadcrumbs={[
          { text: 'Dashboard', path: '/' },
          { text: 'Scrapers', path: '/scrapers' },
          { text: selectedScraper.name }
        ]}
      />

      {/* Action Buttons */}
      <Box sx={{ mb: 3, display: 'flex', gap: 2 }}>
        {status?.isRunning ? (
          <Button
            variant="contained"
            color="warning"
            startIcon={<StopIcon />}
            onClick={handleStopScraper}
            disabled={actionInProgress}
          >
            {actionInProgress ? <CircularProgress size={24} color="inherit" /> : 'Stop Scraper'}
          </Button>
        ) : (
          <Button
            variant="contained"
            color="success"
            startIcon={<PlayIcon />}
            onClick={handleStartScraper}
            disabled={actionInProgress}
          >
            {actionInProgress ? <CircularProgress size={24} color="inherit" /> : 'Start Scraper'}
          </Button>
        )}

        <Button
          variant="outlined"
          startIcon={<EditIcon />}
          onClick={() => navigate(`/scrapers/${id}/edit`)}
        >
          Edit
        </Button>

        <Button
          variant="outlined"
          color="error"
          startIcon={<DeleteIcon />}
          onClick={() => {
            // Show delete confirmation dialog
            // This would typically be implemented with a state variable and a Dialog component
            if (window.confirm('Are you sure you want to delete this scraper? This action cannot be undone.')) {
              // Delete scraper and navigate back to list
              // deleteScraper(id).then(() => navigate('/scrapers'));
            }
          }}
          disabled={status?.isRunning}
        >
          Delete
        </Button>
      </Box>

      {/* Status Summary */}
      <Paper sx={{ mb: 3, p: 2 }}>
        <Typography variant="h6" gutterBottom>Status</Typography>
        <Divider sx={{ mb: 2 }} />

        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 4 }}>
          <Box>
            <Typography variant="body2" color="text.secondary">Status</Typography>
            <Typography variant="body1" fontWeight="bold">
              {status?.isRunning ? 'Running' : status?.hasErrors ? 'Error' : 'Idle'}
            </Typography>
          </Box>

          <Box>
            <Typography variant="body2" color="text.secondary">Last Run</Typography>
            <Typography variant="body1">
              {selectedScraper.lastRun ? new Date(selectedScraper.lastRun).toLocaleString() : 'Never'}
            </Typography>
          </Box>

          <Box>
            <Typography variant="body2" color="text.secondary">URLs Processed</Typography>
            <Typography variant="body1">{selectedScraper.urlsProcessed || 0}</Typography>
          </Box>

          <Box>
            <Typography variant="body2" color="text.secondary">Base URL</Typography>
            <Typography variant="body1">{selectedScraper.baseUrl}</Typography>
          </Box>

          <Box>
            <Typography variant="body2" color="text.secondary">Created</Typography>
            <Typography variant="body1">
              {selectedScraper.createdAt ? new Date(selectedScraper.createdAt).toLocaleString() : 'Unknown'}
            </Typography>
          </Box>
        </Box>
      </Paper>

      {/* Tabs */}
      <Paper sx={{ mb: 3 }}>
        <Tabs
          value={tabValue}
          onChange={handleTabChange}
          variant="scrollable"
          scrollButtons="auto"
        >
          <Tab label="Configuration" />
          <Tab label="Logs" />
          <Tab label="Results" />
          <Tab label="Changes" />
          <Tab label="Documents" />
          <Tab label="Performance" />
        </Tabs>

        {/* Configuration Tab */}
        <TabPanel value={tabValue} index={0}>
          <Typography variant="h6" gutterBottom>Scraper Configuration</Typography>
          <Divider sx={{ mb: 2 }} />

          <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: 3 }}>
            <Box>
              <Typography variant="body2" color="text.secondary">Start URL</Typography>
              <Typography variant="body1">{selectedScraper.startUrl}</Typography>
            </Box>

            <Box>
              <Typography variant="body2" color="text.secondary">Base URL</Typography>
              <Typography variant="body1">{selectedScraper.baseUrl}</Typography>
            </Box>

            <Box>
              <Typography variant="body2" color="text.secondary">Output Directory</Typography>
              <Typography variant="body1">{selectedScraper.outputDirectory}</Typography>
            </Box>

            <Box>
              <Typography variant="body2" color="text.secondary">Max Depth</Typography>
              <Typography variant="body1">{selectedScraper.maxDepth}</Typography>
            </Box>

            <Box>
              <Typography variant="body2" color="text.secondary">Max Concurrent Requests</Typography>
              <Typography variant="body1">{selectedScraper.maxConcurrentRequests}</Typography>
            </Box>

            <Box>
              <Typography variant="body2" color="text.secondary">Delay Between Requests</Typography>
              <Typography variant="body1">{selectedScraper.delayBetweenRequests} ms</Typography>
            </Box>

            <Box>
              <Typography variant="body2" color="text.secondary">Follow External Links</Typography>
              <Typography variant="body1">{selectedScraper.followExternalLinks ? 'Yes' : 'No'}</Typography>
            </Box>

            <Box>
              <Typography variant="body2" color="text.secondary">Respect Robots.txt</Typography>
              <Typography variant="body1">{selectedScraper.respectRobotsTxt ? 'Yes' : 'No'}</Typography>
            </Box>

            <Box>
              <Typography variant="body2" color="text.secondary">Auto Learn Header/Footer</Typography>
              <Typography variant="body1">{selectedScraper.autoLearnHeaderFooter ? 'Yes' : 'No'}</Typography>
            </Box>

            <Box>
              <Typography variant="body2" color="text.secondary">Learning Pages Count</Typography>
              <Typography variant="body1">{selectedScraper.learningPagesCount}</Typography>
            </Box>

            <Box>
              <Typography variant="body2" color="text.secondary">Enable Change Detection</Typography>
              <Typography variant="body1">{selectedScraper.enableChangeDetection ? 'Yes' : 'No'}</Typography>
            </Box>

            <Box>
              <Typography variant="body2" color="text.secondary">Track Content Versions</Typography>
              <Typography variant="body1">{selectedScraper.trackContentVersions ? 'Yes' : 'No'}</Typography>
            </Box>
          </Box>
        </TabPanel>

        {/* Logs Tab */}
        <TabPanel value={tabValue} index={1}>
          <Typography variant="h6" gutterBottom>Scraper Logs</Typography>
          <Divider sx={{ mb: 2 }} />

          {logs && logs.length > 0 ? (
            <Box
              sx={{
                maxHeight: '500px',
                overflow: 'auto',
                bgcolor: '#f5f5f5',
                p: 2,
                borderRadius: 1,
                fontFamily: 'monospace'
              }}
            >
              {logs.map((log, index) => (
                <Box
                  key={index}
                  sx={{
                    mb: 1,
                    color: log.level === 'Error' ? 'error.main' :
                           log.level === 'Warning' ? 'warning.main' : 'text.primary'
                  }}
                >
                  <Typography variant="body2" component="span" sx={{ mr: 2, color: 'text.secondary' }}>
                    {new Date(log.timestamp).toLocaleString()}
                  </Typography>
                  <Typography variant="body2" component="span" sx={{ mr: 2, fontWeight: 'bold' }}>
                    [{log.level}]
                  </Typography>
                  <Typography variant="body2" component="span">
                    {log.message}
                  </Typography>
                </Box>
              ))}
            </Box>
          ) : (
            <Typography variant="body1" color="text.secondary" align="center" sx={{ py: 4 }}>
              No logs available
            </Typography>
          )}
        </TabPanel>

        {/* Results Tab */}
        <TabPanel value={tabValue} index={2}>
          <Typography variant="h6" gutterBottom>Scraper Results</Typography>
          <Divider sx={{ mb: 2 }} />

          <Typography variant="body1" color="text.secondary" align="center" sx={{ py: 4 }}>
            Results functionality will be implemented in a future update
          </Typography>
        </TabPanel>

        {/* Changes Tab */}
        <TabPanel value={tabValue} index={3}>
          <Typography variant="h6" gutterBottom>Detected Changes</Typography>
          <Divider sx={{ mb: 2 }} />

          <Typography variant="body1" color="text.secondary" align="center" sx={{ py: 4 }}>
            Changes functionality will be implemented in a future update
          </Typography>
        </TabPanel>

        {/* Documents Tab */}
        <TabPanel value={tabValue} index={4}>
          <Typography variant="h6" gutterBottom>Processed Documents</Typography>
          <Divider sx={{ mb: 2 }} />

          <Typography variant="body1" color="text.secondary" align="center" sx={{ py: 4 }}>
            Documents functionality will be implemented in a future update
          </Typography>
        </TabPanel>

        {/* Performance Tab */}
        <TabPanel value={tabValue} index={5}>
          <Typography variant="h6" gutterBottom>Performance Metrics</Typography>
          <Divider sx={{ mb: 2 }} />

          <Typography variant="body1" color="text.secondary" align="center" sx={{ py: 4 }}>
            Performance metrics will be implemented in a future update
          </Typography>
        </TabPanel>
      </Paper>
    </Container>
  );
};

export default ScraperDetail;
