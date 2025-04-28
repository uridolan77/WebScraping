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
import { useScheduling } from '../../hooks/useScheduling';
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
    loading: scrapersLoading,
    error: scrapersError
  } = useScrapers();

  const {
    schedules,
    executionHistory,
    loading,
    error,
    fetchSchedules,
    fetchSchedule,
    createSchedule,
    updateSchedule,
    deleteSchedule,
    runNow,
    toggleScheduleStatus,
    fetchExecutionHistory
  } = useScheduling();

  const [tabValue, setTabValue] = useState(0);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [selectedSchedule, setSelectedSchedule] = useState(null);

  // Fetch initial data
  useEffect(() => {
    const loadInitialData = async () => {
      try {
        await fetchSchedules();
        if (tabValue === 1) {
          // If on the history tab, fetch execution history for all schedules
          await fetchExecutionHistory();
        }
      } catch (err) {
        console.error('Error loading initial data:', err);
      }
    };

    loadInitialData();
  }, [fetchSchedules, fetchExecutionHistory, tabValue]);

  // Handle tab change
  const handleTabChange = (event, newValue) => {
    setTabValue(newValue);

    // If switching to history tab, fetch execution history
    if (newValue === 1) {
      fetchExecutionHistory();
    }
  };

  // Handle refresh
  const handleRefresh = async () => {
    setIsRefreshing(true);

    try {
      await fetchSchedules();
      if (tabValue === 1) {
        await fetchExecutionHistory();
      }
    } catch (err) {
      console.error('Error refreshing data:', err);
    } finally {
      setIsRefreshing(false);
    }
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
  const handleSaveSchedule = async (scheduleData) => {
    try {
      if (selectedSchedule) {
        // Edit existing schedule
        await updateSchedule(selectedSchedule.id, scheduleData);
      } else {
        // Create new schedule
        await createSchedule(scheduleData);
      }

      setDialogOpen(false);
    } catch (err) {
      console.error('Error saving schedule:', err);
    }
  };

  // Handle delete schedule
  const handleDeleteSchedule = async (id) => {
    try {
      await deleteSchedule(id);
    } catch (err) {
      console.error('Error deleting schedule:', err);
    }
  };

  // Handle toggle schedule status
  const handleToggleScheduleStatus = async (id) => {
    try {
      await toggleScheduleStatus(id);
    } catch (err) {
      console.error('Error toggling schedule status:', err);
    }
  };

  // Handle run now
  const handleRunNow = async (id) => {
    try {
      await runNow(id);

      // Refresh execution history if on history tab
      if (tabValue === 1) {
        await fetchExecutionHistory();
      }
    } catch (err) {
      console.error('Error running schedule:', err);
    }
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
              {loading ? (
                <CircularProgress size={24} />
              ) : (
                <Typography variant="h4" component="div">
                  {schedules.length}
                </Typography>
              )}
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
              {loading ? (
                <CircularProgress size={24} />
              ) : (
                <Typography variant="h4" component="div">
                  {schedules.filter(s => s.status === 'active').length}
                </Typography>
              )}
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
              {loading ? (
                <CircularProgress size={24} />
              ) : (
                <Typography variant="h4" component="div">
                  {schedules.filter(s => s.status === 'paused').length}
                </Typography>
              )}
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
              {loading ? (
                <CircularProgress size={24} />
              ) : (
                <Typography variant="h4" component="div">
                  {executionHistory?.filter(h => h.status === 'completed').length || 0}
                </Typography>
              )}
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
              history={executionHistory}
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
