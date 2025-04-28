import React, { useState, useEffect } from 'react';
import { 
  Container, Grid, Paper, Typography, Box, 
  Button, CircularProgress, Divider, Tabs, Tab, Alert,
  Card, CardContent, CardActions, Chip, IconButton,
  List, ListItem, ListItemText, ListItemIcon, ListItemSecondary,
  Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, FormControl, InputLabel, Select, MenuItem,
  Switch, FormControlLabel, Tooltip, Badge
} from '@mui/material';
import {
  Add as AddIcon,
  Refresh as RefreshIcon,
  Schedule as ScheduleIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
  PlayArrow as PlayIcon,
  Stop as StopIcon,
  Pause as PauseIcon,
  Notifications as NotificationsIcon,
  Email as EmailIcon,
  CalendarToday as CalendarIcon,
  AccessTime as TimeIcon,
  Repeat as RepeatIcon,
  History as HistoryIcon
} from '@mui/icons-material';
import { useScrapers } from '../../contexts/ScraperContext';
import { format, formatDistanceToNow } from 'date-fns';
import { Link as RouterLink } from 'react-router-dom';
import ScheduledTasksList from './ScheduledTasksList';
import ScheduleForm from './ScheduleForm';
import ScheduleHistory from './ScheduleHistory';

// TabPanel component for tab content
function TabPanel(props) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`scheduling-tabpanel-${index}`}
      aria-labelledby={`scheduling-tab-${index}`}
      {...other}
      style={{ padding: '24px 0' }}
    >
      {value === index && children}
    </div>
  );
}

// Helper function for tab accessibility
function a11yProps(index) {
  return {
    id: `scheduling-tab-${index}`,
    'aria-controls': `scheduling-tabpanel-${index}`,
  };
}

