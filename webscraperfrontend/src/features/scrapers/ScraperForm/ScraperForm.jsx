import { useState, useEffect } from 'react';
import { 
  Box, 
  TextField, 
  Button, 
  Grid, 
  Paper, 
  Typography, 
  FormControl, 
  FormLabel, 
  RadioGroup, 
  FormControlLabel, 
  Radio, 
  Switch, 
  Divider,
  MenuItem,
  InputAdornment,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Chip,
  CircularProgress
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import SaveIcon from '@mui/icons-material/Save';
import CancelIcon from '@mui/icons-material/Cancel';
import DeleteIcon from '@mui/icons-material/Delete';
import BuildIcon from '@mui/icons-material/Build';

const ScraperForm = ({ 
  initialData = null, 
  onSubmit, 
  onCancel,
  onDelete = null,
  loading = false,
  error = null
}) => {
  // Default data for a new scraper
  const defaultData = {
    name: '',
    url: '',
    runMode: 'onDemand',
    outputFormat: 'json',
    contentSelector: 'body',
    navigationType: 'static',
    browserSettings: {
      useHeadlessBrowser: false,
      userAgent: '',
      waitForSelector: '',
      navigationTimeout: 30000,
    },
    advancedSettings: {
      maxDepth: 1,
      maxPages: 10,
      rateLimitDelay: 1000,
      followExternalLinks: false,
      captureScreenshots: false,
      scripts: '',
      cookies: '',
      headers: '',
    },
    active: true
  };

  // Initialize form state
  const [formData, setFormData] = useState(initialData || defaultData);
  const [expanded, setExpanded] = useState({
    browserSettings: false,
    advancedSettings: false
  });

  // Update form data if initialData changes (like when editing)
  useEffect(() => {
    if (initialData) {
      setFormData(initialData);
    }
  }, [initialData]);

  const isEditMode = !!initialData;

  const handleChange = (event) => {
    const { name, value, checked, type } = event.target;
    
    // Handle nested properties
    if (name.includes('.')) {
      const [section, field] = name.split('.');
      setFormData({
        ...formData,
        [section]: {
          ...formData[section],
          [field]: type === 'checkbox' ? checked : value
        }
      });
    } else {
      setFormData({
        ...formData,
        [name]: type === 'checkbox' ? checked : value
      });
    }
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    onSubmit(formData);
  };

  const handleTestSelector = () => {
    // This would be implemented to test the CSS selector against the target URL
    console.log("Testing selector:", formData.contentSelector);
    alert(`Testing selector: ${formData.contentSelector} (Not implemented yet)`);
  };

  const handleExpandChange = (panel) => (event, isExpanded) => {
    setExpanded({
      ...expanded,
      [panel]: isExpanded
    });
  };

  return (
    <Box component="form" onSubmit={handleSubmit} noValidate sx={{ mt: 2 }}>
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" gutterBottom>
          Basic Settings
        </Typography>
        
        <Grid container spacing={3}>
          <Grid item xs={12} sm={6}>
            <TextField
              required
              fullWidth
              label="Scraper Name"
              name="name"
              value={formData.name}
              onChange={handleChange}
              disabled={loading}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField
              required
              fullWidth
              label="Target URL"
              name="url"
              value={formData.url}
              onChange={handleChange}
              placeholder="https://example.com"
              disabled={loading}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              label="CSS Selector for Content"
              name="contentSelector"
              value={formData.contentSelector}
              onChange={handleChange}
              disabled={loading}
              helperText="CSS selector to extract specific content"
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <Button 
                      onClick={handleTestSelector} 
                      variant="text" 
                      size="small"
                      disabled={!formData.url || !formData.contentSelector || loading}
                    >
                      Test
                    </Button>
                  </InputAdornment>
                ),
              }}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField
              select
              fullWidth
              label="Navigation Type"
              name="navigationType"
              value={formData.navigationType}
              onChange={handleChange}
              disabled={loading}
            >
              <MenuItem value="static">Static</MenuItem>
              <MenuItem value="dynamic">Dynamic (JavaScript)</MenuItem>
              <MenuItem value="pagination">Pagination</MenuItem>
              <MenuItem value="sitemap">Sitemap Based</MenuItem>
            </TextField>
          </Grid>
          <Grid item xs={12} sm={6}>
            <FormControl component="fieldset">
              <FormLabel component="legend">Run Mode</FormLabel>
              <RadioGroup 
                row 
                name="runMode" 
                value={formData.runMode} 
                onChange={handleChange}
              >
                <FormControlLabel 
                  value="onDemand" 
                  control={<Radio disabled={loading} />} 
                  label="On Demand" 
                />
                <FormControlLabel 
                  value="scheduled" 
                  control={<Radio disabled={loading} />} 
                  label="Scheduled" 
                />
              </RadioGroup>
            </FormControl>
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField
              select
              fullWidth
              label="Output Format"
              name="outputFormat"
              value={formData.outputFormat}
              onChange={handleChange}
              disabled={loading}
            >
              <MenuItem value="json">JSON</MenuItem>
              <MenuItem value="csv">CSV</MenuItem>
              <MenuItem value="html">HTML</MenuItem>
              <MenuItem value="text">Plain Text</MenuItem>
            </TextField>
          </Grid>
          <Grid item xs={12}>
            <FormControlLabel
              control={
                <Switch 
                  checked={formData.active}
                  onChange={handleChange}
                  name="active"
                  color="primary"
                  disabled={loading}
                />
              }
              label="Active"
            />
          </Grid>
        </Grid>
      </Paper>

      {/* Browser Settings */}
      <Accordion 
        expanded={expanded.browserSettings} 
        onChange={handleExpandChange('browserSettings')}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography>Browser Settings</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Switch 
                    checked={formData.browserSettings.useHeadlessBrowser}
                    onChange={handleChange}
                    name="browserSettings.useHeadlessBrowser"
                    color="primary"
                    disabled={loading}
                  />
                }
                label="Use Headless Browser"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="User Agent"
                name="browserSettings.userAgent"
                value={formData.browserSettings.userAgent}
                onChange={handleChange}
                disabled={loading}
                placeholder="Mozilla/5.0 (Windows NT 10.0; Win64; x64)..."
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Wait for Selector"
                name="browserSettings.waitForSelector"
                value={formData.browserSettings.waitForSelector}
                onChange={handleChange}
                disabled={loading}
                placeholder="#content, .main-section, etc."
                helperText="Wait for this element to load before scraping"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Navigation Timeout (ms)"
                name="browserSettings.navigationTimeout"
                type="number"
                value={formData.browserSettings.navigationTimeout}
                onChange={handleChange}
                disabled={loading}
              />
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* Advanced Settings */}
      <Accordion 
        expanded={expanded.advancedSettings} 
        onChange={handleExpandChange('advancedSettings')}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography>Advanced Settings</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Max Depth"
                name="advancedSettings.maxDepth"
                type="number"
                value={formData.advancedSettings.maxDepth}
                onChange={handleChange}
                disabled={loading}
                helperText="Maximum link traversal depth"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Max Pages"
                name="advancedSettings.maxPages"
                type="number"
                value={formData.advancedSettings.maxPages}
                onChange={handleChange}
                disabled={loading}
                helperText="Maximum pages to scrape"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Rate Limit Delay (ms)"
                name="advancedSettings.rateLimitDelay"
                type="number"
                value={formData.advancedSettings.rateLimitDelay}
                onChange={handleChange}
                disabled={loading}
                helperText="Delay between requests"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={
                  <Switch 
                    checked={formData.advancedSettings.followExternalLinks}
                    onChange={handleChange}
                    name="advancedSettings.followExternalLinks"
                    color="primary"
                    disabled={loading}
                  />
                }
                label="Follow External Links"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={
                  <Switch 
                    checked={formData.advancedSettings.captureScreenshots}
                    onChange={handleChange}
                    name="advancedSettings.captureScreenshots"
                    color="primary"
                    disabled={loading}
                  />
                }
                label="Capture Screenshots"
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Custom Scripts"
                name="advancedSettings.scripts"
                value={formData.advancedSettings.scripts}
                onChange={handleChange}
                disabled={loading}
                multiline
                rows={3}
                placeholder="JavaScript code to execute on the page"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Cookies"
                name="advancedSettings.cookies"
                value={formData.advancedSettings.cookies}
                onChange={handleChange}
                disabled={loading}
                multiline
                rows={3}
                placeholder="name=value; name2=value2"
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Headers"
                name="advancedSettings.headers"
                value={formData.advancedSettings.headers}
                onChange={handleChange}
                disabled={loading}
                multiline
                rows={3}
                placeholder="HeaderName: value"
              />
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {error && (
        <Typography color="error" sx={{ mt: 2 }}>
          {error}
        </Typography>
      )}

      <Box sx={{ mt: 3, display: 'flex', justifyContent: 'space-between' }}>
        <Box>
          <Button 
            variant="outlined" 
            color="secondary"
            onClick={onCancel}
            disabled={loading}
            startIcon={<CancelIcon />}
            sx={{ mr: 1 }}
          >
            Cancel
          </Button>
          <Button
            variant="contained"
            color="primary"
            type="submit"
            disabled={loading}
            startIcon={loading ? <CircularProgress size={20} /> : <SaveIcon />}
          >
            {loading ? 'Saving...' : isEditMode ? 'Update Scraper' : 'Create Scraper'}
          </Button>
        </Box>
        
        {isEditMode && onDelete && (
          <Button
            variant="outlined"
            color="error"
            disabled={loading}
            onClick={onDelete}
            startIcon={<DeleteIcon />}
          >
            Delete Scraper
          </Button>
        )}
      </Box>
    </Box>
  );
};

export default ScraperForm;