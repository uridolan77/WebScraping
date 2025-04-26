import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  FormControlLabel,
  Switch,
  MenuItem,
  FormHelperText,
  Box,
  Alert,
  Typography,
  CircularProgress
} from '@mui/material';
import { updateScraperMonitoring } from '../services/api';

const INTERVAL_OPTIONS = [
  { value: 60, label: '1 hour' },
  { value: 120, label: '2 hours' },
  { value: 360, label: '6 hours' },
  { value: 720, label: '12 hours' },
  { value: 1440, label: '24 hours (daily)' },
  { value: 10080, label: '1 week' },
];

/**
 * MonitoringDialog component for configuring continuous monitoring settings
 * 
 * @param {Object} props - Component properties
 * @param {boolean} props.open - Whether the dialog is visible
 * @param {Function} props.onClose - Callback when dialog is closed without saving
 * @param {Function} props.onSave - Callback when settings are saved
 * @param {string} props.scraperId - ID of the scraper being configured
 * @param {Object} props.initialValues - Initial monitoring settings values
 */
const MonitoringDialog = ({
  open,
  onClose,
  onSave,
  scraperId,
  initialValues = {}
}) => {
  const [formValues, setFormValues] = useState({
    enabled: initialValues.enabled || false,
    intervalMinutes: initialValues.intervalMinutes || 1440,
    notifyOnChanges: initialValues.notifyOnChanges || false, 
    notificationEmail: initialValues.notificationEmail || '',
    trackChangesHistory: initialValues.trackChangesHistory || true
  });
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  
  // Handle form changes
  const handleChange = (e) => {
    const { name, value, checked } = e.target;
    const newValue = e.target.type === 'checkbox' ? checked : value;
    
    setFormValues({
      ...formValues,
      [name]: newValue
    });
  };
  
  // Handle form submission
  const handleSubmit = async () => {
    setLoading(true);
    setError(null);
    
    try {
      await updateScraperMonitoring(scraperId, formValues);
      onSave();
    } catch (err) {
      setError(`Failed to update monitoring settings: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };
  
  return (
    <Dialog 
      open={open} 
      onClose={loading ? undefined : onClose}
      maxWidth="sm"
      fullWidth
    >
      <DialogTitle>
        Configure Continuous Monitoring
      </DialogTitle>
      
      <DialogContent>
        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {error}
          </Alert>
        )}
        
        <Typography variant="body2" color="text.secondary" paragraph>
          Set up continuous monitoring to periodically check for changes on the target website
          without requiring manual intervention.
        </Typography>
        
        <Box sx={{ my: 2 }}>
          <FormControlLabel
            control={
              <Switch
                checked={formValues.enabled}
                onChange={handleChange}
                name="enabled"
                color="primary"
              />
            }
            label="Enable continuous monitoring"
          />
        </Box>
        
        {formValues.enabled && (
          <>
            <TextField
              select
              label="Check interval"
              name="intervalMinutes"
              value={formValues.intervalMinutes}
              onChange={handleChange}
              fullWidth
              margin="normal"
            >
              {INTERVAL_OPTIONS.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {option.label}
                </MenuItem>
              ))}
            </TextField>
            <FormHelperText>
              How often to check for changes on the website
            </FormHelperText>
            
            <Box sx={{ mt: 3, mb: 1 }}>
              <FormControlLabel
                control={
                  <Switch
                    checked={formValues.trackChangesHistory}
                    onChange={handleChange}
                    name="trackChangesHistory"
                    color="primary"
                  />
                }
                label="Track changes history"
              />
              <FormHelperText>
                Store a history of all detected changes for later review
              </FormHelperText>
            </Box>
            
            <Box sx={{ mt: 3, mb: 1 }}>
              <FormControlLabel
                control={
                  <Switch
                    checked={formValues.notifyOnChanges}
                    onChange={handleChange}
                    name="notifyOnChanges"
                    color="primary"
                  />
                }
                label="Email notifications"
              />
              <FormHelperText>
                Receive email notifications when changes are detected
              </FormHelperText>
            </Box>
            
            {formValues.notifyOnChanges && (
              <TextField
                label="Email address"
                name="notificationEmail"
                value={formValues.notificationEmail}
                onChange={handleChange}
                type="email"
                fullWidth
                margin="normal"
                placeholder="your@email.com"
              />
            )}
          </>
        )}
      </DialogContent>
      
      <DialogActions>
        <Button 
          onClick={onClose} 
          disabled={loading}
        >
          Cancel
        </Button>
        <Button 
          onClick={handleSubmit} 
          color="primary" 
          variant="contained"
          disabled={loading}
        >
          {loading ? (
            <CircularProgress size={24} color="inherit" />
          ) : (
            'Save Settings'
          )}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default MonitoringDialog;