import React, { useState, useEffect } from 'react';
import {
  Box,
  TextField,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormControlLabel,
  Switch,
  Button,
  Typography,
  Paper,
  Divider,
  Alert,
  Tooltip,
  IconButton,
  InputAdornment,
  Slider,
  CircularProgress
} from '@mui/material';
import {
  Save as SaveIcon,
  Cancel as CancelIcon,
  Help as HelpIcon,
  Info as InfoIcon
} from '@mui/icons-material';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import { isValidUrl } from '../../utils/validators';

// Validation schema using Yup
const validationSchema = Yup.object({
  name: Yup.string()
    .required('Name is required')
    .max(100, 'Name must be at most 100 characters'),
  id: Yup.string()
    .required('ID is required')
    .matches(/^[a-zA-Z0-9-_]+$/, 'ID can only contain letters, numbers, hyphens, and underscores')
    .max(50, 'ID must be at most 50 characters'),
  email: Yup.string()
    .email('Enter a valid email')
    .required('Email is required'),
  startUrl: Yup.string()
    .required('Start URL is required')
    .test('is-url', 'Enter a valid URL', isValidUrl),
  baseUrl: Yup.string()
    .required('Base URL is required')
    .test('is-url', 'Enter a valid URL', isValidUrl),
  outputDirectory: Yup.string()
    .required('Output directory is required'),
  maxDepth: Yup.number()
    .required('Max depth is required')
    .min(1, 'Min value is 1')
    .max(100, 'Max value is 100')
    .integer('Must be an integer'),
  maxConcurrentRequests: Yup.number()
    .required('Max concurrent requests is required')
    .min(1, 'Min value is 1')
    .max(20, 'Max value is 20')
    .integer('Must be an integer'),
  delayBetweenRequests: Yup.number()
    .required('Delay between requests is required')
    .min(0, 'Min value is 0')
    .max(60000, 'Max value is 60000'),
  learningPagesCount: Yup.number()
    .when('autoLearnHeaderFooter', {
      is: true,
      then: Yup.number()
        .required('Learning pages count is required')
        .min(1, 'Min value is 1')
        .max(20, 'Max value is 20')
        .integer('Must be an integer'),
      otherwise: Yup.number().notRequired()
    }),
  maxVersionsToKeep: Yup.number()
    .when('trackContentVersions', {
      is: true,
      then: Yup.number()
        .required('Max versions to keep is required')
        .min(1, 'Min value is 1')
        .max(100, 'Max value is 100')
        .integer('Must be an integer'),
      otherwise: Yup.number().notRequired()
    })
});

