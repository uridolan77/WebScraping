import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { 
  Box, 
  IconButton, 
  Tooltip, 
  Menu, 
  MenuItem,
  Chip
} from '@mui/material';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import VisibilityIcon from '@mui/icons-material/Visibility';

import PageHeader from '../components/Common/PageHeader';
import DataTable from '../components/Common/DataTable/DataTable';
import ErrorMessage from '../components/Common/ErrorMessage';
import useApiClient from '../hooks/useApiClient';
import { formatDate } from '../utils/formatters';

const ScheduleListPage = () => {
  const navigate = useNavigate();
  const { api, loading, error, execute } = useApiClient();
  const [schedules, setSchedules] = useState([]);
  const [menuAnchorEl, setMenuAnchorEl] = useState(null);
  const [selectedSchedule, setSelectedSchedule] = useState(null);

  // Fetch schedules on component mount
  useEffect(() => {
    fetchSchedules();
  }, []);
  
  const fetchSchedules = async () => {
    try {
      const data = await execute(() => api.scheduling.getAll());
      setSchedules(data || []);
    } catch (error) {
      console.error('Error fetching schedules:', error);
    }
  };

  const handleMenuOpen = (event, schedule) => {
    setMenuAnchorEl(event.currentTarget);
    setSelectedSchedule(schedule);
  };

  const handleMenuClose = () => {
    setMenuAnchorEl(null);
    setSelectedSchedule(null);
  };

  const handleEdit = () => {
    handleMenuClose();
    navigate(`/schedules/${selectedSchedule.id}/edit`);
  };

  const handleViewScraper = () => {
    handleMenuClose();
    navigate(`/scrapers/${selectedSchedule.scraperId}`);
  };

  const handleDelete = async () => {
    if (!selectedSchedule) return;
    
    if (window.confirm(`Are you sure you want to delete this schedule?`)) {
      try {
        await execute(() => api.scheduling.delete(selectedSchedule.id));
        setSchedules(schedules.filter(s => s.id !== selectedSchedule.id));
        handleMenuClose();
      } catch (error) {
        console.error('Error deleting schedule:', error);
      }
    } else {
      handleMenuClose();
    }
  };

  // Format cron expression into something more readable
  const formatSchedule = (cronExpression) => {
    // This is a simplified conversion - in a real app you'd want a more robust solution
    if (!cronExpression) return 'Invalid schedule';
    
    const parts = cronExpression.split(' ');
    if (parts.length !== 5) return cronExpression;
    
    const [minute, hour, dayOfMonth, month, dayOfWeek] = parts;

    // Handle common patterns
    if (minute === '*' && hour === '*' && dayOfMonth === '*' && month === '*' && dayOfWeek === '*') {
      return 'Every minute';
    }
    
    if (minute === '0' && hour === '*' && dayOfMonth === '*' && month === '*' && dayOfWeek === '*') {
      return 'Hourly';
    }
    
    if (minute === '0' && hour === '0' && dayOfMonth === '*' && month === '*' && dayOfWeek === '*') {
      return 'Daily at midnight';
    }
    
    if (minute === '0' && hour === '12' && dayOfMonth === '*' && month === '*' && dayOfWeek === '*') {
      return 'Daily at noon';
    }
    
    if (minute === '0' && hour === '0' && dayOfMonth === '*' && month === '*' && dayOfWeek === '0') {
      return 'Every Sunday at midnight';
    }
    
    if (minute === '0' && hour === '0' && dayOfMonth === '1' && month === '*' && dayOfWeek === '*') {
      return 'First day of each month';
    }
    
    // Default just return the cron expression
    return cronExpression;
  };

  const columns = [
    { 
      id: 'scraperName', 
      label: 'Scraper',
    },
    { 
      id: 'schedule', 
      label: 'Schedule',
      render: (row) => formatSchedule(row.cronExpression),
    },
    { 
      id: 'nextRun', 
      label: 'Next Run',
      render: (row) => formatDate(row.nextRun, { 
        hour: '2-digit', 
        minute: '2-digit' 
      }),
    },
    { 
      id: 'lastRun', 
      label: 'Last Run',
      render: (row) => formatDate(row.lastRun, { 
        hour: '2-digit', 
        minute: '2-digit' 
      }),
    },
    { 
      id: 'status', 
      label: 'Status',
      render: (row) => (
        <Chip 
          label={row.active ? 'Active' : 'Inactive'} 
          color={row.active ? 'success' : 'default'} 
          size="small" 
          variant="outlined"
        />
      ),
    },
    { 
      id: 'actions', 
      label: 'Actions',
      sortable: false,
      align: 'right',
      render: (row) => (
        <Box>
          <Tooltip title="Options">
            <IconButton 
              size="small"
              onClick={(e) => {
                e.stopPropagation();
                handleMenuOpen(e, row);
              }}
            >
              <MoreVertIcon />
            </IconButton>
          </Tooltip>
        </Box>
      ),
    },
  ];

  // Context menu for more options
  const renderMenu = (
    <Menu
      anchorEl={menuAnchorEl}
      open={Boolean(menuAnchorEl)}
      onClose={handleMenuClose}
    >
      <MenuItem onClick={handleEdit}>
        <EditIcon fontSize="small" sx={{ mr: 1 }} />
        Edit Schedule
      </MenuItem>
      <MenuItem onClick={handleViewScraper}>
        <VisibilityIcon fontSize="small" sx={{ mr: 1 }} />
        View Scraper
      </MenuItem>
      <MenuItem onClick={handleDelete} sx={{ color: 'error.main' }}>
        <DeleteIcon fontSize="small" sx={{ mr: 1 }} />
        Delete Schedule
      </MenuItem>
    </Menu>
  );

  return (
    <>
      <PageHeader 
        title="Schedules" 
        subtitle="Manage your automated scraping schedules"
      />
      
      {error && (
        <ErrorMessage 
          title="Failed to load schedules" 
          message={error}
          onRetry={fetchSchedules}
        />
      )}
      
      <DataTable
        columns={columns}
        data={schedules}
        loading={loading}
        emptyMessage="No schedules found. Create a schedule to automate your scrapers."
        onRowClick={(row) => navigate(`/schedules/${row.id}/edit`)}
      />
      
      {renderMenu}
    </>
  );
};

export default ScheduleListPage;