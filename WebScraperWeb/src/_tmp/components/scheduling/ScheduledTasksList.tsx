import React, { useState, useCallback, useMemo } from 'react';
import {
  Box, Typography, Button, Chip, IconButton, Tooltip,
  CircularProgress, Alert, Dialog, DialogTitle,
  DialogContent, DialogContentText, DialogActions,
  Menu, MenuItem, ListItemIcon, ListItemText
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
import PaginatedTable from '../common/PaginatedTable';

interface Schedule {
  id: string;
  name: string;
  scraperName: string;
  schedule: string;
  nextRun?: string;
  lastRun?: string;
  status: 'active' | 'paused';
  scraperId: string;
}

interface ScheduledTasksListProps {
  schedules: Schedule[];
  onEdit: (schedule: Schedule) => void;
  onDelete: (id: string) => void;
  onToggleStatus: (id: string) => void;
  onRunNow: (id: string) => void;
  isLoading: boolean;
  onRefresh: () => void;
}

const ScheduledTasksList = React.memo<ScheduledTasksListProps>(({ schedules, onEdit, onDelete, onToggleStatus, onRunNow, isLoading, onRefresh }) => {
  const [deleteDialogOpen, setDeleteDialogOpen] = useState<boolean>(false);
  const [scheduleToDelete, setScheduleToDelete] = useState<Schedule | null>(null);
  const [actionInProgress, setActionInProgress] = useState<string | null>(null);
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);
  const [selectedScheduleId, setSelectedScheduleId] = useState<string | null>(null);

  // Pagination state
  const [page, setPage] = useState<number>(0);
  const [rowsPerPage, setRowsPerPage] = useState<number>(10);
  const [sortBy, setSortBy] = useState<string>('name');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');
  const [searchTerm, setSearchTerm] = useState('');

  // Handle menu open
  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>, scheduleId: string) => {
    setAnchorEl(event.currentTarget);
    setSelectedScheduleId(scheduleId);
  };

  // Handle menu close
  const handleMenuClose = () => {
    setAnchorEl(null);
    setSelectedScheduleId(null);
  };

  // Handle delete click
  const handleDeleteClick = (schedule: Schedule) => {
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
  const handleToggleStatus = (id: string) => {
    setActionInProgress(id);
    setTimeout(() => {
      onToggleStatus(id);
      setActionInProgress(null);
    }, 500);
    handleMenuClose();
  };

  // Handle run now
  const handleRunNow = (id: string) => {
    setActionInProgress(id);
    setTimeout(() => {
      onRunNow(id);
      setActionInProgress(null);
    }, 500);
    handleMenuClose();
  };

  // Handle edit
  const handleEdit = (schedule: Schedule) => {
    onEdit(schedule);
    handleMenuClose();
  };

  // Format cron expression to human-readable text
  const formatCronExpression = (cronExpression: string): string => {
    try {
      return cronstrue.toString(cronExpression);
    } catch (error) {
      return cronExpression;
    }
  };

  // Handle page change
  const handlePageChange = useCallback((newPage: number) => {
    setPage(newPage);
  }, []);

  // Handle rows per page change
  const handleRowsPerPageChange = useCallback((newRowsPerPage: number) => {
    setRowsPerPage(newRowsPerPage);
    setPage(0);
  }, []);

  // Handle sort
  const handleSort = useCallback((column: string, direction: 'asc' | 'desc') => {
    setSortBy(column);
    setSortDirection(direction);
  }, []);

  // Handle search
  const handleSearch = useCallback((term: string) => {
    setSearchTerm(term);
    setPage(0);
  }, []);

  // Filter and sort data
  const filteredData = useMemo(() => {
    if (!searchTerm) return schedules;

    const lowerSearchTerm = searchTerm.toLowerCase();
    return schedules.filter(schedule =>
      schedule.name.toLowerCase().includes(lowerSearchTerm) ||
      schedule.scraperName.toLowerCase().includes(lowerSearchTerm) ||
      schedule.schedule.toLowerCase().includes(lowerSearchTerm)
    );
  }, [schedules, searchTerm]);

  // Sort data
  const sortedData = useMemo(() => {
    if (!filteredData) return [];

    return [...filteredData].sort((a, b) => {
      let aValue: any = (a as any)[sortBy];
      let bValue: any = (b as any)[sortBy];

      // Handle special cases
      if (sortBy === 'nextRun' || sortBy === 'lastRun') {
        aValue = aValue ? new Date(aValue).getTime() : 0;
        bValue = bValue ? new Date(bValue).getTime() : 0;
      }

      if (aValue < bValue) return sortDirection === 'asc' ? -1 : 1;
      if (aValue > bValue) return sortDirection === 'asc' ? 1 : -1;
      return 0;
    });
  }, [filteredData, sortBy, sortDirection]);

  // Paginate data
  const paginatedData = useMemo(() => {
    const startIndex = page * rowsPerPage;
    return sortedData.slice(startIndex, startIndex + rowsPerPage);
  }, [sortedData, page, rowsPerPage]);

  interface TableColumn {
    id: string;
    label: string;
    sortable: boolean;
    render?: (value: any, row?: Schedule) => React.ReactNode;
  }

  // Define table columns
  const columns = useMemo<TableColumn[]>(() => [
    {
      id: 'name',
      label: 'Name',
      sortable: true,
      render: (value: string, row?: Schedule) => (
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <ScheduleIcon sx={{ mr: 1, color: 'primary.main' }} />
          <Typography variant="body2">{value}</Typography>
        </Box>
      )
    },
    {
      id: 'scraperName',
      label: 'Scraper',
      sortable: true
    },
    {
      id: 'schedule',
      label: 'Schedule',
      sortable: true,
      render: (value: string) => (
        <Tooltip title={formatCronExpression(value)}>
          <Typography variant="body2">{value}</Typography>
        </Tooltip>
      )
    },
    {
      id: 'nextRun',
      label: 'Next Run',
      sortable: true,
      render: (value: string | undefined) => value ? (
        <Tooltip title={format(new Date(value), 'PPpp')}>
          <Typography variant="body2">
            {formatDistanceToNow(new Date(value), { addSuffix: true })}
          </Typography>
        </Tooltip>
      ) : 'Not scheduled'
    },
    {
      id: 'lastRun',
      label: 'Last Run',
      sortable: true,
      render: (value: string | undefined) => value ? (
        <Tooltip title={format(new Date(value), 'PPpp')}>
          <Typography variant="body2">
            {formatDistanceToNow(new Date(value), { addSuffix: true })}
          </Typography>
        </Tooltip>
      ) : 'Never'
    },
    {
      id: 'status',
      label: 'Status',
      sortable: true,
      render: (value: string) => (
        <Chip
          label={value === 'active' ? 'Active' : 'Paused'}
          color={value === 'active' ? 'success' : 'default'}
          size="small"
        />
      )
    }
  ], []);

  // Define row actions
  const renderRowActions = useCallback((row: Schedule) => (
    <Box sx={{ display: 'flex', gap: 1 }}>
      <Tooltip title={row.status === 'active' ? 'Pause' : 'Activate'}>
        <IconButton
          size="small"
          onClick={() => handleToggleStatus(row.id)}
          disabled={actionInProgress === row.id}
          color={row.status === 'active' ? 'warning' : 'success'}
        >
          {actionInProgress === row.id ? (
            <CircularProgress size={20} />
          ) : row.status === 'active' ? (
            <PauseIcon fontSize="small" />
          ) : (
            <PlayIcon fontSize="small" />
          )}
        </IconButton>
      </Tooltip>
      <Tooltip title="Run Now">
        <IconButton
          size="small"
          onClick={() => handleRunNow(row.id)}
          disabled={actionInProgress === row.id}
          color="primary"
        >
          {actionInProgress === row.id ? (
            <CircularProgress size={20} />
          ) : (
            <PlayIcon fontSize="small" />
          )}
        </IconButton>
      </Tooltip>
      <Tooltip title="More Actions">
        <IconButton
          size="small"
          onClick={(event) => handleMenuOpen(event, row.id)}
        >
          <MoreVertIcon fontSize="small" />
        </IconButton>
      </Tooltip>
    </Box>
  ), [actionInProgress, handleMenuOpen, handleRunNow, handleToggleStatus]);

  // Empty message
  const emptyMessage = "No scheduled tasks found. Create a schedule to automate your scraping tasks.";

  if (isLoading && schedules.length === 0) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <PaginatedTable
        columns={columns}
        data={paginatedData}
        totalCount={filteredData.length}
        page={page}
        rowsPerPage={rowsPerPage}
        onPageChange={handlePageChange}
        onRowsPerPageChange={handleRowsPerPageChange}
        onSort={handleSort}
        onSearch={handleSearch}
        onRefresh={onRefresh}
        sortBy={sortBy}
        sortDirection={sortDirection}
        searchTerm={searchTerm}
        isLoading={isLoading}
        emptyMessage={emptyMessage}
        title="Scheduled Tasks"
        searchPlaceholder="Search schedules..."
        renderRowActions={renderRowActions}
        rowKey="id"
      />

      {/* Action Menu */}
      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleMenuClose}
      >
        <MenuItem onClick={() => {
          const schedule = schedules.find(s => s.id === selectedScheduleId);
          if (schedule) handleEdit(schedule);
        }}>
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
        <MenuItem onClick={() => {
          const schedule = schedules.find(s => s.id === selectedScheduleId);
          if (schedule) handleDeleteClick(schedule);
        }}>
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
});

export default ScheduledTasksList;
