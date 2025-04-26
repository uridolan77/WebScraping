import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Grid,
  TextField,
  Switch,
  FormControlLabel,
  Paper,
  Divider,
  Button,
  Slider,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  InputAdornment
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import SaveIcon from '@mui/icons-material/Save';
import UploadFileIcon from '@mui/icons-material/UploadFile';

const ScraperConfigForm = ({ initialConfig, onSave, loading }) => {
  const [config, setConfig] = useState({
    name: '',
    startUrl: '',
    baseUrl: '',
    outputDirectory: 'ScrapedData',
    maxDepth: 5,
    delayBetweenRequests: 1000,
    maxConcurrentRequests: 5,
    followExternalLinks: false,
    respectRobotsTxt: true,
    
    // Header/Footer Pattern Learning
    autoLearnHeaderFooter: true,
    learningPagesCount: 5,
    
    // Content Change Detection
    enableChangeDetection: true,
    trackContentVersions: true,
    maxVersionsToKeep: 5,
    
    // Adaptive Crawling
    enableAdaptiveCrawling: true,
    
    // Smart Rate Limiting
    enableAdaptiveRateLimiting: true,
    minDelayBetweenRequests: 1000,
    maxDelayBetweenRequests: 5000,
    
    // Additional if available from config
    ...initialConfig
  });

  // Update state when initialConfig changes
  useEffect(() => {
    if (initialConfig) {
      setConfig(prev => ({
        ...prev,
        ...initialConfig
      }));
    }
  }, [initialConfig]);

  const handleChange = (field) => (event) => {
    const value = event.target.type === 'checkbox'
      ? event.target.checked
      : event.target.value;
    
    setConfig({
      ...config,
      [field]: value
    });
  };

  const handleSliderChange = (field) => (_, value) => {
    setConfig({
      ...config,
      [field]: value
    });
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    onSave(config);
  };

  return (
    <Box component="form" onSubmit={handleSubmit}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h6" gutterBottom>
          Basic Settings
        </Typography>
        
        <Grid container spacing={3}>
          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              label="Start URL"
              value={config.startUrl || ''}
              onChange={handleChange('startUrl')}
              placeholder="https://example.com"
              variant="outlined"
              required
              disabled={loading}
              helperText="The URL where scraping will begin"
            />
          </Grid>
          
          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              label="Base URL"
              value={config.baseUrl || ''}
              onChange={handleChange('baseUrl')}
              placeholder="https://example.com"
              variant="outlined"
              disabled={loading}
              helperText="Domain to stay within (if not following external links)"
            />
          </Grid>
          
          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              label="Output Directory"
              value={config.outputDirectory || 'ScrapedData'}
              onChange={handleChange('outputDirectory')}
              variant="outlined"
              disabled={loading}
              helperText="Where scraped data will be stored"
            />
          </Grid>
          
          <Grid item xs={12} sm={6}>
            <Box>
              <Typography id="max-depth-slider" gutterBottom>
                Max Depth
              </Typography>
              <Slider
                value={config.maxDepth || 5}
                onChange={handleSliderChange('maxDepth')}
                disabled={loading}
                aria-labelledby="max-depth-slider"
                min={1}
                max={20}
                marks={[
                  { value: 1, label: '1' },
                  { value: 10, label: '10' },
                  { value: 20, label: '20' }
                ]}
                valueLabelDisplay="auto"
              />
              <Typography variant="caption" color="text.secondary">
                How many links deep to crawl from start URL
              </Typography>
            </Box>
          </Grid>
          
          <Grid item xs={12}>
            <Box sx={{ display: 'flex', gap: 2 }}>
              <FormControlLabel
                control={
                  <Switch
                    checked={config.followExternalLinks || false}
                    onChange={handleChange('followExternalLinks')}
                    disabled={loading}
                    color="primary"
                  />
                }
                label="Follow External Links"
              />
              
              <FormControlLabel
                control={
                  <Switch
                    checked={config.respectRobotsTxt !== false}
                    onChange={handleChange('respectRobotsTxt')}
                    disabled={loading}
                    color="primary"
                  />
                }
                label="Respect robots.txt"
              />
            </Box>
          </Grid>
        </Grid>
      </Box>

      <Box sx={{ mb: 4 }}>
        <Typography variant="h6" gutterBottom>
          Rate Limiting
        </Typography>
        
        <Grid container spacing={3}>
          <Grid item xs={12} sm={6}>
            <Box>
              <Typography id="max-concurrent-slider" gutterBottom>
                Max Concurrent Requests
              </Typography>
              <Slider
                value={config.maxConcurrentRequests || 5}
                onChange={handleSliderChange('maxConcurrentRequests')}
                disabled={loading}
                aria-labelledby="max-concurrent-slider"
                min={1}
                max={20}
                marks={[
                  { value: 1, label: '1' },
                  { value: 10, label: '10' },
                  { value: 20, label: '20' }
                ]}
                valueLabelDisplay="auto"
              />
            </Box>
          </Grid>
          
          <Grid item xs={12} sm={6}>
            <Box>
              <Typography id="delay-slider" gutterBottom>
                Delay Between Requests (ms)
              </Typography>
              <Slider
                value={config.delayBetweenRequests || 1000}
                onChange={handleSliderChange('delayBetweenRequests')}
                disabled={loading}
                aria-labelledby="delay-slider"
                min={100}
                max={5000}
                step={100}
                marks={[
                  { value: 100, label: '100ms' },
                  { value: 1000, label: '1s' },
                  { value: 5000, label: '5s' }
                ]}
                valueLabelDisplay="auto"
              />
            </Box>
          </Grid>
        </Grid>
      </Box>
      
      <Accordion sx={{ mb: 2 }}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography>Header/Footer Pattern Learning</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Switch
                    checked={config.autoLearnHeaderFooter !== false}
                    onChange={handleChange('autoLearnHeaderFooter')}
                    disabled={loading}
                    color="primary"
                  />
                }
                label="Auto-Learn Header/Footer Patterns"
              />
              <Typography variant="caption" display="block" color="text.secondary">
                Automatically identify and filter out repeated header/footer elements from content
              </Typography>
            </Grid>
            
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                type="number"
                label="Learning Pages Count"
                value={config.learningPagesCount || 5}
                onChange={handleChange('learningPagesCount')}
                variant="outlined"
                disabled={loading || !config.autoLearnHeaderFooter}
                InputProps={{
                  inputProps: { min: 1, max: 100 }
                }}
                helperText="Number of pages to analyze for pattern learning"
              />
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>
      
      <Accordion sx={{ mb: 2 }}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography>Content Change Detection</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Switch
                    checked={config.enableChangeDetection !== false}
                    onChange={handleChange('enableChangeDetection')}
                    disabled={loading}
                    color="primary"
                  />
                }
                label="Enable Content Change Detection"
              />
            </Grid>
            
            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Switch
                    checked={config.trackContentVersions !== false}
                    onChange={handleChange('trackContentVersions')}
                    disabled={loading || !config.enableChangeDetection}
                    color="primary"
                  />
                }
                label="Track Content Versions"
              />
            </Grid>
            
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                type="number"
                label="Max Versions To Keep"
                value={config.maxVersionsToKeep || 5}
                onChange={handleChange('maxVersionsToKeep')}
                variant="outlined"
                disabled={loading || !config.enableChangeDetection || !config.trackContentVersions}
                InputProps={{
                  inputProps: { min: 1, max: 100 }
                }}
                helperText="Maximum number of historical versions to keep per page"
              />
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>
      
      <Accordion sx={{ mb: 2 }}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography>Adaptive Crawling Strategies</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Switch
                    checked={config.enableAdaptiveCrawling !== false}
                    onChange={handleChange('enableAdaptiveCrawling')}
                    disabled={loading}
                    color="primary"
                  />
                }
                label="Enable Adaptive Crawling"
              />
              <Typography variant="caption" display="block" color="text.secondary">
                Automatically adjust crawl strategies based on page content quality and structure
              </Typography>
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>
      
      <Accordion sx={{ mb: 4 }}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography>Smart Rate Limiting</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Switch
                    checked={config.enableAdaptiveRateLimiting !== false}
                    onChange={handleChange('enableAdaptiveRateLimiting')}
                    disabled={loading}
                    color="primary"
                  />
                }
                label="Enable Adaptive Rate Limiting"
              />
              <Typography variant="caption" display="block" color="text.secondary">
                Automatically adjust request rates based on server response times
              </Typography>
            </Grid>
            
            <Grid item xs={12}>
              <Box>
                <Typography id="delay-range-slider" gutterBottom>
                  Delay Between Requests (ms)
                </Typography>
                <Slider
                  value={[
                    config.minDelayBetweenRequests || 1000,
                    config.maxDelayBetweenRequests || 5000
                  ]}
                  onChange={(_, newValue) => {
                    setConfig({
                      ...config,
                      minDelayBetweenRequests: newValue[0],
                      maxDelayBetweenRequests: newValue[1]
                    });
                  }}
                  disabled={loading || !config.enableAdaptiveRateLimiting}
                  aria-labelledby="delay-range-slider"
                  min={100}
                  max={10000}
                  step={100}
                  marks={[
                    { value: 1000, label: '1s' },
                    { value: 5000, label: '5s' },
                    { value: 10000, label: '10s' }
                  ]}
                  valueLabelDisplay="auto"
                />
              </Box>
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>
      
      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
        <Box>
          <Button
            variant="outlined"
            startIcon={<SaveIcon />}
            onClick={() => {
              localStorage.setItem('scraperConfig', JSON.stringify(config));
              alert('Configuration saved to browser storage!');
            }}
            sx={{ mr: 1 }}
            disabled={loading}
          >
            Save Config
          </Button>
          <Button
            variant="outlined"
            startIcon={<UploadFileIcon />}
            onClick={() => {
              const savedConfig = localStorage.getItem('scraperConfig');
              if (savedConfig) {
                setConfig(JSON.parse(savedConfig));
                alert('Configuration loaded from browser storage!');
              } else {
                alert('No saved configuration found!');
              }
            }}
            disabled={loading}
          >
            Load Config
          </Button>
        </Box>
        
        <Button
          type="submit"
          variant="contained"
          color="primary"
          size="large"
          startIcon={<SaveIcon />}
          disabled={loading}
        >
          Save Changes
        </Button>
      </Box>
    </Box>
  );
};

export default ScraperConfigForm;