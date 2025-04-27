import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  Box, 
  Button, 
  Grid, 
  Paper, 
  Typography,
  Tabs,
  Tab,
  Divider, 
  IconButton,
  Tooltip,
  Chip
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import StopIcon from '@mui/icons-material/Stop';
import ScheduleIcon from '@mui/icons-material/Schedule';
import DeleteIcon from '@mui/icons-material/Delete';
import DownloadIcon from '@mui/icons-material/Download';

import PageHeader from '../components/Common/PageHeader';
import DataTable from '../components/Common/DataTable/DataTable';
import StatusBadge from '../components/Common/StatusBadge';
import LoadingSpinner from '../components/Common/LoadingSpinner';
import ErrorMessage from '../components/Common/ErrorMessage';
import useApiClient from '../hooks/useApiClient';
import { formatDate, formatDateTime, formatUrl } from '../utils/formatters';

const ScraperDetailPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { api, loading, error, execute } = useApiClient();
  
  const [scraper, setScraper] = useState(null);
  const [results, setResults] = useState([]);
  const [logs, setLogs] = useState([]);
  const [activeTab, setActiveTab] = useState(0);
  const [resultsLoading, setResultsLoading] = useState(false);
  const [logsLoading, setLogsLoading] = useState(false);
  
  // Fetch scraper data
  useEffect(() => {
    const fetchScraper = async () => {
      try {
        const data = await execute(() => api.scrapers.getById(id));
        setScraper(data);
      } catch (err) {
        console.error('Error fetching scraper:', err);
      }
    };
    
    fetchScraper();
    
    // Set up polling for status if scraper is running
    let statusInterval;
    if (scraper && scraper.status === 'running') {
      statusInterval = setInterval(() => {
        updateScraperStatus();
      }, 5000); // Poll every 5 seconds
    }
    
    return () => {
      if (statusInterval) clearInterval(statusInterval);
    };
  }, [id, execute, api.scrapers]);
  
  // Fetch results and logs when tab changes
  useEffect(() => {
    if (activeTab === 1 && scraper) {
      fetchScraperResults();
    } else if (activeTab === 2 && scraper) {
      fetchScraperLogs();
    }
  }, [activeTab, scraper]);
  
  const updateScraperStatus = async () => {
    try {
      const statusData = await execute(() => api.scrapers.getStatus(id), { showLoading: false });
      setScraper(prev => ({ ...prev, status: statusData.status }));
      
      // If scraper is no longer running, stop polling
      if (statusData.status !== 'running') {
        // Reload scraper data to get updated info
        const updatedScraper = await execute(() => api.scrapers.getById(id), { showLoading: false });
        setScraper(updatedScraper);
      }
    } catch (error) {
      console.error('Error updating scraper status:', error);
    }
  };
  
  const fetchScraperResults = async () => {
    setResultsLoading(true);
    try {
      const data = await execute(() => api.scrapers.getResults(id, { limit: 100 }), { showLoading: false });
      setResults(data || []);
    } catch (error) {
      console.error('Error fetching scraper results:', error);
    } finally {
      setResultsLoading(false);
    }
  };
  
  const fetchScraperLogs = async () => {
    setLogsLoading(true);
    try {
      const data = await execute(() => api.scrapers.getLogs(id, { limit: 100 }), { showLoading: false });
      setLogs(data || []);
    } catch (error) {
      console.error('Error fetching scraper logs:', error);
    } finally {
      setLogsLoading(false);
    }
  };
  
  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
  };
  
  const handleStartScraper = async () => {
    try {
      await execute(() => api.scrapers.start(id));
      // Update status after starting
      await updateScraperStatus();
    } catch (error) {
      console.error('Error starting scraper:', error);
    }
  };
  
  const handleStopScraper = async () => {
    try {
      await execute(() => api.scrapers.stop(id));
      // Update status after stopping
      await updateScraperStatus();
    } catch (error) {
      console.error('Error stopping scraper:', error);
    }
  };
  
  const handleDeleteScraper = async () => {
    if (window.confirm(`Are you sure you want to delete the scraper "${scraper.name}"?`)) {
      try {
        await execute(() => api.scrapers.delete(id));
        navigate('/scrapers');
      } catch (error) {
        console.error('Error deleting scraper:', error);
      }
    }
  };
  
  const handleCreateSchedule = () => {
    navigate(`/schedules/new?scraperId=${id}`);
  };
  
  const handleExport = () => {
    // This would be implemented to export scraper results
    console.log('Export functionality not implemented yet');
    alert('Export functionality not implemented yet');
  };
  
  // Format breadcrumbs for navigation
  const breadcrumbs = [
    { label: 'Scrapers', path: '/scrapers' },
    { label: scraper?.name || 'Scraper Details' }
  ];

  if (loading && !scraper) {
    return <LoadingSpinner message="Loading scraper data..." />;
  }

  if (error && !scraper) {
    return (
      <ErrorMessage 
        title="Failed to load scraper" 
        message={error}
        onRetry={() => window.location.reload()}
      />
    );
  }

  if (!scraper) {
    return (
      <ErrorMessage 
        title="Scraper not found" 
        message="The scraper you're looking for does not exist or has been deleted."
      />
    );
  }

  // Define table columns for results tab
  const resultColumns = [
    { id: 'timestamp', label: 'Timestamp', render: (row) => formatDateTime(row.timestamp) },
    { id: 'url', label: 'URL', render: (row) => formatUrl(row.url) },
    { id: 'status', label: 'Status', render: (row) => row.status === 200 ? 'Success' : `Error (${row.status})` },
    { id: 'size', label: 'Content Size', render: (row) => `${Math.round(row.size / 1024)} KB` }
  ];

  // Define table columns for logs tab
  const logColumns = [
    { id: 'timestamp', label: 'Time', render: (row) => formatDateTime(row.timestamp) },
    { id: 'level', label: 'Level' },
    { id: 'message', label: 'Message' }
  ];

  return (
    <>
      <PageHeader 
        title={scraper.name} 
        subtitle={formatUrl(scraper.url)}
        breadcrumbs={breadcrumbs}
        action={
          <Box sx={{ display: 'flex', gap: 1 }}>
            {scraper.status === 'running' ? (
              <Button 
                variant="contained" 
                color="error"
                startIcon={<StopIcon />}
                onClick={handleStopScraper}
              >
                Stop
              </Button>
            ) : (
              <Button 
                variant="contained" 
                color="success"
                startIcon={<PlayArrowIcon />}
                onClick={handleStartScraper}
              >
                Start
              </Button>
            )}
            
            <Button 
              variant="outlined"
              startIcon={<EditIcon />}
              onClick={() => navigate(`/scrapers/${id}/edit`)}
            >
              Edit
            </Button>
          </Box>
        }
      />

      {/* Scraper Overview */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <Typography variant="h6" gutterBottom>
              Scraper Overview
            </Typography>
            
            <Grid container spacing={2} sx={{ mb: 2 }}>
              <Grid item xs={4}>
                <Typography variant="body2" color="text.secondary">Status</Typography>
                <Box sx={{ mt: 0.5 }}>
                  <StatusBadge status={scraper.status} />
                </Box>
              </Grid>
              <Grid item xs={4}>
                <Typography variant="body2" color="text.secondary">Last Run</Typography>
                <Typography>{scraper.lastRun ? formatDateTime(scraper.lastRun) : 'Never'}</Typography>
              </Grid>
              <Grid item xs={4}>
                <Typography variant="body2" color="text.secondary">Run Count</Typography>
                <Typography>{scraper.runCount || 0}</Typography>
              </Grid>
              <Grid item xs={4}>
                <Typography variant="body2" color="text.secondary">Created</Typography>
                <Typography>{formatDate(scraper.createdAt)}</Typography>
              </Grid>
              <Grid item xs={4}>
                <Typography variant="body2" color="text.secondary">Run Mode</Typography>
                <Typography>{scraper.runMode || 'On demand'}</Typography>
              </Grid>
              <Grid item xs={4}>
                <Typography variant="body2" color="text.secondary">Output Format</Typography>
                <Typography sx={{ textTransform: 'uppercase' }}>{scraper.outputFormat}</Typography>
              </Grid>
            </Grid>
            
            {/* Additional settings */}
            <Typography variant="subtitle1" gutterBottom sx={{ mt: 2 }}>
              Scraping Settings
            </Typography>
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">Content Selector</Typography>
                <Typography fontFamily="monospace">{scraper.contentSelector}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">Navigation Type</Typography>
                <Typography sx={{ textTransform: 'capitalize' }}>{scraper.navigationType}</Typography>
              </Grid>
            </Grid>
          </Grid>
          
          <Grid item xs={12} md={6}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6">Actions</Typography>
            </Box>
            
            <Grid container spacing={2}>
              <Grid item xs={6} md={4}>
                <Button 
                  fullWidth 
                  variant="outlined"
                  startIcon={<ScheduleIcon />}
                  onClick={handleCreateSchedule}
                >
                  Schedule
                </Button>
              </Grid>
              <Grid item xs={6} md={4}>
                <Button 
                  fullWidth 
                  variant="outlined"
                  startIcon={<DownloadIcon />}
                  onClick={handleExport}
                >
                  Export Data
                </Button>
              </Grid>
              <Grid item xs={6} md={4}>
                <Button 
                  fullWidth 
                  variant="outlined" 
                  color="error"
                  startIcon={<DeleteIcon />}
                  onClick={handleDeleteScraper}
                >
                  Delete
                </Button>
              </Grid>
            </Grid>
            
            {/* Statistics */}
            <Typography variant="subtitle1" gutterBottom sx={{ mt: 4 }}>
              Statistics
            </Typography>
            <Grid container spacing={2}>
              <Grid item xs={4}>
                <Typography variant="body2" color="text.secondary">Pages Scraped</Typography>
                <Typography>{scraper.stats?.pagesScraped || 0}</Typography>
              </Grid>
              <Grid item xs={4}>
                <Typography variant="body2" color="text.secondary">Success Rate</Typography>
                <Typography>{scraper.stats?.successRate || 0}%</Typography>
              </Grid>
              <Grid item xs={4}>
                <Typography variant="body2" color="text.secondary">Avg. Duration</Typography>
                <Typography>{scraper.stats?.avgDuration || '0s'}</Typography>
              </Grid>
            </Grid>
          </Grid>
        </Grid>
      </Paper>
      
      {/* Tabs */}
      <Paper sx={{ mb: 3 }}>
        <Tabs
          value={activeTab}
          onChange={handleTabChange}
          textColor="primary"
          indicatorColor="primary"
        >
          <Tab label="Configuration" />
          <Tab label="Results" />
          <Tab label="Logs" />
        </Tabs>
        
        <Divider />
        
        {/* Tab content */}
        <Box sx={{ p: 3 }}>
          {activeTab === 0 && (
            <Grid container spacing={3}>
              {/* Configuration details */}
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle1" gutterBottom>
                  Browser Settings
                </Typography>
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <Typography variant="body2" color="text.secondary">Headless Browser</Typography>
                    <Typography>{scraper.browserSettings?.useHeadlessBrowser ? 'Enabled' : 'Disabled'}</Typography>
                  </Grid>
                  {scraper.browserSettings?.userAgent && (
                    <Grid item xs={12}>
                      <Typography variant="body2" color="text.secondary">User Agent</Typography>
                      <Typography sx={{ fontSize: '0.85rem' }}>{scraper.browserSettings.userAgent}</Typography>
                    </Grid>
                  )}
                  {scraper.browserSettings?.waitForSelector && (
                    <Grid item xs={12}>
                      <Typography variant="body2" color="text.secondary">Wait for Selector</Typography>
                      <Typography fontFamily="monospace">{scraper.browserSettings.waitForSelector}</Typography>
                    </Grid>
                  )}
                  <Grid item xs={12}>
                    <Typography variant="body2" color="text.secondary">Navigation Timeout</Typography>
                    <Typography>{scraper.browserSettings?.navigationTimeout || 30000} ms</Typography>
                  </Grid>
                </Grid>
              </Grid>
              
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle1" gutterBottom>
                  Advanced Settings
                </Typography>
                <Grid container spacing={2}>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Max Depth</Typography>
                    <Typography>{scraper.advancedSettings?.maxDepth || 1}</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Max Pages</Typography>
                    <Typography>{scraper.advancedSettings?.maxPages || 10}</Typography>
                  </Grid>
                  <Grid item xs={12}>
                    <Typography variant="body2" color="text.secondary">Rate Limit Delay</Typography>
                    <Typography>{scraper.advancedSettings?.rateLimitDelay || 1000} ms</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Follow External Links</Typography>
                    <Typography>{scraper.advancedSettings?.followExternalLinks ? 'Yes' : 'No'}</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Capture Screenshots</Typography>
                    <Typography>{scraper.advancedSettings?.captureScreenshots ? 'Yes' : 'No'}</Typography>
                  </Grid>
                </Grid>
              </Grid>
            </Grid>
          )}

          {activeTab === 1 && (
            <Box>
              <DataTable 
                columns={resultColumns}
                data={results}
                loading={resultsLoading}
                emptyMessage="No results available for this scraper yet. Run the scraper to collect data."
              />
            </Box>
          )}

          {activeTab === 2 && (
            <Box>
              <DataTable 
                columns={logColumns}
                data={logs}
                loading={logsLoading}
                emptyMessage="No logs available for this scraper."
              />
            </Box>
          )}
        </Box>
      </Paper>
    </>
  );
};

export default ScraperDetailPage;