const ScraperForm = ({ initialValues, onSubmit, isSubmitting, isEditMode = false }) => {
  const [showAdvancedOptions, setShowAdvancedOptions] = useState(false);

  // Default values for a new scraper
  const defaultValues = {
    name: '',
    id: '',
    email: '',
    startUrl: '',
    baseUrl: '',
    outputDirectory: 'ScrapedData',
    maxDepth: 5,
    maxConcurrentRequests: 5,
    delayBetweenRequests: 1000,
    followExternalLinks: false,
    respectRobotsTxt: true,
    autoLearnHeaderFooter: true,
    learningPagesCount: 5,
    enableChangeDetection: true,
    trackContentVersions: true,
    maxVersionsToKeep: 5,
    notificationEndpoint: '',
    enableContinuousMonitoring: false,
    monitoringInterval: 86400, // 24 hours in seconds
  };

  // Merge default values with provided initial values
  const mergedInitialValues = { ...defaultValues, ...initialValues };

  // Initialize formik
  const formik = useFormik({
    initialValues: mergedInitialValues,
    validationSchema,
    onSubmit: (values) => {
      onSubmit(values);
    },
  });

  // Generate a unique ID based on the name when creating a new scraper
  useEffect(() => {
    if (!isEditMode && formik.values.name && !formik.values.id) {
      // Generate ID from name: lowercase, replace spaces with hyphens, remove special chars
      const generatedId = formik.values.name
        .toLowerCase()
        .replace(/\s+/g, '-')
        .replace(/[^a-z0-9-_]/g, '');
      
      formik.setFieldValue('id', generatedId);
    }
  }, [formik.values.name, isEditMode, formik]);

  // Auto-generate baseUrl from startUrl if empty
  useEffect(() => {
    if (formik.values.startUrl && !formik.values.baseUrl) {
      try {
        const url = new URL(formik.values.startUrl);
        const baseUrl = `${url.protocol}//${url.hostname}`;
        formik.setFieldValue('baseUrl', baseUrl);
      } catch (error) {
        // Invalid URL, do nothing
      }
    }
  }, [formik.values.startUrl, formik]);

  return (
    <form onSubmit={formik.handleSubmit}>
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" gutterBottom>Basic Information</Typography>
        <Divider sx={{ mb: 3 }} />
        
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <TextField
              fullWidth
              id="name"
              name="name"
              label="Scraper Name"
              value={formik.values.name}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.name && Boolean(formik.errors.name)}
              helperText={formik.touched.name && formik.errors.name}
              required
              disabled={isSubmitting}
            />
          </Grid>
          
          <Grid item xs={12} md={6}>
            <TextField
              fullWidth
              id="id"
              name="id"
              label="Scraper ID"
              value={formik.values.id}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.id && Boolean(formik.errors.id)}
              helperText={
                (formik.touched.id && formik.errors.id) || 
                "Unique identifier for the scraper (letters, numbers, hyphens, underscores)"
              }
              required
              disabled={isEditMode || isSubmitting} // ID cannot be changed in edit mode
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <Tooltip title="This ID will be used in API calls and cannot be changed later">
                      <InfoIcon color="action" fontSize="small" />
                    </Tooltip>
                  </InputAdornment>
                ),
              }}
            />
          </Grid>
          
          <Grid item xs={12} md={6}>
            <TextField
              fullWidth
              id="email"
              name="email"
              label="Notification Email"
              type="email"
              value={formik.values.email}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.email && Boolean(formik.errors.email)}
              helperText={
                (formik.touched.email && formik.errors.email) || 
                "Email for notifications about scraper status and changes"
              }
              required
              disabled={isSubmitting}
            />
          </Grid>
          
          <Grid item xs={12} md={6}>
            <TextField
              fullWidth
              id="outputDirectory"
              name="outputDirectory"
              label="Output Directory"
              value={formik.values.outputDirectory}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.outputDirectory && Boolean(formik.errors.outputDirectory)}
              helperText={
                (formik.touched.outputDirectory && formik.errors.outputDirectory) || 
                "Directory where scraped data will be stored"
              }
              required
              disabled={isSubmitting}
            />
          </Grid>
        </Grid>
      </Paper>
      
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" gutterBottom>URL Configuration</Typography>
        <Divider sx={{ mb: 3 }} />
        
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <TextField
              fullWidth
              id="startUrl"
              name="startUrl"
              label="Start URL"
              value={formik.values.startUrl}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.startUrl && Boolean(formik.errors.startUrl)}
              helperText={
                (formik.touched.startUrl && formik.errors.startUrl) || 
                "The URL where the scraper will begin crawling"
              }
              required
              disabled={isSubmitting}
            />
          </Grid>
          
          <Grid item xs={12}>
            <TextField
              fullWidth
              id="baseUrl"
              name="baseUrl"
              label="Base URL"
              value={formik.values.baseUrl}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              error={formik.touched.baseUrl && Boolean(formik.errors.baseUrl)}
              helperText={
                (formik.touched.baseUrl && formik.errors.baseUrl) || 
                "The base URL for the website (e.g., https://example.com)"
              }
              required
              disabled={isSubmitting}
            />
          </Grid>
          
          <Grid item xs={12} md={6}>
            <FormControlLabel
              control={
                <Switch
                  id="followExternalLinks"
                  name="followExternalLinks"
                  checked={formik.values.followExternalLinks}
                  onChange={formik.handleChange}
                  disabled={isSubmitting}
                />
              }
              label={
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  Follow External Links
                  <Tooltip title="If enabled, the scraper will follow links to other domains">
                    <IconButton size="small">
                      <HelpIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </Box>
              }
            />
          </Grid>
          
          <Grid item xs={12} md={6}>
            <FormControlLabel
              control={
                <Switch
                  id="respectRobotsTxt"
                  name="respectRobotsTxt"
                  checked={formik.values.respectRobotsTxt}
                  onChange={formik.handleChange}
                  disabled={isSubmitting}
                />
              }
              label={
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  Respect robots.txt
                  <Tooltip title="If enabled, the scraper will respect the rules in the website's robots.txt file">
                    <IconButton size="small">
                      <HelpIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </Box>
              }
            />
          </Grid>
        </Grid>
      </Paper>
      
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" gutterBottom>Crawling Settings</Typography>
        <Divider sx={{ mb: 3 }} />
        
        <Grid container spacing={3}>
          <Grid item xs={12} md={4}>
            <Typography gutterBottom>Max Depth</Typography>
            <Slider
              id="maxDepth"
              name="maxDepth"
              value={formik.values.maxDepth}
              onChange={(e, value) => formik.setFieldValue('maxDepth', value)}
              onBlur={() => formik.setFieldTouched('maxDepth', true)}
              valueLabelDisplay="auto"
              step={1}
              marks
              min={1}
              max={20}
              disabled={isSubmitting}
            />
            <Typography variant="caption" color={formik.touched.maxDepth && formik.errors.maxDepth ? 'error' : 'text.secondary'}>
              {formik.touched.maxDepth && formik.errors.maxDepth ? 
                formik.errors.maxDepth : 
                `Maximum link depth to crawl: ${formik.values.maxDepth}`}
            </Typography>
          </Grid>
          
          <Grid item xs={12} md={4}>
            <Typography gutterBottom>Max Concurrent Requests</Typography>
            <Slider
              id="maxConcurrentRequests"
              name="maxConcurrentRequests"
              value={formik.values.maxConcurrentRequests}
              onChange={(e, value) => formik.setFieldValue('maxConcurrentRequests', value)}
              onBlur={() => formik.setFieldTouched('maxConcurrentRequests', true)}
              valueLabelDisplay="auto"
              step={1}
              marks
              min={1}
              max={20}
              disabled={isSubmitting}
            />
            <Typography variant="caption" color={formik.touched.maxConcurrentRequests && formik.errors.maxConcurrentRequests ? 'error' : 'text.secondary'}>
              {formik.touched.maxConcurrentRequests && formik.errors.maxConcurrentRequests ? 
                formik.errors.maxConcurrentRequests : 
                `Maximum parallel requests: ${formik.values.maxConcurrentRequests}`}
            </Typography>
          </Grid>
          
          <Grid item xs={12} md={4}>
            <Typography gutterBottom>Delay Between Requests (ms)</Typography>
            <Slider
              id="delayBetweenRequests"
              name="delayBetweenRequests"
              value={formik.values.delayBetweenRequests}
              onChange={(e, value) => formik.setFieldValue('delayBetweenRequests', value)}
              onBlur={() => formik.setFieldTouched('delayBetweenRequests', true)}
              valueLabelDisplay="auto"
              step={100}
              marks
              min={0}
              max={5000}
              disabled={isSubmitting}
            />
            <Typography variant="caption" color={formik.touched.delayBetweenRequests && formik.errors.delayBetweenRequests ? 'error' : 'text.secondary'}>
              {formik.touched.delayBetweenRequests && formik.errors.delayBetweenRequests ? 
                formik.errors.delayBetweenRequests : 
                `Delay between requests: ${formik.values.delayBetweenRequests}ms`}
            </Typography>
          </Grid>
        </Grid>
      </Paper>
      
      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="h6">Content Processing</Typography>
          <Button 
            variant="text" 
            onClick={() => setShowAdvancedOptions(!showAdvancedOptions)}
          >
            {showAdvancedOptions ? 'Hide Advanced Options' : 'Show Advanced Options'}
          </Button>
        </Box>
        <Divider sx={{ mb: 3 }} />
        
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <FormControlLabel
              control={
                <Switch
                  id="autoLearnHeaderFooter"
                  name="autoLearnHeaderFooter"
                  checked={formik.values.autoLearnHeaderFooter}
                  onChange={formik.handleChange}
                  disabled={isSubmitting}
                />
              }
              label={
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  Auto-Learn Header/Footer
                  <Tooltip title="Automatically detect and remove common headers and footers from content">
                    <IconButton size="small">
                      <HelpIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </Box>
              }
            />
          </Grid>
          
          <Grid item xs={12} md={6}>
            <FormControlLabel
              control={
                <Switch
                  id="enableChangeDetection"
                  name="enableChangeDetection"
                  checked={formik.values.enableChangeDetection}
                  onChange={formik.handleChange}
                  disabled={isSubmitting}
                />
              }
              label={
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  Enable Change Detection
                  <Tooltip title="Detect and track changes in content over time">
                    <IconButton size="small">
                      <HelpIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </Box>
              }
            />
          </Grid>
          
          {formik.values.autoLearnHeaderFooter && (
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                id="learningPagesCount"
                name="learningPagesCount"
                label="Learning Pages Count"
                type="number"
                value={formik.values.learningPagesCount}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                error={formik.touched.learningPagesCount && Boolean(formik.errors.learningPagesCount)}
                helperText={
                  (formik.touched.learningPagesCount && formik.errors.learningPagesCount) || 
                  "Number of pages to analyze for header/footer detection"
                }
                InputProps={{ inputProps: { min: 1, max: 20 } }}
                disabled={isSubmitting}
              />
            </Grid>
          )}
          
          <Grid item xs={12} md={6}>
            <FormControlLabel
              control={
                <Switch
                  id="trackContentVersions"
                  name="trackContentVersions"
                  checked={formik.values.trackContentVersions}
                  onChange={formik.handleChange}
                  disabled={isSubmitting}
                />
              }
              label={
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  Track Content Versions
                  <Tooltip title="Keep track of different versions of the same content over time">
                    <IconButton size="small">
                      <HelpIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </Box>
              }
            />
          </Grid>
          
          {formik.values.trackContentVersions && (
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                id="maxVersionsToKeep"
                name="maxVersionsToKeep"
                label="Max Versions to Keep"
                type="number"
                value={formik.values.maxVersionsToKeep}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                error={formik.touched.maxVersionsToKeep && Boolean(formik.errors.maxVersionsToKeep)}
                helperText={
                  (formik.touched.maxVersionsToKeep && formik.errors.maxVersionsToKeep) || 
                  "Maximum number of content versions to store"
                }
                InputProps={{ inputProps: { min: 1, max: 100 } }}
                disabled={isSubmitting}
              />
            </Grid>
          )}
        </Grid>
        
        {showAdvancedOptions && (
          <>
            <Divider sx={{ my: 3 }} />
            <Typography variant="subtitle1" gutterBottom>Advanced Options</Typography>
            
            <Grid container spacing={3}>
              <Grid item xs={12} md={6}>
                <FormControlLabel
                  control={
                    <Switch
                      id="enableContinuousMonitoring"
                      name="enableContinuousMonitoring"
                      checked={formik.values.enableContinuousMonitoring}
                      onChange={formik.handleChange}
                      disabled={isSubmitting}
                    />
                  }
                  label={
                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                      Enable Continuous Monitoring
                      <Tooltip title="Automatically run the scraper at regular intervals">
                        <IconButton size="small">
                          <HelpIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </Box>
                  }
                />
              </Grid>
              
              {formik.values.enableContinuousMonitoring && (
                <Grid item xs={12} md={6}>
                  <FormControl fullWidth>
                    <InputLabel id="monitoringInterval-label">Monitoring Interval</InputLabel>
                    <Select
                      labelId="monitoringInterval-label"
                      id="monitoringInterval"
                      name="monitoringInterval"
                      value={formik.values.monitoringInterval}
                      onChange={formik.handleChange}
                      label="Monitoring Interval"
                      disabled={isSubmitting}
                    >
                      <MenuItem value={3600}>Hourly</MenuItem>
                      <MenuItem value={21600}>Every 6 hours</MenuItem>
                      <MenuItem value={43200}>Every 12 hours</MenuItem>
                      <MenuItem value={86400}>Daily</MenuItem>
                      <MenuItem value={604800}>Weekly</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
              )}
              
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  id="notificationEndpoint"
                  name="notificationEndpoint"
                  label="Notification Webhook URL"
                  value={formik.values.notificationEndpoint}
                  onChange={formik.handleChange}
                  onBlur={formik.handleBlur}
                  error={formik.touched.notificationEndpoint && Boolean(formik.errors.notificationEndpoint)}
                  helperText={
                    (formik.touched.notificationEndpoint && formik.errors.notificationEndpoint) || 
                    "Optional webhook URL for receiving notifications (leave empty to disable)"
                  }
                  disabled={isSubmitting}
                />
              </Grid>
            </Grid>
          </>
        )}
      </Paper>
      
      {/* Form submission error */}
      {formik.status && formik.status.error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {formik.status.error}
        </Alert>
      )}
      
      {/* Form Actions */}
      <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2 }}>
        <Button
          variant="outlined"
          startIcon={<CancelIcon />}
          onClick={() => window.history.back()}
          disabled={isSubmitting}
        >
          Cancel
        </Button>
        <Button
          type="submit"
          variant="contained"
          startIcon={isSubmitting ? <CircularProgress size={24} /> : <SaveIcon />}
          disabled={isSubmitting || !formik.isValid}
        >
          {isSubmitting ? 'Saving...' : isEditMode ? 'Update Scraper' : 'Create Scraper'}
        </Button>
      </Box>
    </form>
  );
};

export default ScraperForm;
