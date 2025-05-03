import { useState, useEffect } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
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
  CircularProgress,
  Alert,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import SaveIcon from '@mui/icons-material/Save';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getScraper, updateScraper } from '../api/scrapers';

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

const ScraperEdit = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  // Local state
  const [activeTab, setActiveTab] = useState(0);
  const [formData, setFormData] = useState(null);
  const [errors, setErrors] = useState({});
  const [alert, setAlert] = useState({ show: false, message: '', severity: 'info' });

  // Fetch scraper data with React Query
  const {
    data: scraper,
    isLoading,
    error,
    isError
  } = useQuery({
    queryKey: ['scraper', id],
    queryFn: () => getScraper(id),
    staleTime: 60 * 1000, // 1 minute
    refetchOnWindowFocus: false,
    retry: 1,
    enabled: !!id
  });

  // Initialize form data when scraper data is loaded
  useEffect(() => {
    if (scraper) {
      // Ensure arrays are properly initialized
      const processedData = {
        ...scraper,
        startUrls: Array.isArray(scraper.startUrls) ? scraper.startUrls : [],
        contentExtractorSelectors: Array.isArray(scraper.contentExtractorSelectors) ? scraper.contentExtractorSelectors : [],
        contentExtractorExcludeSelectors: Array.isArray(scraper.contentExtractorExcludeSelectors) ? scraper.contentExtractorExcludeSelectors : [],
        keywordAlertList: Array.isArray(scraper.keywordAlertList) ? scraper.keywordAlertList : [],
        webhookTriggers: Array.isArray(scraper.webhookTriggers) ? scraper.webhookTriggers : [],
        schedules: Array.isArray(scraper.schedules) ? scraper.schedules : []
      };

      console.log('Processed scraper data:', processedData);
      console.log('Initial StartUrl value:', processedData.startUrl);
      console.log('Initial StartUrl type:', typeof processedData.startUrl);

      // Make sure startUrl is not undefined or null
      if (!processedData.startUrl) {
        console.warn('StartUrl is missing in the initial data!');
        // If StartUrls array has values, use the first one
        if (Array.isArray(processedData.startUrls) && processedData.startUrls.length > 0) {
          processedData.startUrl = processedData.startUrls[0];
          console.log('Set startUrl from startUrls array:', processedData.startUrl);
        }
      }

      setFormData(processedData);
    }
  }, [scraper]);

  // Update scraper mutation
  const updateScraperMutation = useMutation({
    mutationFn: (updatedScraper) => updateScraper(id, updatedScraper),
    onSuccess: () => {
      // Invalidate and refetch
      queryClient.invalidateQueries({ queryKey: ['scraper', id] });
      queryClient.invalidateQueries({ queryKey: ['scrapers'] });

      setAlert({
        show: true,
        message: 'Scraper updated successfully',
        severity: 'success'
      });

      // Navigate back to scraper detail page after a short delay
      setTimeout(() => {
        navigate(`/scrapers/${id}`);
      }, 1500);
    },
    onError: (error) => {
      setAlert({
        show: true,
        message: `Error updating scraper: ${error.message || 'Unknown error'}`,
        severity: 'error'
      });
    }
  });

  // Handle input change
  const handleChange = (field, value) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));

    // Clear error for this field if any
    if (errors[field]) {
      setErrors(prev => ({
        ...prev,
        [field]: ''
      }));
    }
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

  // Validate current tab
  const validateCurrentTab = (tabIndex) => {
    if (!formData) return true;

    const newErrors = {};
    let isValid = true;

    switch (tabIndex) {
      case 0: // Basic Settings
        if (!formData.name?.trim()) {
          newErrors.name = 'Name is required';
          isValid = false;
        }

        if (!formData.startUrl?.trim()) {
          newErrors.startUrl = 'Start URL is required';
          isValid = false;
        } else if (!isValidUrl(formData.startUrl)) {
          newErrors.startUrl = 'Please enter a valid URL';
          isValid = false;
        }

        if (!formData.baseUrl?.trim()) {
          // Try to derive from startUrl
          if (formData.startUrl && isValidUrl(formData.startUrl)) {
            try {
              const url = new URL(formData.startUrl);
              setFormData(prev => ({
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
        } else if (!isValidUrl(formData.baseUrl)) {
          newErrors.baseUrl = 'Please enter a valid URL';
          isValid = false;
        }
        break;

      case 1: // Crawling Options
        if (formData.maxDepth <= 0) {
          newErrors.maxDepth = 'Max depth must be a positive number';
          isValid = false;
        }

        if (formData.maxPages <= 0) {
          newErrors.maxPages = 'Max pages must be a positive number';
          isValid = false;
        }

        if (formData.maxConcurrentRequests <= 0) {
          newErrors.maxConcurrentRequests = 'Max concurrent requests must be a positive number';
          isValid = false;
        }

        if (formData.delayBetweenRequests < 0) {
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

  // Validate form
  const validateForm = () => {
    if (!formData) return false;

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

  // Show loading state
  if (isLoading && !formData) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  // Show error state
  if (isError && !formData) {
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
          Error loading scraper details: {error?.message || 'Unknown error'}
        </Alert>
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
          component={Link}
          to={`/scrapers/${id}`}
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
        <form onSubmit={(e) => {
          e.preventDefault();
          if (validateForm()) {
            // Ensure arrays are properly initialized before submission
            const dataToSubmit = {
              ...formData,  // Include all properties from formData
              id: id,       // Ensure ID is set correctly
              // Use actual values from the form without defaults
              name: formData.name,
              startUrl: formData.startUrl,
              baseUrl: formData.baseUrl,
              // Ensure arrays are properly initialized
              startUrls: Array.isArray(formData.startUrls) ? [...formData.startUrls] : [],
              contentExtractorSelectors: Array.isArray(formData.contentExtractorSelectors) ? [...formData.contentExtractorSelectors] : [],
              contentExtractorExcludeSelectors: Array.isArray(formData.contentExtractorExcludeSelectors) ? [...formData.contentExtractorExcludeSelectors] : [],
              keywordAlertList: Array.isArray(formData.keywordAlertList) ? [...formData.keywordAlertList] : [],
              webhookTriggers: Array.isArray(formData.webhookTriggers) ? [...formData.webhookTriggers] : [],
              schedules: Array.isArray(formData.schedules) ? [...formData.schedules] : [],
              // Use actual values from the form without defaults
              outputDirectory: formData.outputDirectory,
              delayBetweenRequests: formData.delayBetweenRequests,
              maxConcurrentRequests: formData.maxConcurrentRequests,
              maxDepth: formData.maxDepth,
              maxPages: formData.maxPages,
              followLinks: formData.followLinks,
              followExternalLinks: formData.followExternalLinks,
              userAgent: formData.userAgent
            };

            console.log('Submitting data:', dataToSubmit);
            console.log('StartUrl value:', dataToSubmit.startUrl);
            console.log('StartUrl type:', typeof dataToSubmit.startUrl);
            console.log('StartUrl empty check:', !dataToSubmit.startUrl);
            console.log('StartUrl trim empty check:', !dataToSubmit.startUrl?.trim());

            // Make sure startUrl is not undefined or null
            if (!dataToSubmit.startUrl) {
              setAlert({
                show: true,
                message: 'Start URL is required',
                severity: 'error'
              });
              setActiveTab(0); // Switch to the Basic Settings tab
              setErrors(prev => ({
                ...prev,
                startUrl: 'Start URL is required'
              }));
              return;
            }

            updateScraperMutation.mutate(dataToSubmit);
          }
        }}>
          {/* Tabs */}
          <Box sx={{ mb: 3 }}>
            <Tabs
              value={activeTab}
              onChange={(_, newValue) => {
                if (newValue > activeTab) {
                  const isValid = validateCurrentTab(activeTab);
                  if (!isValid) return;
                }
                setActiveTab(newValue);
              }}
              indicatorColor="primary"
              textColor="primary"
              variant="scrollable"
              scrollButtons="auto"
            >
              <Tab label="Basic Settings" {...a11yProps(0)} />
              <Tab label="Crawling Options" {...a11yProps(1)} />
              <Tab label="Advanced Features" {...a11yProps(2)} />
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
                    value={formData?.name || ''}
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
                    value={formData?.startUrl || ''}
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
                    value={formData?.baseUrl || ''}
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
                    value={formData?.outputDirectory || ''}
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
                    value={formData?.maxDepth || 1}
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
                    value={formData?.maxPages || 100}
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
                    value={formData?.maxConcurrentRequests || 5}
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
                    value={formData?.delayBetweenRequests || 0}
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
                        checked={formData?.followLinks || false}
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
                        checked={formData?.followExternalLinks || false}
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
                        checked={formData?.respectRobotsTxt || false}
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
                        checked={formData?.enableChangeDetection || false}
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
                        checked={formData?.trackContentVersions || false}
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
                    value={formData?.maxVersionsToKeep || 5}
                    onChange={(e) => handleChange('maxVersionsToKeep', parseInt(e.target.value) || 1)}
                    disabled={!formData?.trackContentVersions}
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
                        checked={formData?.enableAdaptiveCrawling || false}
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
                        checked={formData?.adjustDepthBasedOnQuality || false}
                        onChange={(e) => handleChange('adjustDepthBasedOnQuality', e.target.checked)}
                      />
                    }
                    label="Adjust Depth Based on Quality"
                    disabled={!formData?.enableAdaptiveCrawling}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Priority Queue Size"
                    fullWidth
                    type="number"
                    inputProps={{ min: 10, max: 500 }}
                    value={formData?.priorityQueueSize || 100}
                    onChange={(e) => handleChange('priorityQueueSize', parseInt(e.target.value) || 10)}
                    disabled={!formData?.enableAdaptiveCrawling}
                    helperText="Size of priority queue for adaptive crawling"
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={formData?.enableAdaptiveRateLimiting || false}
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
              component={Link}
              to={`/scrapers/${id}`}
              disabled={updateScraperMutation.isPending}
            >
              Cancel
            </Button>

            <Box sx={{ display: 'flex', gap: 2 }}>
              {activeTab > 0 && (
                <Button
                  variant="outlined"
                  onClick={() => setActiveTab(activeTab - 1)}
                  disabled={updateScraperMutation.isPending}
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
                  disabled={updateScraperMutation.isPending}
                >
                  Next
                </Button>
              ) : (
                <Button
                  type="submit"
                  variant="contained"
                  color="primary"
                  startIcon={updateScraperMutation.isPending ? <CircularProgress size={24} color="inherit" /> : <SaveIcon />}
                  disabled={updateScraperMutation.isPending}
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