import React, { useState, useEffect } from 'react';
import { 
  Container, 
  Box, 
  Typography, 
  Paper, 
  Button, 
  IconButton, 
  List, 
  ListItem, 
  ListItemText, 
  ListItemIcon, 
  ListItemSecondaryAction,
  Divider,
  Tabs,
  Tab,
  TextField,
  Switch,
  FormControlLabel,
  FormGroup,
  Grid,
  Card,
  CardContent,
  Chip,
  Tooltip,
  CircularProgress,
  Alert
} from '@mui/material';
import { 
  Notifications as NotificationsIcon,
  NotificationsActive as NotificationsActiveIcon,
  NotificationsOff as NotificationsOffIcon,
  Delete as DeleteIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Warning as WarningIcon,
  Info as InfoIcon,
  Settings as SettingsIcon,
  Refresh as RefreshIcon,
  Send as SendIcon
} from '@mui/icons-material';
import PageHeader from '../components/common/PageHeader';
import LoadingSpinner from '../components/common/LoadingSpinner';
import { formatRelativeTime } from '../utils/formatters';

// Mock data for notifications
const mockNotifications = [
  {
    id: '1',
    title: 'Significant content change detected',
    message: 'UKGC website has significant changes on the licensing page',
    type: 'change',
    severity: 'high',
    timestamp: new Date(Date.now() - 3600000),
    read: false,
    scraperId: 'd6d6eb97-7136-4eaf-b7ca-16ed6202c7ad',
    scraperName: 'ukgc',
    url: 'https://www.gamblingcommission.gov.uk/licensees-and-businesses'
  },
  {
    id: '2',
    title: 'Scraper completed successfully',
    message: 'MGA scraper has completed its run successfully',
    type: 'status',
    severity: 'info',
    timestamp: new Date(Date.now() - 86400000),
    read: true,
    scraperId: '1af8de30-c878-4507-9ff6-5e595960e14c',
    scraperName: 'mga'
  },
  {
    id: '3',
    title: 'Scraper failed',
    message: 'Gibraltar scraper failed due to connection timeout',
    type: 'error',
    severity: 'high',
    timestamp: new Date(Date.now() - 172800000),
    read: false,
    scraperId: '3',
    scraperName: 'gibraltar'
  },
  {
    id: '4',
    title: 'New regulatory document detected',
    message: 'New PDF document found on UKGC website: "Updated AML Guidelines 2023"',
    type: 'document',
    severity: 'medium',
    timestamp: new Date(Date.now() - 259200000),
    read: true,
    scraperId: 'd6d6eb97-7136-4eaf-b7ca-16ed6202c7ad',
    scraperName: 'ukgc',
    documentUrl: 'https://www.gamblingcommission.gov.uk/documents/aml-guidelines-2023.pdf'
  }
];

// Mock notification settings
const mockSettings = {
  emailNotifications: true,
  emailAddress: 'admin@example.com',
  webhookNotifications: false,
  webhookUrl: '',
  notifyOnChanges: true,
  notifyOnErrors: true,
  notifyOnCompletion: false,
  notifyOnNewDocuments: true,
  highPriorityOnly: false
};

// Tab panel component
function TabPanel(props) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`notifications-tabpanel-${index}`}
      aria-labelledby={`notifications-tab-${index}`}
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

