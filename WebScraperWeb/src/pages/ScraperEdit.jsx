import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Container,
  Box,
  Typography,
  TextField,
  Button,
  Paper,
  Grid,
  FormControlLabel,
  Switch,
  Tabs,
  Tab,
  Divider,
  Alert,
  CircularProgress,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import SaveIcon from '@mui/icons-material/Save';
import { useScrapers } from '../hooks';

// Tab Panel component
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

const ScraperEdit = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { getScraper, updateScraper } = useScrapers();
  
  const [scraper, setScraper] = useState(null);
  const [activeTab, setActiveTab] = useState(0);
  const [errors, setErrors] = useState({});
  const [isSaving, setIsSaving] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [alert, setAlert] = useState({ show: false, message: '', severity: 'success' });

  // Load scraper data
  useEffect(() => {
    const fetchScraper = async () => {
      try {
        setIsLoading(true);
        const scraperData = await getScraper(id);
        
        // Check if we got a valid response - prevent infinite loop on error
        if (!scraperData) {
          setAlert({
            show: true,
            message: `Could not load scraper with ID: ${id}. The server may be experiencing issues.`,
            severity: 'error'
          });
          return;
        }
        
        setScraper(scraperData);
      } catch (error) {
        console.error('Error fetching scraper:', error);
        setAlert({
          show: true,
          message: `Error loading scraper: ${error.message || 'Unknown error'}`,
          severity: 'error'
        });
      } finally {
        setIsLoading(false);
      }
    };

    if (id) {
      fetchScraper();
    }
  }, [id, getScraper]);

  // Handle tab change
  const handleTabChange = (event, newValue) => {
    // Validate current tab before allowing user to proceed
    if (newValue > activeTab) {
      const isValid = validateCurrentTab(activeTab);
      if (!isValid) return; // Don't change tabs if current tab isn't valid
    }
    setActiveTab(newValue);
  };

  // Handle input change
  const handleChange = (field, value) => {
    setScraper({
      ...scraper,
      [field]: value
    });

    // Clear error for this field if any
    if (errors[field]) {
      setErrors({
        ...errors,
        [field]: ''
      });
    }
  };

  // Validate current tab
  const validateCurrentTab = (tabIndex) => {
    if (!scraper) return true;
    
    const newErrors = {};
    let isValid = true;

    switch (tabIndex) {
      case 0: // Basic Settings
        if (!scraper.name.trim()) {
          newErrors.name = 'Name is required';
          isValid = false;
        }

        if (!scraper.startUrl.trim()) {
          newErrors.startUrl = 'Start URL is required';
          isValid = false;
        } else if (!isValidUrl(scraper.startUrl)) {
          newErrors.startUrl = 'Please enter a valid URL';
          isValid = false;
        }

        if (!scraper.baseUrl.trim()) {
          // Try to derive from startUrl
          if (scraper.startUrl && isValidUrl(scraper.startUrl)) {
            try {
              const url = new URL(scraper.startUrl);
              setScraper(prev => ({
                ...prev,
                baseUrl: `${url.protocol}//${url.hostname}`
              }));
            } catch (e) {
              newErrors.baseUrl = 'Base URL is required';
              isValid = false;
            }
          } else {
            newErrors.baseUrl = 'Base URL is required';
            isValid = false;
          }
        } else if (!isValidUrl(scraper.baseUrl)) {
          newErrors.baseUrl = 'Please enter a valid URL';
          isValid = false;
        }
        break;

      case 1: // Crawling Options
        if (scraper.maxDepth <= 0) {
          newErrors.maxDepth = 'Max depth must be a positive number';
          isValid = false;
        }

        if (scraper.maxPages <= 0) {
          newErrors.maxPages = 'Max pages must be a positive number';
          isValid = false;
        }

        if (scraper.maxConcurrentRequests <= 0) {
          newErrors.maxConcurrentRequests = 'Max concurrent requests must be a positive number';
          isValid = false;
        }

        if (scraper.delayBetweenRequests < 0) {
          newErrors.delayBetweenRequests = 'Delay between requests cannot be negative';
          isValid = false;
        }
        break;

      // No validation needed for tab 2 (Advanced Features) as all fields have sensible defaults
      default:
        break;
    }

    setErrors(newErrors);
    return isValid;
  };

  // Check if valid URL
  const isValidUrl = (url) => {
    try {
      new URL(url);
      return true;
    } catch (e) {
      return false;
    }
  };

  // Validate form
  const validateForm = () => {
    if (!scraper) return false;
    
    // Validate all tabs
    const isTab0Valid = validateCurrentTab(0);
    const isTab1Valid = validateCurrentTab(1);

    // If any tab is invalid, switch to the first invalid tab
    if (!isTab0Valid) {
      setActiveTab(0);
      return false;
    }

    if (!isTab1Valid) {
      setActiveTab(1);
      return false;
    }

    return true;
  };

  // Handle submit
  const handleSubmit = async (event) => {
    event.preventDefault();

    if (!validateForm()) {
      return;
    }

    try {
      setIsSaving(true);

      // Debug: Log submission data
      console.log('Submitting updated scraper data:', scraper);

      // Ensure baseUrl is set if empty but startUrl is valid
      if (!scraper.baseUrl && scraper.startUrl && isValidUrl(scraper.startUrl)) {
        try {
          const url = new URL(scraper.startUrl);
          scraper.baseUrl = `${url.protocol}//${url.hostname}`;
        } catch (e) {
          // Invalid URL, will be caught by validation above
        }
      }

      // Update the scraper
      const updatedScraper = await updateScraper(id, scraper);

      setAlert({
        show: true,
        message: 'Scraper updated successfully',
        severity: 'success'
      });

      // Redirect to scraper detail page after a short delay
      setTimeout(() => {
        navigate(`/scrapers/${id}`);
      }, 1500);
    } catch (error) {
      console.error('Error updating scraper:', error);
      setAlert({
        show: true,
        message: `Error updating scraper: ${error.message || 'Unknown error'}`,
        severity: 'error'
      });
    } finally {
      setIsSaving(false);
    }
  };

  if (isLoading) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (!scraper) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Alert severity="error">
          Could not find scraper with ID: {id}
        </Alert>
        <Box sx={{ mt: 2 }}>
          <Button
            variant="outlined"
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate('/scrapers')}
          >
            Back to Scrapers
          </Button>
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      {/* Back Button */}
      <Box sx={{ mb: 3 }}>
        <Button
          variant="outlined"
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate(`/scrapers/${id}`)}
        >
          Back to Scraper Details
        </Button>
      </Box>

      {/* Header */}
      <Typography variant="h4" gutterBottom>
        Edit Scraper
      </Typography>
      <Typography variant="body1" color="textSecondary" sx={{ mb: 4 }}>
        Update your scraper's configuration
      </Typography>

      {/* Alert */}
      {alert.show && (
        <Alert 
          severity={alert.severity} 
          sx={{ mb: 3 }}
          onClose={() => setAlert({ ...alert, show: false })}
        >
          {alert.message}
        </Alert>
      )}

      {/* Form */}
      <Paper sx={{ p: 3, mb: 4 }}>
        <form onSubmit={handleSubmit}>
          {/* Tabs */}
          <Box sx={{ mb: 3 }}>
            <Tabs
              value={activeTab}
              onChange={handleTabChange}
              indicatorColor="primary"
              textColor="primary"
              variant="scrollable"
              scrollButtons="auto"
            >
              <Tab label="Basic Settings" />
              <Tab label="Crawling Options" />
              <Tab label="Advanced Features" />
            </Tabs>
          </Box>

          {/* Tab Panels */}
          <Box sx={{ mb: 3 }}>
            {/* Basic Settings Tab */}
            <TabPanel value={activeTab} index={0}>
              <Grid container spacing={3}>
                <Grid item xs={12}>
                  <TextField
                    label="Scraper Name"
                    fullWidth
                    required
                    value={scraper.name}
                    onChange={(e) => handleChange('name', e.target.value)}
                    error={!!errors.name}
                    helperText={errors.name || 'Give your scraper a descriptive name'}
                  />
                </Grid>

                <Grid item xs={12}>
                  <TextField
                    label="Start URL"
                    fullWidth
                    required
                    value={scraper.startUrl}
                    onChange={(e) => handleChange('startUrl', e.target.value)}
                    error={!!errors.startUrl}
                    helperText={errors.startUrl || 'The URL where scraping will begin'}
                    placeholder="https://example.com/start-page"
                  />
                </Grid>

                <Grid item xs={12}>
                  <TextField
                    label="Base URL"
                    fullWidth
                    required
                    value={scraper.baseUrl}
                    onChange={(e) => handleChange('baseUrl', e.target.value)}
                    error={!!errors.baseUrl}
                    helperText={errors.baseUrl || 'The base domain to constrain scraping'}
                    placeholder="https://example.com"
                  />
                </Grid>

                <Grid item xs={12}>
                  <TextField
                    label="Output Directory"
                    fullWidth
                    value={scraper.outputDirectory}
                    onChange={(e) => handleChange('outputDirectory', e.target.value)}
                    helperText="Directory where scraped data will be stored"
                  />
                </Grid>
              </Grid>
            </TabPanel>

            {/* Crawling Options Tab */}
            <TabPanel value={activeTab} index={1}>
              <Grid container spacing={3}>
                <Grid item xs={12} md={4}>
                  <TextField
                    label="Max Crawl Depth"
                    fullWidth
                    type="number"
                    inputProps={{ min: 1, max: 100 }}
                    value={scraper.maxDepth}
                    onChange={(e) => handleChange('maxDepth', parseInt(e.target.value) || 1)}
                    error={!!errors.maxDepth}
                    helperText={errors.maxDepth || "Maximum depth of links to follow from start URL"}
                  />
                </Grid>

                <Grid item xs={12} md={4}>
                  <TextField
                    label="Max Pages"
                    fullWidth
                    type="number"
                    inputProps={{ min: 1 }}
                    value={scraper.maxPages}
                    onChange={(e) => handleChange('maxPages', parseInt(e.target.value) || 1)}
                    error={!!errors.maxPages}
                    helperText={errors.maxPages || "Maximum number of pages to scrape"}
                  />
                </Grid>

                <Grid item xs={12} md={4}>
                  <TextField
                    label="Max Concurrent Requests"
                    fullWidth
                    type="number"
                    inputProps={{ min: 1, max: 20 }}
                    value={scraper.maxConcurrentRequests}
                    onChange={(e) => handleChange('maxConcurrentRequests', parseInt(e.target.value) || 1)}
                    error={!!errors.maxConcurrentRequests}
                    helperText={errors.maxConcurrentRequests || "Number of simultaneous requests"}
                  />
                </Grid>

                <Grid item xs={12} md={4}>
                  <TextField
                    label="Delay Between Requests (ms)"
                    fullWidth
                    type="number"
                    inputProps={{ min: 0 }}
                    value={scraper.delayBetweenRequests}
                    onChange={(e) => handleChange('delayBetweenRequests', parseInt(e.target.value) || 0)}
                    error={!!errors.delayBetweenRequests}
                    helperText={errors.delayBetweenRequests || "Milliseconds to wait between requests"}
                  />
                </Grid>

                <Grid item xs={12}>
                  <Divider sx={{ my: 2 }} />
                  <Typography variant="h6" gutterBottom>URL Handling</Typography>
                </Grid>

                <Grid item xs={12} md={4}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={scraper.followLinks}
                        onChange={(e) => handleChange('followLinks', e.target.checked)}
                      />
                    }
                    label="Follow Internal Links"
                  />
                </Grid>

                <Grid item xs={12} md={4}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={scraper.followExternalLinks}
                        onChange={(e) => handleChange('followExternalLinks', e.target.checked)}
                      />
                    }
                    label="Follow External Links"
                  />
                </Grid>

                <Grid item xs={12} md={4}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={scraper.respectRobotsTxt}
                        onChange={(e) => handleChange('respectRobotsTxt', e.target.checked)}
                      />
                    }
                    label="Respect robots.txt"
                  />
                </Grid>
              </Grid>
            </TabPanel>

            {/* Advanced Features Tab */}
            <TabPanel value={activeTab} index={2}>
              <Grid container spacing={3}>
                <Grid item xs={12}>
                  <Typography variant="h6" gutterBottom>
                    Content Change Detection
                  </Typography>
                </Grid>

                <Grid item xs={12} md={6}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={scraper.enableChangeDetection}
                        onChange={(e) => handleChange('enableChangeDetection', e.target.checked)}
                      />
                    }
                    label="Enable Change Detection"
                  />
                  <Typography variant="body2" color="textSecondary" sx={{ ml: 3 }}>
                    Detects meaningful changes in content between scrapes
                  </Typography>
                </Grid>

                <Grid item xs={12} md={6}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={scraper.trackContentVersions}
                        onChange={(e) => handleChange('trackContentVersions', e.target.checked)}
                      />
                    }
                    label="Track Content Versions"
                  />
                  <Typography variant="body2" color="textSecondary" sx={{ ml: 3 }}>
                    Keep track of content changes over time
                  </Typography>
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Max Versions to Keep"
                    fullWidth
                    type="number"
                    inputProps={{ min: 1, max: 20 }}
                    value={scraper.maxVersionsToKeep}
                    onChange={(e) => handleChange('maxVersionsToKeep', parseInt(e.target.value) || 1)}
                    disabled={!scraper.trackContentVersions}
                    helperText="Maximum number of content versions to store"
                  />
                </Grid>

                <Grid item xs={12}>
                  <Divider sx={{ my: 2 }} />
                  <Typography variant="h6" gutterBottom>
                    Adaptive Crawling
                  </Typography>
                </Grid>

                <Grid item xs={12} md={6}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={scraper.enableAdaptiveCrawling}
                        onChange={(e) => handleChange('enableAdaptiveCrawling', e.target.checked)}
                      />
                    }
                    label="Enable Adaptive Crawling"
                  />
                  <Typography variant="body2" color="textSecondary" sx={{ ml: 3 }}>
                    Intelligently adjust crawling behavior based on content value
                  </Typography>
                </Grid>

                <Grid item xs={12} md={6}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={scraper.adjustDepthBasedOnQuality}
                        onChange={(e) => handleChange('adjustDepthBasedOnQuality', e.target.checked)}
                      />
                    }
                    label="Adjust Depth Based on Quality"
                    disabled={!scraper.enableAdaptiveCrawling}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Priority Queue Size"
                    fullWidth
                    type="number"
                    inputProps={{ min: 10, max: 500 }}
                    value={scraper.priorityQueueSize}
                    onChange={(e) => handleChange('priorityQueueSize', parseInt(e.target.value) || 10)}
                    disabled={!scraper.enableAdaptiveCrawling}
                    helperText="Size of priority queue for adaptive crawling"
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={scraper.enableAdaptiveRateLimiting}
                        onChange={(e) => handleChange('enableAdaptiveRateLimiting', e.target.checked)}
                      />
                    }
                    label="Enable Adaptive Rate Limiting"
                  />
                  <Typography variant="body2" color="textSecondary" sx={{ ml: 3 }}>
                    Automatically adjust request rates based on server response
                  </Typography>
                </Grid>
              </Grid>
            </TabPanel>
          </Box>

          {/* Form Actions */}
          <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
            <Button
              variant="outlined"
              onClick={() => navigate(`/scrapers/${id}`)}
              disabled={isSaving}
            >
              Cancel
            </Button>

            <Box sx={{ display: 'flex', gap: 2 }}>
              {activeTab > 0 && (
                <Button
                  variant="outlined"
                  onClick={() => setActiveTab(activeTab - 1)}
                  disabled={isSaving}
                >
                  Previous
                </Button>
              )}

              {activeTab < 2 ? (
                <Button
                  variant="contained"
                  color="primary"
                  onClick={() => {
                    if (validateCurrentTab(activeTab)) {
                      setActiveTab(activeTab + 1);
                    }
                  }}
                  disabled={isSaving}
                >
                  Next
                </Button>
              ) : (
                <Button
                  type="submit"
                  variant="contained"
                  color="primary"
                  startIcon={isSaving ? <CircularProgress size={24} color="inherit" /> : <SaveIcon />}
                  disabled={isSaving}
                >
                  Save Changes
                </Button>
              )}
            </Box>
          </Box>
        </form>
      </Paper>
    </Container>
  );
};

export default ScraperEdit;