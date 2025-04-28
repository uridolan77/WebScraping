import React, { useState } from 'react';
import {
  Box, Typography, Paper, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, Chip, Button,
  IconButton, Tooltip, CircularProgress, Alert,
  FormControl, InputLabel, Select, MenuItem, TextField,
  InputAdornment, Dialog, DialogTitle, DialogContent,
  DialogActions, List, ListItem, ListItemText, ListItemIcon,
  Divider, Grid
} from '@mui/material';
import {
  Search as SearchIcon,
  Clear as ClearIcon,
  FilterList as FilterIcon,
  Visibility as VisibilityIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Warning as WarningIcon,
  Schedule as ScheduleIcon,
  AccessTime as TimeIcon,
  Link as LinkIcon,
  FindInPage as FindInPageIcon,
  ChangeHistory as ChangeHistoryIcon
} from '@mui/icons-material';
import { format, formatDistanceToNow, formatDuration, intervalToDuration } from 'date-fns';

const ScheduleHistory = ({ history, isLoading }) => {
  const [statusFilter, setStatusFilter] = useState('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [detailsDialogOpen, setDetailsDialogOpen] = useState(false);
  const [selectedRun, setSelectedRun] = useState(null);

  // Handle status filter change
  const handleStatusFilterChange = (event) => {
    setStatusFilter(event.target.value);
  };

  // Handle search change
  const handleSearchChange = (event) => {
    setSearchTerm(event.target.value);
  };

  // Handle clear filters
  const handleClearFilters = () => {
    setStatusFilter('all');
    setSearchTerm('');
  };

  // Handle view details
  const handleViewDetails = (run) => {
    setSelectedRun(run);
    setDetailsDialogOpen(true);
  };

  // Get status chip
  const getStatusChip = (status) => {
    switch (status) {
      case 'completed':
        return <Chip label="Completed" color="success" size="small" icon={<CheckCircleIcon />} />;
      case 'failed':
        return <Chip label="Failed" color="error" size="small" icon={<ErrorIcon />} />;
      case 'running':
        return <Chip label="Running" color="primary" size="small" />;
      default:
        return <Chip label={status} size="small" />;
    }
  };

  // Calculate duration
  const calculateDuration = (startTime, endTime) => {
    if (!startTime) return 'N/A';

    const start = new Date(startTime);
    const end = endTime ? new Date(endTime) : new Date();

    const duration = intervalToDuration({ start, end });

    let result = '';
    if (duration.hours > 0) result += `${duration.hours}h `;
    if (duration.minutes > 0) result += `${duration.minutes}m `;
    result += `${duration.seconds}s`;

    return result;
  };

  // Filter history
  const filteredHistory = history.filter(run => {
    // Filter by status
    if (statusFilter !== 'all' && run.status !== statusFilter) {
      return false;
    }

    // Filter by search term
    if (searchTerm && !run.scheduleName.toLowerCase().includes(searchTerm.toLowerCase()) &&
        !run.scraperName.toLowerCase().includes(searchTerm.toLowerCase())) {
      return false;
    }

    return true;
  });

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (history.length === 0) {
    return (
      <Alert severity="info">
        No execution history found. Schedule and run tasks to see their execution history.
      </Alert>
    );
  }

  return (
    <Box>
      {/* Filters */}
      <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
        <FormControl variant="outlined" size="small" sx={{ minWidth: 150 }}>
          <InputLabel id="status-filter-label">Status</InputLabel>
          <Select
            labelId="status-filter-label"
            value={statusFilter}
            onChange={handleStatusFilterChange}
            label="Status"
          >
            <MenuItem value="all">All Status</MenuItem>
            <MenuItem value="completed">Completed</MenuItem>
            <MenuItem value="failed">Failed</MenuItem>
            <MenuItem value="running">Running</MenuItem>
          </Select>
        </FormControl>

        <TextField
          size="small"
          placeholder="Search by name..."
          value={searchTerm}
          onChange={handleSearchChange}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon fontSize="small" />
              </InputAdornment>
            ),
            endAdornment: searchTerm && (
              <InputAdornment position="end">
                <IconButton
                  size="small"
                  onClick={() => setSearchTerm('')}
                  edge="end"
                >
                  <ClearIcon fontSize="small" />
                </IconButton>
              </InputAdornment>
            )
          }}
          sx={{ flexGrow: 1 }}
        />

        {(statusFilter !== 'all' || searchTerm) && (
          <Button
            variant="text"
            startIcon={<ClearIcon />}
            onClick={handleClearFilters}
          >
            Clear Filters
          </Button>
        )}
      </Box>

      {/* History Table */}
      {filteredHistory.length > 0 ? (
        <TableContainer component={Paper} variant="outlined">
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Schedule</TableCell>
                <TableCell>Scraper</TableCell>
                <TableCell>Start Time</TableCell>
                <TableCell>Duration</TableCell>
                <TableCell>URLs Processed</TableCell>
                <TableCell>Changes</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredHistory.map((run) => (
                <TableRow key={run.id} hover>
                  <TableCell>{run.scheduleName}</TableCell>
                  <TableCell>{run.scraperName}</TableCell>
                  <TableCell>
                    <Tooltip title={format(new Date(run.startTime), 'PPpp')}>
                      <Typography variant="body2">
                        {formatDistanceToNow(new Date(run.startTime), { addSuffix: true })}
                      </Typography>
                    </Tooltip>
                  </TableCell>
                  <TableCell>
                    {calculateDuration(run.startTime, run.endTime)}
                  </TableCell>
                  <TableCell>{run.urlsProcessed}</TableCell>
                  <TableCell>{run.changesDetected}</TableCell>
                  <TableCell>
                    {getStatusChip(run.status)}
                  </TableCell>
                  <TableCell>
                    <Tooltip title="View Details">
                      <IconButton
                        size="small"
                        onClick={() => handleViewDetails(run)}
                      >
                        <VisibilityIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      ) : (
        <Alert severity="info">
          No execution history found matching your filters.
        </Alert>
      )}

      {/* Details Dialog */}
      <Dialog
        open={detailsDialogOpen}
        onClose={() => setDetailsDialogOpen(false)}
        maxWidth="md"
        fullWidth
      >
        {selectedRun && (
          <>
            <DialogTitle>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <ScheduleIcon sx={{ mr: 1 }} />
                Execution Details: {selectedRun.scheduleName}
              </Box>
            </DialogTitle>
            <DialogContent dividers>
              <Grid container spacing={3}>
                <Grid item xs={12} md={6}>
                  <Typography variant="subtitle1" gutterBottom>
                    General Information
                  </Typography>
                  <List dense>
                    <ListItem>
                      <ListItemIcon>
                        <ScheduleIcon />
                      </ListItemIcon>
                      <ListItemText
                        primary="Schedule"
                        secondary={selectedRun.scheduleName}
                      />
                    </ListItem>
                    <Divider component="li" />
                    <ListItem>
                      <ListItemIcon>
                        <LinkIcon />
                      </ListItemIcon>
                      <ListItemText
                        primary="Scraper"
                        secondary={selectedRun.scraperName}
                      />
                    </ListItem>
                    <Divider component="li" />
                    <ListItem>
                      <ListItemIcon>
                        {selectedRun.status === 'completed' ? (
                          <CheckCircleIcon color="success" />
                        ) : selectedRun.status === 'failed' ? (
                          <ErrorIcon color="error" />
                        ) : (
                          <CircularProgress size={20} />
                        )}
                      </ListItemIcon>
                      <ListItemText
                        primary="Status"
                        secondary={selectedRun.status.charAt(0).toUpperCase() + selectedRun.status.slice(1)}
                      />
                    </ListItem>
                  </List>
                </Grid>

                <Grid item xs={12} md={6}>
                  <Typography variant="subtitle1" gutterBottom>
                    Timing Information
                  </Typography>
                  <List dense>
                    <ListItem>
                      <ListItemIcon>
                        <TimeIcon />
                      </ListItemIcon>
                      <ListItemText
                        primary="Start Time"
                        secondary={format(new Date(selectedRun.startTime), 'PPpp')}
                      />
                    </ListItem>
                    <Divider component="li" />
                    <ListItem>
                      <ListItemIcon>
                        <TimeIcon />
                      </ListItemIcon>
                      <ListItemText
                        primary="End Time"
                        secondary={selectedRun.endTime ? format(new Date(selectedRun.endTime), 'PPpp') : 'Still running'}
                      />
                    </ListItem>
                    <Divider component="li" />
                    <ListItem>
                      <ListItemIcon>
                        <TimeIcon />
                      </ListItemIcon>
                      <ListItemText
                        primary="Duration"
                        secondary={calculateDuration(selectedRun.startTime, selectedRun.endTime)}
                      />
                    </ListItem>
                  </List>
                </Grid>

                <Grid item xs={12}>
                  <Divider sx={{ my: 2 }} />
                </Grid>

                <Grid item xs={12} md={6}>
                  <Typography variant="subtitle1" gutterBottom>
                    Results
                  </Typography>
                  <List dense>
                    <ListItem>
                      <ListItemIcon>
                        <FindInPageIcon />
                      </ListItemIcon>
                      <ListItemText
                        primary="URLs Processed"
                        secondary={selectedRun.urlsProcessed}
                      />
                    </ListItem>
                    <Divider component="li" />
                    <ListItem>
                      <ListItemIcon>
                        <ChangeHistoryIcon />
                      </ListItemIcon>
                      <ListItemText
                        primary="Changes Detected"
                        secondary={selectedRun.changesDetected}
                      />
                    </ListItem>
                    <Divider component="li" />
                    <ListItem>
                      <ListItemIcon>
                        <ErrorIcon />
                      </ListItemIcon>
                      <ListItemText
                        primary="Errors"
                        secondary={selectedRun.errors}
                      />
                    </ListItem>
                  </List>
                </Grid>

                <Grid item xs={12} md={6}>
                  {selectedRun.errorMessage && (
                    <Alert severity="error" sx={{ mb: 2 }}>
                      <Typography variant="subtitle2">Error Message:</Typography>
                      <Typography variant="body2">{selectedRun.errorMessage}</Typography>
                    </Alert>
                  )}

                  {selectedRun.status === 'completed' && (
                    <Alert severity="success">
                      Execution completed successfully.
                    </Alert>
                  )}

                  {selectedRun.status === 'running' && (
                    <Alert severity="info" icon={<CircularProgress size={20} />}>
                      Execution is currently in progress.
                    </Alert>
                  )}
                </Grid>
              </Grid>
            </DialogContent>
            <DialogActions>
              <Button onClick={() => setDetailsDialogOpen(false)}>Close</Button>
            </DialogActions>
          </>
        )}
      </Dialog>
    </Box>
  );
};

export default ScheduleHistory;
