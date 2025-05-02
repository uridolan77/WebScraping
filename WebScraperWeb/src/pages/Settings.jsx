import React, { useState, useEffect } from 'react';
import {
  Box,
  Paper,
  Typography,
  Tabs,
  Tab,
  Grid,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Switch,
  FormControlLabel,
  Button,
  Divider,
  Alert,
  IconButton,
  Tooltip,
  Card,
  CardContent
} from '@mui/material';
import {
  Save as SaveIcon,
  Refresh as RefreshIcon,
  Help as HelpIcon,
  Settings as SettingsIcon,
  Security as SecurityIcon,
  Notifications as NotificationsIcon,
  Storage as StorageIcon
} from '@mui/icons-material';
import PageHeader from '../components/common/PageHeader';
import LoadingSpinner from '../components/common/LoadingSpinner';
// import { useSettings } from '../contexts/SettingsContext'; // Uncomment if you have this context

const Settings = () => {
  const [loading, setLoading] = useState(false);
  const [saveStatus, setSaveStatus] = useState(null);
  const [activeTab, setActiveTab] = useState(0);
  
  // Example settings state
  const [settings, setSettings] = useState({
    general: {
      theme: 'light',
      language: 'en',
      dateFormat: 'MM/DD/YYYY',
      timeFormat: '12h'
    },
    scraper: {
      defaultTimeout: 30000,
      maxRetries: 3,
      userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
      respectRobotsTxt: true,
      useProxy: false,
      proxyUrl: ''
    },
    notifications: {
      email: true,
      slack: false,
      slackWebhook: '',
      notifyOnError: true,
      notifyOnCompletion: true
    },
    storage: {
      retentionPeriod: 30,
      autoCleanup: true,
      exportFormat: 'json'
    }
  });

  // Handle tab change
  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
  };

  // Handle setting changes
  const handleSettingChange = (category, setting, value) => {
    setSettings(prevSettings => ({
      ...prevSettings,
      [category]: {
        ...prevSettings[category],
        [setting]: value
      }
    }));
  };

  // Mock save settings
  const handleSaveSettings = () => {
    setLoading(true);
    // Simulate API call
    setTimeout(() => {
      setLoading(false);
      setSaveStatus('success');
      // Clear success message after 3 seconds
      setTimeout(() => setSaveStatus(null), 3000);
    }, 1000);
  };

  // Reset settings to default
  const handleResetSettings = () => {
    setLoading(true);
    // Simulate API call to get default settings
    setTimeout(() => {
      // This would be replaced with actual defaults from your backend
      setSettings({
        general: {
          theme: 'light',
          language: 'en',
          dateFormat: 'MM/DD/YYYY',
          timeFormat: '12h'
        },
        scraper: {
          defaultTimeout: 30000,
          maxRetries: 3,
          userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
          respectRobotsTxt: true,
          useProxy: false,
          proxyUrl: ''
        },
        notifications: {
          email: true,
          slack: false,
          slackWebhook: '',
          notifyOnError: true,
          notifyOnCompletion: true
        },
        storage: {
          retentionPeriod: 30,
          autoCleanup: true,
          exportFormat: 'json'
        }
      });
      setLoading(false);
      setSaveStatus('reset');
      // Clear reset message after 3 seconds
      setTimeout(() => setSaveStatus(null), 3000);
    }, 1000);
  };

  // Status message component
  const StatusMessage = () => {
    if (!saveStatus) return null;
    
    return (
      <Alert 
        severity={saveStatus === 'success' ? 'success' : 'info'} 
        sx={{ mb: 2 }}
      >
        {saveStatus === 'success' ? 'Settings saved successfully!' : 'Settings reset to defaults.'}
      </Alert>
    );
  };

  return (
    <Box>
      <PageHeader title="Settings" icon={<SettingsIcon />} />
      
      <StatusMessage />
      
      <Paper sx={{ mb: 3 }}>
        <Tabs
          value={activeTab}
          onChange={handleTabChange}
          variant="scrollable"
          scrollButtons="auto"
          sx={{ borderBottom: 1, borderColor: 'divider' }}
        >
          <Tab label="General" icon={<SettingsIcon />} iconPosition="start" />
          <Tab label="Scraper" icon={<StorageIcon />} iconPosition="start" />
          <Tab label="Notifications" icon={<NotificationsIcon />} iconPosition="start" />
          <Tab label="Storage" icon={<SecurityIcon />} iconPosition="start" />
        </Tabs>
        
        <Box sx={{ p: 3 }}>
          {/* General Settings Tab */}
          {activeTab === 0 && (
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <Typography variant="h6" gutterBottom>General Settings</Typography>
                <Divider sx={{ mb: 2 }} />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel>Theme</InputLabel>
                  <Select
                    value={settings.general.theme}
                    label="Theme"
                    onChange={(e) => handleSettingChange('general', 'theme', e.target.value)}
                  >
                    <MenuItem value="light">Light</MenuItem>
                    <MenuItem value="dark">Dark</MenuItem>
                    <MenuItem value="system">System Default</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel>Language</InputLabel>
                  <Select
                    value={settings.general.language}
                    label="Language"
                    onChange={(e) => handleSettingChange('general', 'language', e.target.value)}
                  >
                    <MenuItem value="en">English</MenuItem>
                    <MenuItem value="es">Spanish</MenuItem>
                    <MenuItem value="fr">French</MenuItem>
                    <MenuItem value="de">German</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel>Date Format</InputLabel>
                  <Select
                    value={settings.general.dateFormat}
                    label="Date Format"
                    onChange={(e) => handleSettingChange('general', 'dateFormat', e.target.value)}
                  >
                    <MenuItem value="MM/DD/YYYY">MM/DD/YYYY</MenuItem>
                    <MenuItem value="DD/MM/YYYY">DD/MM/YYYY</MenuItem>
                    <MenuItem value="YYYY-MM-DD">YYYY-MM-DD</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel>Time Format</InputLabel>
                  <Select
                    value={settings.general.timeFormat}
                    label="Time Format"
                    onChange={(e) => handleSettingChange('general', 'timeFormat', e.target.value)}
                  >
                    <MenuItem value="12h">12-hour (AM/PM)</MenuItem>
                    <MenuItem value="24h">24-hour</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
            </Grid>
          )}

          {/* Scraper Settings Tab */}
          {activeTab === 1 && (
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <Typography variant="h6" gutterBottom>Scraper Settings</Typography>
                <Divider sx={{ mb: 2 }} />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Default Timeout (ms)"
                  type="number"
                  value={settings.scraper.defaultTimeout}
                  onChange={(e) => handleSettingChange('scraper', 'defaultTimeout', parseInt(e.target.value))}
                  InputProps={{
                    endAdornment: (
                      <Tooltip title="Maximum time in milliseconds to wait for a page to load">
                        <IconButton edge="end"><HelpIcon /></IconButton>
                      </Tooltip>
                    ),
                  }}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Max Retries"
                  type="number"
                  value={settings.scraper.maxRetries}
                  onChange={(e) => handleSettingChange('scraper', 'maxRetries', parseInt(e.target.value))}
                  InputProps={{
                    endAdornment: (
                      <Tooltip title="Number of times to retry a failed request">
                        <IconButton edge="end"><HelpIcon /></IconButton>
                      </Tooltip>
                    ),
                  }}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="User Agent"
                  value={settings.scraper.userAgent}
                  onChange={(e) => handleSettingChange('scraper', 'userAgent', e.target.value)}
                  multiline
                  rows={2}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.scraper.respectRobotsTxt}
                      onChange={(e) => handleSettingChange('scraper', 'respectRobotsTxt', e.target.checked)}
                    />
                  }
                  label="Respect robots.txt"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.scraper.useProxy}
                      onChange={(e) => handleSettingChange('scraper', 'useProxy', e.target.checked)}
                    />
                  }
                  label="Use Proxy"
                />
              </Grid>
              {settings.scraper.useProxy && (
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Proxy URL"
                    value={settings.scraper.proxyUrl}
                    onChange={(e) => handleSettingChange('scraper', 'proxyUrl', e.target.value)}
                    placeholder="http://proxy.example.com:8080"
                  />
                </Grid>
              )}
            </Grid>
          )}

          {/* Notifications Tab */}
          {activeTab === 2 && (
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <Typography variant="h6" gutterBottom>Notification Settings</Typography>
                <Divider sx={{ mb: 2 }} />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.notifications.email}
                      onChange={(e) => handleSettingChange('notifications', 'email', e.target.checked)}
                    />
                  }
                  label="Email Notifications"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.notifications.slack}
                      onChange={(e) => handleSettingChange('notifications', 'slack', e.target.checked)}
                    />
                  }
                  label="Slack Notifications"
                />
              </Grid>
              {settings.notifications.slack && (
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Slack Webhook URL"
                    value={settings.notifications.slackWebhook}
                    onChange={(e) => handleSettingChange('notifications', 'slackWebhook', e.target.value)}
                    placeholder="https://hooks.slack.com/services/..."
                  />
                </Grid>
              )}
              <Grid item xs={12} md={6}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.notifications.notifyOnError}
                      onChange={(e) => handleSettingChange('notifications', 'notifyOnError', e.target.checked)}
                    />
                  }
                  label="Notify on Errors"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.notifications.notifyOnCompletion}
                      onChange={(e) => handleSettingChange('notifications', 'notifyOnCompletion', e.target.checked)}
                    />
                  }
                  label="Notify on Completion"
                />
              </Grid>
            </Grid>
          )}

          {/* Storage Tab */}
          {activeTab === 3 && (
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <Typography variant="h6" gutterBottom>Storage Settings</Typography>
                <Divider sx={{ mb: 2 }} />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Data Retention Period (days)"
                  type="number"
                  value={settings.storage.retentionPeriod}
                  onChange={(e) => handleSettingChange('storage', 'retentionPeriod', parseInt(e.target.value))}
                  InputProps={{
                    endAdornment: (
                      <Tooltip title="Number of days to keep scraped data before automatic cleanup">
                        <IconButton edge="end"><HelpIcon /></IconButton>
                      </Tooltip>
                    ),
                  }}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.storage.autoCleanup}
                      onChange={(e) => handleSettingChange('storage', 'autoCleanup', e.target.checked)}
                    />
                  }
                  label="Automatic Data Cleanup"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel>Export Format</InputLabel>
                  <Select
                    value={settings.storage.exportFormat}
                    label="Export Format"
                    onChange={(e) => handleSettingChange('storage', 'exportFormat', e.target.value)}
                  >
                    <MenuItem value="json">JSON</MenuItem>
                    <MenuItem value="csv">CSV</MenuItem>
                    <MenuItem value="xml">XML</MenuItem>
                    <MenuItem value="excel">Excel</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
            </Grid>
          )}
        </Box>
        
        <Box sx={{ p: 3, pt: 0, display: 'flex', justifyContent: 'space-between' }}>
          <Button 
            variant="outlined" 
            startIcon={<RefreshIcon />}
            onClick={handleResetSettings}
            disabled={loading}
          >
            Reset to Defaults
          </Button>
          <Button 
            variant="contained" 
            color="primary" 
            startIcon={<SaveIcon />}
            onClick={handleSaveSettings}
            disabled={loading}
          >
            Save Settings
          </Button>
        </Box>
      </Paper>
      
      {/* Settings Information Cards */}
      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                <SettingsIcon sx={{ verticalAlign: 'middle', mr: 1 }} />
                About Settings
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Configure your web scraping application settings here. Changes will be applied across all scrapers and affect how the application behaves.
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                <SecurityIcon sx={{ verticalAlign: 'middle', mr: 1 }} />
                Security Note
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Proxy settings, API keys, and webhook URLs are stored securely. Make sure to use proxies and respect website terms of service when scraping.
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
      
      {loading && <LoadingSpinner />}
    </Box>
  );
};

export default Settings;