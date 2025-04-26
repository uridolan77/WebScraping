import React, { useState } from 'react';
import {
  Paper,
  Typography,
  Box,
  Button,
  List,
  ListItem,
  ListItemText,
  CircularProgress,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Divider,
  IconButton,
  Tooltip
} from '@mui/material';
import RefreshIcon from '@mui/icons-material/Refresh';
import FilterListIcon from '@mui/icons-material/FilterList';
import DownloadIcon from '@mui/icons-material/Download';

/**
 * LogViewer component for displaying scraper logs with filtering and download options
 * 
 * @param {Object} props - Component properties
 * @param {Array} props.logs - Array of log entries to display
 * @param {boolean} props.loading - Whether logs are currently loading
 * @param {Function} props.onRefresh - Callback to refresh logs
 * @param {string} props.title - Title for the log viewer
 * @param {number} props.maxHeight - Maximum height for the log viewer container
 */
const LogViewer = ({
  logs = [],
  loading = false,
  onRefresh,
  title = "Logs",
  maxHeight = 500
}) => {
  const [logLevel, setLogLevel] = useState('all');
  const [expanded, setExpanded] = useState(false);

  // Filter logs by level if filter is applied
  const filteredLogs = logLevel === 'all' 
    ? logs 
    : logs.filter(log => log.level === logLevel);
  
  // Format timestamp to readable format
  const formatTimestamp = (timestamp) => {
    if (!timestamp) return 'N/A';
    const date = new Date(timestamp);
    return date.toLocaleString();
  };
  
  // Get appropriate color for log level
  const getLevelColor = (level) => {
    switch (level?.toLowerCase()) {
      case 'error': return 'error.main';
      case 'warning': return 'warning.main';
      case 'info': return 'info.main';
      case 'debug': return 'text.secondary';
      default: return 'text.primary';
    }
  };
  
  // Download logs as a JSON file
  const handleDownload = () => {
    const logData = JSON.stringify(logs, null, 2);
    const blob = new Blob([logData], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `scraper-logs-${new Date().toISOString().split('T')[0]}.json`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  };
  
  return (
    <Paper sx={{ p: 2, height: '100%', display: 'flex', flexDirection: 'column' }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h6">
          {title} {filteredLogs.length > 0 && `(${filteredLogs.length})`}
        </Typography>
        
        <Box sx={{ display: 'flex', gap: 1 }}>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel id="log-level-filter-label">Level</InputLabel>
            <Select
              labelId="log-level-filter-label"
              value={logLevel}
              label="Level"
              onChange={(e) => setLogLevel(e.target.value)}
            >
              <MenuItem value="all">All</MenuItem>
              <MenuItem value="error">Error</MenuItem>
              <MenuItem value="warning">Warning</MenuItem>
              <MenuItem value="info">Info</MenuItem>
              <MenuItem value="debug">Debug</MenuItem>
            </Select>
          </FormControl>
          
          <Tooltip title="Refresh logs">
            <IconButton onClick={onRefresh} disabled={loading}>
              <RefreshIcon />
            </IconButton>
          </Tooltip>
          
          <Tooltip title="Download logs">
            <IconButton onClick={handleDownload} disabled={loading || logs.length === 0}>
              <DownloadIcon />
            </IconButton>
          </Tooltip>
        </Box>
      </Box>
      
      <Divider sx={{ mb: 2 }} />
      
      <Box 
        sx={{
          flexGrow: 1,
          overflow: 'auto',
          maxHeight: expanded ? 'none' : maxHeight,
          transition: 'max-height 0.3s ease-in-out'
        }}
      >
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : filteredLogs.length === 0 ? (
          <Box sx={{ py: 4, textAlign: 'center' }}>
            <Typography color="text.secondary">
              No logs available
            </Typography>
          </Box>
        ) : (
          <List dense disablePadding>
            {filteredLogs.map((log, index) => (
              <ListItem 
                key={index} 
                divider={index < filteredLogs.length - 1}
                sx={{
                  py: 1,
                  backgroundColor: log.level?.toLowerCase() === 'error' ? 'error.lightest' : 'inherit'
                }}
              >
                <ListItemText
                  primary={
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography 
                        component="span" 
                        variant="body2"
                        sx={{ fontWeight: 'medium' }}
                      >
                        {log.message}
                      </Typography>
                      <Typography
                        component="span"
                        variant="body2"
                        color={getLevelColor(log.level)}
                        sx={{ ml: 2, fontWeight: 'medium' }}
                      >
                        {log.level}
                      </Typography>
                    </Box>
                  }
                  secondary={
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 0.5 }}>
                      <Typography component="span" variant="caption" color="text.secondary">
                        {log.source || 'System'}
                      </Typography>
                      <Typography component="span" variant="caption" color="text.secondary">
                        {formatTimestamp(log.timestamp)}
                      </Typography>
                    </Box>
                  }
                  sx={{ my: 0 }}
                />
              </ListItem>
            ))}
          </List>
        )}
      </Box>
      
      {logs.length > 10 && (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 2 }}>
          <Button
            size="small"
            onClick={() => setExpanded(!expanded)}
            startIcon={<FilterListIcon />}
          >
            {expanded ? 'Show Less' : 'Show All'}
          </Button>
        </Box>
      )}
    </Paper>
  );
};

export default LogViewer;