// src/pages/SchedulingUI.jsx
import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import {
  Container, Typography, Box, Button, Divider, Paper, Grid, Card, CardContent,
  TextField, FormControl, InputLabel, Select, MenuItem, Switch, FormControlLabel,
  Dialog, DialogTitle, DialogContent, DialogActions, IconButton, Tooltip,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Snackbar, Alert, CircularProgress
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
  PlayArrow as StartIcon,
  Schedule as ScheduleIcon,
  Event as EventIcon,
  EventRepeat as RecurringIcon,
  EventAvailable as OneTimeIcon,
  ArrowBack as BackIcon
} from '@mui/icons-material';
import { Link } from 'react-router-dom';
import { getScraper, getScraperSchedules, addSchedule, updateSchedule, deleteSchedule, validateCronExpression } from '../api/scheduling';

const SchedulingUI = () => {
  const { id } = useParams();
  const [scraper, setScraper] = useState(null);
  const [schedules, setSchedules] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editMode, setEditMode] = useState(false);
  const [currentSchedule, setCurrentSchedule] = useState(null);
  const [formErrors, setFormErrors] = useState({});
  const [formState, setFormState] = useState({
    name: '',
    isRecurring: true,
    cronExpression: '0 0 * * *', // Default: daily at midnight
    oneTimeExecutionDate: new Date(Date.now() + 86400000).toISOString().split('T')[0], // Tomorrow
    description: '',
    enabled: true
  });
  const [selectedScheduleId, setSelectedScheduleId] = useState(null);
  const [snackbar, setSnackbar] = useState({
    open: false,
    message: '',
    severity: 'success'
  });
  const [cronValidation, setCronValidation] = useState({
    isValid: true,
    nextOccurrences: []
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Fetch scraper info and schedules
  useEffect(() => {
    const fetchData = async () => {
      try {
        setIsLoading(true);
        const [scraperData, schedulesData] = await Promise.all([
          getScraper(id),
          getScraperSchedules(id)
        ]);
        
        setScraper(scraperData);
        setSchedules(schedulesData.schedules || []);
      } catch (error) {
        console.error('Error fetching scheduling data:', error);
        setSnackbar({
          open: true,
          message: 'Error loading scheduling data',
          severity: 'error'
        });
      } finally {
        setIsLoading(false);
      }
    };
    
    fetchData();
  }, [id]);

  // Validate cron expression
  const validateCron = async (cronExpression) => {
    if (!cronExpression) return;
    
    try {
      const result = await validateCronExpression(cronExpression);
      setCronValidation(result);
      
      if (!result.isValid) {
        setFormErrors({
          ...formErrors,
          cronExpression: 'Invalid cron expression'
        });
      } else {
        const newErrors = { ...formErrors };
        delete newErrors.cronExpression;
        setFormErrors(newErrors);
      }
    } catch (error) {
      console.error('Error validating cron expression:', error);
      setCronValidation({
        isValid: false,
        errorMessage: 'Error validating expression'
      });
    }
  };

  // Debounced cron validation
  useEffect(() => {
    if (formState.isRecurring) {
      const timer = setTimeout(() => {
        validateCron(formState.cronExpression);
      }, 500);
      
      return () => clearTimeout(timer);
    }
  }, [formState.cronExpression, formState.isRecurring]);

  // Handle form field change
  const handleFormChange = (field, value) => {
    setFormState({
      ...formState,
      [field]: value
    });
    
    // Clear error for this field
    if (formErrors[field]) {
      const newErrors = { ...formErrors };
      delete newErrors[field];
      setFormErrors(newErrors);
    }
  };

  // Handle form submission
  const handleSubmit = async () => {
    // Validate form
    const errors = {};
    
    if (!formState.name) {
      errors.name = 'Name is required';
    }
    
    if (formState.isRecurring && (!formState.cronExpression || !cronValidation.isValid)) {
      errors.cronExpression = 'Valid cron expression is required';
    }
    
    if (!formState.isRecurring && !formState.oneTimeExecutionDate) {
      errors.oneTimeExecutionDate = 'Execution date is required';
    }
    
    if (Object.keys(errors).length > 0) {
      setFormErrors(errors);
      return;
    }
    
    setIsSubmitting(true);
    
    try {
      const scheduleData = {
        scheduleName: formState.name,
        isRecurring: formState.isRecurring,
        cronExpression: formState.isRecurring ? formState.cronExpression : null,
        oneTimeExecutionDate: !formState.isRecurring ? new Date(formState.oneTimeExecutionDate) : null,
        description: formState.description || '',
        enabled: formState.enabled
      };
      
      if (editMode && currentSchedule) {
        await updateSchedule(currentSchedule.id, scheduleData);
        setSnackbar({
          open: true,
          message: 'Schedule updated successfully',
          severity: 'success'
        });
      } else {
        await addSchedule(id, scheduleData);
        setSnackbar({
          open: true,
          message: 'Schedule created successfully',
          severity: 'success'
        });
      }
      
      // Refresh schedules
      const schedulesData = await getScraperSchedules(id);
      setSchedules(schedulesData.schedules || []);
      
      // Close dialog and reset form
      setDialogOpen(false);
      resetForm();
    } catch (error) {
      console.error('Error saving schedule:', error);
      setSnackbar({
        open: true,
        message: `Error ${editMode ? 'updating' : 'creating'} schedule: ${error.message || 'Unknown error'}`,
        severity: 'error'
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  // Reset form state
  const resetForm = () => {
    setFormState({
      name: '',
      isRecurring: true,
      cronExpression: '0 0 * * *',
      oneTimeExecutionDate: new Date(Date.now() + 86400000).toISOString().split('T')[0],
      description: '',
      enabled: true
    });
    setFormErrors({});
    setCurrentSchedule(null);
    setEditMode(false);
  };

  // Handle edit schedule
  const handleEditSchedule = (schedule) => {
    setCurrentSchedule(schedule);
    setEditMode(true);
    setFormState({
      name: schedule.name,
      isRecurring: Boolean(schedule.cronExpression),
      cronExpression: schedule.cronExpression || '0 0 * * *',
      oneTimeExecutionDate: schedule.oneTimeExecutionDate ? 
        new Date(schedule.oneTimeExecutionDate).toISOString().split('T')[0] : 
        new Date(Date.now() + 86400000).toISOString().split('T')[0],
      description: schedule.description || '',
      enabled: schedule.enabled
    });
    
    if (schedule.cronExpression) {
      validateCron(schedule.cronExpression);
    }
    
    setDialogOpen(true);
  };

  // Handle delete schedule
  const handleDeleteSchedule = async (scheduleId) => {
    try {
      setSelectedScheduleId(scheduleId);
      await deleteSchedule(scheduleId);
      
      // Refresh schedules
      const schedulesData = await getScraperSchedules(id);
      setSchedules(schedulesData.schedules || []);
      
      setSnackbar({
        open: true,
        message: 'Schedule deleted successfully',
        severity: 'success'
      });
    } catch (error) {
      console.error('Error deleting schedule:', error);
      setSnackbar({
        open: true,
        message: `Error deleting schedule: ${error.message || 'Unknown error'}`,
        severity: 'error'
      });
    } finally {
      setSelectedScheduleId(null);
    }
  };

  // Common schedule examples
  const commonCronExamples = [
    { label: 'Every hour', value: '0 * * * *' },
    { label: 'Every day at midnight', value: '0 0 * * *' },
    { label: 'Every Monday at 9:00 AM', value: '0 9 * * 1' },
    { label: 'Twice daily (noon and midnight)', value: '0 0,12 * * *' },
    { label: 'Every 30 minutes', value: '*/30 * * * *' },
    { label: 'First day of every month', value: '0 0 1 * *' }
  ];

  // Format next occurrence dates
  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleString();
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
          Schedule Management
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
          
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
            <Typography variant="h6">
              <ScheduleIcon sx={{ verticalAlign: 'middle', mr: 1 }} />
              Configured Schedules
            </Typography>
            <Button
              variant="contained"
              color="primary"
              startIcon={<AddIcon />}
              onClick={() => {
                resetForm();
                setDialogOpen(true);
              }}
            >
              Add Schedule
            </Button>
          </Box>
          
          {schedules.length > 0 ? (
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Name</TableCell>
                    <TableCell>Type</TableCell>
                    <TableCell>Schedule</TableCell>
                    <TableCell>Next Run</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {schedules.map((schedule) => (
                    <TableRow key={schedule.id}>
                      <TableCell>{schedule.name}</TableCell>
                      <TableCell>
                        {schedule.cronExpression ? (
                          <Tooltip title="Recurring schedule">
                            <RecurringIcon color="primary" />
                          </Tooltip>
                        ) : (
                          <Tooltip title="One-time schedule">
                            <OneTimeIcon color="secondary" />
                          </Tooltip>
                        )}
                        <Typography variant="body2" component="span" sx={{ ml: 1 }}>
                          {schedule.cronExpression ? 'Recurring' : 'One-time'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        {schedule.cronExpression ? (
                          <Tooltip title="Cron expression">
                            <Typography variant="body2">
                              {schedule.cronExpression}
                            </Typography>
                          </Tooltip>
                        ) : (
                          <Typography variant="body2">
                            {new Date(schedule.oneTimeExecutionDate).toLocaleString()}
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>
                        {schedule.nextRunTime ? (
                          formatDate(schedule.nextRunTime)
                        ) : (
                          <Typography variant="body2" color="textSecondary">
                            Not scheduled
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>
                        {schedule.enabled ? (
                          <Alert severity="success" icon={false} sx={{ py: 0 }}>
                            Enabled
                          </Alert>
                        ) : (
                          <Alert severity="warning" icon={false} sx={{ py: 0 }}>
                            Disabled
                          </Alert>
                        )}
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex' }}>
                          <Tooltip title="Edit Schedule">
                            <IconButton 
                              color="primary" 
                              size="small" 
                              onClick={() => handleEditSchedule(schedule)}
                            >
                              <EditIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete Schedule">
                            <IconButton 
                              color="error" 
                              size="small" 
                              onClick={() => handleDeleteSchedule(schedule.id)}
                              disabled={selectedScheduleId === schedule.id}
                            >
                              {selectedScheduleId === schedule.id ? (
                                <CircularProgress size={24} />
                              ) : (
                                <DeleteIcon />
                              )}
                            </IconButton>
                          </Tooltip>
                        </Box>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          ) : (
            <Box sx={{ textAlign: 'center', py: 4 }}>
              <Typography variant="body1" color="textSecondary" gutterBottom>
                No schedules configured yet
              </Typography>
              <Button
                variant="outlined"
                startIcon={<AddIcon />}
                onClick={() => {
                  resetForm();
                  setDialogOpen(true);
                }}
                sx={{ mt: 2 }}
              >
                Add Your First Schedule
              </Button>
            </Box>
          )}
        </Paper>
      )}
      
      {/* Common Schedule Templates */}
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Common Schedule Templates
        </Typography>
        <Divider sx={{ mb: 2 }} />
        
        <Grid container spacing={2}>
          {commonCronExamples.map((example, index) => (
            <Grid item xs={12} sm={6} md={4} key={index}>
              <Card variant="outlined">
                <CardContent>
                  <Typography variant="subtitle1" gutterBottom>
                    {example.label}
                  </Typography>
                  <Typography variant="body2" color="textSecondary" gutterBottom>
                    <code>{example.value}</code>
                  </Typography>
                  <Button
                    variant="text"
                    size="small"
                    onClick={() => {
                      resetForm();
                      setFormState({
                        ...formState,
                        name: example.label,
                        isRecurring: true,
                        cronExpression: example.value
                      });
                      setDialogOpen(true);
                    }}
                  >
                    Use This Template
                  </Button>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Paper>
      
      {/* Schedule Dialog */}
      <Dialog 
        open={dialogOpen} 
        onClose={() => setDialogOpen(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          {editMode ? 'Edit Schedule' : 'Add Schedule'}
        </DialogTitle>
        <DialogContent dividers>
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Schedule Name"
                value={formState.name}
                onChange={(e) => handleFormChange('name', e.target.value)}
                error={!!formErrors.name}
                helperText={formErrors.name || 'Give your schedule a descriptive name'}
                required
              />
            </Grid>
            
            <Grid item xs={12}>
              <FormControl component="fieldset" sx={{ mb: 2 }}>
                <Typography variant="subtitle1" gutterBottom>
                  Schedule Type
                </Typography>
                <Grid container spacing={2}>
                  <Grid item>
                    <Button
                      variant={formState.isRecurring ? 'contained' : 'outlined'}
                      startIcon={<RecurringIcon />}
                      onClick={() => handleFormChange('isRecurring', true)}
                    >
                      Recurring
                    </Button>
                  </Grid>
                  <Grid item>
                    <Button
                      variant={!formState.isRecurring ? 'contained' : 'outlined'}
                      startIcon={<OneTimeIcon />}
                      onClick={() => handleFormChange('isRecurring', false)}
                    >
                      One-Time
                    </Button>
                  </Grid>
                </Grid>
              </FormControl>
            </Grid>
            
            {formState.isRecurring ? (
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Cron Expression"
                  value={formState.cronExpression}
                  onChange={(e) => handleFormChange('cronExpression', e.target.value)}
                  error={!!formErrors.cronExpression}
                  helperText={
                    formErrors.cronExpression || 
                    'Enter a cron expression (e.g. "0 0 * * *" for daily at midnight)'
                  }
                  required
                />
                
                {/* Cron validation results */}
                {formState.cronExpression && (
                  <Box sx={{ mt: 2 }}>
                    {cronValidation.isValid ? (
                      <Alert severity="success" sx={{ mb: 2 }}>
                        Valid cron expression
                      </Alert>
                    ) : (
                      <Alert severity="error" sx={{ mb: 2 }}>
                        {cronValidation.errorMessage || 'Invalid cron expression'}
                      </Alert>
                    )}
                    
                    {cronValidation.isValid && cronValidation.nextOccurrences && (
                      <Box sx={{ mt: 1 }}>
                        <Typography variant="subtitle2" gutterBottom>
                          Next occurrences:
                        </Typography>
                        <ul>
                          {cronValidation.nextOccurrences.map((date, index) => (
                            <li key={index}>
                              <Typography variant="body2">
                                {formatDate(date)}
                              </Typography>
                            </li>
                          ))}
                        </ul>
                      </Box>
                    )}
                  </Box>
                )}
              </Grid>
            ) : (
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Execution Date"
                  type="datetime-local"
                  value={formState.oneTimeExecutionDate}
                  onChange={(e) => handleFormChange('oneTimeExecutionDate', e.target.value)}
                  error={!!formErrors.oneTimeExecutionDate}
                  helperText={formErrors.oneTimeExecutionDate || 'Set the date and time for execution'}
                  InputLabelProps={{ shrink: true }}
                  required
                />
              </Grid>
            )}
            
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Description"
                value={formState.description}
                onChange={(e) => handleFormChange('description', e.target.value)}
                multiline
                rows={2}
                helperText="Optional description for this schedule"
              />
            </Grid>
            
            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Switch
                    checked={formState.enabled}
                    onChange={(e) => handleFormChange('enabled', e.target.checked)}
                    color="primary"
                  />
                }
                label="Enable this schedule"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button 
            onClick={() => setDialogOpen(false)}
            disabled={isSubmitting}
          >
            Cancel
          </Button>
          <Button 
            variant="contained"
            color="primary"
            onClick={handleSubmit}
            disabled={isSubmitting}
          >
            {isSubmitting ? (
              <CircularProgress size={24} color="inherit" />
            ) : (
              editMode ? 'Update Schedule' : 'Add Schedule'
            )}
          </Button>
        </DialogActions>
      </Dialog>
      
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

export default SchedulingUI;