const SchedulingDashboard = () => {
  const { 
    scrapers, 
    loading, 
    error
  } = useScrapers();
  
  const [tabValue, setTabValue] = useState(0);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [schedules, setSchedules] = useState([]);
  const [history, setHistory] = useState([]);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [selectedSchedule, setSelectedSchedule] = useState(null);
  
  // Fetch initial data
  useEffect(() => {
    // Mock schedules data
    setSchedules([
      {
        id: 1,
        name: 'Daily UKGC Scrape',
        scraperId: 'ukgc',
        scraperName: 'UKGC',
        schedule: '0 0 * * *', // Cron expression: At midnight every day
        nextRun: new Date(Date.now() + 24 * 60 * 60 * 1000), // Tomorrow
        lastRun: new Date(Date.now() - 24 * 60 * 60 * 1000), // Yesterday
        status: 'active',
        email: 'user@example.com',
        notifyOnCompletion: true,
        notifyOnError: true,
        maxRuntime: 3600, // 1 hour in seconds
        createdAt: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000) // 30 days ago
      },
      {
        id: 2,
        name: 'Weekly Content Analysis',
        scraperId: 'example-scraper',
        scraperName: 'Example Scraper',
        schedule: '0 9 * * 1', // Cron expression: At 9:00 AM on Monday
        nextRun: new Date(Date.now() + 5 * 24 * 60 * 60 * 1000), // 5 days from now
        lastRun: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000), // 2 days ago
        status: 'active',
        email: 'user@example.com',
        notifyOnCompletion: true,
        notifyOnError: true,
        maxRuntime: 7200, // 2 hours in seconds
        createdAt: new Date(Date.now() - 60 * 24 * 60 * 60 * 1000) // 60 days ago
      },
      {
        id: 3,
        name: 'Monthly Archive',
        scraperId: 'test-scraper',
        scraperName: 'Test Scraper',
        schedule: '0 0 1 * *', // Cron expression: At midnight on the first day of the month
        nextRun: new Date(Date.now() + 15 * 24 * 60 * 60 * 1000), // 15 days from now
        lastRun: new Date(Date.now() - 15 * 24 * 60 * 60 * 1000), // 15 days ago
        status: 'paused',
        email: 'admin@example.com',
        notifyOnCompletion: false,
        notifyOnError: true,
        maxRuntime: 14400, // 4 hours in seconds
        createdAt: new Date(Date.now() - 90 * 24 * 60 * 60 * 1000) // 90 days ago
      }
    ]);
    
    // Mock history data
    setHistory([
      {
        id: 1,
        scheduleId: 1,
        scheduleName: 'Daily UKGC Scrape',
        scraperId: 'ukgc',
        scraperName: 'UKGC',
        startTime: new Date(Date.now() - 24 * 60 * 60 * 1000), // Yesterday
        endTime: new Date(Date.now() - 24 * 60 * 60 * 1000 + 45 * 60 * 1000), // 45 minutes after start
        status: 'completed',
        urlsProcessed: 250,
        changesDetected: 15,
        errors: 0
      },
      {
        id: 2,
        scheduleId: 2,
        scheduleName: 'Weekly Content Analysis',
        scraperId: 'example-scraper',
        scraperName: 'Example Scraper',
        startTime: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000), // 2 days ago
        endTime: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000 + 95 * 60 * 1000), // 95 minutes after start
        status: 'completed',
        urlsProcessed: 520,
        changesDetected: 42,
        errors: 2
      },
      {
        id: 3,
        scheduleId: 1,
        scheduleName: 'Daily UKGC Scrape',
        scraperId: 'ukgc',
        scraperName: 'UKGC',
        startTime: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000), // 2 days ago
        endTime: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000 + 50 * 60 * 1000), // 50 minutes after start
        status: 'completed',
        urlsProcessed: 248,
        changesDetected: 8,
        errors: 0
      },
      {
        id: 4,
        scheduleId: 3,
        scheduleName: 'Monthly Archive',
        scraperId: 'test-scraper',
        scraperName: 'Test Scraper',
        startTime: new Date(Date.now() - 15 * 24 * 60 * 60 * 1000), // 15 days ago
        endTime: new Date(Date.now() - 15 * 24 * 60 * 60 * 1000 + 180 * 60 * 1000), // 3 hours after start
        status: 'failed',
        urlsProcessed: 1250,
        changesDetected: 0,
        errors: 1,
        errorMessage: 'Connection timeout after 180 seconds'
      }
    ]);
  }, []);
  
  // Handle tab change
  const handleTabChange = (event, newValue) => {
    setTabValue(newValue);
  };
  
  // Handle refresh
  const handleRefresh = async () => {
    setIsRefreshing(true);
    
    // Simulate API call
    setTimeout(() => {
      setIsRefreshing(false);
    }, 1000);
  };
  
  // Handle create schedule
  const handleCreateSchedule = () => {
    setSelectedSchedule(null);
    setDialogOpen(true);
  };
  
  // Handle edit schedule
  const handleEditSchedule = (schedule) => {
    setSelectedSchedule(schedule);
    setDialogOpen(true);
  };
  
  // Handle save schedule
  const handleSaveSchedule = (scheduleData) => {
    if (selectedSchedule) {
      // Edit existing schedule
      setSchedules(schedules.map(schedule => 
        schedule.id === selectedSchedule.id ? { ...schedule, ...scheduleData } : schedule
      ));
    } else {
      // Create new schedule
      const newSchedule = {
        id: Math.max(0, ...schedules.map(s => s.id)) + 1,
        ...scheduleData,
        status: 'active',
        nextRun: new Date(Date.now() + 24 * 60 * 60 * 1000), // Tomorrow
        createdAt: new Date()
      };
      setSchedules([...schedules, newSchedule]);
    }
    
    setDialogOpen(false);
  };
  
  // Handle delete schedule
  const handleDeleteSchedule = (id) => {
    setSchedules(schedules.filter(schedule => schedule.id !== id));
  };
  
  // Handle toggle schedule status
  const handleToggleScheduleStatus = (id) => {
    setSchedules(schedules.map(schedule => 
      schedule.id === id ? 
        { ...schedule, status: schedule.status === 'active' ? 'paused' : 'active' } : 
        schedule
    ));
  };
  
  // Handle run now
  const handleRunNow = (id) => {
    // Simulate running the schedule now
    const schedule = schedules.find(s => s.id === id);
    if (!schedule) return;
    
    // Create a new history entry
    const newHistoryEntry = {
      id: Math.max(0, ...history.map(h => h.id)) + 1,
      scheduleId: schedule.id,
      scheduleName: schedule.name,
      scraperId: schedule.scraperId,
      scraperName: schedule.scraperName,
      startTime: new Date(),
      status: 'running',
      urlsProcessed: 0,
      changesDetected: 0,
      errors: 0
    };
    
    setHistory([newHistoryEntry, ...history]);
    
    // Simulate completion after a delay
    setTimeout(() => {
      setHistory(prev => prev.map(h => 
        h.id === newHistoryEntry.id ? 
          {
            ...h,
            endTime: new Date(),
            status: 'completed',
            urlsProcessed: Math.floor(Math.random() * 300) + 100,
            changesDetected: Math.floor(Math.random() * 30)
          } : 
          h
      ));
      
      // Update last run time for the schedule
      setSchedules(prev => prev.map(s => 
        s.id === id ? { ...s, lastRun: new Date() } : s
      ));
    }, 5000);
  };
  
  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">Scheduling Dashboard</Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Button
            variant="outlined"
            startIcon={isRefreshing ? <CircularProgress size={20} /> : <RefreshIcon />}
            onClick={handleRefresh}
            disabled={isRefreshing || loading}
          >
            Refresh
          </Button>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={handleCreateSchedule}
          >
            Create Schedule
          </Button>
        </Box>
      </Box>
      
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}
      
      {/* Summary Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <Box sx={{ 
                  display: 'flex', 
                  alignItems: 'center', 
                  justifyContent: 'center',
                  bgcolor: 'primary.100', 
                  color: 'primary.800',
                  borderRadius: '50%',
                  p: 1,
                  mr: 2
                }}>
                  <ScheduleIcon />
                </Box>
                <Typography variant="subtitle1" color="text.secondary">
                  Total Schedules
                </Typography>
              </Box>
              <Typography variant="h4" component="div">
                {schedules.length}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} sm={6} md={3}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <Box sx={{ 
                  display: 'flex', 
                  alignItems: 'center', 
                  justifyContent: 'center',
                  bgcolor: 'success.100', 
                  color: 'success.800',
                  borderRadius: '50%',
                  p: 1,
                  mr: 2
                }}>
                  <PlayIcon />
                </Box>
                <Typography variant="subtitle1" color="text.secondary">
                  Active Schedules
                </Typography>
              </Box>
              <Typography variant="h4" component="div">
                {schedules.filter(s => s.status === 'active').length}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} sm={6} md={3}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <Box sx={{ 
                  display: 'flex', 
                  alignItems: 'center', 
                  justifyContent: 'center',
                  bgcolor: 'warning.100', 
                  color: 'warning.800',
                  borderRadius: '50%',
                  p: 1,
                  mr: 2
                }}>
                  <PauseIcon />
                </Box>
                <Typography variant="subtitle1" color="text.secondary">
                  Paused Schedules
                </Typography>
              </Box>
              <Typography variant="h4" component="div">
                {schedules.filter(s => s.status === 'paused').length}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} sm={6} md={3}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <Box sx={{ 
                  display: 'flex', 
                  alignItems: 'center', 
                  justifyContent: 'center',
                  bgcolor: 'info.100', 
                  color: 'info.800',
                  borderRadius: '50%',
                  p: 1,
                  mr: 2
                }}>
                  <HistoryIcon />
                </Box>
                <Typography variant="subtitle1" color="text.secondary">
                  Completed Runs
                </Typography>
              </Box>
              <Typography variant="h4" component="div">
                {history.filter(h => h.status === 'completed').length}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
      
      {/* Tabs */}
      <Paper sx={{ mt: 3 }}>
        <Tabs 
          value={tabValue} 
          onChange={handleTabChange} 
          aria-label="scheduling tabs"
          variant="scrollable"
          scrollButtons="auto"
          sx={{ borderBottom: 1, borderColor: 'divider' }}
        >
          <Tab label="Scheduled Tasks" {...a11yProps(0)} />
          <Tab label="Execution History" {...a11yProps(1)} />
        </Tabs>
        
        <Box sx={{ p: 3 }}>
          <TabPanel value={tabValue} index={0}>
            <ScheduledTasksList 
              schedules={schedules}
              onEdit={handleEditSchedule}
              onDelete={handleDeleteSchedule}
              onToggleStatus={handleToggleScheduleStatus}
              onRunNow={handleRunNow}
              isLoading={loading}
            />
          </TabPanel>
          
          <TabPanel value={tabValue} index={1}>
            <ScheduleHistory 
              history={history}
              isLoading={loading}
            />
          </TabPanel>
        </Box>
      </Paper>
      
      {/* Schedule Form Dialog */}
      <Dialog 
        open={dialogOpen} 
        onClose={() => setDialogOpen(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          {selectedSchedule ? 'Edit Schedule' : 'Create Schedule'}
        </DialogTitle>
        <DialogContent dividers>
          <ScheduleForm 
            schedule={selectedSchedule}
            scrapers={scrapers}
            onSave={handleSaveSchedule}
            onCancel={() => setDialogOpen(false)}
          />
        </DialogContent>
      </Dialog>
    </Container>
  );
};

export default SchedulingDashboard;
