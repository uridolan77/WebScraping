import React, { useState, useEffect } from 'react';
import {
  Container,
  Typography,
  Box,
  Paper,
  Grid,
  Card,
  CardContent,
  Tabs,
  Tab,
  Divider,
  Alert,
  CircularProgress,
  Button,
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Switch,
  FormControlLabel,
  Tooltip,
  Snackbar,
} from '@mui/material';

import {
  Schedule as ScheduleIcon,
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  PlayArrow as PlayArrowIcon,
  Pause as PauseIcon,
  Refresh as RefreshIcon,
  Info as InfoIcon,
} from '@mui/icons-material';

import { useScrapers } from '../hooks';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';

// Tab Panel component
function TabPanel(props) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`scheduling-tab-${index}`}
      aria-labelledby={`scheduling-tab-${index}`}
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

// Sample scheduled jobs
const generateSampleSchedules = (scrapers) => {
  if (!scrapers || !scrapers.length) return [];
  
  const scheduleTypes = ['daily', 'weekly', 'monthly', 'custom'];
  const statuses = ['active', 'paused', 'completed', 'failed'];
  
  return Array.from({ length: 8 }, (_, i) => {
    const scraper = scrapers[Math.floor(Math.random() * scrapers.length)];
    const scheduleType = scheduleTypes[Math.floor(Math.random() * scheduleTypes.length)];
    const status = statuses[Math.floor(Math.random() * statuses.length)];
    
    const now = new Date();
    const nextRun = new Date(now);
    
    // Set next run based on schedule type
    if (scheduleType === 'daily') {
      nextRun.setDate(nextRun.getDate() + 1);
      nextRun.setHours(Math.floor(Math.random() * 24), 0, 0);
    } else if (scheduleType === 'weekly') {
      nextRun.setDate(nextRun.getDate() + (Math.floor(Math.random() * 7) + 1));
      nextRun.setHours(Math.floor(Math.random() * 24), 0, 0);
    } else if (scheduleType === 'monthly') {
      nextRun.setMonth(nextRun.getMonth() + 1);
      nextRun.setDate(Math.floor(Math.random() * 28) + 1);
      nextRun.setHours(Math.floor(Math.random() * 24), 0, 0);
    } else {
      // Custom - random time in next 14 days
      nextRun.setDate(nextRun.getDate() + Math.floor(Math.random() * 14) + 1);
      nextRun.setHours(Math.floor(Math.random() * 24), Math.floor(Math.random() * 60), 0);
    }
    
    // Last run is sometime in the past
    const lastRun = new Date(now);
    lastRun.setDate(lastRun.getDate() - Math.floor(Math.random() * 14) - 1);
    
    return {
      id: `schedule-${i + 1}`,
      scraperId: scraper.id,
      scraperName: scraper.name,
      scheduleType,
      cronExpression: scheduleType === 'custom' ? '0 0 * * *' : null, // Example cron
      nextRun,
      lastRun: Math.random() > 0.3 ? lastRun : null,
      status,
      createdAt: new Date(now.getFullYear(), now.getMonth() - 1, now.getDate()),
      maxDepth: Math.floor(Math.random() * 5) + 1,
      maxPages: Math.floor(Math.random() * 1000) + 100,
      notifyOnCompletion: Math.random() > 0.5,
      saveSnapshot: Math.random() > 0.5,
      priority: Math.random() > 0.7 ? 'high' : Math.random() > 0.5 ? 'medium' : 'low',
    };
  });
};

