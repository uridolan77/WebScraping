import React, { useState } from 'react';
import { 
  Box, Typography, Paper, List, ListItem, ListItemText, 
  ListItemIcon, ListItemSecondaryAction, IconButton, 
  Button, Divider, Chip, FormControl, InputLabel, 
  Select, MenuItem, TextField, InputAdornment, Tooltip,
  Badge, Alert, SelectChangeEvent
} from '@mui/material';
import {
  Notifications as NotificationsIcon,
  NotificationsActive as NotificationsActiveIcon,
  NotificationsOff as NotificationsOffIcon,
  Error as ErrorIcon,
  Warning as WarningIcon,
  Info as InfoIcon,
  CheckCircle as CheckCircleIcon,
  Delete as DeleteIcon,
  MarkEmailRead as MarkReadIcon,
  Search as SearchIcon,
  Clear as ClearIcon,
  FilterList as FilterIcon
} from '@mui/icons-material';
import { formatDistanceToNow } from 'date-fns';
import { Notification } from '../../types';

interface NotificationsListProps {
  notifications: Notification[];
  onMarkAsRead: (id: string) => void;
  onMarkAllAsRead: () => void;
}

const NotificationsList: React.FC<NotificationsListProps> = ({ notifications, onMarkAsRead, onMarkAllAsRead }) => {
  const [typeFilter, setTypeFilter] = useState<string>('all');
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [readFilter, setReadFilter] = useState<string>('all');
  
  // Handle type filter change
  const handleTypeFilterChange = (event: SelectChangeEvent) => {
    setTypeFilter(event.target.value);
  };
  
  // Handle read filter change
  const handleReadFilterChange = (event: SelectChangeEvent) => {
    setReadFilter(event.target.value);
  };
  
  // Handle search change
  const handleSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(event.target.value);
  };
  
  // Clear filters
  const handleClearFilters = () => {
    setTypeFilter('all');
    setReadFilter('all');
    setSearchTerm('');
  };
  
  // Get notification icon based on type
  const getNotificationIcon = (type: string) => {
    switch (type) {
      case 'error':
        return <ErrorIcon color="error" />;
      case 'warning':
        return <WarningIcon color="warning" />;
      case 'success':
        return <CheckCircleIcon color="success" />;
      case 'info':
      default:
        return <InfoIcon color="info" />;
    }
  };
  
  // Get notification chip based on type
  const getNotificationChip = (type: string) => {
    switch (type) {
      case 'error':
        return <Chip label="Error" size="small" color="error" />;
      case 'warning':
        return <Chip label="Warning" size="small" color="warning" />;
      case 'success':
        return <Chip label="Success" size="small" color="success" />;
      case 'info':
      default:
        return <Chip label="Info" size="small" color="info" />;
    }
  };
  
  // Filter notifications
  const filteredNotifications = notifications.filter(notification => {
    // Filter by type
    if (typeFilter !== 'all' && notification.type !== typeFilter) {
      return false;
    }
    
    // Filter by read status
    if (readFilter === 'read' && !notification.read) {
      return false;
    }
    if (readFilter === 'unread' && notification.read) {
      return false;
    }
    
    // Filter by search term
    if (searchTerm && !notification.message.toLowerCase().includes(searchTerm.toLowerCase())) {
      return false;
    }
    
    return true;
  });
  
  // Count unread notifications
  const unreadCount = notifications.filter(notification => !notification.read).length;
  
  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <Badge badgeContent={unreadCount} color="error" sx={{ mr: 2 }}>
            <NotificationsIcon color="action" />
          </Badge>
          <Typography variant="h6">
            Notifications
          </Typography>
        </Box>
        <Button
          variant="outlined"
          startIcon={<MarkReadIcon />}
          onClick={onMarkAllAsRead}
          disabled={unreadCount === 0}
        >
          Mark All as Read
        </Button>
      </Box>
      
      {/* Filters */}
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, mb: 3 }}>
        <FormControl size="small" sx={{ minWidth: 120 }}>
          <InputLabel id="notification-type-label">Type</InputLabel>
          <Select
            labelId="notification-type-label"
            value={typeFilter}
            label="Type"
            onChange={handleTypeFilterChange}
          >
            <MenuItem value="all">All Types</MenuItem>
            <MenuItem value="info">Info</MenuItem>
            <MenuItem value="success">Success</MenuItem>
            <MenuItem value="warning">Warning</MenuItem>
            <MenuItem value="error">Error</MenuItem>
          </Select>
        </FormControl>
        
        <FormControl size="small" sx={{ minWidth: 120 }}>
          <InputLabel id="notification-read-label">Status</InputLabel>
          <Select
            labelId="notification-read-label"
            value={readFilter}
            label="Status"
            onChange={handleReadFilterChange}
          >
            <MenuItem value="all">All Status</MenuItem>
            <MenuItem value="read">Read</MenuItem>
            <MenuItem value="unread">Unread</MenuItem>
          </Select>
        </FormControl>
        
        <TextField
          size="small"
          placeholder="Search notifications..."
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
        
        {(typeFilter !== 'all' || readFilter !== 'all' || searchTerm) && (
          <Button
            variant="outlined"
            size="small"
            startIcon={<FilterIcon />}
            onClick={handleClearFilters}
          >
            Clear Filters
          </Button>
        )}
      </Box>
      
      {/* Notifications List */}
      {filteredNotifications.length > 0 ? (
        <Paper variant="outlined">
          <List>
            {filteredNotifications.map((notification, index) => (
              <React.Fragment key={notification.id}>
                <ListItem 
                  alignItems="flex-start"
                  sx={{ 
                    bgcolor: notification.read ? 'inherit' : 'action.hover',
                    transition: 'background-color 0.3s'
                  }}
                >
                  <ListItemIcon>
                    {getNotificationIcon(notification.type)}
                  </ListItemIcon>
                  <ListItemText
                    primary={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography 
                          variant="subtitle2"
                          sx={{ fontWeight: notification.read ? 'normal' : 'bold' }}
                        >
                          {notification.message}
                        </Typography>
                        {getNotificationChip(notification.type)}
                        {!notification.read && (
                          <Chip label="New" size="small" color="primary" />
                        )}
                      </Box>
                    }
                    secondary={
                      <Typography
                        variant="body2"
                        color="text.secondary"
                      >
                        {notification.timestamp ? formatDistanceToNow(new Date(notification.timestamp), { addSuffix: true }) : ''}
                      </Typography>
                    }
                  />
                  <ListItemSecondaryAction>
                    {!notification.read && (
                      <Tooltip title="Mark as Read">
                        <IconButton 
                          edge="end" 
                          onClick={() => onMarkAsRead(notification.id)}
                          size="small"
                        >
                          <MarkReadIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    )}
                  </ListItemSecondaryAction>
                </ListItem>
                {index < filteredNotifications.length - 1 && <Divider component="li" />}
              </React.Fragment>
            ))}
          </List>
        </Paper>
      ) : (
        <Alert severity="info">
          No notifications found. {typeFilter !== 'all' || readFilter !== 'all' || searchTerm ? 'Try changing your filters.' : ''}
        </Alert>
      )}
    </Box>
  );
};

export default NotificationsList;
