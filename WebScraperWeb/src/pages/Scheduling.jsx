import React, { useState, useEffect } from 'react';
import { 
  Container, 
  Box, 
  Typography, 
  Paper, 
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
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Grid,
  Tooltip,
  CircularProgress,
  Alert,
  Divider
} from '@mui/material';
import { 
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  PlayArrow as PlayIcon,
  Pause as PauseIcon,
  Refresh as RefreshIcon,
  Schedule as ScheduleIcon
} from '@mui/icons-material';
import { Link } from 'react-router-dom';
import PageHeader from '../components/common/PageHeader';
import LoadingSpinner from '../components/common/LoadingSpinner';
import { useScrapers } from '../contexts/ScraperContext';
import { formatDate } from '../utils/formatters';

// Mock data for scheduled tasks
const mockScheduledTasks = [
  {
    id: '1',
    name: 'Daily UKGC Scrape',
    scraperId: 'd6d6eb97-7136-4eaf-b7ca-16ed6202c7ad',
    scraperName: 'ukgc',
    schedule: 'DAILY',
    time: '02:00',
    lastRun: new Date(Date.now() - 86400000),
    nextRun: new Date(Date.now() + 86400000),
    status: 'active'
  },
  {
    id: '2',
    name: 'Weekly MGA Scrape',
    scraperId: '1af8de30-c878-4507-9ff6-5e595960e14c',
    scraperName: 'mga',
    schedule: 'WEEKLY',
    day: 'MONDAY',
    time: '03:00',
    lastRun: new Date(Date.now() - 604800000),
    nextRun: new Date(Date.now() + 604800000),
    status: 'active'
  },
  {
    id: '3',
    name: 'Monthly Gibraltar Scrape',
    scraperId: '3',
    scraperName: 'gibraltar',
    schedule: 'MONTHLY',
    day: '1',
    time: '04:00',
    lastRun: new Date(Date.now() - 2592000000),
    nextRun: new Date(Date.now() + 2592000000),
    status: 'paused'
  }
];