const Notifications = () => {
  const [notifications, setNotifications] = useState(mockNotifications);
  const [settings, setSettings] = useState(mockSettings);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [tabValue, setTabValue] = useState(0);
  const [testWebhookLoading, setTestWebhookLoading] = useState(false);
  const [testWebhookResult, setTestWebhookResult] = useState(null);

  // Handle tab change
  const handleTabChange = (event, newValue) => {
    setTabValue(newValue);
  };

  // Mark notification as read
  const handleMarkAsRead = (id) => {
    setNotifications(prev => 
      prev.map(notification => 
        notification.id === id ? { ...notification, read: true } : notification
      )
    );
  };

  // Delete notification
  const handleDeleteNotification = (id) => {
    setNotifications(prev => prev.filter(notification => notification.id !== id));
  };

  // Mark all notifications as read
  const handleMarkAllAsRead = () => {
    setNotifications(prev => 
      prev.map(notification => ({ ...notification, read: true }))
    );
  };

  // Handle settings change
  const handleSettingsChange = (e) => {
    const { name, value, checked } = e.target;
    setSettings(prev => ({
      ...prev,
      [name]: e.target.type === 'checkbox' ? checked : value
    }));
  };

  // Test webhook
  const handleTestWebhook = async () => {
    if (!settings.webhookUrl) {
      setError('Please enter a webhook URL');
      return;
    }

    try {
      setTestWebhookLoading(true);
      setTestWebhookResult(null);
      
      // Mock API call
      await new Promise(resolve => setTimeout(resolve, 1500));
      
      setTestWebhookResult({
        success: true,
        message: 'Test notification sent successfully'
      });
    } catch (err) {
      setTestWebhookResult({
        success: false,
        message: `Failed to send test notification: ${err.message}`
      });
    } finally {
      setTestWebhookLoading(false);
    }
  };

  // Save settings
  const handleSaveSettings = () => {
    // Mock API call
    setLoading(true);
    
    setTimeout(() => {
      setLoading(false);
      // Show success message
      alert('Settings saved successfully');
    }, 1000);
  };

  // Get icon for notification type
  const getNotificationIcon = (notification) => {
    switch (notification.type) {
      case 'change':
        return <NotificationsActiveIcon color="primary" />;
      case 'status':
        return <CheckCircleIcon color="success" />;
      case 'error':
        return <ErrorIcon color="error" />;
      case 'document':
        return <InfoIcon color="info" />;
      default:
        return <NotificationsIcon />;
    }
  };

  // Get severity chip
  const getSeverityChip = (severity) => {
    switch (severity) {
      case 'high':
        return <Chip label="High" color="error" size="small" />;
      case 'medium':
        return <Chip label="Medium" color="warning" size="small" />;
      case 'low':
        return <Chip label="Low" color="info" size="small" />;
      default:
        return <Chip label="Info" color="default" size="small" />;
    }
  };

  // Count unread notifications
  const unreadCount = notifications.filter(n => !n.read).length;

  if (loading && notifications.length === 0) {
    return <LoadingSpinner message="Loading notifications..." />;
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      {/* Header */}
      <PageHeader
        title="Notifications"
        subtitle="Manage your notifications and alert settings"
        breadcrumbs={[
          { text: 'Dashboard', path: '/' },
          { text: 'Notifications' }
        ]}
      />

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
          <Tab 
            label={
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <NotificationsIcon sx={{ mr: 1 }} />
                Notifications
                {unreadCount > 0 && (
                  <Chip 
                    label={unreadCount} 
                    color="primary" 
                    size="small" 
                    sx={{ ml: 1 }}
                  />
                )}
              </Box>
            } 
          />
          <Tab 
            label={
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <SettingsIcon sx={{ mr: 1 }} />
                Notification Settings
              </Box>
            } 
          />
        </Tabs>
        
        {/* Notifications Tab */}
        <TabPanel value={tabValue} index={0}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
            <Typography variant="h6">
              Notifications {unreadCount > 0 && `(${unreadCount} unread)`}
            </Typography>
            <Box>
              <Button 
                variant="outlined" 
                startIcon={<RefreshIcon />} 
                sx={{ mr: 1 }}
                onClick={() => {
                  // Refresh logic would go here
                }}
              >
                Refresh
              </Button>
              <Button 
                variant="outlined" 
                startIcon={<CheckCircleIcon />}
                onClick={handleMarkAllAsRead}
                disabled={unreadCount === 0}
              >
                Mark All as Read
              </Button>
            </Box>
          </Box>
          
          <Divider sx={{ mb: 2 }} />
          
          {notifications.length > 0 ? (
            <List>
              {notifications.map((notification) => (
                <React.Fragment key={notification.id}>
                  <ListItem 
                    alignItems="flex-start"
                    sx={{ 
                      bgcolor: notification.read ? 'transparent' : 'action.hover',
                      borderLeft: `4px solid ${
                        notification.severity === 'high' ? '#f44336' : 
                        notification.severity === 'medium' ? '#ff9800' : 
                        notification.severity === 'low' ? '#2196f3' : 
                        '#9e9e9e'
                      }`
                    }}
                  >
                    <ListItemIcon>
                      {getNotificationIcon(notification)}
                    </ListItemIcon>
                    <ListItemText
                      primary={
                        <Box sx={{ display: 'flex', alignItems: 'center' }}>
                          <Typography 
                            variant="subtitle1" 
                            component="span" 
                            sx={{ fontWeight: notification.read ? 'normal' : 'bold', mr: 1 }}
                          >
                            {notification.title}
                          </Typography>
                          {getSeverityChip(notification.severity)}
                        </Box>
                      }
                      secondary={
                        <>
                          <Typography
                            component="span"
                            variant="body2"
                            color="text.primary"
                            display="block"
                          >
                            {notification.message}
                          </Typography>
                          <Typography
                            component="span"
                            variant="body2"
                            color="text.secondary"
                          >
                            {notification.scraperName} • {formatRelativeTime(notification.timestamp)}
                          </Typography>
                        </>
                      }
                    />
                    <ListItemSecondaryAction>
                      <Box sx={{ display: 'flex' }}>
                        {!notification.read && (
                          <Tooltip title="Mark as Read">
                            <IconButton 
                              edge="end" 
                              onClick={() => handleMarkAsRead(notification.id)}
                            >
                              <CheckCircleIcon />
                            </IconButton>
                          </Tooltip>
                        )}
                        <Tooltip title="Delete">
                          <IconButton 
                            edge="end" 
                            onClick={() => handleDeleteNotification(notification.id)}
                          >
                            <DeleteIcon />
                          </IconButton>
                        </Tooltip>
                      </Box>
                    </ListItemSecondaryAction>
                  </ListItem>
                  <Divider component="li" />
                </React.Fragment>
              ))}
            </List>
          ) : (
            <Box sx={{ textAlign: 'center', py: 4 }}>
              <NotificationsOffIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
              <Typography variant="h6" color="text.secondary">
                No notifications
              </Typography>
              <Typography variant="body2" color="text.secondary">
                You don't have any notifications at the moment
              </Typography>
            </Box>
          )}
        </TabPanel>
        
        {/* Settings Tab */}
        <TabPanel value={tabValue} index={1}>
          <Typography variant="h6" gutterBottom>
            Notification Settings
          </Typography>
          <Divider sx={{ mb: 3 }} />
          
          <Grid container spacing={3}>
            {/* Email Notifications */}
            <Grid item xs={12} md={6}>
              <Card sx={{ mb: 3 }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Email Notifications
                  </Typography>
                  
                  <FormGroup>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={settings.emailNotifications}
                          onChange={handleSettingsChange}
                          name="emailNotifications"
                        />
                      }
                      label="Enable Email Notifications"
                    />
                  </FormGroup>
                  
                  <TextField
                    label="Email Address"
                    fullWidth
                    margin="normal"
                    name="emailAddress"
                    value={settings.emailAddress}
                    onChange={handleSettingsChange}
                    disabled={!settings.emailNotifications}
                  />
                </CardContent>
              </Card>
            </Grid>
            
            {/* Webhook Notifications */}
            <Grid item xs={12} md={6}>
              <Card sx={{ mb: 3 }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Webhook Notifications
                  </Typography>
                  
                  <FormGroup>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={settings.webhookNotifications}
                          onChange={handleSettingsChange}
                          name="webhookNotifications"
                        />
                      }
                      label="Enable Webhook Notifications"
                    />
                  </FormGroup>
                  
                  <TextField
                    label="Webhook URL"
                    fullWidth
                    margin="normal"
                    name="webhookUrl"
                    value={settings.webhookUrl}
                    onChange={handleSettingsChange}
                    disabled={!settings.webhookNotifications}
                    placeholder="https://example.com/webhook"
                  />
                  
                  <Box sx={{ mt: 2, display: 'flex', alignItems: 'center' }}>
                    <Button
                      variant="outlined"
                      startIcon={testWebhookLoading ? <CircularProgress size={20} /> : <SendIcon />}
                      onClick={handleTestWebhook}
                      disabled={!settings.webhookNotifications || !settings.webhookUrl || testWebhookLoading}
                    >
                      Test Webhook
                    </Button>
                    
                    {testWebhookResult && (
                      <Box sx={{ ml: 2 }}>
                        <Chip
                          icon={testWebhookResult.success ? <CheckCircleIcon /> : <ErrorIcon />}
                          label={testWebhookResult.message}
                          color={testWebhookResult.success ? 'success' : 'error'}
                        />
                      </Box>
                    )}
                  </Box>
                </CardContent>
              </Card>
            </Grid>
            
            {/* Notification Types */}
            <Grid item xs={12}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Notification Types
                  </Typography>
                  
                  <Grid container spacing={2}>
                    <Grid item xs={12} sm={6}>
                      <FormGroup>
                        <FormControlLabel
                          control={
                            <Switch
                              checked={settings.notifyOnChanges}
                              onChange={handleSettingsChange}
                              name="notifyOnChanges"
                            />
                          }
                          label="Content Changes"
                        />
                        
                        <FormControlLabel
                          control={
                            <Switch
                              checked={settings.notifyOnErrors}
                              onChange={handleSettingsChange}
                              name="notifyOnErrors"
                            />
                          }
                          label="Scraper Errors"
                        />
                      </FormGroup>
                    </Grid>
                    
                    <Grid item xs={12} sm={6}>
                      <FormGroup>
                        <FormControlLabel
                          control={
                            <Switch
                              checked={settings.notifyOnCompletion}
                              onChange={handleSettingsChange}
                              name="notifyOnCompletion"
                            />
                          }
                          label="Scraper Completion"
                        />
                        
                        <FormControlLabel
                          control={
                            <Switch
                              checked={settings.notifyOnNewDocuments}
                              onChange={handleSettingsChange}
                              name="notifyOnNewDocuments"
                            />
                          }
                          label="New Documents"
                        />
                      </FormGroup>
                    </Grid>
                  </Grid>
                  
                  <Divider sx={{ my: 2 }} />
                  
                  <FormGroup>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={settings.highPriorityOnly}
                          onChange={handleSettingsChange}
                          name="highPriorityOnly"
                        />
                      }
                      label="Only send high priority notifications"
                    />
                  </FormGroup>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
          
          <Box sx={{ mt: 3, display: 'flex', justifyContent: 'flex-end' }}>
            <Button
              variant="contained"
              onClick={handleSaveSettings}
              disabled={loading}
            >
              {loading ? <CircularProgress size={24} /> : 'Save Settings'}
            </Button>
          </Box>
        </TabPanel>
      </Paper>
    </Container>
  );
};

export default Notifications;
