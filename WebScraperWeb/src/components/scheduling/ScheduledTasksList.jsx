import React, { useState } from 'react';
import { 
  Box, Typography, Paper, Table, TableBody, TableCell, 
  TableContainer, TableHead, TableRow, Chip, Button,
  IconButton, Tooltip, CircularProgress, Alert,
  Dialog, DialogTitle, DialogContent, DialogContentText,
  DialogActions, Menu, MenuItem, ListItemIcon, ListItemText
} from '@mui/material';
import {
  PlayArrow as PlayIcon,
  Pause as PauseIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
  Schedule as ScheduleIcon,
  MoreVert as MoreVertIcon,
  Notifications as NotificationsIcon,
  Email as EmailIcon,
  CalendarToday as CalendarIcon,
  AccessTime as TimeIcon,
  Repeat as RepeatIcon,
  History as HistoryIcon
} from '@mui/icons-material';
import { format, formatDistanceToNow } from 'date-fns';
import { Link as RouterLink } from 'react-router-dom';
import cronstrue from 'cronstrue';

const ScheduledTasksList = ({ schedules, onEdit, onDelete, onToggleStatus, onRunNow, isLoading }) => {
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [scheduleToDelete, setScheduleToDelete] = useState(null);
  const [actionInProgress, setActionInProgress] = useState(null);
  const [anchorEl, setAnchorEl] = useState(null);
  const [selectedScheduleId, setSelectedScheduleId] = useState(null);
  
  // Handle menu open
  const handleMenuOpen = (event, scheduleId) => {
    setAnchorEl(event.currentTarget);
    setSelectedScheduleId(scheduleId);
  };
  
  // Handle menu close
  const handleMenuClose = () => {
    setAnchorEl(null);
    setSelectedScheduleId(null);
  };
  
  // Handle delete click
  const handleDeleteClick = (schedule) => {
    setScheduleToDelete(schedule);
    setDeleteDialogOpen(true);
    handleMenuClose();
  };
  
  // Handle delete confirm
  const handleDeleteConfirm = () => {
    if (scheduleToDelete) {
      onDelete(scheduleToDelete.id);
      setDeleteDialogOpen(false);
      setScheduleToDelete(null);
    }
  };
  
  // Handle toggle status
  const handleToggleStatus = (id) => {
    setActionInProgress(id);
    setTimeout(() => {
      onToggleStatus(id);
      setActionInProgress(null);
    }, 500);
    handleMenuClose();
  };
  
  // Handle run now
  const handleRunNow = (id) => {
    setActionInProgress(id);
    setTimeout(() => {
      onRunNow(id);
      setActionInProgress(null);
    }, 500);
    handleMenuClose();
  };
  
  // Handle edit
  const handleEdit = (schedule) => {
    onEdit(schedule);
    handleMenuClose();
  };
  
  // Format cron expression to human-readable text
  const formatCronExpression = (cronExpression) => {
    try {
      return cronstrue.toString(cronExpression);
    } catch (error) {
      return cronExpression;
    }
  };
  
  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }
  
  if (schedules.length === 0) {
    return (
      <Alert severity="info">
        No scheduled tasks found. Create a schedule to automate your scraping tasks.
      </Alert>
    );
  }
  
  return (
    <Box>
      <TableContainer component={Paper} variant="outlined">
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Scraper</TableCell>
              <TableCell>Schedule</TableCell>
              <TableCell>Next Run</TableCell>
              <TableCell>Last Run</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {schedules.map((schedule) => (
              <TableRow key={schedule.id} hover>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center' }}>
                    <ScheduleIcon sx={{ mr: 1, color: 'primary.main' }} />
                    <Typography variant="body2">{schedule.name}</Typography>
                  </Box>
                </TableCell>
                <TableCell>{schedule.scraperName}</TableCell>
                <TableCell>
                  <Tooltip title={formatCronExpression(schedule.schedule)}>
                    <Typography variant="body2">{schedule.schedule}</Typography>
                  </Tooltip>
                </TableCell>
                <TableCell>
                  {schedule.nextRun ? (
                    <Tooltip title={format(new Date(schedule.nextRun), 'PPpp')}>
                      <Typography variant="body2">
                        {formatDistanceToNow(new Date(schedule.nextRun), { addSuffix: true })}
                      </Typography>
                    </Tooltip>
                  ) : (
                    'Not scheduled'
                  )}
                </TableCell>
                <TableCell>
                  {schedule.lastRun ? (
                    <Tooltip title={format(new Date(schedule.lastRun), 'PPpp')}>
                      <Typography variant="body2">
                        {formatDistanceToNow(new Date(schedule.lastRun), { addSuffix: true })}
                      </Typography>
                    </Tooltip>
                  ) : (
                    'Never'
                  )}
                </TableCell>
                <TableCell>
                  <Chip 
                    label={schedule.status === 'active' ? 'Active' : 'Paused'} 
                    color={schedule.status === 'active' ? 'success' : 'default'} 
                    size="small" 
                  />
                </TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    <Tooltip title={schedule.status === 'active' ? 'Pause' : 'Activate'}>
                      <IconButton 
                        size="small"
                        onClick={() => handleToggleStatus(schedule.id)}
                        disabled={actionInProgress === schedule.id}
                        color={schedule.status === 'active' ? 'warning' : 'success'}
                      >
                        {actionInProgress === schedule.id ? (
                          <CircularProgress size={20} />
                        ) : schedule.status === 'active' ? (
                          <PauseIcon fontSize="small" />
                        ) : (
                          <PlayIcon fontSize="small" />
                        )}
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Run Now">
                      <IconButton 
                        size="small"
                        onClick={() => handleRunNow(schedule.id)}
                        disabled={actionInProgress === schedule.id}
                        color="primary"
                      >
                        {actionInProgress === schedule.id ? (
                          <CircularProgress size={20} />
                        ) : (
                          <PlayIcon fontSize="small" />
                        )}
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="More Actions">
                      <IconButton 
                        size="small"
                        onClick={(event) => handleMenuOpen(event, schedule.id)}
                      >
                        <MoreVertIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </Box>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
      
      {/* Action Menu */}
      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleMenuClose}
      >
        <MenuItem onClick={() => handleEdit(schedules.find(s => s.id === selectedScheduleId))}>
          <ListItemIcon>
            <EditIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Edit</ListItemText>
        </MenuItem>
        <MenuItem onClick={() => handleToggleStatus(selectedScheduleId)}>
          <ListItemIcon>
            {schedules.find(s => s.id === selectedScheduleId)?.status === 'active' ? (
              <PauseIcon fontSize="small" />
            ) : (
              <PlayIcon fontSize="small" />
            )}
          </ListItemIcon>
          <ListItemText>
            {schedules.find(s => s.id === selectedScheduleId)?.status === 'active' ? 'Pause' : 'Activate'}
          </ListItemText>
        </MenuItem>
        <MenuItem onClick={() => handleRunNow(selectedScheduleId)}>
          <ListItemIcon>
            <PlayIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Run Now</ListItemText>
        </MenuItem>
        <MenuItem onClick={() => handleDeleteClick(schedules.find(s => s.id === selectedScheduleId))}>
          <ListItemIcon>
            <DeleteIcon fontSize="small" color="error" />
          </ListItemIcon>
          <ListItemText sx={{ color: 'error.main' }}>Delete</ListItemText>
        </MenuItem>
      </Menu>
      
      {/* Delete Confirmation Dialog */}
      <Dialog
        open={deleteDialogOpen}
        onClose={() => setDeleteDialogOpen(false)}
      >
        <DialogTitle>Delete Schedule</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete the schedule "{scheduleToDelete?.name}"? This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleDeleteConfirm} color="error">Delete</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ScheduledTasksList;