const Scheduling = () => {
  const { getScrapers } = useScrapers();
  const [activeTab, setActiveTab] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [scrapers, setScrapers] = useState([]);
  const [schedules, setSchedules] = useState([]);
  const [openDialog, setOpenDialog] = useState(false);
  const [dialogMode, setDialogMode] = useState('create'); // 'create' or 'edit'
  const [selectedSchedule, setSelectedSchedule] = useState(null);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });
  const [filterStatus, setFilterStatus] = useState('all');
  
  // New schedule form state
  const [formData, setFormData] = useState({
    scraperId: '',
    scheduleType: 'daily',
    cronExpression: '',
    nextRun: new Date(),
    maxDepth: 3,
    maxPages: 500,
    notifyOnCompletion: true,
    saveSnapshot: true,
    priority: 'medium',
  });
  
  // Load data
  useEffect(() => {
    const fetchData = async () => {
      try {
        setIsLoading(true);
        
        // Fetch scrapers
        const scrapersData = await getScrapers();
        setScrapers(scrapersData || []);
        
        // Generate sample schedules (in real app, would fetch from API)
        const schedulesData = generateSampleSchedules(scrapersData);
        setSchedules(schedulesData);
        
      } catch (error) {
        console.error('Error loading scheduling data:', error);
        setSnackbar({
          open: true,
          message: `Error loading data: ${error.message || 'Unknown error'}`,
          severity: 'error',
        });
      } finally {
        setIsLoading(false);
      }
    };
    
    fetchData();
  }, [getScrapers]);
  
  // Handle tab change
  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
  };
  
  // Open dialog for creating new schedule
  const handleOpenCreateDialog = () => {
    setDialogMode('create');
    setFormData({
      scraperId: scrapers.length > 0 ? scrapers[0].id : '',
      scheduleType: 'daily',
      cronExpression: '0 0 * * *',
      nextRun: new Date(new Date().setHours(new Date().getHours() + 1)),
      maxDepth: 3,
      maxPages: 500,
      notifyOnCompletion: true,
      saveSnapshot: true,
      priority: 'medium',
    });
    setOpenDialog(true);
  };
  
  // Open dialog for editing schedule
  const handleOpenEditDialog = (schedule) => {
    setDialogMode('edit');
    setSelectedSchedule(schedule);
    setFormData({
      scraperId: schedule.scraperId,
      scheduleType: schedule.scheduleType,
      cronExpression: schedule.cronExpression || '0 0 * * *',
      nextRun: schedule.nextRun,
      maxDepth: schedule.maxDepth,
      maxPages: schedule.maxPages,
      notifyOnCompletion: schedule.notifyOnCompletion,
      saveSnapshot: schedule.saveSnapshot,
      priority: schedule.priority,
    });
    setOpenDialog(true);
  };
  
  // Close dialog
  const handleCloseDialog = () => {
    setOpenDialog(false);
    setSelectedSchedule(null);
  };
  
  // Handle form input changes
  const handleFormChange = (field, value) => {
    setFormData({
      ...formData,
      [field]: value
    });
  };
  
  // Handle form submission
  const handleSubmitForm = () => {
    try {
      const now = new Date();
      
      if (dialogMode === 'create') {
        // Create new schedule
        const newSchedule = {
          id: `schedule-${schedules.length + 1}`,
          scraperId: formData.scraperId,
          scraperName: scrapers.find(s => s.id === formData.scraperId)?.name,
          scheduleType: formData.scheduleType,
          cronExpression: formData.scheduleType === 'custom' ? formData.cronExpression : null,
          nextRun: formData.nextRun,
          lastRun: null,
          status: 'active',
          createdAt: now,
          maxDepth: formData.maxDepth,
          maxPages: formData.maxPages,
          notifyOnCompletion: formData.notifyOnCompletion,
          saveSnapshot: formData.saveSnapshot,
          priority: formData.priority,
        };
        
        setSchedules([...schedules, newSchedule]);
        
        setSnackbar({
          open: true,
          message: 'Schedule created successfully',
          severity: 'success',
        });
      } else {
        // Update existing schedule
        const updatedSchedules = schedules.map(schedule => {
          if (schedule.id === selectedSchedule.id) {
            return {
              ...schedule,
              scraperId: formData.scraperId,
              scraperName: scrapers.find(s => s.id === formData.scraperId)?.name,
              scheduleType: formData.scheduleType,
              cronExpression: formData.scheduleType === 'custom' ? formData.cronExpression : null,
              nextRun: formData.nextRun,
              maxDepth: formData.maxDepth,
              maxPages: formData.maxPages,
              notifyOnCompletion: formData.notifyOnCompletion,
              saveSnapshot: formData.saveSnapshot,
              priority: formData.priority,
            };
          }
          return schedule;
        });
        
        setSchedules(updatedSchedules);
        
        setSnackbar({
          open: true,
          message: 'Schedule updated successfully',
          severity: 'success',
        });
      }
      
      handleCloseDialog();
    } catch (error) {
      console.error('Error saving schedule:', error);
      setSnackbar({
        open: true,
        message: `Error: ${error.message || 'Unknown error'}`,
        severity: 'error',
      });
    }
  };
  
  // Handle delete schedule
  const handleDeleteSchedule = (scheduleId) => {
    if (window.confirm('Are you sure you want to delete this schedule?')) {
      try {
        // Filter out the deleted schedule
        const updatedSchedules = schedules.filter(schedule => schedule.id !== scheduleId);
        setSchedules(updatedSchedules);
        
        setSnackbar({
          open: true,
          message: 'Schedule deleted successfully',
          severity: 'success',
        });
      } catch (error) {
        console.error('Error deleting schedule:', error);
        setSnackbar({
          open: true,
          message: `Error: ${error.message || 'Unknown error'}`,
          severity: 'error',
        });
      }
    }
  };
  
  // Handle toggle schedule status
  const handleToggleStatus = (scheduleId) => {
    try {
      const updatedSchedules = schedules.map(schedule => {
        if (schedule.id === scheduleId) {
          const newStatus = schedule.status === 'active' ? 'paused' : 'active';
          return {
            ...schedule,
            status: newStatus
          };
        }
        return schedule;
      });
      
      setSchedules(updatedSchedules);
      
      setSnackbar({
        open: true,
        message: 'Schedule status updated',
        severity: 'success',
      });
    } catch (error) {
      console.error('Error updating schedule status:', error);
      setSnackbar({
        open: true,
        message: `Error: ${error.message || 'Unknown error'}`,
        severity: 'error',
      });
    }
  };
  
  // Handle run now
  const handleRunNow = (scheduleId) => {
    try {
      // In a real app, this would call the API to run the scraper immediately
      setSnackbar({
        open: true,
        message: 'Scraper started manually',
        severity: 'success',
      });
    } catch (error) {
      console.error('Error running scraper:', error);
      setSnackbar({
        open: true,
        message: `Error: ${error.message || 'Unknown error'}`,
        severity: 'error',
      });
    }
  };
  
  // Handle filter change
  const handleFilterChange = (event) => {
    setFilterStatus(event.target.value);
  };
  
  // Handle refresh
  const handleRefresh = async () => {
    setIsLoading(true);
    
    try {
      // Fetch scrapers
      const scrapersData = await getScrapers();
      setScrapers(scrapersData || []);
      
      // Generate new sample schedules (in real app, would fetch from API)
      const schedulesData = generateSampleSchedules(scrapersData);
      setSchedules(schedulesData);
      
      setSnackbar({
        open: true,
        message: 'Data refreshed successfully',
        severity: 'success',
      });
    } catch (error) {
      console.error('Error refreshing data:', error);
      setSnackbar({
        open: true,
        message: `Error: ${error.message || 'Unknown error'}`,
        severity: 'error',
      });
    } finally {
      setIsLoading(false);
    }
  };
  
  // Handle snackbar close
  const handleSnackbarClose = () => {
    setSnackbar({ ...snackbar, open: false });
  };
  
  // Format date
  const formatDate = (date) => {
    if (!date) return 'Never';
    return new Date(date).toLocaleString();
  };
  
  // Filter schedules based on status
  const filteredSchedules = filterStatus === 'all' 
    ? schedules 
    : schedules.filter(schedule => schedule.status === filterStatus);
  
  if (isLoading && !schedules.length) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }
  
  return (
    <LocalizationProvider dateAdapter={AdapterDateFns}>
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Box>
            <Typography variant="h4" gutterBottom>
              Scheduling
            </Typography>
            <Typography variant="body1" color="textSecondary">
              Schedule and manage your automated scraping tasks
            </Typography>
          </Box>
          <Box>
            <Button
              variant="outlined"
              startIcon={<RefreshIcon />}
              onClick={handleRefresh}
              sx={{ mr: 2 }}
              disabled={isLoading}
            >
              Refresh
            </Button>
            <Button
              variant="contained"
              color="primary"
              startIcon={<AddIcon />}
              onClick={handleOpenCreateDialog}
            >
              New Schedule
            </Button>
          </Box>
        </Box>
        
        {isLoading && <CircularProgress size={24} sx={{ mb: 2 }} />}
        
        {/* Schedule Summary */}
        <Grid container spacing={3} sx={{ mb: 4 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                  Total Schedules
                </Typography>
                <Typography variant="h5" component="div" sx={{ fontWeight: 'bold' }}>
                  {schedules.length}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                  Active Schedules
                </Typography>
                <Typography variant="h5" component="div" sx={{ fontWeight: 'bold' }}>
                  {schedules.filter(s => s.status === 'active').length}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                  Next Execution
                </Typography>
                <Typography variant="h5" component="div" sx={{ fontWeight: 'bold' }}>
                  {schedules.filter(s => s.status === 'active').length > 0 ? 
                    formatDate(schedules
                      .filter(s => s.status === 'active')
                      .sort((a, b) => new Date(a.nextRun) - new Date(b.nextRun))[0]?.nextRun) : 
                    'No active schedules'}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                  Failed Executions
                </Typography>
                <Typography variant="h5" component="div" sx={{ fontWeight: 'bold' }}>
                  {schedules.filter(s => s.status === 'failed').length}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
        
        {/* Tabs */}
        <Paper sx={{ mb: 4 }}>
          <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
            <Tabs
              value={activeTab}
              onChange={handleTabChange}
              indicatorColor="primary"
              textColor="primary"
              variant="scrollable"
              scrollButtons="auto"
            >
              <Tab label="All Schedules" />
              <Tab label="Schedule History" />
              <Tab label="Configuration" />
            </Tabs>
          </Box>
          
          {/* All Schedules Tab */}
          <TabPanel value={activeTab} index={0}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
              <Typography variant="h6">
                Scheduled Tasks
              </Typography>
              <FormControl sx={{ width: 200 }} size="small">
                <InputLabel id="status-filter-label">Status</InputLabel>
                <Select
                  labelId="status-filter-label"
                  id="status-filter"
                  value={filterStatus}
                  label="Status"
                  onChange={handleFilterChange}
                >
                  <MenuItem value="all">All Statuses</MenuItem>
                  <MenuItem value="active">Active</MenuItem>
                  <MenuItem value="paused">Paused</MenuItem>
                  <MenuItem value="completed">Completed</MenuItem>
                  <MenuItem value="failed">Failed</MenuItem>
                </Select>
              </FormControl>
            </Box>
            
            <TableContainer component={Paper} variant="outlined">
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Scraper Name</TableCell>
                    <TableCell>Schedule Type</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Next Run</TableCell>
                    <TableCell>Last Run</TableCell>
                    <TableCell>Priority</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {filteredSchedules.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={7} align="center">
                        No schedules found with the selected status.
                      </TableCell>
                    </TableRow>
                  )}
                  {filteredSchedules.map((schedule) => (
                    <TableRow key={schedule.id}>
                      <TableCell>{schedule.scraperName}</TableCell>
                      <TableCell>
                        <Tooltip title={schedule.cronExpression || ''}>
                          <span>
                            {schedule.scheduleType.charAt(0).toUpperCase() + schedule.scheduleType.slice(1)}
                          </span>
                        </Tooltip>
                      </TableCell>
                      <TableCell>
                        <Chip 
                          label={schedule.status.charAt(0).toUpperCase() + schedule.status.slice(1)}
                          color={
                            schedule.status === 'active' ? 'success' : 
                            schedule.status === 'paused' ? 'default' : 
                            schedule.status === 'completed' ? 'info' : 
                            'error'
                          }
                          size="small"
                        />
                      </TableCell>
                      <TableCell>{formatDate(schedule.nextRun)}</TableCell>
                      <TableCell>{formatDate(schedule.lastRun)}</TableCell>
                      <TableCell>
                        <Chip 
                          label={schedule.priority.charAt(0).toUpperCase() + schedule.priority.slice(1)}
                          color={
                            schedule.priority === 'high' ? 'error' : 
                            schedule.priority === 'medium' ? 'warning' : 
                            'info'
                          }
                          size="small"
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell align="right">
                        <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
                          <Tooltip title="Run Now">
                            <IconButton 
                              size="small" 
                              onClick={() => handleRunNow(schedule.id)}
                              color="success"
                              sx={{ mr: 1 }}
                              disabled={schedule.status === 'failed'}
                            >
                              <PlayArrowIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          
                          <Tooltip title={schedule.status === 'active' ? 'Pause' : 'Activate'}>
                            <IconButton 
                              size="small" 
                              onClick={() => handleToggleStatus(schedule.id)}
                              color={schedule.status === 'active' ? 'warning' : 'success'}
                              sx={{ mr: 1 }}
                            >
                              {schedule.status === 'active' ? 
                                <PauseIcon fontSize="small" /> : 
                                <PlayArrowIcon fontSize="small" />
                              }
                            </IconButton>
                          </Tooltip>
                          
                          <Tooltip title="Edit">
                            <IconButton 
                              size="small" 
                              onClick={() => handleOpenEditDialog(schedule)}
                              sx={{ mr: 1 }}
                            >
                              <EditIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          
                          <Tooltip title="Delete">
                            <IconButton 
                              size="small" 
                              onClick={() => handleDeleteSchedule(schedule.id)}
                              color="error"
                            >
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </Box>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </TabPanel>
          
          {/* Schedule History Tab */}
          <TabPanel value={activeTab} index={1}>
            <Typography variant="h6" gutterBottom>
              Execution History
            </Typography>
            <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
              View the history of all scheduled execution attempts.
            </Typography>
            
            <TableContainer component={Paper} variant="outlined">
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Scraper</TableCell>
                    <TableCell>Execution Time</TableCell>
                    <TableCell>Duration</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Pages Scraped</TableCell>
                    <TableCell>Result</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {/* Generate some sample execution history */}
                  {Array.from({ length: 5 }, (_, i) => {
                    const scraper = scrapers[Math.floor(Math.random() * scrapers.length)];
                    const now = new Date();
                    const execDate = new Date(now);
                    execDate.setDate(execDate.getDate() - Math.floor(Math.random() * 7));
                    const duration = Math.floor(Math.random() * 300) + 60; // 1-5 minutes
                    const status = Math.random() > 0.2 ? 'completed' : Math.random() > 0.5 ? 'failed' : 'cancelled';
                    const pages = status === 'completed' ? Math.floor(Math.random() * 500) + 50 : Math.floor(Math.random() * 50);
                    
                    return (
                      <TableRow key={`history-${i}`}>
                        <TableCell>{scraper?.name || 'Unknown'}</TableCell>
                        <TableCell>{formatDate(execDate)}</TableCell>
                        <TableCell>{duration} seconds</TableCell>
                        <TableCell>
                          <Chip 
                            label={status.charAt(0).toUpperCase() + status.slice(1)}
                            color={
                              status === 'completed' ? 'success' : 
                              status === 'cancelled' ? 'warning' : 
                              'error'
                            }
                            size="small"
                          />
                        </TableCell>
                        <TableCell>{pages}</TableCell>
                        <TableCell>
                          {status === 'completed' ? 
                            `${Math.floor(Math.random() * 20) + 1} changes detected` : 
                            status === 'failed' ? 
                            'Error: Connection timed out' : 
                            'Cancelled by user'
                          }
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </TableContainer>
            
            <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3 }}>
              <Button variant="outlined">
                View Full History
              </Button>
            </Box>
          </TabPanel>
          
          {/* Configuration Tab */}
          <TabPanel value={activeTab} index={2}>
            <Typography variant="h6" gutterBottom>
              Default Schedule Settings
            </Typography>
            <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
              Configure default settings for all new schedules.
            </Typography>
            
            <Paper variant="outlined" sx={{ p: 3, mb: 4 }}>
              <Grid container spacing={3}>
                <Grid item xs={12} md={6}>
                  <FormControl fullWidth>
                    <InputLabel id="default-schedule-type-label">Default Schedule Type</InputLabel>
                    <Select
                      labelId="default-schedule-type-label"
                      id="default-schedule-type"
                      value="daily"
                      label="Default Schedule Type"
                    >
                      <MenuItem value="daily">Daily</MenuItem>
                      <MenuItem value="weekly">Weekly</MenuItem>
                      <MenuItem value="monthly">Monthly</MenuItem>
                      <MenuItem value="custom">Custom</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={12} md={6}>
                  <FormControl fullWidth>
                    <InputLabel id="default-priority-label">Default Priority</InputLabel>
                    <Select
                      labelId="default-priority-label"
                      id="default-priority"
                      value="medium"
                      label="Default Priority"
                    >
                      <MenuItem value="high">High</MenuItem>
                      <MenuItem value="medium">Medium</MenuItem>
                      <MenuItem value="low">Low</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={12} md={6}>
                  <TextField
                    label="Default Max Depth"
                    type="number"
                    fullWidth
                    value={3}
                    inputProps={{ min: 1, max: 10 }}
                  />
                </Grid>
                <Grid item xs={12} md={6}>
                  <TextField
                    label="Default Max Pages"
                    type="number"
                    fullWidth
                    value={500}
                    inputProps={{ min: 10, max: 10000 }}
                  />
                </Grid>
                
                <Grid item xs={12}>
                  <Divider sx={{ my: 2 }} />
                  <Typography variant="subtitle1" gutterBottom>
                    Notifications & Storage
                  </Typography>
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <FormControlLabel
                    control={<Switch checked={true} />}
                    label="Notify on Completion"
                  />
                </Grid>
                <Grid item xs={12} md={6}>
                  <FormControlLabel
                    control={<Switch checked={true} />}
                    label="Save Content Snapshot"
                  />
                </Grid>
                
                <Grid item xs={12}>
                  <Button variant="contained" color="primary">
                    Save Default Settings
                  </Button>
                </Grid>
              </Grid>
            </Paper>
            
            <Typography variant="h6" gutterBottom>
              System Limits
            </Typography>
            <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
              Configure system-wide limits for scheduling.
            </Typography>
            
            <Paper variant="outlined" sx={{ p: 3 }}>
              <Grid container spacing={3}>
                <Grid item xs={12} md={6}>
                  <TextField
                    label="Maximum Concurrent Schedules"
                    type="number"
                    fullWidth
                    value={5}
                    inputProps={{ min: 1, max: 20 }}
                    helperText="Maximum number of schedules that can run simultaneously"
                  />
                </Grid>
                <Grid item xs={12} md={6}>
                  <TextField
                    label="Maximum Schedules Per Scraper"
                    type="number"
                    fullWidth
                    value={10}
                    inputProps={{ min: 1, max: 50 }}
                    helperText="Maximum number of schedules allowed per scraper"
                  />
                </Grid>
                <Grid item xs={12} md={6}>
                  <TextField
                    label="Rate Limit (executions per hour)"
                    type="number"
                    fullWidth
                    value={20}
                    inputProps={{ min: 1, max: 100 }}
                    helperText="Maximum number of schedule executions per hour"
                  />
                </Grid>
                <Grid item xs={12} md={6}>
                  <TextField
                    label="History Retention (days)"
                    type="number"
                    fullWidth
                    value={30}
                    inputProps={{ min: 1, max: 365 }}
                    helperText="Number of days to keep execution history"
                  />
                </Grid>
                
                <Grid item xs={12}>
                  <FormControlLabel
                    control={<Switch checked={true} />}
                    label="Automatically retry failed schedules"
                  />
                </Grid>
                
                <Grid item xs={12}>
                  <Button variant="contained" color="primary">
                    Save System Limits
                  </Button>
                </Grid>
              </Grid>
            </Paper>
          </TabPanel>
        </Paper>
        
        {/* Create/Edit Schedule Dialog */}
        <Dialog open={openDialog} onClose={handleCloseDialog} fullWidth maxWidth="md">
          <DialogTitle>
            {dialogMode === 'create' ? 'Create New Schedule' : 'Edit Schedule'}
          </DialogTitle>
          <DialogContent>
            <DialogContentText sx={{ mb: 3 }}>
              {dialogMode === 'create' 
                ? 'Configure when and how to run this scraper automatically.'
                : 'Update the schedule configuration for this scraper.'}
            </DialogContentText>
            
            <Grid container spacing={3}>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth sx={{ mb: 2 }}>
                  <InputLabel id="scraper-select-label">Scraper</InputLabel>
                  <Select
                    labelId="scraper-select-label"
                    id="scraper-select"
                    value={formData.scraperId}
                    label="Scraper"
                    onChange={(e) => handleFormChange('scraperId', e.target.value)}
                  >
                    {scrapers.map((scraper) => (
                      <MenuItem key={scraper.id} value={scraper.id}>
                        {scraper.name}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth sx={{ mb: 2 }}>
                  <InputLabel id="schedule-type-label">Schedule Type</InputLabel>
                  <Select
                    labelId="schedule-type-label"
                    id="schedule-type"
                    value={formData.scheduleType}
                    label="Schedule Type"
                    onChange={(e) => handleFormChange('scheduleType', e.target.value)}
                  >
                    <MenuItem value="daily">Daily</MenuItem>
                    <MenuItem value="weekly">Weekly</MenuItem>
                    <MenuItem value="monthly">Monthly</MenuItem>
                    <MenuItem value="custom">Custom (Cron)</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              
              {formData.scheduleType === 'custom' && (
                <Grid item xs={12}>
                  <TextField
                    label="Cron Expression"
                    fullWidth
                    value={formData.cronExpression}
                    onChange={(e) => handleFormChange('cronExpression', e.target.value)}
                    helperText="e.g., '0 0 * * *' for daily at midnight"
                    sx={{ mb: 2 }}
                  />
                </Grid>
              )}
              
              <Grid item xs={12}>
                <DateTimePicker
                  label="Next Run"
                  value={formData.nextRun}
                  onChange={(newValue) => handleFormChange('nextRun', newValue)}
                  renderInput={(params) => <TextField {...params} fullWidth sx={{ mb: 2 }} />}
                />
              </Grid>
              
              <Grid item xs={12} md={6}>
                <TextField
                  label="Max Depth"
                  type="number"
                  fullWidth
                  value={formData.maxDepth}
                  onChange={(e) => handleFormChange('maxDepth', parseInt(e.target.value))}
                  inputProps={{ min: 1, max: 10 }}
                  sx={{ mb: 2 }}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  label="Max Pages"
                  type="number"
                  fullWidth
                  value={formData.maxPages}
                  onChange={(e) => handleFormChange('maxPages', parseInt(e.target.value))}
                  inputProps={{ min: 10, max: 10000 }}
                  sx={{ mb: 2 }}
                />
              </Grid>
              
              <Grid item xs={12} md={6}>
                <FormControl fullWidth sx={{ mb: 2 }}>
                  <InputLabel id="priority-label">Priority</InputLabel>
                  <Select
                    labelId="priority-label"
                    id="priority"
                    value={formData.priority}
                    label="Priority"
                    onChange={(e) => handleFormChange('priority', e.target.value)}
                  >
                    <MenuItem value="high">High</MenuItem>
                    <MenuItem value="medium">Medium</MenuItem>
                    <MenuItem value="low">Low</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              
              <Grid item xs={12}>
                <Typography variant="subtitle1" gutterBottom>
                  Additional Options
                </Typography>
              </Grid>
              
              <Grid item xs={12} md={6}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={formData.notifyOnCompletion}
                      onChange={(e) => handleFormChange('notifyOnCompletion', e.target.checked)}
                    />
                  }
                  label="Notify on Completion"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={formData.saveSnapshot}
                      onChange={(e) => handleFormChange('saveSnapshot', e.target.checked)}
                    />
                  }
                  label="Save Content Snapshot"
                />
              </Grid>
            </Grid>
          </DialogContent>
          <DialogActions>
            <Button onClick={handleCloseDialog}>Cancel</Button>
            <Button 
              onClick={handleSubmitForm} 
              variant="contained" 
              color="primary"
              startIcon={dialogMode === 'create' ? <AddIcon /> : <SaveIcon />}
            >
              {dialogMode === 'create' ? 'Create Schedule' : 'Save Changes'}
            </Button>
          </DialogActions>
        </Dialog>
        
        {/* Snackbar for notifications */}
        <Snackbar
          open={snackbar.open}
          autoHideDuration={6000}
          onClose={handleSnackbarClose}
          message={snackbar.message}
        />
      </Container>
    </LocalizationProvider>
  );
};

export default Scheduling;