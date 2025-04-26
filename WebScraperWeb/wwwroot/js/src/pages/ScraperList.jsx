import React, { useState, useEffect } from 'react';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import {
  Typography,
  Box,
  Button,
  Card,
  CardContent,
  CardActions,
  Grid,
  Chip,
  CircularProgress,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  IconButton,
  Tooltip,
  Paper,
  Collapse,
  Divider
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import LaunchIcon from '@mui/icons-material/Launch';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import DashboardIcon from '@mui/icons-material/Dashboard';
import MonitorHeartIcon from '@mui/icons-material/MonitorHeart';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import PauseCircleIcon from '@mui/icons-material/PauseCircle';
import ErrorIcon from '@mui/icons-material/Error';
import CloseIcon from '@mui/icons-material/Close';
import SaveIcon from '@mui/icons-material/Save';

import { fetchAllScrapers, deleteScraper, fetchScraper, updateScraper } from '../services/api';
import MonitoringDialog from '../components/MonitoringDialog';

// Import the image we saw in the provided screenshot to create a similar UI
import ScraperConfigForm from '../components/ScraperConfigForm';

const getStatusColor = (status) => {
  switch (status?.toLowerCase()) {
    case 'running':
      return 'success';
    case 'idle':
      return 'default';
    case 'error':
      return 'error';
    default:
      return 'primary';
  }
};

const getStatusIcon = (status) => {
  switch (status?.toLowerCase()) {
    case 'running':
      return <CheckCircleIcon fontSize="small" />;
    case 'idle':
      return <PauseCircleIcon fontSize="small" />;
    case 'error':
      return <ErrorIcon fontSize="small" />;
    default:
      return null;
  }
};

const ScraperList = () => {
  const [scrapers, setScrapers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [monitoringDialogOpen, setMonitoringDialogOpen] = useState(false);
  const [selectedScraperId, setSelectedScraperId] = useState(null);
  const [refreshTrigger, setRefreshTrigger] = useState(0);
  const navigate = useNavigate();
  
  // New states for inline configuration
  const [configOpen, setConfigOpen] = useState(false);
  const [selectedScraper, setSelectedScraper] = useState(null);
  const [configLoading, setConfigLoading] = useState(false);
  const [configError, setConfigError] = useState(null);

  useEffect(() => {
    const loadScrapers = async () => {
      try {
        setLoading(true);
        setError(null);
        console.log('ScraperList: Loading scrapers...');

        // Make direct fetch to API with proper headers
        try {
          console.log('ScraperList: Making direct fetch to API...');
          const directResponse = await fetch('/api/scraper', {
            headers: {
              'Accept': 'application/json'
            }
          });
          console.log('ScraperList: Direct API response status:', directResponse.status);

          if (directResponse.ok) {
            const responseText = await directResponse.text();
            console.log('ScraperList: Direct API response text:', responseText);

            // Check if we got HTML instead of JSON
            if (responseText.includes('<!DOCTYPE html>')) {
              console.error('ScraperList: Received HTML instead of JSON');
              throw new Error('Received HTML instead of JSON. API proxy issue detected.');
            }

            try {
              // Try to parse the response as JSON
              const responseData = JSON.parse(responseText);
              console.log('ScraperList: Direct API response parsed:', responseData);

              // If we got valid data, use it directly
              if (Array.isArray(responseData) && responseData.length > 0) {
                console.log('ScraperList: Using direct API response data');

                // Normalize the data to ensure all properties are in camelCase
                const normalizedScrapers = responseData.map(scraper => {
                  // Create a new object with camelCase keys
                  const normalizedScraper = {};
                  Object.keys(scraper).forEach(key => {
                    // Convert first character to lowercase (PascalCase to camelCase)
                    const camelCaseKey = key.charAt(0).toLowerCase() + key.slice(1);
                    normalizedScraper[camelCaseKey] = scraper[key];
                  });

                  // Ensure required properties exist
                  normalizedScraper.id = normalizedScraper.id || scraper.Id;
                  normalizedScraper.name = normalizedScraper.name || scraper.Name || scraper.baseUrl || scraper.BaseUrl;
                  normalizedScraper.status = normalizedScraper.status || 'idle';
                  normalizedScraper.baseUrl = normalizedScraper.baseUrl || scraper.BaseUrl || '';
                  normalizedScraper.lastRun = normalizedScraper.lastRun || scraper.LastRun || null;
                  normalizedScraper.pagesCrawled = normalizedScraper.pagesCrawled || scraper.PagesCrawled || 0;

                  // Ensure monitoring object exists
                  normalizedScraper.monitoring = normalizedScraper.monitoring || {
                    enabled: normalizedScraper.enableContinuousMonitoring || false
                  };

                  console.log('Normalized scraper:', normalizedScraper);
                  return normalizedScraper;
                });

                console.log('All normalized scrapers:', normalizedScrapers);
                setScrapers(normalizedScrapers);
                setLoading(false);
                return;
              }
            } catch (parseError) {
              console.error('ScraperList: Error parsing direct API response:', parseError);
            }
          }
        } catch (directFetchError) {
          console.error('ScraperList: Error making direct API request:', directFetchError);
        }

        // Continue with normal flow if direct fetch didn't work
        const data = await fetchAllScrapers();
        console.log('ScraperList: Received scrapers data:', data);

        if (!data || (Array.isArray(data) && data.length === 0)) {
          console.log('ScraperList: No scrapers found or empty array returned');
          setScrapers([]);
        } else {
          setScrapers(data);
        }
      } catch (err) {
        console.error('ScraperList: Error loading scrapers:', err);
        setError(`Failed to load scrapers: ${err.message}. Please check the API connection and try again.`);
        setScrapers([]); // Set empty array to avoid undefined errors
      } finally {
        setLoading(false);
      }
    };

    console.log('ScraperList: Triggering loadScrapers, refreshTrigger =', refreshTrigger);
    loadScrapers();

    // Set up a timer to retry loading if it fails
    const retryTimer = setTimeout(() => {
      if (error) {
        console.log('ScraperList: Retrying loadScrapers due to previous error');
        loadScrapers();
      }
    }, 5000); // Retry after 5 seconds if there was an error

    return () => clearTimeout(retryTimer);
  }, [refreshTrigger, error]);

  const handleDeleteClick = (id) => {
    setSelectedScraperId(id);
    setDeleteDialogOpen(true);
  };

  const handleConfirmDelete = async () => {
    try {
      await deleteScraper(selectedScraperId);
      setDeleteDialogOpen(false);
      // Close config section if the deleted scraper was being configured
      if (selectedScraper && selectedScraper.id === selectedScraperId) {
        setConfigOpen(false);
        setSelectedScraper(null);
      }
      setRefreshTrigger(prev => prev + 1);
    } catch (err) {
      console.error('Error deleting scraper:', err);
      setError('Failed to delete scraper. Please try again later.');
      setDeleteDialogOpen(false);
    }
  };

  const handleMonitoringClick = (id) => {
    setSelectedScraperId(id);
    setMonitoringDialogOpen(true);
  };

  const handleMonitoringSave = () => {
    setMonitoringDialogOpen(false);
    setRefreshTrigger(prev => prev + 1); // Refresh list to show updated monitoring status
  };

  // New function to handle edit button click
  const handleEditClick = async (id) => {
    try {
      setConfigLoading(true);
      setConfigError(null);
      console.log('Fetching configuration for scraper ID:', id);
      
      const scraperConfig = await fetchScraper(id);
      console.log('Fetched scraper config:', scraperConfig);
      
      setSelectedScraper(scraperConfig);
      setConfigOpen(true);
    } catch (err) {
      console.error('Error fetching scraper configuration:', err);
      setConfigError(`Failed to load configuration: ${err.message}`);
    } finally {
      setConfigLoading(false);
    }
  };

  // New function to handle config save
  const handleConfigSave = async (updatedConfig) => {
    try {
      setConfigLoading(true);
      console.log('Saving updated configuration:', updatedConfig);
      
      await updateScraper(selectedScraper.id, updatedConfig);
      setConfigOpen(false);
      setSelectedScraper(null);
      setRefreshTrigger(prev => prev + 1); // Refresh list to show updated data
    } catch (err) {
      console.error('Error saving configuration:', err);
      setConfigError(`Failed to save configuration: ${err.message}`);
    } finally {
      setConfigLoading(false);
    }
  };

  // New function to close configuration panel
  const handleConfigClose = () => {
    setConfigOpen(false);
    setSelectedScraper(null);
    setConfigError(null);
  };

  const formatLastRun = (timestamp) => {
    if (!timestamp) return 'Never';
    const date = new Date(timestamp);
    return date.toLocaleString();
  };

  return (
    <Box sx={{ mb: 4 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">My Web Scrapers</Typography>
        <Button
          variant="contained"
          color="primary"
          startIcon={<AddIcon />}
          component={RouterLink}
          to="/configure"
        >
          New Scraper
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : scrapers.length === 0 ? (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Typography variant="h6" sx={{ mb: 2 }}>No scrapers configured yet</Typography>
          <Typography variant="body1" sx={{ mb: 3 }}>
            Get started by creating your first web scraper
          </Typography>
          <Button
            variant="contained"
            color="primary"
            startIcon={<AddIcon />}
            component={RouterLink}
            to="/configure"
          >
            Create Your First Scraper
          </Button>
        </Paper>
      ) : (
        <Grid container spacing={3}>
          {scrapers.map((scraper) => (
            <Grid item xs={12} sm={6} md={4} key={scraper.id}>
              <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
                <CardContent sx={{ flexGrow: 1 }}>
                  <Typography variant="h6" component="div" noWrap gutterBottom>
                    {scraper.name || scraper.baseUrl}
                  </Typography>

                  <Box sx={{ mb: 2 }}>
                    <Chip
                      size="small"
                      color={getStatusColor(scraper.status)}
                      label={scraper.status || 'Unknown'}
                      icon={getStatusIcon(scraper.status)}
                      sx={{ mr: 1, mb: 1 }}
                    />
                    {scraper.monitoring?.enabled && (
                      <Chip
                        size="small"
                        color="info"
                        icon={<MonitorHeartIcon fontSize="small" />}
                        label="Monitoring"
                        sx={{ mr: 1, mb: 1 }}
                      />
                    )}
                  </Box>

                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                    <strong>URL:</strong> {scraper.baseUrl}
                  </Typography>

                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                    <strong>Last Run:</strong> {formatLastRun(scraper.lastRun)}
                  </Typography>

                  {scraper.pagesCrawled > 0 && (
                    <Typography variant="body2" color="text.secondary">
                      <strong>Pages Crawled:</strong> {scraper.pagesCrawled.toLocaleString()}
                    </Typography>
                  )}
                </CardContent>

                <CardActions>
                  <Button
                    size="small"
                    startIcon={<DashboardIcon />}
                    component={RouterLink}
                    to={`/dashboard/${scraper.id}`}
                  >
                    Dashboard
                  </Button>

                  <Button
                    size="small"
                    startIcon={<MonitorHeartIcon />}
                    onClick={() => handleMonitoringClick(scraper.id)}
                  >
                    Monitoring
                  </Button>

                  <Box sx={{ flexGrow: 1 }} />

                  <Tooltip title="Edit">
                    <IconButton
                      size="small"
                      onClick={() => handleEditClick(scraper.id)}
                    >
                      <EditIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>

                  <Tooltip title="Delete">
                    <IconButton
                      size="small"
                      onClick={() => handleDeleteClick(scraper.id)}
                      color="error"
                    >
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </CardActions>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}

      {/* Configuration Section */}
      <Collapse in={configOpen} sx={{ mt: 4 }}>
        <Paper sx={{ p: 3, mb: 4 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h5">
              Edit Scraper Configuration
            </Typography>
            <IconButton onClick={handleConfigClose}>
              <CloseIcon />
            </IconButton>
          </Box>
          
          <Divider sx={{ mb: 3 }} />
          
          {configError && (
            <Alert severity="error" sx={{ mb: 3 }}>
              {configError}
            </Alert>
          )}
          
          {configLoading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
              <CircularProgress />
            </Box>
          ) : selectedScraper && (
            <ScraperConfigForm 
              initialConfig={selectedScraper} 
              onSave={handleConfigSave}
              loading={configLoading}
            />
          )}
        </Paper>
      </Collapse>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Delete Scraper</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete this scraper? This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleConfirmDelete} color="error" autoFocus>
            Delete
          </Button>
        </DialogActions>
      </Dialog>

      {/* Monitoring Settings Dialog */}
      {selectedScraperId && (
        <MonitoringDialog
          open={monitoringDialogOpen}
          onClose={() => setMonitoringDialogOpen(false)}
          onSave={handleMonitoringSave}
          scraperId={selectedScraperId}
          initialValues={scrapers.find(s => s.id === selectedScraperId)?.monitoring}
        />
      )}
    </Box>
  );
};

export default ScraperList;