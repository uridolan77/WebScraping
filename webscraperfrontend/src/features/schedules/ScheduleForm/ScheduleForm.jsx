import { useState, useEffect } from 'react';
import {
  Box,
  TextField,
  Button,
  Grid,
  Paper,
  Typography,
  FormControl,
  FormControlLabel,
  Switch,
  Select,
  MenuItem,
  InputLabel,
  CircularProgress,
  FormHelperText,
  Tabs,
  Tab,
  Divider
} from '@mui/material';
import SaveIcon from '@mui/icons-material/Save';
import CancelIcon from '@mui/icons-material/Cancel';
import DeleteIcon from '@mui/icons-material/Delete';
import useApiClient from '../../../hooks/useApiClient';

const ScheduleForm = ({
  initialData = null,
  scraperId = null,
  scraperOptions = [],
  onSubmit,
  onCancel,
  onDelete = null,
  loading = false,
  error = null
}) => {
  // Default data for a new schedule
  const defaultData = {
    scraperId: scraperId || '',
    name: '',
    cronExpression: '0 0 * * *', // Default: daily at midnight
    active: true,
    retryOnFailure: false,
    maxRetries: 3,
    retryDelay: 5,
    timeoutMinutes: 30
  };

  // Initialize form state
  const [formData, setFormData] = useState(initialData || defaultData);
  const [cronType, setCronType] = useState('preset');
  const [validationError, setValidationError] = useState('');
  const { api, execute } = useApiClient();

  // Update form data when initialData changes
  useEffect(() => {
    if (initialData) {
      setFormData(initialData);
      // Determine if we're using a preset or custom cron
      if (isPresetCron(initialData.cronExpression)) {
        setCronType('preset');
      } else {
        setCronType('custom');
      }
    } else if (scraperId) {
      setFormData(prev => ({ ...prev, scraperId }));
    }
  }, [initialData, scraperId]);

  const isEditMode = !!initialData;

  const handleChange = (event) => {
    const { name, value, checked, type } = event.target;
    setFormData({
      ...formData,
      [name]: type === 'checkbox' ? checked : value
    });

    // Clear validation errors when editing the cron expression
    if (name === 'cronExpression') {
      setValidationError('');
    }
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    
    // Validate cron expression if using custom expression
    if (cronType === 'custom') {
      try {
        await execute(() => api.scheduling.validateCron({ expression: formData.cronExpression }));
      } catch (err) {
        setValidationError('Invalid cron expression');
        return;
      }
    }
    
    onSubmit(formData);
  };

  const handleCronTypeChange = (event, newValue) => {
    setCronType(newValue);
    setValidationError('');
  };

  const handlePresetChange = (event) => {
    const preset = event.target.value;
    const cronExpression = getCronFromPreset(preset);
    setFormData({
      ...formData,
      cronExpression
    });
  };

  // Check if the cron expression is one of our presets
  const isPresetCron = (cron) => {
    return Object.values(cronPresets).includes(cron);
  };

  const getCronFromPreset = (preset) => {
    return cronPresets[preset] || '0 0 * * *'; // Default to daily
  };

  // Common cron expression presets
  const cronPresets = {
    hourly: '0 * * * *',
    daily: '0 0 * * *',
    daily_noon: '0 12 * * *',
    weekly: '0 0 * * 0',
    monthly: '0 0 1 * *',
    everyMinute: '* * * * *',
    every5Minutes: '*/5 * * * *',
    every15Minutes: '*/15 * * * *',
    every30Minutes: '*/30 * * * *',
    weekdays: '0 0 * * 1-5',
    weekends: '0 0 * * 0,6'
  };

  // Get the preset key for the current cron (if it's a preset)
  const getCurrentPreset = () => {
    const currentCron = formData.cronExpression;
    for (const [preset, cron] of Object.entries(cronPresets)) {
      if (cron === currentCron) {
        return preset;
      }
    }
    return 'daily'; // Default to daily if no match
  };

  return (
    <Box component="form" onSubmit={handleSubmit} noValidate sx={{ mt: 2 }}>
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Schedule Configuration
        </Typography>

        <Grid container spacing={3}>
          {/* Schedule Name */}
          <Grid item xs={12}>
            <TextField
              required
              fullWidth
              label="Schedule Name"
              name="name"
              value={formData.name}
              onChange={handleChange}
              disabled={loading}
              placeholder="My Daily Scraper Schedule"
            />
          </Grid>

          {/* Scraper Selection */}
          <Grid item xs={12}>
            <FormControl fullWidth required disabled={isEditMode || loading}>
              <InputLabel id="scraper-select-label">Scraper</InputLabel>
              <Select
                labelId="scraper-select-label"
                id="scraper-select"
                name="scraperId"
                value={formData.scraperId}
                label="Scraper"
                onChange={handleChange}
              >
                {scraperOptions.map((scraper) => (
                  <MenuItem key={scraper.id} value={scraper.id}>
                    {scraper.name}
                  </MenuItem>
                ))}
              </Select>
              <FormHelperText>
                {isEditMode ? "Scraper can't be changed after schedule creation" : "Select the scraper to run on this schedule"}
              </FormHelperText>
            </FormControl>
          </Grid>

          {/* Schedule Type Tabs */}
          <Grid item xs={12}>
            <Typography variant="subtitle1">Schedule Type</Typography>
            <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
              <Tabs value={cronType} onChange={handleCronTypeChange} aria-label="schedule type tabs">
                <Tab value="preset" label="Common Schedules" />
                <Tab value="custom" label="Custom Schedule (Cron)" />
              </Tabs>
            </Box>
          </Grid>

          {/* Schedule Content based on selected tab */}
          <Grid item xs={12}>
            {cronType === 'preset' ? (
              <FormControl fullWidth disabled={loading}>
                <InputLabel id="preset-select-label">Frequency</InputLabel>
                <Select
                  labelId="preset-select-label"
                  id="preset-select"
                  value={getCurrentPreset()}
                  label="Frequency"
                  onChange={handlePresetChange}
                >
                  <MenuItem value="everyMinute">Every minute (testing only)</MenuItem>
                  <MenuItem value="every5Minutes">Every 5 minutes</MenuItem>
                  <MenuItem value="every15Minutes">Every 15 minutes</MenuItem>
                  <MenuItem value="every30Minutes">Every 30 minutes</MenuItem>
                  <MenuItem value="hourly">Hourly</MenuItem>
                  <MenuItem value="daily">Daily (at midnight)</MenuItem>
                  <MenuItem value="daily_noon">Daily (at noon)</MenuItem>
                  <MenuItem value="weekdays">Weekdays (Mon-Fri at midnight)</MenuItem>
                  <MenuItem value="weekends">Weekends (Sat-Sun at midnight)</MenuItem>
                  <MenuItem value="weekly">Weekly (Sunday at midnight)</MenuItem>
                  <MenuItem value="monthly">Monthly (1st day at midnight)</MenuItem>
                </Select>
              </FormControl>
            ) : (
              <TextField
                fullWidth
                label="Cron Expression"
                name="cronExpression"
                value={formData.cronExpression}
                onChange={handleChange}
                disabled={loading}
                error={!!validationError}
                helperText={validationError || "Unix-style cron expression (minute hour day-of-month month day-of-week)"}
                placeholder="0 0 * * *"
              />
            )}
          </Grid>

          <Grid item xs={12}>
            <Divider />
            <Typography variant="subtitle1" sx={{ mt: 2, mb: 1 }}>Advanced Options</Typography>
          </Grid>
          
          {/* Timeout */}
          <Grid item xs={12} sm={6} md={4}>
            <TextField
              fullWidth
              type="number"
              label="Timeout (minutes)"
              name="timeoutMinutes"
              value={formData.timeoutMinutes}
              onChange={handleChange}
              disabled={loading}
              InputProps={{ inputProps: { min: 1, max: 1440 } }}
            />
          </Grid>

          {/* Retry Settings */}
          <Grid item xs={12} sm={6} md={4}>
            <FormControlLabel
              control={
                <Switch
                  checked={formData.retryOnFailure}
                  onChange={handleChange}
                  name="retryOnFailure"
                  color="primary"
                  disabled={loading}
                />
              }
              label="Retry on failure"
            />
          </Grid>

          {formData.retryOnFailure && (
            <>
              <Grid item xs={12} sm={6} md={4}>
                <TextField
                  fullWidth
                  type="number"
                  label="Max Retries"
                  name="maxRetries"
                  value={formData.maxRetries}
                  onChange={handleChange}
                  disabled={loading}
                  InputProps={{ inputProps: { min: 1, max: 10 } }}
                />
              </Grid>
              <Grid item xs={12} sm={6} md={4}>
                <TextField
                  fullWidth
                  type="number"
                  label="Retry Delay (minutes)"
                  name="retryDelay"
                  value={formData.retryDelay}
                  onChange={handleChange}
                  disabled={loading}
                  InputProps={{ inputProps: { min: 1, max: 60 } }}
                />
              </Grid>
            </>
          )}

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
              {loading ? 'Saving...' : isEditMode ? 'Update Schedule' : 'Create Schedule'}
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
              Delete Schedule
            </Button>
          )}
        </Box>
      </Paper>
    </Box>
  );
};

export default ScheduleForm;