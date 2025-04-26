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

  // Debug information
  console.log('🔍 Configuration component loaded');
  console.log('🔍 URL params:', { id });
  console.log('🔍 isEditMode:', isEditMode);

  // Add a visual indicator that the component is loaded
  useEffect(() => {
    // Create a floating debug indicator
    const debugIndicator = document.createElement('div');
    debugIndicator.style.position = 'fixed';
    debugIndicator.style.top = '10px';
    debugIndicator.style.left = '10px';
    debugIndicator.style.backgroundColor = 'green';
    debugIndicator.style.color = 'white';
    debugIndicator.style.padding = '5px 10px';
    debugIndicator.style.borderRadius = '5px';
    debugIndicator.style.zIndex = '9999';
    debugIndicator.style.fontSize = '12px';
    debugIndicator.textContent = `Configuration Component Loaded - ID: ${id || 'new'}`;

    document.body.appendChild(debugIndicator);

    // Remove after 5 seconds
    setTimeout(() => {
      try {
        document.body.removeChild(debugIndicator);
      } catch (e) {
        console.error('Error removing debug indicator:', e);
      }
    }, 5000);

    return () => {
      try {
        document.body.removeChild(debugIndicator);
      } catch (e) {
        // Ignore if already removed
      }
    };
  }, [id]);

  // Debug function to save scraper data to localStorage
  const saveScraperToLocalStorage = (scraperData) => {
    try {
      localStorage.setItem('debug_scraper', JSON.stringify(scraperData));
      console.log('Saved scraper data to localStorage');
      alert('Scraper data saved to localStorage');
    } catch (e) {
      console.error('Error saving to localStorage:', e);
      alert('Error saving to localStorage: ' + e.message);
    }
  };

  // Debug button handler
  const handleDebugClick = () => {
    // Get all scrapers first
    fetch('/api/scraper', {
      headers: {
        'Accept': 'application/json'
      }
    })
    .then(response => response.text())
    .then(text => {
      try {
        const scrapers = JSON.parse(text);
        console.log('All scrapers for debug:', scrapers);

        // Find the scraper with matching ID
        const matchingScraper = scrapers.find(s =>
          (s.id && s.id.toLowerCase() === id.toLowerCase()) ||
          (s.Id && s.Id.toLowerCase() === id.toLowerCase()) ||
          (s.name && s.name.toLowerCase() === id.toLowerCase()) ||
          (s.Name && s.Name.toLowerCase() === id.toLowerCase())
        );

        if (matchingScraper) {
          console.log('Found matching scraper for debug:', matchingScraper);
          saveScraperToLocalStorage(matchingScraper);
        } else {
          alert('No matching scraper found with ID: ' + id);
        }
      } catch (e) {
        console.error('Error parsing scrapers for debug:', e);
        alert('Error parsing scrapers: ' + e.message);
      }
    })
    .catch(error => {
      console.error('Error fetching scrapers for debug:', error);
      alert('Error fetching scrapers: ' + error.message);
    });
  };

  // Load scraper data from localStorage
  const loadScraperFromLocalStorage = () => {
    try {
      const savedScraper = localStorage.getItem('debug_scraper');
      if (savedScraper) {
        const scraperData = JSON.parse(savedScraper);
        console.log('Loaded scraper from localStorage:', scraperData);

        // Normalize the data
        const normalizedData = {};
        Object.keys(scraperData).forEach(key => {
          const camelCaseKey = key.charAt(0).toLowerCase() + key.slice(1);
          normalizedData[camelCaseKey] = scraperData[key];
        });

        // Special handling for array properties
        if (normalizedData.includePatterns && Array.isArray(normalizedData.includePatterns)) {
          normalizedData.includePatterns = normalizedData.includePatterns.join('\n');
        }

        if (normalizedData.excludePatterns && Array.isArray(normalizedData.excludePatterns)) {
          normalizedData.excludePatterns = normalizedData.excludePatterns.join('\n');
        }

        // Special handling for object properties
        if (normalizedData.customHeaders && typeof normalizedData.customHeaders === 'object') {
          normalizedData.customHeaders = JSON.stringify(normalizedData.customHeaders, null, 2);
        }

        // Update form
        setFormData(prevState => ({
          ...prevState,
          ...normalizedData
        }));

        setError(null);
        setLoading(false);
        return true;
      } else {
        alert('No saved scraper data found in localStorage');
        return false;
      }
    } catch (e) {
      console.error('Error loading from localStorage:', e);
      alert('Error loading saved scraper: ' + e.message);
      return false;
    }
  };

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
        setError(null);
        console.log('🔍 Configuration: Loading scraper data for ID:', id);
        console.log('🔍 isEditMode:', isEditMode);

        if (!id) {
          console.error('❌ No scraper ID provided');
          setError('No scraper ID provided. Cannot load scraper configuration.');
          setLoading(false);
          return;
        }

        // FIRST APPROACH - Try direct API call to specific endpoint
        console.log('🔍 Making direct API call to specific endpoint...');
        try {
          const response = await fetch(`/api/scraper/${id}`, {
            headers: {
              'Accept': 'application/json'
            }
          });
          
          console.log('📊 Specific scraper API response status:', response.status);
          
          if (response.ok) {
            const text = await response.text();
            console.log('📝 Specific scraper API response text:', text);
            
            if (text && !text.includes('<!DOCTYPE html>')) {
              try {
                const data = JSON.parse(text);
                console.log('✅ Successfully parsed specific scraper data:', data);
                
                // Normalize the data
                const normalizedData = {};
                Object.keys(data).forEach(key => {
                  const camelCaseKey = key.charAt(0).toLowerCase() + key.slice(1);
                  normalizedData[camelCaseKey] = data[key];
                });
                
                // Special handling for array properties
                if (normalizedData.includePatterns && Array.isArray(normalizedData.includePatterns)) {
                  normalizedData.includePatterns = normalizedData.includePatterns.join('\n');
                }
                
                if (normalizedData.excludePatterns && Array.isArray(normalizedData.excludePatterns)) {
                  normalizedData.excludePatterns = normalizedData.excludePatterns.join('\n');
                }
                
                // Special handling for object properties
                if (normalizedData.customHeaders && typeof normalizedData.customHeaders === 'object') {
                  normalizedData.customHeaders = JSON.stringify(normalizedData.customHeaders, null, 2);
                }
                
                // Save to localStorage as a backup
                try {
                  localStorage.setItem('debug_scraper', JSON.stringify(data));
                  console.log('✅ Saved scraper data to localStorage as backup');
                } catch (e) {
                  console.error('❌ Error saving to localStorage:', e);
                }
                
                // Update form
                setFormData(prevState => ({
                  ...prevState,
                  ...normalizedData
                }));
                
                setLoading(false);
                setError(null);
                return; // Exit if successful
              } catch (e) {
                console.error('❌ Error parsing specific scraper response:', e);
              }
            } else {
              console.error('❌ Received HTML instead of JSON for specific scraper');
            }
          }
        } catch (specificError) {
          console.error('❌ Error making specific API call:', specificError);
        }

        // SECOND APPROACH - Get all scrapers and find the matching one
        console.log('🔍 Getting all scrapers...');

        try {
          // Create a visual indicator that we're trying to load
          const loadingIndicator = document.createElement('div');
          loadingIndicator.style.position = 'fixed';
          loadingIndicator.style.top = '50%';
          loadingIndicator.style.left = '50%';
          loadingIndicator.style.transform = 'translate(-50%, -50%)';
          loadingIndicator.style.backgroundColor = 'rgba(0, 0, 0, 0.8)';
          loadingIndicator.style.color = 'white';
          loadingIndicator.style.padding = '20px';
          loadingIndicator.style.borderRadius = '10px';
          loadingIndicator.style.zIndex = '9999';
          loadingIndicator.textContent = 'Loading scraper data...';
          document.body.appendChild(loadingIndicator);

          // Make a direct fetch call to the API
          const response = await fetch('/api/scraper', {
            headers: {
              'Accept': 'application/json'
            }
          });

          console.log('🔍 API response status:', response.status);

          if (!response.ok) {
            throw new Error(`API returned error status: ${response.status}`);
          }

          const responseText = await response.text();
          console.log('🔍 API response length:', responseText.length);

          // Check if we got HTML instead of JSON
          if (responseText.includes('<!DOCTYPE html>')) {
            console.error('❌ API returned HTML instead of JSON');
            throw new Error('API returned HTML instead of JSON. The API proxy might not be working correctly.');
          }

          // Try to parse the response as JSON
          let scrapers;
          try {
            scrapers = JSON.parse(responseText);
            console.log('✅ Successfully parsed scrapers:', scrapers);
          } catch (parseError) {
            console.error('❌ Error parsing scrapers:', parseError);
            console.error('❌ Response text:', responseText.substring(0, 500) + '...');
            throw new Error(`Failed to parse API response as JSON: ${parseError.message}`);
          }

          // Find the scraper with matching ID
          const matchingScraper = scrapers.find(s =>
            (s.id && s.id.toLowerCase() === id.toLowerCase()) ||
            (s.Id && s.Id.toLowerCase() === id.toLowerCase()) ||
            (s.name && s.name.toLowerCase() === id.toLowerCase()) ||
            (s.Name && s.Name.toLowerCase() === id.toLowerCase())
          );

          if (!matchingScraper) {
            console.error('❌ No matching scraper found for ID:', id);
            throw new Error(`No scraper found with ID: ${id}`);
          }

          console.log('✅ Found matching scraper:', matchingScraper);

          // Normalize the data (convert PascalCase to camelCase)
          const normalizedData = {};
          Object.keys(matchingScraper).forEach(key => {
            // Convert first character to lowercase
            const camelCaseKey = key.charAt(0).toLowerCase() + key.slice(1);
            normalizedData[camelCaseKey] = matchingScraper[key];
          });

          // Special handling for array properties
          if (normalizedData.includePatterns && Array.isArray(normalizedData.includePatterns)) {
            normalizedData.includePatterns = normalizedData.includePatterns.join('\n');
          }

          if (normalizedData.excludePatterns && Array.isArray(normalizedData.excludePatterns)) {
            normalizedData.excludePatterns = normalizedData.excludePatterns.join('\n');
          }

          // Special handling for object properties
          if (normalizedData.customHeaders && typeof normalizedData.customHeaders === 'object') {
            normalizedData.customHeaders = JSON.stringify(normalizedData.customHeaders, null, 2);
          }

          // Ensure required properties exist
          normalizedData.id = normalizedData.id || matchingScraper.Id || id;
          normalizedData.name = normalizedData.name || matchingScraper.Name || '';
          normalizedData.startUrl = normalizedData.startUrl || matchingScraper.StartUrl || '';
          normalizedData.baseUrl = normalizedData.baseUrl || matchingScraper.BaseUrl || '';

          console.log('✅ Normalized data:', normalizedData);

          // Save to localStorage as a backup
          try {
            localStorage.setItem('debug_scraper', JSON.stringify(matchingScraper));
            console.log('✅ Saved scraper data to localStorage as backup');
          } catch (e) {
            console.error('❌ Error saving to localStorage:', e);
          }

          // Update form with existing data
          setFormData(prevState => {
            const newState = {
              ...prevState,
              ...normalizedData
            };
            console.log('✅ Updated form data:', newState);
            return newState;
          });

          // Remove the loading indicator
          try {
            document.body.removeChild(loadingIndicator);
          } catch (e) {
            console.error('Error removing loading indicator:', e);
          }

          setLoading(false);

        } catch (error) {
          console.error('❌ Error loading scraper:', error);
          setError(`Failed to load scraper: ${error.message}`);
          setLoading(false);

          // Try to load from localStorage as a fallback
          try {
            const savedScraper = localStorage.getItem('debug_scraper');
            if (savedScraper) {
              console.log('🔍 Trying to load from localStorage as fallback');
              const scraperData = JSON.parse(savedScraper);
              console.log('✅ Loaded scraper from localStorage:', scraperData);

              // Normalize the data
              const normalizedData = {};
              Object.keys(scraperData).forEach(key => {
                const camelCaseKey = key.charAt(0).toLowerCase() + key.slice(1);
                normalizedData[camelCaseKey] = scraperData[key];
              });

              // Special handling for array properties
              if (normalizedData.includePatterns && Array.isArray(normalizedData.includePatterns)) {
                normalizedData.includePatterns = normalizedData.includePatterns.join('\n');
              }

              if (normalizedData.excludePatterns && Array.isArray(normalizedData.excludePatterns)) {
                normalizedData.excludePatterns = normalizedData.excludePatterns.join('\n');
              }

              // Special handling for object properties
              if (normalizedData.customHeaders && typeof normalizedData.customHeaders === 'object') {
                normalizedData.customHeaders = JSON.stringify(normalizedData.customHeaders, null, 2);
              }

              // Update form
              setFormData(prevState => ({
                ...prevState,
                ...normalizedData
              }));

              setError('Loaded from localStorage backup (API call failed)');
              setLoading(false);
            }
          } catch (e) {
            console.error('❌ Error loading from localStorage:', e);
          }
        }
      } catch (err) {
        console.error('❌ Unexpected error:', err);
        setError(`Unexpected error: ${err.message}`);
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

      {isEditMode && (
        <Alert severity="info" sx={{ mb: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 2 }}>
            <Typography>
              Scraper ID: <strong>{id}</strong>
            </Typography>
            <Box>
              <Button
                variant="contained"
                color="primary"
                onClick={() => {
                  console.log('Manual load button clicked for ID:', id);
                  setLoading(true);
                  
                  // Make a direct browser fetch request to the API
                  window.fetch(`/api/scraper/${id}`, {
                    headers: {
                      'Accept': 'application/json'
                    }
                  })
                  .then(response => {
                    console.log('🔍 Direct browser fetch response status:', response.status);
                    if (!response.ok) {
                      throw new Error(`API returned error status: ${response.status}`);
                    }
                    return response.text();
                  })
                  .then(text => {
                    console.log('🔍 API response text:', text);
                    
                    // Check if we got HTML instead of JSON
                    if (text.includes('<!DOCTYPE html>')) {
                      console.error('❌ API returned HTML instead of JSON');
                      throw new Error('API returned HTML instead of JSON. The API proxy might not be working correctly.');
                    }
                    
                    try {
                      // Try to parse the response as JSON
                      const data = JSON.parse(text);
                      console.log('✅ Successfully parsed scraper data:', data);
                      
                      // Normalize the data
                      const normalizedData = {};
                      Object.keys(data).forEach(key => {
                        const camelCaseKey = key.charAt(0).toLowerCase() + key.slice(1);
                        normalizedData[camelCaseKey] = data[key];
                      });
                      
                      // Special handling for array properties
                      if (normalizedData.includePatterns && Array.isArray(normalizedData.includePatterns)) {
                        normalizedData.includePatterns = normalizedData.includePatterns.join('\n');
                      }
                      
                      if (normalizedData.excludePatterns && Array.isArray(normalizedData.excludePatterns)) {
                        normalizedData.excludePatterns = normalizedData.excludePatterns.join('\n');
                      }
                      
                      // Special handling for object properties
                      if (normalizedData.customHeaders && typeof normalizedData.customHeaders === 'object') {
                        normalizedData.customHeaders = JSON.stringify(normalizedData.customHeaders, null, 2);
                      }
                      
                      // Update form data
                      setFormData(prevState => ({
                        ...prevState,
                        ...normalizedData
                      }));
                      
                      setLoading(false);
                      setError(null);
                      alert('Successfully loaded scraper data!');
                    } catch (e) {
                      console.error('Error parsing scraper data:', e);
                      setLoading(false);
                      setError(`Failed to parse API response as JSON: ${e.message}`);
                    }
                  })
                  .catch(error => {
                    console.error('❌ Error fetching scraper:', error);
                    setLoading(false);
                    setError(`Failed to load scraper: ${error.message}`);
                    
                    // Show network request details
                    alert(`Failed to load scraper data. Check browser console for details.\nError: ${error.message}`);
                  });
                }}
              >
                Load from API
              </Button>
              <Button
                variant="contained"
                color="secondary"
                onClick={() => {
                  console.log('Debug endpoint button clicked for ID:', id);
                  setLoading(true);
                  
                  // Use our special debug endpoint
                  window.fetch(`/debug-scraper/${id}`, {
                    headers: {
                      'Accept': 'application/json'
                    }
                  })
                  .then(response => {
                    console.log('🔍 Debug endpoint response status:', response.status);
                    if (!response.ok) {
                      throw new Error(`Debug endpoint returned error status: ${response.status}`);
                    }
                    return response.text();
                  })
                  .then(text => {
                    console.log('🔍 Debug endpoint response text:', text);
                    
                    try {
                      // Try to parse the debug response as JSON
                      const debugData = JSON.parse(text);
                      console.log('✅ Successfully parsed debug response:', debugData);
                      
                      if (!debugData.scraperData) {
                        throw new Error('No scraper data in debug response');
                      }
                      
                      // Extract the actual scraper data from the debug wrapper
                      const data = debugData.scraperData;
                      
                      // Normalize the data
                      const normalizedData = {};
                      Object.keys(data).forEach(key => {
                        const camelCaseKey = key.charAt(0).toLowerCase() + key.slice(1);
                        normalizedData[camelCaseKey] = data[key];
                      });
                      
                      // Special handling for array properties
                      if (normalizedData.includePatterns && Array.isArray(normalizedData.includePatterns)) {
                        normalizedData.includePatterns = normalizedData.includePatterns.join('\n');
                      }
                      
                      if (normalizedData.excludePatterns && Array.isArray(normalizedData.excludePatterns)) {
                        normalizedData.excludePatterns = normalizedData.excludePatterns.join('\n');
                      }
                      
                      // Special handling for object properties
                      if (normalizedData.customHeaders && typeof normalizedData.customHeaders === 'object') {
                        normalizedData.customHeaders = JSON.stringify(normalizedData.customHeaders, null, 2);
                      }
                      
                      // Save to localStorage for backup
                      try {
                        localStorage.setItem('debug_scraper', JSON.stringify(data));
                        console.log('✅ Saved scraper data to localStorage as backup');
                      } catch (e) {
                        console.error('❌ Error saving to localStorage:', e);
                      }
                      
                      // Update form data
                      setFormData(prevState => ({
                        ...prevState,
                        ...normalizedData
                      }));
                      
                      setLoading(false);
                      setError(null);
                      alert('Successfully loaded scraper data from debug endpoint!');
                    } catch (e) {
                      console.error('Error processing debug endpoint data:', e);
                      setLoading(false);
                      setError(`Failed to process debug endpoint data: ${e.message}`);
                    }
                  })
                  .catch(error => {
                    console.error('❌ Error with debug endpoint:', error);
                    setLoading(false);
                    setError(`Debug endpoint error: ${error.message}`);
                    
                    // Show network request details
                    alert(`Failed to load from debug endpoint. Check browser console for details.\nError: ${error.message}`);
                  });
                }}
                sx={{ ml: 1 }}
              >
                Use Debug Endpoint
              </Button>
            </Box>
          </Box>
        </Alert>
      )}

      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">
          {isEditMode ? 'Edit Scraper Configuration' : 'Create New Scraper'}
        </Typography>

        <Box>
          {/* Emergency Load button - only visible in edit mode */}
          {isEditMode && (
            <Button
              variant="contained"
              color="error"
              onClick={() => {
                // Make direct API call to get all scrapers
                fetch('/api/scraper', {
                  headers: {
                    'Accept': 'application/json'
                  }
                })
                .then(response => response.text())
                .then(text => {
                  try {
                    const scrapers = JSON.parse(text);
                    console.log('All scrapers:', scrapers);

                    // Find the scraper with matching ID
                    const matchingScraper = scrapers.find(s =>
                      (s.id && s.id.toLowerCase() === id.toLowerCase()) ||
                      (s.Id && s.Id.toLowerCase() === id.toLowerCase()) ||
                      (s.name && s.name.toLowerCase() === id.toLowerCase()) ||
                      (s.Name && s.Name.toLowerCase() === id.toLowerCase())
                    );

                    if (matchingScraper) {
                      console.log('Found matching scraper:', matchingScraper);

                      // Normalize the data
                      const normalizedData = {};
                      Object.keys(matchingScraper).forEach(key => {
                        const camelCaseKey = key.charAt(0).toLowerCase() + key.slice(1);
                        normalizedData[camelCaseKey] = matchingScraper[key];
                      });

                      // Special handling for array properties
                      if (normalizedData.includePatterns && Array.isArray(normalizedData.includePatterns)) {
                        normalizedData.includePatterns = normalizedData.includePatterns.join('\n');
                      }

                      if (normalizedData.excludePatterns && Array.isArray(normalizedData.excludePatterns)) {
                        normalizedData.excludePatterns = normalizedData.excludePatterns.join('\n');
                      }

                      // Special handling for object properties
                      if (normalizedData.customHeaders && typeof normalizedData.customHeaders === 'object') {
                        normalizedData.customHeaders = JSON.stringify(normalizedData.customHeaders, null, 2);
                      }

                      // Update form data
                      setFormData(prevState => ({
                        ...prevState,
                        ...normalizedData
                      }));

                      alert('Form populated with scraper data!');
                    } else {
                      alert('No matching scraper found with ID: ' + id);
                    }
                  } catch (e) {
                    console.error('Error parsing scrapers:', e);
                    alert('Error parsing scrapers: ' + e.message);
                  }
                })
                .catch(error => {
                  console.error('Error fetching scrapers:', error);
                  alert('Error fetching scrapers: ' + error.message);
                });
              }}
              sx={{ mr: 2 }}
            >
              EMERGENCY LOAD
            </Button>
          )}

          {/* Debug button - only visible in edit mode */}
          {isEditMode && (
            <Button
              variant="outlined"
              color="secondary"
              onClick={handleDebugClick}
              sx={{ mr: 2 }}
            >
              Debug: Save Scraper Data
            </Button>
          )}

          <Button
            component={RouterLink}
            to={isEditMode ? `/dashboard/${id}` : '/scrapers'}
            variant="outlined"
            startIcon={<ArrowBackIcon />}
          >
            {isEditMode ? 'Back to Dashboard' : 'Back to Scrapers'}
          </Button>
        </Box>
      </Box>

      {error && (
        <Alert
          severity="error"
          sx={{ mb: 3 }}
          action={
            <Box>
              <Button
                id="retry-button"
                color="inherit"
                size="small"
                onClick={() => {
                  console.log('Retry button clicked');
                  setError(null);
                  if (isEditMode) {
                    setLoading(true);
                    fetchScraper(id)
                      .then(data => {
                        console.log('Retry successful, data:', data);
                        setFormData(prevState => ({
                          ...prevState,
                          ...data
                        }));
                        setLoading(false);
                      })
                      .catch(err => {
                        console.error('Retry failed:', err);
                        setError(`Retry failed: ${err.message}`);
                        setLoading(false);
                      });
                  }
                }}
              >
                Retry
              </Button>
              <Button
                color="warning"
                size="small"
                sx={{ ml: 1 }}
                onClick={() => {
                  console.log('Direct fetch button clicked for ID:', id);
                  setLoading(true);
                  setError(null);
                  
                  // Make a direct fetch to the specific API endpoint
                  fetch(`/api/scraper/${id}`, {
                    headers: {
                      'Accept': 'application/json'
                    }
                  })
                  .then(response => {
                    console.log('Direct API response status:', response.status);
                    if (!response.ok) {
                      throw new Error(`API returned error status: ${response.status}`);
                    }
                    return response.text();
                  })
                  .then(text => {
                    console.log('API response text length:', text.length);
                    
                    // Check if we got HTML instead of JSON
                    if (text.includes('<!DOCTYPE html>')) {
                      console.error('API returned HTML instead of JSON');
                      throw new Error('API returned HTML instead of JSON. The API proxy might not be working correctly.');
                    }
                    
                    try {
                      const data = JSON.parse(text);
                      console.log('Successfully parsed scraper data:', data);
                      
                      // Normalize the data
                      const normalizedData = {};
                      Object.keys(data).forEach(key => {
                        const camelCaseKey = key.charAt(0).toLowerCase() + key.slice(1);
                        normalizedData[camelCaseKey] = data[key];
                      });
                      
                      // Special handling for array properties
                      if (normalizedData.includePatterns && Array.isArray(normalizedData.includePatterns)) {
                        normalizedData.includePatterns = normalizedData.includePatterns.join('\n');
                      }
                      
                      if (normalizedData.excludePatterns && Array.isArray(normalizedData.excludePatterns)) {
                        normalizedData.excludePatterns = normalizedData.excludePatterns.join('\n');
                      }
                      
                      // Special handling for object properties
                      if (normalizedData.customHeaders && typeof normalizedData.customHeaders === 'object') {
                        normalizedData.customHeaders = JSON.stringify(normalizedData.customHeaders, null, 2);
                      }
                      
                      // Update form data
                      setFormData(prevState => ({
                        ...prevState,
                        ...normalizedData
                      }));
                      
                      setLoading(false);
                      setError(null);
                      alert('Successfully loaded scraper data!');
                    } catch (e) {
                      console.error('Error parsing scraper data:', e);
                      setLoading(false);
                      setError(`Failed to parse API response as JSON: ${e.message}`);
                    }
                  })
                  .catch(error => {
                    console.error('Error fetching scraper:', error);
                    setLoading(false);
                    setError(`Failed to fetch scraper: ${error.message}`);
                  });
                }}
              >
                Direct Fetch
              </Button>
            </Box>
          }
        >
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