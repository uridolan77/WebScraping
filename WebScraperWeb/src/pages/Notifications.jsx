import React, { useState, useEffect } from 'react';
import { 
  Grid, 
  Card, 
  CardContent, 
  Typography, 
  Box, 
  Button,
  Paper,
  Divider,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Chip,
  Switch,
  FormControlLabel,
  TextField,
  MenuItem
} from '@mui/material';
import { 
  Notifications as NotificationsIcon,
  Email as EmailIcon,
  Chat as ChatIcon,
  PhoneAndroid as PhoneAndroidIcon,
  Web as WebIcon,
  NotificationsActive as NotificationsActiveIcon,
  Settings as SettingsIcon
} from '@mui/icons-material';
import { useScrapers } from '../contexts/ScraperContext';
import { formatDate } from '../utils/formatters';
import PageHeader from '../components/common/PageHeader';
import LoadingSpinner from '../components/common/LoadingSpinner';

const Notifications = () => {
  const { scrapers, loading, error } = useScrapers();
  const [notificationSettings, setNotificationSettings] = useState({
    emailEnabled: true,
    slackEnabled: false,
    smsEnabled: false,
    webhookEnabled: true,
    emailAddress: 'admin@example.com',
    slackWebhook: '',
    phoneNumber: '',
    webhookUrl: 'https://api.example.com/webhooks/scraper'
  });

  const [notificationHistory, setNotificationHistory] = useState([
    {
      id: 1,
      type: 'email',
      scraperName: 'UKGC Regulations',
      message: 'New content detected on UKGC website',
      timestamp: new Date(Date.now() - 3600000),
      status: 'delivered'
    },
    {
      id: 2,
      type: 'webhook',
      scraperName: 'MGA Monitoring',
      message: 'Scraper completed with 3 new documents',
      timestamp: new Date(Date.now() - 86400000),
      status: 'delivered'
    },
    {
      id: 3,
      type: 'email',
      scraperName: 'Gibraltar Regulatory Updates',
      message: 'Scraper failed - unable to access website',
      timestamp: new Date(Date.now() - 172800000),
      status: 'failed'
    },
    {
      id: 4,
      type: 'webhook',
      scraperName: 'UKGC Regulations',
      message: 'Significant content change detected',
      timestamp: new Date(Date.now() - 259200000),
      status: 'delivered'
    }
  ]);

  const [notificationEvents, setNotificationEvents] = useState([
    { id: 'scraper-start', name: 'Scraper Started', enabled: true },
    { id: 'scraper-complete', name: 'Scraper Completed', enabled: true },
    { id: 'scraper-error', name: 'Scraper Error', enabled: true },
    { id: 'content-changed', name: 'Content Changed', enabled: true },
    { id: 'significant-change', name: 'Significant Change Detected', enabled: true },
    { id: 'new-document', name: 'New Document Found', enabled: true }
  ]);

  const handleNotificationSettingChange = (setting) => (event) => {
    setNotificationSettings({
      ...notificationSettings,
      [setting]: event.target.checked
    });
  };

  const handleNotificationTextChange = (setting) => (event) => {
    setNotificationSettings({
      ...notificationSettings,
      [setting]: event.target.value
    });
  };

  const handleEventToggle = (eventId) => {
    setNotificationEvents(notificationEvents.map(event => 
      event.id === eventId ? { ...event, enabled: !event.enabled } : event
    ));
  };

  const getNotificationTypeIcon = (type) => {
    switch(type) {
      case 'email':
        return <EmailIcon />;
      case 'slack':
        return <ChatIcon />;
      case 'sms':
        return <PhoneAndroidIcon />;
      case 'webhook':
        return <WebIcon />;
      default:
        return <NotificationsIcon />;
    }
  };

  if (loading) {
    return <LoadingSpinner />;
  }

  if (error) {
    return (
      <Box p={3}>
        <Typography color="error" variant="h6">
          Error loading notifications: {error}
        </Typography>
        <Button variant="contained" sx={{ mt: 2 }}>
          Retry
        </Button>
      </Box>
    );
  }

  return (
    <Box>
      <PageHeader 
        title="Notifications" 
        subtitle="Manage notification settings and view notification history"
        icon={<NotificationsIcon />}
      />

      <Grid container spacing={3}>
        {/* Notification Channels */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" sx={{ mb: 2 }}>
              <SettingsIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
              Notification Channels
            </Typography>
            <Divider sx={{ mb: 3 }} />
            
            <Box sx={{ mb: 3 }}>
              <FormControlLabel
                control={
                  <Switch 
                    checked={notificationSettings.emailEnabled} 
                    onChange={handleNotificationSettingChange('emailEnabled')}
                    color="primary"
                  />
                }
                label="Email Notifications"
              />
              {notificationSettings.emailEnabled && (
                <TextField
                  fullWidth
                  margin="normal"
                  label="Email Address"
                  value={notificationSettings.emailAddress}
                  onChange={handleNotificationTextChange('emailAddress')}
                  variant="outlined"
                  size="small"
                />
              )}
            </Box>
            
            <Box sx={{ mb: 3 }}>
              <FormControlLabel
                control={
                  <Switch 
                    checked={notificationSettings.slackEnabled} 
                    onChange={handleNotificationSettingChange('slackEnabled')}
                    color="primary"
                  />
                }
                label="Slack Notifications"
              />
              {notificationSettings.slackEnabled && (
                <TextField
                  fullWidth
                  margin="normal"
                  label="Slack Webhook URL"
                  value={notificationSettings.slackWebhook}
                  onChange={handleNotificationTextChange('slackWebhook')}
                  variant="outlined"
                  size="small"
                />
              )}
            </Box>
            
            <Box sx={{ mb: 3 }}>
              <FormControlLabel
                control={
                  <Switch 
                    checked={notificationSettings.smsEnabled} 
                    onChange={handleNotificationSettingChange('smsEnabled')}
                    color="primary"
                  />
                }
                label="SMS Notifications"
              />
              {notificationSettings.smsEnabled && (
                <TextField
                  fullWidth
                  margin="normal"
                  label="Phone Number"
                  value={notificationSettings.phoneNumber}
                  onChange={handleNotificationTextChange('phoneNumber')}
                  variant="outlined"
                  size="small"
                />
              )}
            </Box>
            
            <Box sx={{ mb: 3 }}>
              <FormControlLabel
                control={
                  <Switch 
                    checked={notificationSettings.webhookEnabled} 
                    onChange={handleNotificationSettingChange('webhookEnabled')}
                    color="primary"
                  />
                }
                label="Webhook Notifications"
              />
              {notificationSettings.webhookEnabled && (
                <TextField
                  fullWidth
                  margin="normal"
                  label="Webhook URL"
                  value={notificationSettings.webhookUrl}
                  onChange={handleNotificationTextChange('webhookUrl')}
                  variant="outlined"
                  size="small"
                />
              )}
            </Box>
            
            <Button 
              variant="contained" 
              color="primary"
              sx={{ mt: 2 }}
            >
              Save Settings
            </Button>
          </Paper>
        </Grid>
        
        {/* Notification Events */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" sx={{ mb: 2 }}>
              <NotificationsActiveIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
              Notification Events
            </Typography>
            <Divider sx={{ mb: 3 }} />
            
            <List>
              {notificationEvents.map((event) => (
                <ListItem key={event.id} disablePadding sx={{ mb: 1 }}>
                  <ListItemText 
                    primary={event.name} 
                  />
                  <Switch
                    edge="end"
                    checked={event.enabled}
                    onChange={() => handleEventToggle(event.id)}
                    color="primary"
                  />
                </ListItem>
              ))}
            </List>
            
            <Divider sx={{ my: 3 }} />
            
            <Typography variant="subtitle1" sx={{ mb: 2 }}>
              Scraper-Specific Settings
            </Typography>
            
            <TextField
              select
              fullWidth
              label="Select Scraper"
              variant="outlined"
              size="small"
              defaultValue=""
              sx={{ mb: 2 }}
            >
              <MenuItem value="">All Scrapers</MenuItem>
              {scrapers.map((scraper) => (
                <MenuItem key={scraper.id} value={scraper.id}>
                  {scraper.name}
                </MenuItem>
              ))}
            </TextField>
            
            <Button 
              variant="contained" 
              color="primary"
              sx={{ mt: 2 }}
            >
              Apply Settings
            </Button>
          </Paper>
        </Grid>
        
        {/* Notification History */}
        <Grid item xs={12}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" sx={{ mb: 2 }}>
              Notification History
            </Typography>
            <Divider sx={{ mb: 3 }} />
            
            <List>
              {notificationHistory.map((notification) => (
                <ListItem 
                  key={notification.id}
                  sx={{ 
                    mb: 1,
                    borderRadius: 1,
                    border: '1px solid',
                    borderColor: 'divider',
                    '&:hover': { backgroundColor: 'rgba(0, 0, 0, 0.04)' }
                  }}
                >
                  <ListItemIcon>
                    {getNotificationTypeIcon(notification.type)}
                  </ListItemIcon>
                  <ListItemText 
                    primary={notification.message}
                    secondary={`${notification.scraperName} - ${formatDate(notification.timestamp, 'MMM d, yyyy HH:mm')}`}
                  />
                  <Chip 
                    label={notification.status} 
                    color={notification.status === 'delivered' ? 'success' : 'error'}
                    size="small"
                  />
                </ListItem>
              ))}
            </List>
            
            <Button 
              variant="outlined" 
              sx={{ mt: 2 }}
              fullWidth
            >
              Load More
            </Button>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default Notifications;