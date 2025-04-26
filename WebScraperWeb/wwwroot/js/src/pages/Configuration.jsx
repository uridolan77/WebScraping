import React, { useState, useEffect } from 'react';
import {
  Typography,
  Paper,
  Grid,
  Button,
  Box,
  TextField,
  MenuItem,
  Switch,
  FormControlLabel,
  Divider,
  Alert,
  Stepper,
  Step,
  StepLabel,
  FormHelperText,
  InputAdornment,
  CircularProgress,
  Breadcrumbs,
  Link
} from '@mui/material';
import { Link as RouterLink, useParams, useNavigate } from 'react-router-dom';
import SaveIcon from '@mui/icons-material/Save';
import AddIcon from '@mui/icons-material/Add';
import HttpIcon from '@mui/icons-material/Http';
import SpeedIcon from '@mui/icons-material/Speed';
import FilterAltIcon from '@mui/icons-material/FilterAlt';
import HomeIcon from '@mui/icons-material/Home';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';

import { fetchScraper, createScraper, updateScraper } from '../services/api';

const Configuration = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEditMode = Boolean(id);

  // Form steps
  const steps = ['Basic Info', 'URL Settings', 'Advanced Options', 'Filtering Rules'];
  const [activeStep, setActiveStep] = useState(0);

  // Form state
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    startUrl: 'https://',
    baseUrl: '',
    maxDepth: 2,
    maxConcurrentRequests: 5,
    delayBetweenRequests: 1000,
    respectRobotsTxt: true,
    includeSubdomains: false,
    includePatterns: '',
    excludePatterns: '',
    downloadAssets: false,
    followExternalLinks: false,
    customHeaders: '',
    customCss: '',
    proxy: '',
    userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
    notificationEmail: '',
    id: ''
  });

  // UI state
  const [loading, setLoading] = useState(isEditMode);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(false);
  const [validation, setValidation] = useState({});

  // Load scraper data if in edit mode
  useEffect(() => {
    const loadScraper = async () => {
      try {
        setLoading(true);
        console.log('Configuration: Loading scraper data for ID:', id);
        console.log('isEditMode:', isEditMode);

        // The API now returns normalized data with camelCase properties
        const data = await fetchScraper(id);
        console.log('Received normalized scraper data:', data);

        // Update form with existing data
        setFormData(prevState => {
          const newState = {
            ...prevState,
            ...data
          };
          console.log('Updated form data:', newState);
          return newState;
        });

        setLoading(false);
      } catch (err) {
        console.error('Error loading scraper:', err);
        setError(`Failed to load scraper configuration: ${err.message}`);
        setLoading(false);
      }
    };

    if (isEditMode) {
      loadScraper();
    }
  }, [id, isEditMode]);

  // Handle form changes
  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    const newValue = type === 'checkbox' ? checked : value;

    setFormData(prevState => ({
      ...prevState,
      [name]: newValue
    }));

    // Clear validation error when field is changed
    if (validation[name]) {
      setValidation(prev => ({
        ...prev,
        [name]: null
      }));
    }

    // Clear success message when form is changed
    if (success) {
      setSuccess(false);
    }
  };

  // Validate the current step
  const validateStep = () => {
    const errors = {};

    if (activeStep === 0) {
      if (!formData.name) {
        errors.name = 'Scraper name is required';
      }
    } else if (activeStep === 1) {
      if (!formData.startUrl || formData.startUrl === 'https://') {
        errors.startUrl = 'Start URL is required';
      } else {
        try {
          new URL(formData.startUrl);
        } catch (e) {
          errors.startUrl = 'Please enter a valid URL';
        }
      }

      if (formData.baseUrl) {
        try {
          new URL(formData.baseUrl.startsWith('http') ? formData.baseUrl : `https://${formData.baseUrl}`);
        } catch (e) {
          errors.baseUrl = 'Please enter a valid URL or domain';
        }
      }
    } else if (activeStep === 2) {
      if (formData.maxDepth < 1) {
        errors.maxDepth = 'Depth must be at least 1';
      }
      if (formData.maxConcurrentRequests < 1) {
        errors.maxConcurrentRequests = 'Must be at least 1';
      }
      if (formData.delayBetweenRequests < 0) {
        errors.delayBetweenRequests = 'Cannot be negative';
      }

      // Validate custom headers as JSON if provided
      if (formData.customHeaders) {
        try {
          JSON.parse(formData.customHeaders);
        } catch (e) {
          errors.customHeaders = 'Invalid JSON format';
        }
      }
    }

    setValidation(errors);
    return Object.keys(errors).length === 0;
  };

  // Navigate between steps
  const handleNext = () => {
    if (validateStep()) {
      setActiveStep(prevStep => prevStep + 1);
    }
  };

  const handleBack = () => {
    setActiveStep(prevStep => prevStep - 1);
  };

  // Handle form submission
  const handleSubmit = async (e) => {
    e.preventDefault();

    // Final validation check across all steps
    for (let i = 0; i <= 3; i++) {
      setActiveStep(i);
      if (!validateStep()) {
        return;
      }
    }

    setSaving(true);
    setError(null);

    try {
      // Format form data for API
      const formattedData = {
        ...formData,
        includePatterns: formData.includePatterns ? formData.includePatterns.split('\\n').filter(p => p.trim()) : [],
        excludePatterns: formData.excludePatterns ? formData.excludePatterns.split('\\n').filter(p => p.trim()) : [],
        customHeaders: formData.customHeaders ? JSON.parse(formData.customHeaders) : null
      };

      // If baseUrl is empty, try to extract domain from startUrl
      if (!formattedData.baseUrl) {
        try {
          const url = new URL(formattedData.startUrl);
          formattedData.baseUrl = url.hostname;
        } catch (e) {
          // Ignore and leave baseUrl empty
        }
      }

      // Save scraper
      let result;

      try {
        console.log('Submitting scraper data:', formattedData);

        if (isEditMode) {
          // Update existing scraper
          result = await updateScraper(id, formattedData);
        } else {
          // Create new scraper
          result = await createScraper(formattedData);
        }

        setSaving(false);
        setSuccess(true);

        // Navigate to dashboard after successful save
        setTimeout(() => {
          navigate(isEditMode ? `/dashboard/${id}` : `/dashboard/${result.id}`);
        }, 1500);
      } catch (error) {
        console.error('Error saving scraper:', error);
        throw error; // Re-throw to be caught by the outer catch block
      }
    } catch (err) {
      setError(`Failed to save scraper: ${err.message}`);
      setSaving(false);
    }
  };

  // Render step content
  const renderStepContent = (step) => {
    switch (step) {
      case 0:
        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              Basic Information
            </Typography>

            <Grid container spacing={3}>
              {isEditMode && (
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Scraper ID"
                    name="id"
                    value={formData.id || ''}
                    InputProps={{
                      readOnly: true,
                    }}
                    helperText="Unique identifier for this scraper (read-only)"
                  />
                </Grid>
              )}

              <Grid item xs={12}>
                <TextField
                  fullWidth
                  required
                  label="Scraper Name"
                  name="name"
                  value={formData.name || ''}
                  onChange={handleChange}
                  error={Boolean(validation.name)}
                  helperText={validation.name}
                  placeholder="E.g. Company Blog Scraper"
                />
              </Grid>

              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Notification Email"
                  name="notificationEmail"
                  value={formData.notificationEmail || ''}
                  onChange={handleChange}
                  placeholder="email@example.com"
                  helperText="Email address for notifications (required for API)"
                />
              </Grid>

              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Description (optional)"
                  name="description"
                  value={formData.description || ''}
                  onChange={handleChange}
                  multiline
                  rows={3}
                  placeholder="What is this scraper for? What data will it collect?"
                />
              </Grid>
            </Grid>
          </Box>
        );

      case 1:
        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              URL Settings
            </Typography>

            <Grid container spacing={3}>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  required
                  label="Start URL"
                  name="startUrl"
                  value={formData.startUrl || ''}
                  onChange={handleChange}
                  error={Boolean(validation.startUrl)}
                  helperText={validation.startUrl || "The URL where scraping will begin"}
                  placeholder="https://example.com"
                  sx={{ width: '100%' }}
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <HttpIcon color="primary" />
                      </InputAdornment>
                    ),
                  }}
                />
              </Grid>

              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Base URL/Domain (optional)"
                  name="baseUrl"
                  value={formData.baseUrl || ''}
                  onChange={handleChange}
                  error={Boolean(validation.baseUrl)}
                  helperText={validation.baseUrl || "Limit scraping to this domain. If empty, will use the domain from the Start URL"}
                  placeholder="E.g. example.com"
                  sx={{ width: '100%' }}
                />
              </Grid>

              <Grid item xs={12} sm={6}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={formData.includeSubdomains || false}
                      onChange={handleChange}
                      name="includeSubdomains"
                      color="primary"
                    />
                  }
                  label="Include Subdomains"
                />
                <FormHelperText>
                  Allow scraping of subdomains like blog.example.com
                </FormHelperText>
              </Grid>

              <Grid item xs={12} sm={6}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={formData.followExternalLinks || false}
                      onChange={handleChange}
                      name="followExternalLinks"
                      color="primary"
                    />
                  }
                  label="Follow External Links"
                />
                <FormHelperText>
                  Follow links to different domains (use with caution)
                </FormHelperText>
              </Grid>
            </Grid>
          </Box>
        );

      case 2:
        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              Advanced Options
            </Typography>

            <Grid container spacing={3}>
              <Grid item xs={12} sm={4}>
                <TextField
                  fullWidth
                  type="number"
                  label="Max Depth"
                  name="maxDepth"
                  value={formData.maxDepth || 5}
                  onChange={handleChange}
                  inputProps={{ min: 1 }}
                  error={Boolean(validation.maxDepth)}
                  helperText={validation.maxDepth || "How many links deep to crawl"}
                />
              </Grid>

              <Grid item xs={12} sm={4}>
                <TextField
                  fullWidth
                  type="number"
                  label="Max Concurrent Requests"
                  name="maxConcurrentRequests"
                  value={formData.maxConcurrentRequests || 5}
                  onChange={handleChange}
                  inputProps={{ min: 1 }}
                  error={Boolean(validation.maxConcurrentRequests)}
                  helperText={validation.maxConcurrentRequests}
                />
              </Grid>

              <Grid item xs={12} sm={4}>
                <TextField
                  fullWidth
                  type="number"
                  label="Delay Between Requests (ms)"
                  name="delayBetweenRequests"
                  value={formData.delayBetweenRequests || 1000}
                  onChange={handleChange}
                  inputProps={{ min: 0 }}
                  error={Boolean(validation.delayBetweenRequests)}
                  helperText={validation.delayBetweenRequests}
                />
              </Grid>

              <Grid item xs={12}>
                <Divider sx={{ my: 2 }} />
              </Grid>

              <Grid item xs={12} sm={6}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={formData.respectRobotsTxt}
                      onChange={handleChange}
                      name="respectRobotsTxt"
                      color="primary"
                    />
                  }
                  label="Respect robots.txt"
                />
                <FormHelperText>
                  Follow rules set by website owners
                </FormHelperText>
              </Grid>

              <Grid item xs={12} sm={6}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={formData.downloadAssets}
                      onChange={handleChange}
                      name="downloadAssets"
                      color="primary"
                    />
                  }
                  label="Download Assets"
                />
                <FormHelperText>
                  Download CSS, images, and other assets
                </FormHelperText>
              </Grid>

              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="User Agent"
                  name="userAgent"
                  value={formData.userAgent}
                  onChange={handleChange}
                  helperText="Browser identifier string to use for requests"
                />
              </Grid>

              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Proxy URL (optional)"
                  name="proxy"
                  value={formData.proxy}
                  onChange={handleChange}
                  placeholder="E.g. http://username:password@proxyserver:port"
                  helperText="Route requests through a proxy server"
                />
              </Grid>

              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Custom Headers (JSON format, optional)"
                  name="customHeaders"
                  value={formData.customHeaders}
                  onChange={handleChange}
                  multiline
                  rows={3}
                  error={Boolean(validation.customHeaders)}
                  helperText={validation.customHeaders || "Custom HTTP headers as JSON object"}
                  placeholder='{ "Cookie": "session=abc123", "Referer": "https://example.com" }'
                />
              </Grid>
            </Grid>
          </Box>
        );

      case 3:
        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              Filtering Rules
            </Typography>

            <Grid container spacing={3}>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Include URL Patterns (one per line)"
                  name="includePatterns"
                  value={formData.includePatterns}
                  onChange={handleChange}
                  multiline
                  rows={3}
                  placeholder="/blog/\n/news/\n*.pdf"
                  helperText="Only follow URLs matching these patterns. Supports * as wildcard."
                />
              </Grid>

              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Exclude URL Patterns (one per line)"
                  name="excludePatterns"
                  value={formData.excludePatterns}
                  onChange={handleChange}
                  multiline
                  rows={3}
                  placeholder="/login\n/admin\n*/tag/*"
                  helperText="Skip URLs matching these patterns. Supports * as wildcard."
                />
              </Grid>

              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Custom CSS Selector (optional)"
                  name="customCss"
                  value={formData.customCss}
                  onChange={handleChange}
                  placeholder="article, .content, #main"
                  helperText="Target specific elements to extract (comma-separated CSS selectors)"
                />
              </Grid>
            </Grid>
          </Box>
        );

      default:
        return null;
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '70vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      {/* Breadcrumbs */}
      <Breadcrumbs sx={{ mb: 2 }}>
        <Link
          component={RouterLink}
          to="/scrapers"
          underline="hover"
          sx={{ display: 'flex', alignItems: 'center' }}
        >
          <HomeIcon sx={{ mr: 0.5 }} fontSize="inherit" />
          Scrapers
        </Link>
        <Typography color="text.primary">
          {isEditMode ? 'Edit Scraper' : 'New Scraper'}
        </Typography>
      </Breadcrumbs>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">
          {isEditMode ? 'Edit Scraper Configuration' : 'Create New Scraper'}
        </Typography>

        <Button
          component={RouterLink}
          to={isEditMode ? `/dashboard/${id}` : '/scrapers'}
          variant="outlined"
          startIcon={<ArrowBackIcon />}
        >
          {isEditMode ? 'Back to Dashboard' : 'Back to Scrapers'}
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {success && (
        <Alert severity="success" sx={{ mb: 3 }}>
          Scraper configuration saved successfully! Redirecting...
        </Alert>
      )}

      <Paper sx={{ p: 3, mb: 3 }}>
        <Stepper activeStep={activeStep} sx={{ mb: 4 }}>
          {steps.map((label) => (
            <Step key={label}>
              <StepLabel>{label}</StepLabel>
            </Step>
          ))}
        </Stepper>

        <form onSubmit={handleSubmit}>
          {renderStepContent(activeStep)}

          <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 4 }}>
            <Button
              variant="outlined"
              disabled={activeStep === 0}
              onClick={handleBack}
            >
              Back
            </Button>

            <Box>
              {activeStep === steps.length - 1 ? (
                <Button
                  variant="contained"
                  color="primary"
                  type="submit"
                  startIcon={<SaveIcon />}
                  disabled={saving}
                >
                  {saving ? 'Saving...' : (isEditMode ? 'Save Changes' : 'Create Scraper')}
                </Button>
              ) : (
                <Button
                  variant="contained"
                  color="primary"
                  onClick={handleNext}
                  startIcon={activeStep === 0 ? <HttpIcon /> : (activeStep === 1 ? <SpeedIcon /> : <FilterAltIcon />)}
                >
                  Next
                </Button>
              )}
            </Box>
          </Box>
        </form>
      </Paper>
    </Box>
  );
};

export default Configuration;