// src/pages/ScraperForm.jsx
import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Container, Box, Paper, Typography, TextField, Button, Grid,
  Divider, FormControlLabel, Switch, Tabs, Tab, Stepper,
  Step, StepLabel, StepContent, FormGroup, FormControl,
  InputLabel, Select, MenuItem, CircularProgress, Alert,
  Snackbar, Chip, IconButton, Tooltip, Link
} from '@mui/material';
import {
  Save as SaveIcon,
  Cancel as CancelIcon,
  Help as HelpIcon,
  ArrowBack as ArrowBackIcon,
  ArrowForward as ArrowForwardIcon
} from '@mui/icons-material';
import { getScraper, createScraper, updateScraper } from '../api/scrapers';

// Tab panel component
function TabPanel(props) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`scraper-form-tabpanel-${index}`}
      aria-labelledby={`scraper-form-tab-${index}`}
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

const defaultScraperConfig = {
  name: '',
  startUrl: '',
  baseUrl: '',
  outputDirectory: 'ScrapedData',
  delayBetweenRequests: 1000,
  maxConcurrentRequests: 5,
  maxDepth: 5,
  followExternalLinks: false,
  respectRobotsTxt: true,
  autoLearnHeaderFooter: true,
  learningPagesCount: 5,
  enableChangeDetection: true,
  trackContentVersions: true,
  maxVersionsToKeep: 5,
  enableAdaptiveCrawling: true,
  priorityQueueSize: 100,
  adjustDepthBasedOnQuality: true,
  enableAdaptiveRateLimiting: true,
  minDelayBetweenRequests: 500,
  maxDelayBetweenRequests: 5000,
  monitorResponseTimes: true,
  enableContinuousMonitoring: false,
  monitoringIntervalMinutes: 1440, // 24 hours
  notifyOnChanges: false,
  notificationEmail: '',
  trackChangesHistory: true,
  enableRegulatoryContentAnalysis: false,
  trackRegulatoryChanges: false,
  classifyRegulatoryDocuments: false,
  extractStructuredContent: false,
  processPdfDocuments: false,
  processOfficeDocuments: false,
  monitorHighImpactChanges: false,
  isUKGCWebsite: false,
  webhookEnabled: false,
  webhookUrl: '',
  webhookFormat: 'json'
};