const Scheduling = () => {
  const { scrapers, loading: scrapersLoading, error: scrapersError, fetchScrapers } = useScrapers();
  
  const [scheduledTasks, setScheduledTasks] = useState(mockScheduledTasks);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingTask, setEditingTask] = useState(null);
  const [formData, setFormData] = useState({
    name: '',
    scraperId: '',
    schedule: 'DAILY',
    day: 'MONDAY',
    time: '00:00',
    status: 'active'
  });
  const [actionInProgress, setActionInProgress] = useState(null);

  // Fetch scrapers for dropdown
  useEffect(() => {
    fetchScrapers();
  }, [fetchScrapers]);

  // Handle form input changes
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  // Open dialog for creating a new task
  const handleOpenCreateDialog = () => {
    setEditingTask(null);
    setFormData({
      name: '',
      scraperId: '',
      schedule: 'DAILY',
      day: 'MONDAY',
      time: '00:00',
      status: 'active'
    });
    setOpenDialog(true);
  };

  // Open dialog for editing an existing task
  const handleOpenEditDialog = (task) => {
    setEditingTask(task);
    setFormData({
      name: task.name,
      scraperId: task.scraperId,
      schedule: task.schedule,
      day: task.day || 'MONDAY',
      time: task.time,
      status: task.status
    });
    setOpenDialog(true);
  };

  // Close dialog
  const handleCloseDialog = () => {
    setOpenDialog(false);
  };

  // Save task (create or update)
  const handleSaveTask = () => {
    // Validate form
    if (!formData.name || !formData.scraperId) {
      setError('Please fill in all required fields');
      return;
    }

    if (editingTask) {
      // Update existing task
      setScheduledTasks(prev => 
        prev.map(task => 
          task.id === editingTask.id ? { ...task, ...formData } : task
        )
      );
    } else {
      // Create new task
      const newTask = {
        id: Math.random().toString(36).substring(2, 9),
        ...formData,
        scraperName: scrapers.find(s => s.id === formData.scraperId)?.name || 'Unknown',
        lastRun: null,
        nextRun: new Date(Date.now() + 86400000) // Mock next run time
      };
      setScheduledTasks(prev => [...prev, newTask]);
    }

    setOpenDialog(false);
  };

  // Delete a task
  const handleDeleteTask = (id) => {
    if (window.confirm('Are you sure you want to delete this scheduled task?')) {
      setScheduledTasks(prev => prev.filter(task => task.id !== id));
    }
  };

  // Run a task immediately
  const handleRunNow = async (id) => {
    try {
      setActionInProgress(id);
      // Mock API call
      await new Promise(resolve => setTimeout(resolve, 1500));
      
      // Update last run time
      setScheduledTasks(prev => 
        prev.map(task => 
          task.id === id ? { ...task, lastRun: new Date() } : task
        )
      );
    } catch (err) {
      setError(`Failed to run task: ${err.message}`);
    } finally {
      setActionInProgress(null);
    }
  };

  // Pause/resume a task
  const handleToggleStatus = (id) => {
    setScheduledTasks(prev => 
      prev.map(task => 
        task.id === id ? { 
          ...task, 
          status: task.status === 'active' ? 'paused' : 'active' 
        } : task
      )
    );
  };

  if (loading && scheduledTasks.length === 0) {
    return <LoadingSpinner message="Loading scheduling data..." />;
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      {/* Header */}
      <PageHeader
        title="Scheduling"
        subtitle="Schedule and automate your scraping tasks"
        actionText="Create Schedule"
        onActionClick={handleOpenCreateDialog}
        breadcrumbs={[
          { text: 'Dashboard', path: '/' },
          { text: 'Scheduling' }
        ]}
      />

      {/* Error Alert */}
      {(error || scrapersError) && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error || scrapersError}
        </Alert>
      )}

      {/* Refresh Button */}
      <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 3 }}>
        <Button
          variant="outlined"
          startIcon={<RefreshIcon />}
          onClick={() => {
            // Refresh logic would go here
          }}
        >
          Refresh
        </Button>
      </Box>

      {/* Scheduled Tasks Table */}
      <Paper>
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Scraper</TableCell>
                <TableCell>Schedule</TableCell>
                <TableCell>Last Run</TableCell>
                <TableCell>Next Run</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {scheduledTasks.length > 0 ? (
                scheduledTasks.map((task) => (
                  <TableRow key={task.id}>
                    <TableCell>{task.name}</TableCell>
                    <TableCell>
                      <Link 
                        to={`/scrapers/${task.scraperId}`} 
                        style={{ textDecoration: 'none', color: 'inherit', fontWeight: 'bold' }}
                      >
                        {task.scraperName}
                      </Link>
                    </TableCell>
                    <TableCell>
                      {task.schedule === 'DAILY' && `Daily at ${task.time}`}
                      {task.schedule === 'WEEKLY' && `Weekly on ${task.day} at ${task.time}`}
                      {task.schedule === 'MONTHLY' && `Monthly on day ${task.day} at ${task.time}`}
                    </TableCell>
                    <TableCell>
                      {task.lastRun ? formatDate(task.lastRun, 'MMM d, yyyy HH:mm') : 'Never'}
                    </TableCell>
                    <TableCell>
                      {task.nextRun ? formatDate(task.nextRun, 'MMM d, yyyy HH:mm') : 'Not scheduled'}
                    </TableCell>
                    <TableCell>
                      <Chip 
                        label={task.status === 'active' ? 'Active' : 'Paused'} 
                        color={task.status === 'active' ? 'success' : 'default'}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', gap: 1 }}>
                        <Tooltip title="Run Now">
                          <IconButton 
                            color="primary"
                            onClick={() => handleRunNow(task.id)}
                            disabled={actionInProgress === task.id}
                            size="small"
                          >
                            {actionInProgress === task.id ? (
                              <CircularProgress size={24} />
                            ) : (
                              <PlayIcon />
                            )}
                          </IconButton>
                        </Tooltip>
                        
                        <Tooltip title={task.status === 'active' ? 'Pause' : 'Resume'}>
                          <IconButton 
                            color={task.status === 'active' ? 'warning' : 'success'}
                            onClick={() => handleToggleStatus(task.id)}
                            size="small"
                          >
                            {task.status === 'active' ? <PauseIcon /> : <PlayIcon />}
                          </IconButton>
                        </Tooltip>
                        
                        <Tooltip title="Edit">
                          <IconButton 
                            color="info"
                            onClick={() => handleOpenEditDialog(task)}
                            size="small"
                          >
                            <EditIcon />
                          </IconButton>
                        </Tooltip>
                        
                        <Tooltip title="Delete">
                          <IconButton 
                            color="error"
                            onClick={() => handleDeleteTask(task.id)}
                            size="small"
                          >
                            <DeleteIcon />
                          </IconButton>
                        </Tooltip>
                      </Box>
                    </TableCell>
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell colSpan={7} align="center" sx={{ py: 3 }}>
                    <Typography variant="body1" gutterBottom>
                      No scheduled tasks found. Create your first scheduled task to get started.
                    </Typography>
                    <Button 
                      variant="contained" 
                      startIcon={<AddIcon />}
                      onClick={handleOpenCreateDialog}
                      sx={{ mt: 1 }}
                    >
                      Create Schedule
                    </Button>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>

      {/* Create/Edit Task Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle>
          {editingTask ? 'Edit Scheduled Task' : 'Create Scheduled Task'}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <TextField
                  name="name"
                  label="Task Name"
                  value={formData.name}
                  onChange={handleInputChange}
                  fullWidth
                  required
                />
              </Grid>
              
              <Grid item xs={12}>
                <FormControl fullWidth required>
                  <InputLabel id="scraper-select-label">Scraper</InputLabel>
                  <Select
                    labelId="scraper-select-label"
                    name="scraperId"
                    value={formData.scraperId}
                    onChange={handleInputChange}
                    label="Scraper"
                  >
                    {scrapersLoading ? (
                      <MenuItem disabled>Loading scrapers...</MenuItem>
                    ) : scrapers.length > 0 ? (
                      scrapers.map(scraper => (
                        <MenuItem key={scraper.id} value={scraper.id}>
                          {scraper.name}
                        </MenuItem>
                      ))
                    ) : (
                      <MenuItem disabled>No scrapers available</MenuItem>
                    )}
                  </Select>
                </FormControl>
              </Grid>
              
              <Grid item xs={12} sm={4}>
                <FormControl fullWidth>
                  <InputLabel id="schedule-select-label">Schedule</InputLabel>
                  <Select
                    labelId="schedule-select-label"
                    name="schedule"
                    value={formData.schedule}
                    onChange={handleInputChange}
                    label="Schedule"
                  >
                    <MenuItem value="DAILY">Daily</MenuItem>
                    <MenuItem value="WEEKLY">Weekly</MenuItem>
                    <MenuItem value="MONTHLY">Monthly</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              
              {formData.schedule === 'WEEKLY' && (
                <Grid item xs={12} sm={4}>
                  <FormControl fullWidth>
                    <InputLabel id="day-select-label">Day</InputLabel>
                    <Select
                      labelId="day-select-label"
                      name="day"
                      value={formData.day}
                      onChange={handleInputChange}
                      label="Day"
                    >
                      <MenuItem value="MONDAY">Monday</MenuItem>
                      <MenuItem value="TUESDAY">Tuesday</MenuItem>
                      <MenuItem value="WEDNESDAY">Wednesday</MenuItem>
                      <MenuItem value="THURSDAY">Thursday</MenuItem>
                      <MenuItem value="FRIDAY">Friday</MenuItem>
                      <MenuItem value="SATURDAY">Saturday</MenuItem>
                      <MenuItem value="SUNDAY">Sunday</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
              )}
              
              {formData.schedule === 'MONTHLY' && (
                <Grid item xs={12} sm={4}>
                  <TextField
                    name="day"
                    label="Day of Month"
                    type="number"
                    value={formData.day}
                    onChange={handleInputChange}
                    fullWidth
                    InputProps={{ inputProps: { min: 1, max: 31 } }}
                  />
                </Grid>
              )}
              
              <Grid item xs={12} sm={formData.schedule === 'DAILY' ? 4 : 4}>
                <TextField
                  name="time"
                  label="Time"
                  type="time"
                  value={formData.time}
                  onChange={handleInputChange}
                  fullWidth
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>
              
              <Grid item xs={12} sm={formData.schedule === 'DAILY' ? 4 : 4}>
                <FormControl fullWidth>
                  <InputLabel id="status-select-label">Status</InputLabel>
                  <Select
                    labelId="status-select-label"
                    name="status"
                    value={formData.status}
                    onChange={handleInputChange}
                    label="Status"
                  >
                    <MenuItem value="active">Active</MenuItem>
                    <MenuItem value="paused">Paused</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
            </Grid>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button 
            onClick={handleSaveTask} 
            variant="contained" 
            startIcon={<ScheduleIcon />}
          >
            {editingTask ? 'Update Schedule' : 'Create Schedule'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default Scheduling;
