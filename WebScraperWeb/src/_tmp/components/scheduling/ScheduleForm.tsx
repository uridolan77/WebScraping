import React, { useState, useEffect } from 'react';
import { 
  Box, Typography, Grid, TextField, Button, 
  FormControl, InputLabel, Select, MenuItem,
  FormControlLabel, Switch, Divider, Alert,
  Tooltip, IconButton, InputAdornment, Chip
} from '@mui/material';
import {
  Schedule as ScheduleIcon,
  Info as InfoIcon,
  Help as HelpIcon
} from '@mui/icons-material';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import { isValidCronExpression } from '../../utils/validators';
import cronstrue from 'cronstrue';

const ScheduleForm = ({ schedule, scrapers, onSave, onCancel }) => {
  const [cronError, setCronError] = useState('');
  const [cronDescription, setCronDescription] = useState('');
  
  // Define validation schema
  const validationSchema = Yup.object({
    name: Yup.string()
      .required('Name is required')
      .max(100, 'Name must be at most 100 characters'),
    scraperId: Yup.string()
      .required('Scraper is required'),
    schedule: Yup.string()
      .required('Schedule is required')
      .test('is-valid-cron', 'Invalid cron expression', value => isValidCronExpression(value)),
    email: Yup.string()
      .email('Invalid email format')
      .nullable(),
    maxRuntime: Yup.number()
      .positive('Must be a positive number')
      .integer('Must be an integer')
      .nullable()
  });
  
  // Initialize form
  const formik = useFormik({
    initialValues: {
      name: schedule?.name || '',
      scraperId: schedule?.scraperId || '',
      schedule: schedule?.schedule || '0 0 * * *', // Default: daily at midnight
      notifyOnCompletion: schedule?.notifyOnCompletion || false,
      notifyOnError: schedule?.notifyOnError || true,
      email: schedule?.email || '',
      maxRuntime: schedule?.maxRuntime || 3600 // Default: 1 hour
    },
    validationSchema,
    onSubmit: (values) => {
      onSave(values);
    }
  });
  
  // Update cron description when schedule changes
  useEffect(() => {
    try {
      if (formik.values.schedule) {
        const description = cronstrue.toString(formik.values.schedule);
        setCronDescription(description);
        setCronError('');
      }
    } catch (error) {
      setCronDescription('');
      setCronError(error.message);
    }
  }, [formik.values.schedule]);
  
  // Common schedule presets
  const schedulePresets = [
    { label: 'Every hour', value: '0 * * * *' },
    { label: 'Every day at midnight', value: '0 0 * * *' },
    { label: 'Every day at noon', value: '0 12 * * *' },
    { label: 'Every Monday at 9 AM', value: '0 9 * * 1' },
    { label: 'Every weekday at 8 AM', value: '0 8 * * 1-5' },
    { label: 'First day of month at midnight', value: '0 0 1 * *' },
    { label: 'Every Sunday at 2 AM', value: '0 2 * * 0' }
  ];
  
  // Handle preset selection
  const handlePresetSelect = (preset) => {
    formik.setFieldValue('schedule', preset);
  };
  
  return (
    <Box component="form" onSubmit={formik.handleSubmit} noValidate>
      <Grid container spacing={3}>
        <Grid item xs={12}>
          <TextField
            fullWidth
            id="name"
            name="name"
            label="Schedule Name"
            value={formik.values.name}
            onChange={formik.handleChange}
            error={formik.touched.name && Boolean(formik.errors.name)}
            helperText={formik.touched.name && formik.errors.name}
            placeholder="e.g., Daily UKGC Scrape"
          />
        </Grid>
        
        <Grid item xs={12}>
          <FormControl fullWidth error={formik.touched.scraperId && Boolean(formik.errors.scraperId)}>
            <InputLabel id="scraper-select-label">Scraper</InputLabel>
            <Select
              labelId="scraper-select-label"
              id="scraperId"
              name="scraperId"
              value={formik.values.scraperId}
              onChange={formik.handleChange}
              label="Scraper"
            >
              {scrapers.map((scraper) => (
                <MenuItem key={scraper.id} value={scraper.id}>
                  {scraper.name}
                </MenuItem>
              ))}
            </Select>
            {formik.touched.scraperId && formik.errors.scraperId && (
              <Typography variant="caption" color="error">
                {formik.errors.scraperId}
              </Typography>
            )}
          </FormControl>
        </Grid>
        
        <Grid item xs={12}>
          <Divider>
            <Chip label="Schedule Settings" />
          </Divider>
        </Grid>
        
        <Grid item xs={12}>
          <TextField
            fullWidth
            id="schedule"
            name="schedule"
            label="Cron Schedule"
            value={formik.values.schedule}
            onChange={formik.handleChange}
            error={formik.touched.schedule && (Boolean(formik.errors.schedule) || Boolean(cronError))}
            helperText={
              (formik.touched.schedule && formik.errors.schedule) || 
              cronError || 
              (cronDescription && `Runs ${cronDescription.toLowerCase()}`)
            }
            placeholder="0 0 * * *"
            InputProps={{
              endAdornment: (
                <InputAdornment position="end">
                  <Tooltip title="Cron format: minute hour day-of-month month day-of-week">
                    <IconButton edge="end">
                      <HelpIcon />
                    </IconButton>
                  </Tooltip>
                </InputAdornment>
              )
            }}
          />
        </Grid>
        
        <Grid item xs={12}>
          <Typography variant="subtitle2" gutterBottom>
            Common Schedules
          </Typography>
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
            {schedulePresets.map((preset) => (
              <Chip
                key={preset.value}
                label={preset.label}
                onClick={() => handlePresetSelect(preset.value)}
                variant={formik.values.schedule === preset.value ? 'filled' : 'outlined'}
                color={formik.values.schedule === preset.value ? 'primary' : 'default'}
                clickable
              />
            ))}
          </Box>
        </Grid>
        
        <Grid item xs={12}>
          <TextField
            fullWidth
            id="maxRuntime"
            name="maxRuntime"
            label="Maximum Runtime (seconds)"
            type="number"
            value={formik.values.maxRuntime}
            onChange={formik.handleChange}
            error={formik.touched.maxRuntime && Boolean(formik.errors.maxRuntime)}
            helperText={
              (formik.touched.maxRuntime && formik.errors.maxRuntime) ||
              "Maximum time the scraper is allowed to run (0 for no limit)"
            }
            InputProps={{
              inputProps: { min: 0 }
            }}
          />
        </Grid>
        
        <Grid item xs={12}>
          <Divider>
            <Chip label="Notification Settings" />
          </Divider>
        </Grid>
        
        <Grid item xs={12}>
          <TextField
            fullWidth
            id="email"
            name="email"
            label="Notification Email"
            value={formik.values.email}
            onChange={formik.handleChange}
            error={formik.touched.email && Boolean(formik.errors.email)}
            helperText={
              (formik.touched.email && formik.errors.email) ||
              "Email address for notifications (optional)"
            }
            placeholder="user@example.com"
          />
        </Grid>
        
        <Grid item xs={12} sm={6}>
          <FormControlLabel
            control={
              <Switch
                id="notifyOnCompletion"
                name="notifyOnCompletion"
                checked={formik.values.notifyOnCompletion}
                onChange={formik.handleChange}
                color="primary"
              />
            }
            label="Notify on Completion"
          />
        </Grid>
        
        <Grid item xs={12} sm={6}>
          <FormControlLabel
            control={
              <Switch
                id="notifyOnError"
                name="notifyOnError"
                checked={formik.values.notifyOnError}
                onChange={formik.handleChange}
                color="primary"
              />
            }
            label="Notify on Error"
          />
        </Grid>
        
        <Grid item xs={12}>
          <Alert severity="info" icon={<InfoIcon />}>
            Notifications will be sent to the specified email address based on your notification settings.
          </Alert>
        </Grid>
        
        <Grid item xs={12}>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2, mt: 2 }}>
            <Button onClick={onCancel}>
              Cancel
            </Button>
            <Button
              type="submit"
              variant="contained"
              color="primary"
              startIcon={<ScheduleIcon />}
              disabled={!formik.isValid || formik.isSubmitting}
            >
              {schedule ? 'Update Schedule' : 'Create Schedule'}
            </Button>
          </Box>
        </Grid>
      </Grid>
    </Box>
  );
};

export default ScheduleForm;
