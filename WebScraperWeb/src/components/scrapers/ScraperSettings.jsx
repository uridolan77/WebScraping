import React, { useState } from 'react';
import { 
  Box, Typography, Paper, Grid, Button, 
  Divider, Switch, FormControlLabel, TextField,
  FormControl, InputLabel, Select, MenuItem,
  Alert, Snackbar, CircularProgress, Accordion,
  AccordionSummary, AccordionDetails, Tooltip,
  Dialog, DialogTitle, DialogContent, DialogContentText,
  DialogActions
} from '@mui/material';
import {
  Save as SaveIcon,
  Delete as DeleteIcon,
  ExpandMore as ExpandMoreIcon,
  Refresh as RefreshIcon,
  Backup as BackupIcon,
  Restore as RestoreIcon,
  Settings as SettingsIcon,
  Notifications as NotificationsIcon,
  Schedule as ScheduleIcon,
  Warning as WarningIcon
} from '@mui/icons-material';
import { useScrapers } from '../../contexts/ScraperContext';
import { useNavigate } from 'react-router-dom';
import { getUserFriendlyErrorMessage } from '../../utils/errorHandler';
import { compressStoredContent } from '../../api/scrapers';

const ScraperSettings = ({ scraper }) => {
  const navigate = useNavigate();
  const { editScraper, removeScraper } = useScrapers();
  
  const [settings, setSettings] = useState({
    followExternalLinks: scraper?.followExternalLinks || false,
    respectRobotsTxt: scraper?.respectRobotsTxt || true,
    autoLearnHeaderFooter: scraper?.autoLearnHeaderFooter || true,
    learningPagesCount: scraper?.learningPagesCount || 5,
    enableChangeDetection: scraper?.enableChangeDetection || true,
    trackContentVersions: scraper?.trackContentVersions || true,
    maxVersionsToKeep: scraper?.maxVersionsToKeep || 5,
    enableContinuousMonitoring: scraper?.enableContinuousMonitoring || false,
    monitoringInterval: scraper?.monitoringInterval || 86400,
    notificationEndpoint: scraper?.notificationEndpoint || '',
    email: scraper?.email || ''
  });
  
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isCompressing, setIsCompressing] = useState(false);
  const [notification, setNotification] = useState({ open: false, message: '', severity: 'info' });
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  
  // Handle settings change
  const handleSettingsChange = (event) => {
    const { name, value, type, checked } = event.target;
    setSettings(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };
  
  // Handle save settings
  const handleSaveSettings = async () => {
    if (!scraper) return;
    
    setIsSubmitting(true);
    try {
      // Prepare updated scraper data
      const updatedScraper = {
        ...scraper,
        ...settings
      };
      
      await editScraper(scraper.id, updatedScraper);
      showNotification('Settings saved successfully', 'success');
    } catch (error) {
      showNotification(getUserFriendlyErrorMessage(error, 'Failed to save settings'), 'error');
    } finally {
      setIsSubmitting(false);
    }
  };
  
  // Handle compress content
  const handleCompressContent = async () => {
    if (!scraper) return;
    
    setIsCompressing(true);
    try {
      await compressStoredContent(scraper.id);
      showNotification('Content compressed successfully', 'success');
    } catch (error) {
      showNotification(getUserFriendlyErrorMessage(error, 'Failed to compress content'), 'error');
    } finally {
      setIsCompressing(false);
    }
  };
  
  // Handle delete scraper
  const handleDeleteScraper = async () => {
    if (!scraper) return;
    
    setIsSubmitting(true);
    try {
      await removeScraper(scraper.id);
      showNotification('Scraper deleted successfully', 'success');
      setDeleteDialogOpen(false);
      
      // Navigate back to scrapers list
      setTimeout(() => {
        navigate('/scrapers');
      }, 1500);
    } catch (error) {
      showNotification(getUserFriendlyErrorMessage(error, 'Failed to delete scraper'), 'error');
    } finally {
      setIsSubmitting(false);
    }
  };
  
  // Show notification
  const showNotification = (message, severity = 'info') => {
    setNotification({
      open: true,
      message,
      severity
    });
  };
  
  // Handle notification close
  const handleNotificationClose = () => {
    setNotification(prev => ({ ...prev, open: false }));
  };
  
  return (
    <Box>
      <Typography variant="h6" gutterBottom>Scraper Settings</Typography>
      
      {/* Crawling Settings */}
      <Accordion defaultExpanded>
        <AccordionSummary
          expandIcon={<ExpandMoreIcon />}
          aria-controls="crawling-settings-content"
          id="crawling-settings-header"
        >
          <Typography variant="subtitle1">Crawling Settings</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={settings.followExternalLinks}
                    onChange={handleSettingsChange}
                    name="followExternalLinks"
                  />
                }
                label="Follow External Links"
              />
              <Typography variant="caption" color="text.secondary" display="block">
                If enabled, the scraper will follow links to other domains
              </Typography>
            </Grid>
            
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={settings.respectRobotsTxt}
                    onChange={handleSettingsChange}
                    name="respectRobotsTxt"
                  />
                }
                label="Respect robots.txt"
              />
              <Typography variant="caption" color="text.secondary" display="block">
                If enabled, the scraper will respect the rules in the website's robots.txt file
              </Typography>
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>
      
      {/* Content Processing Settings */}
      <Accordion defaultExpanded>
        <AccordionSummary
          expandIcon={<ExpandMoreIcon />}
          aria-controls="content-processing-settings-content"
          id="content-processing-settings-header"
        >
          <Typography variant="subtitle1">Content Processing</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={settings.autoLearnHeaderFooter}
                    onChange={handleSettingsChange}
                    name="autoLearnHeaderFooter"
                  />
                }
                label="Auto-Learn Header/Footer"
              />
              <Typography variant="caption" color="text.secondary" display="block">
                Automatically detect and remove common headers and footers from content
              </Typography>
            </Grid>
            
            {settings.autoLearnHeaderFooter && (
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Learning Pages Count"
                  type="number"
                  name="learningPagesCount"
                  value={settings.learningPagesCount}
                  onChange={handleSettingsChange}
                  InputProps={{ inputProps: { min: 1, max: 20 } }}
                  helperText="Number of pages to analyze for header/footer detection"
                />
              </Grid>
            )}
            
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={settings.enableChangeDetection}
                    onChange={handleSettingsChange}
                    name="enableChangeDetection"
                  />
                }
                label="Enable Change Detection"
              />
              <Typography variant="caption" color="text.secondary" display="block">
                Detect and track changes in content over time
              </Typography>
            </Grid>
            
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={settings.trackContentVersions}
                    onChange={handleSettingsChange}
                    name="trackContentVersions"
                  />
                }
                label="Track Content Versions"
              />
              <Typography variant="caption" color="text.secondary" display="block">
                Keep track of different versions of the same content over time
              </Typography>
            </Grid>
            
            {settings.trackContentVersions && (
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Max Versions to Keep"
                  type="number"
                  name="maxVersionsToKeep"
                  value={settings.maxVersionsToKeep}
                  onChange={handleSettingsChange}
                  InputProps={{ inputProps: { min: 1, max: 100 } }}
                  helperText="Maximum number of content versions to store"
                />
              </Grid>
            )}
          </Grid>
        </AccordionDetails>
      </Accordion>
      
      {/* Monitoring Settings */}
      <Accordion defaultExpanded>
        <AccordionSummary
          expandIcon={<ExpandMoreIcon />}
          aria-controls="monitoring-settings-content"
          id="monitoring-settings-header"
        >
          <Typography variant="subtitle1">Monitoring & Notifications</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={settings.enableContinuousMonitoring}
                    onChange={handleSettingsChange}
                    name="enableContinuousMonitoring"
                  />
                }
                label="Enable Continuous Monitoring"
              />
              <Typography variant="caption" color="text.secondary" display="block">
                Automatically run the scraper at regular intervals
              </Typography>
            </Grid>
            
            {settings.enableContinuousMonitoring && (
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel id="monitoring-interval-label">Monitoring Interval</InputLabel>
                  <Select
                    labelId="monitoring-interval-label"
                    name="monitoringInterval"
                    value={settings.monitoringInterval}
                    onChange={handleSettingsChange}
                    label="Monitoring Interval"
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
                label="Notification Email"
                type="email"
                name="email"
                value={settings.email}
                onChange={handleSettingsChange}
                helperText="Email for notifications about scraper status and changes"
              />
            </Grid>
            
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Notification Webhook URL"
                name="notificationEndpoint"
                value={settings.notificationEndpoint}
                onChange={handleSettingsChange}
                helperText="Optional webhook URL for receiving notifications (leave empty to disable)"
              />
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>
      
      {/* Maintenance */}
      <Accordion>
        <AccordionSummary
          expandIcon={<ExpandMoreIcon />}
          aria-controls="maintenance-content"
          id="maintenance-header"
        >
          <Typography variant="subtitle1">Maintenance</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <Paper variant="outlined" sx={{ p: 2 }}>
                <Typography variant="subtitle2" gutterBottom>Storage Optimization</Typography>
                <Typography variant="body2" color="text.secondary" paragraph>
                  Compress stored content to reduce storage usage. This will remove duplicate content and optimize storage.
                </Typography>
                <Button
                  variant="outlined"
                  startIcon={isCompressing ? <CircularProgress size={20} /> : <BackupIcon />}
                  onClick={handleCompressContent}
                  disabled={isCompressing}
                >
                  {isCompressing ? 'Compressing...' : 'Compress Content'}
                </Button>
              </Paper>
            </Grid>
            
            <Grid item xs={12}>
              <Paper variant="outlined" sx={{ p: 2, bgcolor: 'error.light' }}>
                <Typography variant="subtitle2" gutterBottom color="error.contrastText">
                  Danger Zone
                </Typography>
                <Typography variant="body2" color="error.contrastText" paragraph>
                  Permanently delete this scraper and all its data. This action cannot be undone.
                </Typography>
                <Button
                  variant="contained"
                  color="error"
                  startIcon={<DeleteIcon />}
                  onClick={() => setDeleteDialogOpen(true)}
                >
                  Delete Scraper
                </Button>
              </Paper>
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>
      
      {/* Save Button */}
      <Box sx={{ mt: 3, display: 'flex', justifyContent: 'flex-end' }}>
        <Button
          variant="contained"
          color="primary"
          startIcon={isSubmitting ? <CircularProgress size={24} /> : <SaveIcon />}
          onClick={handleSaveSettings}
          disabled={isSubmitting}
        >
          {isSubmitting ? 'Saving...' : 'Save Settings'}
        </Button>
      </Box>
      
      {/* Delete Confirmation Dialog */}
      <Dialog
        open={deleteDialogOpen}
        onClose={() => setDeleteDialogOpen(false)}
      >
        <DialogTitle>Delete Scraper</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete the scraper "{scraper?.name}"? This action cannot be undone and will permanently delete all data associated with this scraper.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button 
            onClick={handleDeleteScraper} 
            color="error"
            disabled={isSubmitting}
            startIcon={isSubmitting ? <CircularProgress size={20} /> : <DeleteIcon />}
          >
            {isSubmitting ? 'Deleting...' : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>
      
      {/* Notification Snackbar */}
      <Snackbar
        open={notification.open}
        autoHideDuration={6000}
        onClose={handleNotificationClose}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert 
          onClose={handleNotificationClose} 
          severity={notification.severity} 
          variant="filled"
          sx={{ width: '100%' }}
        >
          {notification.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default ScraperSettings;
