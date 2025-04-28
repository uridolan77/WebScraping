import React, { useState } from 'react';
import { 
  Container, 
  Box, 
  Typography, 
  Paper, 
  Button, 
  TextField, 
  FormControl, 
  FormControlLabel, 
  Switch, 
  Divider, 
  Grid, 
  Card, 
  CardContent, 
  Tabs, 
  Tab, 
  Alert, 
  CircularProgress, 
  Slider, 
  Select, 
  MenuItem, 
  InputLabel
} from '@mui/material';
import { 
  Save as SaveIcon, 
  Refresh as RefreshIcon, 
  Delete as DeleteIcon, 
  Backup as BackupIcon, 
  Restore as RestoreIcon
} from '@mui/icons-material';
import PageHeader from '../components/common/PageHeader';

// Tab panel component
function TabPanel(props) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`settings-tabpanel-${index}`}
      aria-labelledby={`settings-tab-${index}`}
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

const Settings = () => {
  const [tabValue, setTabValue] = useState(0);
  const [loading, setLoading] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);
  const [error, setError] = useState(null);
  
  // General settings
  const [generalSettings, setGeneralSettings] = useState({
    apiUrl: 'https://localhost:7143',
    defaultOutputDirectory: 'ScrapedData',
    enableLogging: true,
    logLevel: 'INFO',
    maxLogSize: 100,
    autoUpdateCheck: true
  });
  
  // Scraper settings
  const [scraperSettings, setScraperSettings] = useState({
    defaultDelayBetweenRequests: 1000,
    defaultMaxConcurrentRequests: 5,
    defaultMaxDepth: 5,
    respectRobotsTxt: true,
    userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
    defaultTimeout: 30000,
    maxRetries: 3,
    enableProxy: false,
    proxyUrl: ''
  });
  
  // Storage settings
  const [storageSettings, setStorageSettings] = useState({
    storageType: 'LOCAL',
    compressStoredContent: true,
    maxVersionsToKeep: 5,
    enableContentDedupe: true,
    backupFrequency: 'DAILY',
    backupLocation: 'Backups',
    retentionPeriod: 30
  });

  // Handle tab change
  const handleTabChange = (event, newValue) => {
    setTabValue(newValue);
  };

  // Handle general settings change
  const handleGeneralSettingsChange = (e) => {
    const { name, value, checked, type } = e.target;
    setGeneralSettings(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  // Handle scraper settings change
  const handleScraperSettingsChange = (e) => {
    const { name, value, checked, type } = e.target;
    setScraperSettings(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  // Handle storage settings change
  const handleStorageSettingsChange = (e) => {
    const { name, value, checked, type } = e.target;
    setStorageSettings(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  // Handle slider change
  const handleSliderChange = (name, section) => (event, newValue) => {
    if (section === 'general') {
      setGeneralSettings(prev => ({
        ...prev,
        [name]: newValue
      }));
    } else if (section === 'scraper') {
      setScraperSettings(prev => ({
        ...prev,
        [name]: newValue
      }));
    } else if (section === 'storage') {
      setStorageSettings(prev => ({
        ...prev,
        [name]: newValue
      }));
    }
  };

  // Save settings
  const handleSaveSettings = () => {
    setLoading(true);
    setSaveSuccess(false);
    setError(null);
    
    // Mock API call
    setTimeout(() => {
      setLoading(false);
      setSaveSuccess(true);
      
      // Reset success message after 3 seconds
      setTimeout(() => {
        setSaveSuccess(false);
      }, 3000);
    }, 1500);
  };

  // Create backup
  const handleCreateBackup = () => {
    setLoading(true);
    
    // Mock API call
    setTimeout(() => {
      setLoading(false);
      alert('Backup created successfully');
    }, 1500);
  };

  // Restore from backup
  const handleRestoreBackup = () => {
    if (window.confirm('Are you sure you want to restore from backup? This will overwrite current settings.')) {
      setLoading(true);
      
      // Mock API call
      setTimeout(() => {
        setLoading(false);
        alert('Settings restored from backup');
      }, 1500);
    }
  };

  // Clear all data
  const handleClearAllData = () => {
    if (window.confirm('Are you sure you want to clear all data? This action cannot be undone.')) {
      setLoading(true);
      
      // Mock API call
      setTimeout(() => {
        setLoading(false);
        alert('All data has been cleared');
      }, 1500);
    }
  };

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      {/* Header */}
      <PageHeader
        title="Settings"
        subtitle="Configure application settings"
        breadcrumbs={[
          { text: 'Dashboard', path: '/' },
          { text: 'Settings' }
        ]}
      />

      {/* Success Alert */}
      {saveSuccess && (
        <Alert severity="success" sx={{ mb: 3 }}>
          Settings saved successfully
        </Alert>
      )}

      {/* Error Alert */}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Tabs */}
      <Paper sx={{ mb: 3 }}>
        <Tabs 
          value={tabValue} 
          onChange={handleTabChange}
          variant="fullWidth"
        >
          <Tab label="General" />
          <Tab label="Scraper" />
          <Tab label="Storage" />
          <Tab label="Backup & Restore" />
        </Tabs>
        
        {/* General Settings Tab */}
        <TabPanel value={tabValue} index={0}>
          <Typography variant="h6" gutterBottom>
            General Settings
          </Typography>
          <Divider sx={{ mb: 3 }} />
          
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <TextField
                label="API URL"
                fullWidth
                margin="normal"
                name="apiUrl"
                value={generalSettings.apiUrl}
                onChange={handleGeneralSettingsChange}
              />
              
              <TextField
                label="Default Output Directory"
                fullWidth
                margin="normal"
                name="defaultOutputDirectory"
                value={generalSettings.defaultOutputDirectory}
                onChange={handleGeneralSettingsChange}
              />
              
              <FormControl fullWidth margin="normal">
                <InputLabel id="log-level-label">Log Level</InputLabel>
                <Select
                  labelId="log-level-label"
                  name="logLevel"
                  value={generalSettings.logLevel}
                  onChange={handleGeneralSettingsChange}
                  label="Log Level"
                >
                  <MenuItem value="DEBUG">Debug</MenuItem>
                  <MenuItem value="INFO">Info</MenuItem>
                  <MenuItem value="WARNING">Warning</MenuItem>
                  <MenuItem value="ERROR">Error</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            
            <Grid item xs={12} md={6}>
              <Box sx={{ mt: 2 }}>
                <Typography gutterBottom>Max Log Size (MB)</Typography>
                <Slider
                  value={generalSettings.maxLogSize}
                  onChange={handleSliderChange('maxLogSize', 'general')}
                  valueLabelDisplay="auto"
                  step={10}
                  marks
                  min={10}
                  max={500}
                />
              </Box>
              
              <Box sx={{ mt: 4 }}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={generalSettings.enableLogging}
                      onChange={handleGeneralSettingsChange}
                      name="enableLogging"
                    />
                  }
                  label="Enable Logging"
                />
              </Box>
              
              <Box sx={{ mt: 2 }}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={generalSettings.autoUpdateCheck}
                      onChange={handleGeneralSettingsChange}
                      name="autoUpdateCheck"
                    />
                  }
                  label="Check for Updates Automatically"
                />
              </Box>
            </Grid>
          </Grid>
        </TabPanel>
        
        {/* Scraper Settings Tab */}
        <TabPanel value={tabValue} index={1}>
          <Typography variant="h6" gutterBottom>
            Scraper Settings
          </Typography>
          <Divider sx={{ mb: 3 }} />
          
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Box sx={{ mb: 3 }}>
                <Typography gutterBottom>Default Delay Between Requests (ms)</Typography>
                <Slider
                  value={scraperSettings.defaultDelayBetweenRequests}
                  onChange={handleSliderChange('defaultDelayBetweenRequests', 'scraper')}
                  valueLabelDisplay="auto"
                  step={100}
                  marks
                  min={0}
                  max={5000}
                />
              </Box>
              
              <Box sx={{ mb: 3 }}>
                <Typography gutterBottom>Default Max Concurrent Requests</Typography>
                <Slider
                  value={scraperSettings.defaultMaxConcurrentRequests}
                  onChange={handleSliderChange('defaultMaxConcurrentRequests', 'scraper')}
                  valueLabelDisplay="auto"
                  step={1}
                  marks
                  min={1}
                  max={20}
                />
              </Box>
              
              <Box sx={{ mb: 3 }}>
                <Typography gutterBottom>Default Max Depth</Typography>
                <Slider
                  value={scraperSettings.defaultMaxDepth}
                  onChange={handleSliderChange('defaultMaxDepth', 'scraper')}
                  valueLabelDisplay="auto"
                  step={1}
                  marks
                  min={1}
                  max={20}
                />
              </Box>
              
              <Box sx={{ mb: 3 }}>
                <Typography gutterBottom>Default Timeout (ms)</Typography>
                <Slider
                  value={scraperSettings.defaultTimeout}
                  onChange={handleSliderChange('defaultTimeout', 'scraper')}
                  valueLabelDisplay="auto"
                  step={1000}
                  marks
                  min={5000}
                  max={60000}
                />
              </Box>
            </Grid>
            
            <Grid item xs={12} md={6}>
              <TextField
                label="User Agent"
                fullWidth
                margin="normal"
                name="userAgent"
                value={scraperSettings.userAgent}
                onChange={handleScraperSettingsChange}
              />
              
              <Box sx={{ mt: 3 }}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={scraperSettings.respectRobotsTxt}
                      onChange={handleScraperSettingsChange}
                      name="respectRobotsTxt"
                    />
                  }
                  label="Respect robots.txt"
                />
              </Box>
              
              <Box sx={{ mt: 2 }}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={scraperSettings.enableProxy}
                      onChange={handleScraperSettingsChange}
                      name="enableProxy"
                    />
                  }
                  label="Enable Proxy"
                />
              </Box>
              
              <TextField
                label="Proxy URL"
                fullWidth
                margin="normal"
                name="proxyUrl"
                value={scraperSettings.proxyUrl}
                onChange={handleScraperSettingsChange}
                disabled={!scraperSettings.enableProxy}
                placeholder="http://proxy:port"
              />
              
              <Box sx={{ mt: 3 }}>
                <Typography gutterBottom>Max Retries</Typography>
                <Slider
                  value={scraperSettings.maxRetries}
                  onChange={handleSliderChange('maxRetries', 'scraper')}
                  valueLabelDisplay="auto"
                  step={1}
                  marks
                  min={0}
                  max={10}
                />
              </Box>
            </Grid>
          </Grid>
        </TabPanel>
        
        {/* Storage Settings Tab */}
        <TabPanel value={tabValue} index={2}>
          <Typography variant="h6" gutterBottom>
            Storage Settings
          </Typography>
          <Divider sx={{ mb: 3 }} />
          
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth margin="normal">
                <InputLabel id="storage-type-label">Storage Type</InputLabel>
                <Select
                  labelId="storage-type-label"
                  name="storageType"
                  value={storageSettings.storageType}
                  onChange={handleStorageSettingsChange}
                  label="Storage Type"
                >
                  <MenuItem value="LOCAL">Local Storage</MenuItem>
                  <MenuItem value="DATABASE">Database</MenuItem>
                  <MenuItem value="CLOUD">Cloud Storage</MenuItem>
                </Select>
              </FormControl>
              
              <Box sx={{ mt: 3 }}>
                <Typography gutterBottom>Max Versions to Keep</Typography>
                <Slider
                  value={storageSettings.maxVersionsToKeep}
                  onChange={handleSliderChange('maxVersionsToKeep', 'storage')}
                  valueLabelDisplay="auto"
                  step={1}
                  marks
                  min={1}
                  max={20}
                />
              </Box>
              
              <FormControl fullWidth margin="normal">
                <InputLabel id="backup-frequency-label">Backup Frequency</InputLabel>
                <Select
                  labelId="backup-frequency-label"
                  name="backupFrequency"
                  value={storageSettings.backupFrequency}
                  onChange={handleStorageSettingsChange}
                  label="Backup Frequency"
                >
                  <MenuItem value="DAILY">Daily</MenuItem>
                  <MenuItem value="WEEKLY">Weekly</MenuItem>
                  <MenuItem value="MONTHLY">Monthly</MenuItem>
                  <MenuItem value="NEVER">Never</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            
            <Grid item xs={12} md={6}>
              <TextField
                label="Backup Location"
                fullWidth
                margin="normal"
                name="backupLocation"
                value={storageSettings.backupLocation}
                onChange={handleStorageSettingsChange}
              />
              
              <Box sx={{ mt: 3 }}>
                <Typography gutterBottom>Retention Period (days)</Typography>
                <Slider
                  value={storageSettings.retentionPeriod}
                  onChange={handleSliderChange('retentionPeriod', 'storage')}
                  valueLabelDisplay="auto"
                  step={1}
                  marks
                  min={1}
                  max={365}
                />
              </Box>
              
              <Box sx={{ mt: 3 }}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={storageSettings.compressStoredContent}
                      onChange={handleStorageSettingsChange}
                      name="compressStoredContent"
                    />
                  }
                  label="Compress Stored Content"
                />
              </Box>
              
              <Box sx={{ mt: 2 }}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={storageSettings.enableContentDedupe}
                      onChange={handleStorageSettingsChange}
                      name="enableContentDedupe"
                    />
                  }
                  label="Enable Content Deduplication"
                />
              </Box>
            </Grid>
          </Grid>
        </TabPanel>
        
        {/* Backup & Restore Tab */}
        <TabPanel value={tabValue} index={3}>
          <Typography variant="h6" gutterBottom>
            Backup & Restore
          </Typography>
          <Divider sx={{ mb: 3 }} />
          
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Backup
                  </Typography>
                  <Typography variant="body2" color="text.secondary" paragraph>
                    Create a backup of all your scrapers, settings, and data.
                  </Typography>
                  <Button
                    variant="contained"
                    startIcon={loading ? <CircularProgress size={24} /> : <BackupIcon />}
                    onClick={handleCreateBackup}
                    disabled={loading}
                    fullWidth
                  >
                    Create Backup
                  </Button>
                </CardContent>
              </Card>
            </Grid>
            
            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Restore
                  </Typography>
                  <Typography variant="body2" color="text.secondary" paragraph>
                    Restore your scrapers, settings, and data from a backup.
                  </Typography>
                  <Button
                    variant="outlined"
                    startIcon={loading ? <CircularProgress size={24} /> : <RestoreIcon />}
                    onClick={handleRestoreBackup}
                    disabled={loading}
                    fullWidth
                  >
                    Restore from Backup
                  </Button>
                </CardContent>
              </Card>
            </Grid>
            
            <Grid item xs={12}>
              <Card sx={{ bgcolor: '#fff8f8' }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom color="error">
                    Danger Zone
                  </Typography>
                  <Typography variant="body2" paragraph>
                    Clear all data including scrapers, settings, and stored content.
                    This action cannot be undone.
                  </Typography>
                  <Button
                    variant="outlined"
                    color="error"
                    startIcon={loading ? <CircularProgress size={24} /> : <DeleteIcon />}
                    onClick={handleClearAllData}
                    disabled={loading}
                  >
                    Clear All Data
                  </Button>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </TabPanel>
      </Paper>
      
      {/* Save Button */}
      <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
        <Button
          variant="contained"
          startIcon={loading ? <CircularProgress size={24} /> : <SaveIcon />}
          onClick={handleSaveSettings}
          disabled={loading}
        >
          Save Settings
        </Button>
      </Box>
    </Container>
  );
};

export default Settings;