const ScraperForm = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEditMode = !!id;
  
  const [activeTab, setActiveTab] = useState(0);
  const [scraper, setScraper] = useState(defaultScraperConfig);
  const [isLoading, setIsLoading] = useState(isEditMode);
  const [isSaving, setIsSaving] = useState(false);
  const [errors, setErrors] = useState({});
  const [snackbar, setSnackbar] = useState({
    open: false,
    message: '',
    severity: 'success'
  });
  const [validationErrors, setValidationErrors] = useState([]);
  
  // Fetch scraper details in edit mode
  useEffect(() => {
    if (isEditMode) {
      const fetchScraper = async () => {
        try {
          setIsLoading(true);
          const data = await getScraper(id);
          setScraper(data);
        } catch (error) {
          console.error('Error fetching scraper:', error);
          setSnackbar({
            open: true,
            message: 'Error loading scraper configuration',
            severity: 'error'
          });
        } finally {
          setIsLoading(false);
        }
      };
      
      fetchScraper();
    } else {
      // Handle URL prefilling in create mode
      const urlParams = new URLSearchParams(window.location.search);
      const startUrl = urlParams.get('url');
      if (startUrl) {
        try {
          const url = new URL(startUrl);
          setScraper({
            ...scraper,
            startUrl,
            baseUrl: `${url.protocol}//${url.hostname}`
          });
        } catch (e) {
          // Invalid URL, ignore
        }
      }
    }
  }, [id, isEditMode]);
  
  // When startUrl changes, update baseUrl suggestion if empty
  useEffect(() => {
    if (!scraper.baseUrl && scraper.startUrl) {
      try {
        const url = new URL(scraper.startUrl);
        setScraper({
          ...scraper,
          baseUrl: `${url.protocol}//${url.hostname}`
        });
      } catch (e) {
        // Invalid URL, ignore
      }
    }
  }, [scraper.startUrl]);
  
  // Handle tab change
  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
  };
  
  // Handle form field changes
  const handleChange = (field, value) => {
    setScraper({
      ...scraper,
      [field]: value
    });
    
    // Clear error for this field
    if (errors[field]) {
      setErrors({
        ...errors,
        [field]: null
      });
    }
  };
  
  // Validate form
  const validateForm = () => {
    const newErrors = {};
    const validationMessages = [];
    
    // Required fields
    if (!scraper.name) {
      newErrors.name = 'Name is required';
      validationMessages.push('Name is required');
    }
    
    if (!scraper.startUrl) {
      newErrors.startUrl = 'Start URL is required';
      validationMessages.push('Start URL is required');
    } else {
      try {
        new URL(scraper.startUrl);
      } catch (e) {
        newErrors.startUrl = 'Invalid URL format';
        validationMessages.push('Start URL has invalid format');
      }
    }
    
    if (!scraper.baseUrl) {
      newErrors.baseUrl = 'Base URL is required';
      validationMessages.push('Base URL is required');
    } else {
      try {
        new URL(scraper.baseUrl);
      } catch (e) {
        newErrors.baseUrl = 'Invalid URL format';
        validationMessages.push('Base URL has invalid format');
      }
    }
    
    // Numeric values
    if (scraper.delayBetweenRequests < 0) {
      newErrors.delayBetweenRequests = 'Must be a positive number';
      validationMessages.push('Delay between requests must be a positive number');
    }
    
    if (scraper.maxConcurrentRequests < 1) {
      newErrors.maxConcurrentRequests = 'Must be at least 1';
      validationMessages.push('Max concurrent requests must be at least 1');
    }
    
    if (scraper.maxDepth < 1) {
      newErrors.maxDepth = 'Must be at least 1';
      validationMessages.push('Max depth must be at least 1');
    }
    
    if (scraper.notifyOnChanges && !scraper.notificationEmail) {
      newErrors.notificationEmail = 'Email is required when notifications are enabled';
      validationMessages.push('Notification email is required when notifications are enabled');
    }
    
    if (scraper.webhookEnabled && !scraper.webhookUrl) {
      newErrors.webhookUrl = 'Webhook URL is required when webhooks are enabled';
      validationMessages.push('Webhook URL is required when webhooks are enabled');
    }
    
    setErrors(newErrors);
    setValidationErrors(validationMessages);
    
    return Object.keys(newErrors).length === 0;
  };
  
  // Handle form submission
  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!validateForm()) {
      setSnackbar({
        open: true,
        message: 'Please fix the validation errors before saving',
        severity: 'error'
      });
      return;
    }
    
    try {
      setIsSaving(true);
      
      if (isEditMode) {
        await updateScraper(id, scraper);
        setSnackbar({
          open: true,
          message: 'Scraper updated successfully',
          severity: 'success'
        });
      } else {
        const createdScraper = await createScraper(scraper);
        setSnackbar({
          open: true,
          message: 'Scraper created successfully',
          severity: 'success'
        });
        
        // Navigate to the edit page for the newly created scraper
        navigate(`/scrapers/${createdScraper.id}`);
      }
    } catch (error) {
      console.error('Error saving scraper:', error);
      setSnackbar({
        open: true,
        message: `Error ${isEditMode ? 'updating' : 'creating'} scraper: ${error.message || 'Unknown error'}`,
        severity: 'error'
      });
    } finally {
      setIsSaving(false);
    }
  };
  
  // Handle snackbar close
  const handleSnackbarClose = () => {
    setSnackbar({ ...snackbar, open: false });
  };
  
  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '80vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Paper sx={{ p: 3, mb: 4 }}>
        <Typography variant="h4" gutterBottom>
          {isEditMode ? 'Edit Scraper' : 'Create Scraper'}
        </Typography>
        
        <Divider sx={{ mb: 3 }} />
        
        <form onSubmit={handleSubmit}>
          {/* Main tabs */}
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
              <Tab label="Monitoring & Notifications" />
            </Tabs>
          </Box>
          
          {/* Tab panels */}
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
                    helperText={errors.baseUrl || 'The base domain to restrict scraping to'}
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
                
                <Grid item xs={12} md={6}>
                  <FormGroup>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={scraper.respectRobotsTxt}
                          onChange={(e) => handleChange('respectRobotsTxt', e.target.checked)}
                        />
                      }
                      label="Respect robots.txt rules"
                    />
                    <FormControlLabel
                      control={
                        <Switch
                          checked={scraper.followExternalLinks}
                          onChange={(e) => handleChange('followExternalLinks', e.target.checked)}
                        />
                      }
                      label="Follow external links"
                    />
                  </FormGroup>
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
                    onChange={(e) => handleChange('maxDepth', parseInt(e.target.value, 10))}
                    error={!!errors.maxDepth}
                    helperText={errors.maxDepth || 'Maximum link depth to follow from start URL'}
                  />
                </Grid>
                
                <Grid item xs={12} md={4}>
                  <TextField
                    label="Max Concurrent Requests"
                    fullWidth
                    type="number"
                    inputProps={{ min: 1, max: 20 }}
                    value={scraper.maxConcurrentRequests}
                    onChange={(e) => handleChange('maxConcurrentRequests', parseInt(e.target.value, 10))}
                    error={!!errors.maxConcurrentRequests}
                    helperText={errors.maxConcurrentRequests || 'How many URLs to process simultaneously'}
                  />
                </Grid>
                
                <Grid item xs={12} md={4}>
                  <TextField
                    label="Delay Between Requests (ms)"
                    fullWidth
                    type="number"
                    inputProps={{ min: 0, max: 10000 }}
                    value={scraper.delayBetweenRequests}
                    onChange={(e) => handleChange('delayBetweenRequests', parseInt(e.target.value, 10))}
                    error={!!errors.delayBetweenRequests}
                    helperText={errors.delayBetweenRequests || 'Delay between requests in milliseconds'}
                  />
                </Grid>
                
                <Grid item xs={12}>
                  <Typography variant="h6" gutterBottom>
                    Adaptive Crawling
                  </Typography>
                  <FormGroup>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={scraper.enableAdaptiveCrawling}
                          onChange={(e) => handleChange('enableAdaptiveCrawling', e.target.checked)}
                        />
                      }
                      label="Enable adaptive crawling"
                    />
                    <Typography variant="body2" color="textSecondary" sx={{ ml: 3, mb: 2 }}>
                      Adapts crawling strategy based on content relevance and link structure.
                    </Typography>
                    
                    <FormControlLabel
                      control={
                        <Switch
                          checked={scraper.adjustDepthBasedOnQuality}
                          onChange={(e) => handleChange('adjustDepthBasedOnQuality', e.target.checked)}
                          disabled={!scraper.enableAdaptiveCrawling}
                        />
                      }
                      label="Adjust depth based on content quality"
                    />
                  </FormGroup>
                </Grid>
                
                <Grid item xs={12}>
                  <Typography variant="h6" gutterBottom>
                    Rate Limiting
                  </Typography>
                  <FormGroup>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={scraper.enableAdaptiveRateLimiting}
                          onChange={(e) => handleChange('enableAdaptiveRateLimiting', e.target.checked)}
                        />
                      }
                      label="Enable adaptive rate limiting"
                    />
                    <Typography variant="body2" color="textSecondary" sx={{ ml: 3, mb: 2 }}>
                      Automatically adjusts request rate based on server response times and error rates.
                    </Typography>
                  </FormGroup>
                  
                  <Grid container spacing={3} sx={{ mt: 1 }}>
                    <Grid item xs={12} md={6}>
                      <TextField
                        label="Min Delay Between Requests (ms)"
                        fullWidth
                        type="number"
                        inputProps={{ min: 0, max: 10000 }}
                        value={scraper.minDelayBetweenRequests}
                        onChange={(e) => handleChange('minDelayBetweenRequests', parseInt(e.target.value, 10))}
                        disabled={!scraper.enableAdaptiveRateLimiting}
                        helperText="Minimum delay when adaptive rate limiting is enabled"
                      />
                    </Grid>
                    
                    <Grid item xs={12} md={6}>
                      <TextField
                        label="Max Delay Between Requests (ms)"
                        fullWidth
                        type="number"
                        inputProps={{ min: 1000, max: 60000 }}
                        value={scraper.maxDelayBetweenRequests}
                        onChange={(e) => handleChange('maxDelayBetweenRequests', parseInt(e.target.value, 10))}
                        disabled={!scraper.enableAdaptiveRateLimiting}
                        helperText="Maximum delay when adaptive rate limiting is enabled"
                      />
                    </Grid>
                  </Grid>
                </Grid>
              </Grid>
            </TabPanel>
            
            {/* Advanced Features Tab */}
            <TabPanel value={activeTab} index={2}>
              <Grid container spacing={3}>
                <Grid item xs={12}>
                  <Typography variant="h6" gutterBottom>
                    Content Processing
                  </Typography>
                  <FormGroup>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={scraper.extractStructuredContent}
                          onChange={(e) => handleChange('extractStructuredContent', e.target.checked)}
                        />
                      }
                      label="Extract structured content"
                    />
                    <Typography variant="body2" color="textSecondary" sx={{ ml: 3, mb: 2 }}>
                      Extracts structured content from HTML pages (headings, paragraphs, lists, etc.)
                    </Typography>
                  </FormGroup>
                </Grid>
                
                <Grid item xs={12}>
                  <Typography variant="h6" gutterBottom>
                    Document Processing
                  </Typography>
                  <FormGroup>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={scraper.processPdfDocuments}
                          onChange={(e) => handleChange('processPdfDocuments', e.target.checked)}
                        />
                      }
                      label="Process PDF documents"
                    />
                    <Typography variant="body2" color="textSecondary" sx={{ ml: 3, mb: 1 }}>
                      Downloads and extracts text from PDF documents found during crawling
                    </Typography>
                    
                    <FormControlLabel
                      control={
                        <Switch
                          checked={scraper.processOfficeDocuments}
                          onChange={(e) => handleChange('processOfficeDocuments', e.target.checked)}
                        />
                      }
                      label="Process Office documents"
                    />
                    <Typography variant="body2" color="textSecondary" sx={{ ml: 3, mb: 2 }}>
                      Downloads and extracts text from Word, Excel, and PowerPoint documents
                    </Typography>
                  </FormGroup>
                </Grid>
                
                <Grid item xs={12}>
                  <Typography variant="h6" gutterBottom>
                    Change Detection
                  </Typography>
                  <FormGroup>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={scraper.enableChangeDetection}
                          onChange={(e) => handleChange('enableChangeDetection', e.target.checked)}
                        />
                      }
                      label="Enable content change detection"
                    />
                    <Typography variant="body2" color="textSecondary" sx={{ ml: 3, mb: 1 }}>
                      Detects when content on previously visited pages has changed
                    </Typography>
                    
                    <FormControlLabel
                      control={
                        <Switch
                          checked={scraper.trackContentVersions}
                          onChange={(e) => handleChange('trackContentVersions', e.target.checked)}
                          disabled={!scraper.enableChangeDetection}
                        />
                      }
                      label="Track content versions"
                    />
                    <Typography variant="body2" color="textSecondary" sx={{ ml: 3, mb: 1 }}>
                      Maintains a history of content versions for each page
                    </Typography>
                    
                    <TextField
                      label="Max Versions to Keep"
                      type="number"
                      inputProps={{ min: 1, max: 100 }}
                      value={scraper.maxVersionsToKeep}
                      onChange={(e) => handleChange('maxVersionsToKeep', parseInt(e.target.value, 10))}
                      disabled={!scraper.enableChangeDetection || !scraper.trackContentVersions}
                      sx={{ ml: 3, mt: 1, width: '200px' }}
                    />
                  </FormGroup>
                </Grid>
                
                <Grid item xs={12}>
                  <Typography variant="h6" gutterBottom>
                    Regulatory Content Analysis
                  </Typography>
                  <FormGroup>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={scraper.enableRegulatoryContentAnalysis}
                          onChange={(e) => handleChange('enableRegulatoryContentAnalysis', e.target.checked)}
                        />
                      }
                      label="Enable regulatory content analysis"
                    />
                    <Typography variant="body2" color="textSecondary" sx={{ ml: 3, mb: 1 }}>
                      Identifies and classifies regulatory content in scraped pages
                    </Typography>
                    
                    <FormControlLabel
                      control={
                        <Switch
                          checked={scraper.trackRegulatoryChanges}
                          onChange={(e) => handleChange('trackRegulatoryChanges', e.target.checked)}
                          disabled={!scraper.enableRegulatoryContentAnalysis}
                        />
                      }
                      label="Track regulatory changes"
                    />
                    <Typography variant="body2" color="textSecondary" sx={{ ml: 3, mb: 1 }}>
                      Monitors changes specifically in regulatory content
                    </Typography>
                    
                    <FormControlLabel
                      control={
                        <Switch
                          checked={scraper.monitorHighImpactChanges}
                          onChange={(e) => handleChange('monitorHighImpactChanges', e.target.checked)}
                          disabled={!scraper.enableRegulatoryContentAnalysis || !scraper.trackRegulatoryChanges}
                        />
                      }
                      label="Monitor high-impact changes"
                    />
                    <Typography variant="body2" color="textSecondary" sx={{ ml: 3, mb: 1 }}>
                      Prioritizes detection of high-impact regulatory changes
                    </Typography>
                    
                    <FormControlLabel
                      control={
                        <Switch
                          checked={scraper.classifyRegulatoryDocuments}
                          onChange={(e) => handleChange('classifyRegulatoryDocuments', e.target.checked)}
                          disabled={!scraper.enableRegulatoryContentAnalysis}
                        />
                      }
                      label="Classify regulatory documents"
                    />
                    <Typography variant="body2" color="textSecondary" sx={{ ml: 3, mb: 1 }}>
                      Classifies documents by regulatory category
                    </Typography>
                  </FormGroup>
                </Grid>
                
                <Grid item xs={12}>
                  <Typography variant="h6" gutterBottom>
                    Special Site Settings
                  </Typography>
                  <FormGroup>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={scraper.isUKGCWebsite}
                          onChange={(e) => handleChange('isUKGCWebsite', e.target.checked)}
                        />
                      }
                      label="UK Gambling Commission website"
                    />
                    <Typography variant="body2" color="textSecondary" sx={{ ml: 3, mb: 1 }}>
                      Enables specialized crawling optimized for UK Gambling Commission websites
                    </Typography>
                  </FormGroup>
                </Grid>
              </Grid>
            </TabPanel>
            
            {/* Monitoring & Notifications Tab */}
            <TabPanel value={activeTab} index={3}>
              <Grid container spacing={3}>
                <Grid item xs={12}>
                  <Typography variant="h6" gutterBottom>
                    Continuous Monitoring
                  </Typography>
                  <FormGroup>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={scraper.enableContinuousMonitoring}
                          onChange={(e) => handleChange('enableContinuousMonitoring', e.target.checked)}
                        />
                      }
                      label="Enable continuous monitoring"
                    />
                    <Typography variant="body2" color="textSecondary" sx={{ ml: 3, mb: 2 }}>
                      Periodically crawls the site to detect changes
                    </Typography>
                  </FormGroup>
                  
                  <FormControl 
                    fullWidth 
                    sx={{ mb: 3, maxWidth: 300 }}
                    disabled={!scraper.enableContinuousMonitoring}
                  >
                    <InputLabel id="monitoring-interval-label">Monitoring Interval</InputLabel>
                    <Select
                      labelId="monitoring-interval-label"
                      value={scraper.monitoringIntervalMinutes}
                      label="Monitoring Interval"
                      onChange={(e) => handleChange('monitoringIntervalMinutes', e.target.value)}
                    >
                      <MenuItem value={60}>Every hour</MenuItem>
                      <MenuItem value={180}>Every 3 hours</MenuItem>
                      <MenuItem value={360}>Every 6 hours</MenuItem>
                      <MenuItem value={720}>Every 12 hours</MenuItem>
                      <MenuItem value={1440}>Every day</MenuItem>
                      <MenuItem value={10080}>Every week</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
                
                <Grid item xs={12}>
                  <Typography variant="h6" gutterBottom>
                    Email Notifications
                  </Typography>
                  <FormGroup>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={scraper.notifyOnChanges}
                          onChange={(e) => handleChange('notifyOnChanges', e.target.checked)}
                        />
                      }
                      label="Send email notifications on content changes"
                    />
                    <Typography variant="body2" color="textSecondary" sx={{ ml: 3, mb: 2 }}>
                      Sends email notifications when significant content changes are detected
                    </Typography>
                  </FormGroup>
                  
                  <TextField
                    label="Notification Email"
                    fullWidth
                    value={scraper.notificationEmail}
                    onChange={(e) => handleChange('notificationEmail', e.target.value)}
                    disabled={!scraper.notifyOnChanges}
                    error={!!errors.notificationEmail}
                    helperText={errors.notificationEmail || 'Email address to send notifications to'}
                    sx={{ mb: 3, maxWidth: 500 }}
                  />
                </Grid>
                
                <Grid item xs={12}>
                  <Typography variant="h6" gutterBottom>
                    Webhook Notifications
                  </Typography>
                  <FormGroup>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={scraper.webhookEnabled}
                          onChange={(e) => handleChange('webhookEnabled', e.target.checked)}
                        />
                      }
                      label="Enable webhook notifications"
                    />
                    <Typography variant="body2" color="textSecondary" sx={{ ml: 3, mb: 2 }}>
                      Sends HTTP notifications to a specified endpoint when events occur
                    </Typography>
                  </FormGroup>
                  
                  <TextField
                    label="Webhook URL"
                    fullWidth
                    value={scraper.webhookUrl}
                    onChange={(e) => handleChange('webhookUrl', e.target.value)}
                    disabled={!scraper.webhookEnabled}
                    error={!!errors.webhookUrl}
                    helperText={errors.webhookUrl || 'URL to send webhook notifications to'}
                    sx={{ mb: 2, maxWidth: 500 }}
                  />
                  
                  <FormControl 
                    sx={{ mb: 3, minWidth: 200 }}
                    disabled={!scraper.webhookEnabled}
                  >
                    <InputLabel id="webhook-format-label">Webhook Format</InputLabel>
                    <Select
                      labelId="webhook-format-label"
                      value={scraper.webhookFormat}
                      label="Webhook Format"
                      onChange={(e) => handleChange('webhookFormat', e.target.value)}
                    >
                      <MenuItem value="json">JSON</MenuItem>
                      <MenuItem value="form">Form Data</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
              </Grid>
            </TabPanel>
          </Box>
          
          {/* Validation Errors */}
          {validationErrors.length > 0 && (
            <Alert severity="error" sx={{ mb: 3 }}>
              <Typography variant="subtitle1" gutterBottom>
                Please fix the following errors:
              </Typography>
              <ul style={{ marginTop: 0, paddingLeft: 20 }}>
                {validationErrors.map((error, index) => (
                  <li key={index}>{error}</li>
                ))}
              </ul>
            </Alert>
          )}
          
          {/* Action Buttons */}
          <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
            <Button
              variant="outlined"
              color="secondary"
              startIcon={<CancelIcon />}
              onClick={() => navigate(isEditMode ? `/scrapers/${id}` : '/scrapers')}
              disabled={isSaving}
            >
              Cancel
            </Button>
            
            <Box sx={{ display: 'flex', gap: 2 }}>
              {activeTab > 0 && (
                <Button
                  variant="outlined"
                  startIcon={<ArrowBackIcon />}
                  onClick={() => setActiveTab(activeTab - 1)}
                >
                  Previous
                </Button>
              )}
              
              {activeTab < 3 && (
                <Button
                  variant="outlined"
                  endIcon={<ArrowForwardIcon />}
                  onClick={() => setActiveTab(activeTab + 1)}
                >
                  Next
                </Button>
              )}
              
              <Button
                type="submit"
                variant="contained"
                color="primary"
                startIcon={isSaving ? <CircularProgress size={24} color="inherit" /> : <SaveIcon />}
                disabled={isSaving}
              >
                {isEditMode ? 'Update Scraper' : 'Create Scraper'}
              </Button>
            </Box>
          </Box>
        </form>
      </Paper>
      
      {/* Help Box */}
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          <HelpIcon sx={{ verticalAlign: 'middle', mr: 1 }} />
          Tips & Information
        </Typography>
        
        <Box sx={{ pl: 4 }}>
          <Typography variant="body2" paragraph>
            <strong>Basic Settings:</strong> Configure the essential parameters for your scraper, including the starting URL and domain restrictions.
          </Typography>
          
          <Typography variant="body2" paragraph>
            <strong>Crawling Options:</strong> Control how the scraper navigates through the website, including depth limits and request rates.
          </Typography>
          
          <Typography variant="body2" paragraph>
            <strong>Advanced Features:</strong> Enable specialized features like document processing, content change detection, and regulatory content analysis.
          </Typography>
          
          <Typography variant="body2" paragraph>
            <strong>Monitoring & Notifications:</strong> Set up continuous monitoring and notifications for changes in the website content.
          </Typography>
          
          <Typography variant="body2">
            Need more help? Check out the <Link href="/documentation" target="_blank">documentation</Link> for detailed information.
          </Typography>
        </Box>
      </Paper>
      
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

export default ScraperForm;