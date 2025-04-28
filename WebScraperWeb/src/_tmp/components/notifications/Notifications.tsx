// src/components/notifications/Notifications.tsx
import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import {
  Container, Typography, Box, Button, Divider, Paper, Grid, Card, CardContent,
  TextField, FormControl, InputLabel, Select, MenuItem, Switch, FormControlLabel,
  Chip, Snackbar, Alert, CircularProgress, List, ListItem, ListItemIcon,
  ListItemText, Checkbox, IconButton, Tooltip
} from '@mui/material';
import {
  Notifications as NotificationsIcon,
  Email as EmailIcon,
  Code as WebhookIcon,
  ArrowBack as BackIcon,
  Send as SendIcon,
  Save as SaveIcon,
  PlayArrow as TestIcon,
  Check as CheckIcon,
  Error as ErrorIcon,
  Delete as DeleteIcon
} from '@mui/icons-material';
import { getScraper, getWebhookConfig, updateWebhookConfig, testWebhook } from '../../api/notifications';

const NotificationsConfig: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  interface WebhookConfig {
    enabled: boolean;
    webhookUrl: string;
    format: string;
    triggers: string[];
    notifyOnContentChanges: boolean;
    notifyOnDocumentProcessed: boolean;
    notifyOnScraperStatusChange: boolean;
    sendTestNotification?: boolean;
  }

  interface Scraper {
    id: string;
    name: string;
    baseUrl: string;
    notifyOnChanges?: boolean;
    notificationEmail?: string;
  }

  interface FormErrors {
    [key: string]: string;
  }

  interface SnackbarState {
    open: boolean;
    message: string;
    severity: 'success' | 'info' | 'warning' | 'error';
  }

  interface TestResult {
    success: boolean;
    message: string;
    details?: any;
    status?: string;
  }

  const [scraper, setScraper] = useState<Scraper | null>(null);
  const [config, setConfig] = useState<WebhookConfig>({
    enabled: false,
    webhookUrl: '',
    format: 'json',
    triggers: ['all'],
    notifyOnContentChanges: true,
    notifyOnDocumentProcessed: false,
    notifyOnScraperStatusChange: true,
    sendTestNotification: false
  });
  const [originalConfig, setOriginalConfig] = useState<WebhookConfig | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isSaving, setIsSaving] = useState<boolean>(false);
  const [isTesting, setIsTesting] = useState<boolean>(false);
  const [formErrors, setFormErrors] = useState<FormErrors>({});
  const [snackbar, setSnackbar] = useState<SnackbarState>({
    open: false,
    message: '',
    severity: 'success'
  });
  const [testResult, setTestResult] = useState<TestResult | null>(null);

  interface TriggerType {
    id: string;
    name: string;
    description: string;
  }

  // Available trigger types
  const triggerTypes: TriggerType[] = [
    { id: 'all', name: 'All Events', description: 'Receive notifications for all events' },
    { id: 'content_change', name: 'Content Changes', description: 'When content on a monitored page changes' },
    { id: 'document_processed', name: 'Document Processed', description: 'When a document (PDF, DOCX, etc.) is processed' },
    { id: 'status_change', name: 'Status Changes', description: 'When the scraper status changes (start, stop, error)' },
    { id: 'schedule', name: 'Schedule Events', description: 'When a scheduled run starts or completes' },
    { id: 'error', name: 'Errors', description: 'When errors occur during scraping' }
  ];

  // Fetch scraper and webhook config
  useEffect(() => {
    const fetchData = async () => {
      if (!id) return;

      try {
        setIsLoading(true);
        const [scraperData, configData] = await Promise.all([
          getScraper(id),
          getWebhookConfig(id)
        ]);

        setScraper(scraperData);

        // Initialize config from API data
        const webhookConfig = configData || {
          enabled: false,
          webhookUrl: '',
          format: 'json',
          triggers: ['all'],
          notifyOnContentChanges: true,
          notifyOnDocumentProcessed: false,
          notifyOnScraperStatusChange: true
        };

        setConfig(webhookConfig);
        setOriginalConfig(JSON.parse(JSON.stringify(webhookConfig))); // Deep copy
      } catch (error) {
        console.error('Error fetching notification config:', error);
        setSnackbar({
          open: true,
          message: 'Error loading notification configuration',
          severity: 'error'
        });
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, [id]);

  // Handle form field change
  const handleConfigChange = (field: keyof WebhookConfig, value: any) => {
    setConfig({
      ...config,
      [field]: value
    });

    // Clear error for this field
    if (formErrors[field]) {
      const newErrors = { ...formErrors };
      delete newErrors[field];
      setFormErrors(newErrors);
    }
  };

  // Handle trigger selection
  const handleTriggerToggle = (triggerId: string) => {
    let newTriggers: string[];

    // Handle 'all' special case
    if (triggerId === 'all') {
      // If 'all' is being added, only keep 'all'
      if (!config.triggers.includes('all')) {
        newTriggers = ['all'];
      }
      // If 'all' is being removed, remove it
      else {
        newTriggers = [];
      }
    } else {
      // Start with current triggers
      newTriggers = [...config.triggers];

      // Remove 'all' if it's there
      if (newTriggers.includes('all')) {
        newTriggers = newTriggers.filter(t => t !== 'all');
      }

      // Toggle the specific trigger
      if (newTriggers.includes(triggerId)) {
        newTriggers = newTriggers.filter(t => t !== triggerId);
      } else {
        newTriggers.push(triggerId);
      }

      // If no triggers selected, default to 'all'
      if (newTriggers.length === 0) {
        newTriggers = ['all'];
      }

      // If all specific triggers are selected, switch to 'all'
      if (newTriggers.length === triggerTypes.length - 1) {
        newTriggers = ['all'];
      }
    }

    setConfig({
      ...config,
      triggers: newTriggers
    });
  };

  // Save webhook configuration
  const handleSaveConfig = async () => {
    if (!id) return;

    // Validate form
    const errors: FormErrors = {};

    if (config.enabled && !config.webhookUrl) {
      errors.webhookUrl = 'Webhook URL is required when notifications are enabled';
    } else if (config.enabled && config.webhookUrl) {
      try {
        new URL(config.webhookUrl);
      } catch (e) {
        errors.webhookUrl = 'Invalid URL format';
      }
    }

    if (Object.keys(errors).length > 0) {
      setFormErrors(errors);
      return;
    }

    setIsSaving(true);

    try {
      await updateWebhookConfig(id, config);

      setSnackbar({
        open: true,
        message: 'Webhook configuration saved successfully',
        severity: 'success'
      });

      setOriginalConfig(JSON.parse(JSON.stringify(config))); // Deep copy
      setTestResult(null); // Clear previous test results
    } catch (error: any) {
      console.error('Error saving webhook config:', error);
      setSnackbar({
        open: true,
        message: `Error saving configuration: ${error?.message || 'Unknown error'}`,
        severity: 'error'
      });
    } finally {
      setIsSaving(false);
    }
  };

  // Test webhook
  const handleTestWebhook = async () => {
    if (!config.webhookUrl) {
      setFormErrors({
        webhookUrl: 'Webhook URL is required for testing'
      });
      return;
    }

    if (!id) return;

    setIsTesting(true);
    setTestResult(null);

    try {
      const result = await testWebhook(config.webhookUrl, id);

      setTestResult({
        success: result.success || true,
        message: result.message || 'Test notification sent successfully',
        details: result
      });

      setSnackbar({
        open: true,
        message: 'Webhook test completed',
        severity: 'info'
      });
    } catch (error: any) {
      console.error('Error testing webhook:', error);
      setTestResult({
        success: false,
        message: `Error testing webhook: ${error?.message || 'Unknown error'}`,
        details: error
      });

      setSnackbar({
        open: true,
        message: `Error testing webhook: ${error?.message || 'Unknown error'}`,
        severity: 'error'
      });
    } finally {
      setIsTesting(false);
    }
  };

  // Check for unsaved changes
  const hasUnsavedChanges = () => {
    if (!originalConfig) return false;

    return JSON.stringify(config) !== JSON.stringify(originalConfig);
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
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
        <Button
          component={Link}
          to={`/scrapers/${id}`}
          startIcon={<BackIcon />}
          sx={{ mr: 2 }}
        >
          Back to Scraper
        </Button>
        <Typography variant="h4">
          Notification Settings
        </Typography>
      </Box>

      {scraper && (
        <Paper sx={{ p: 3, mb: 3 }}>
          <Typography variant="h5" gutterBottom>
            {scraper.name}
          </Typography>
          <Typography variant="body1" color="textSecondary">
            {scraper.baseUrl}
          </Typography>

          <Divider sx={{ my: 2 }} />

          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Card variant="outlined" sx={{ mb: 3 }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    <WebhookIcon sx={{ verticalAlign: 'middle', mr: 1 }} />
                    Webhook Configuration
                  </Typography>

                  <FormControlLabel
                    control={
                      <Switch
                        checked={config.enabled}
                        onChange={(e) => handleConfigChange('enabled', e.target.checked)}
                        color="primary"
                      />
                    }
                    label="Enable webhook notifications"
                    sx={{ mb: 2, display: 'block' }}
                  />

                  <TextField
                    fullWidth
                    label="Webhook URL"
                    value={config.webhookUrl}
                    onChange={(e) => handleConfigChange('webhookUrl', e.target.value)}
                    disabled={!config.enabled}
                    error={!!formErrors.webhookUrl}
                    helperText={formErrors.webhookUrl || 'URL to send notifications to'}
                    sx={{ mb: 3 }}
                  />

                  <FormControl
                    fullWidth
                    sx={{ mb: 3 }}
                    disabled={!config.enabled}
                  >
                    <InputLabel id="webhook-format-label">Webhook Format</InputLabel>
                    <Select
                      labelId="webhook-format-label"
                      value={config.format}
                      label="Webhook Format"
                      onChange={(e) => handleConfigChange('format', e.target.value)}
                    >
                      <MenuItem value="json">JSON</MenuItem>
                      <MenuItem value="form">Form Data</MenuItem>
                    </Select>
                  </FormControl>
                </CardContent>
              </Card>

              <Card variant="outlined">
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Notification Triggers
                  </Typography>

                  <List>
                    {triggerTypes.map((trigger) => (
                      <ListItem key={trigger.id} dense>
                        <ListItemIcon>
                          <Checkbox
                            edge="start"
                            checked={config.triggers.includes(trigger.id)}
                            onChange={() => handleTriggerToggle(trigger.id)}
                            disabled={!config.enabled}
                          />
                        </ListItemIcon>
                        <ListItemText
                          primary={trigger.name}
                          secondary={trigger.description}
                        />
                      </ListItem>
                    ))}
                  </List>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} md={6}>
              <Card variant="outlined" sx={{ mb: 3 }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Email Notifications
                  </Typography>

                  <FormControlLabel
                    control={
                      <Switch
                        checked={scraper.notifyOnChanges || false}
                        disabled={true} // Read-only here, edited in scraper settings
                        color="primary"
                      />
                    }
                    label="Email notifications enabled"
                  />

                  {scraper.notifyOnChanges && scraper.notificationEmail && (
                    <Box sx={{ mt: 2 }}>
                      <Typography variant="body2">
                        Notifications will be sent to:
                      </Typography>
                      <Chip
                        icon={<EmailIcon />}
                        label={scraper.notificationEmail}
                        sx={{ mt: 1 }}
                      />
                    </Box>
                  )}

                  <Box sx={{ mt: 2 }}>
                    <Typography variant="body2" color="textSecondary">
                      Email notification settings can be configured in the main scraper settings.
                    </Typography>
                    <Button
                      component={Link}
                      to={`/scrapers/${id}/edit`}
                      size="small"
                      sx={{ mt: 1 }}
                    >
                      Go to Scraper Settings
                    </Button>
                  </Box>
                </CardContent>
              </Card>

              <Card variant="outlined">
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Test Webhook
                  </Typography>

                  <Typography variant="body2" paragraph>
                    Send a test notification to verify your webhook configuration is working correctly.
                  </Typography>

                  <Button
                    variant="contained"
                    color="primary"
                    startIcon={<TestIcon />}
                    onClick={handleTestWebhook}
                    disabled={isTesting || !config.enabled || !config.webhookUrl}
                    sx={{ mb: 2 }}
                  >
                    {isTesting ? <CircularProgress size={24} /> : 'Send Test Notification'}
                  </Button>

                  {testResult && (
                    <Box sx={{ mt: 2 }}>
                      <Alert
                        severity={testResult.success ? 'success' : 'error'}
                        icon={testResult.success ? <CheckIcon /> : <ErrorIcon />}
                        sx={{ mb: 1 }}
                      >
                        {testResult.message}
                      </Alert>

                      {testResult.details && (
                        <Typography variant="body2" sx={{ mt: 1, mb: 2 }}>
                          Webhook endpoint response status: {testResult.details.status || 'Unknown'}
                        </Typography>
                      )}
                    </Box>
                  )}
                </CardContent>
              </Card>
            </Grid>
          </Grid>

          <Box sx={{ mt: 3, display: 'flex', justifyContent: 'flex-end' }}>
            <Button
              variant="contained"
              color="primary"
              startIcon={isSaving ? <CircularProgress size={24} /> : <SaveIcon />}
              onClick={handleSaveConfig}
              disabled={isSaving || !hasUnsavedChanges()}
            >
              Save Configuration
            </Button>
          </Box>
        </Paper>
      )}

      {/* Help information */}
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          About Notifications
        </Typography>
        <Divider sx={{ mb: 2 }} />

        <Typography variant="body1" paragraph>
          Notifications allow you to receive alerts when certain events occur during the scraping process.
        </Typography>

        <Typography variant="subtitle1" gutterBottom>
          Webhook Notifications
        </Typography>
        <Typography variant="body2" paragraph>
          Webhooks send HTTP requests to a URL you specify when events occur. They allow you to integrate with other systems like Slack, Discord, custom applications, or automation tools like Zapier.
        </Typography>
        <Typography variant="body2" sx={{ mb: 3 }}>
          Format: {config.format === 'json' ?
            'JSON format sends data in a structured JSON object.' :
            'Form data format sends data as application/x-www-form-urlencoded.'}
        </Typography>

        <Typography variant="subtitle1" gutterBottom>
          Email Notifications
        </Typography>
        <Typography variant="body2">
          Email notifications send alerts to your email address. These are configured in the main scraper settings and can be used for critical alerts like content changes or errors.
        </Typography>
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