import React, { useState, useRef, useEffect } from 'react';
import {
  Paper,
  Typography,
  Box,
  IconButton,
  TextField,
  InputAdornment,
  Divider,
  FormControl,
  Select,
  MenuItem,
  FormHelperText,
  Stack,
} from '@mui/material';
import {
  Search as SearchIcon,
  FilterList as FilterIcon,
  Clear as ClearIcon,
  GetApp as DownloadIcon,
} from '@mui/icons-material';

/**
 * Component to display and filter logs from the scraper
 */
const LogViewer = ({ logs = [] }) => {
  const [filter, setFilter] = useState('');
  const [logLevel, setLogLevel] = useState('all');
  const [filteredLogs, setFilteredLogs] = useState(logs);
  const logContainerRef = useRef(null);
  const [autoScroll, setAutoScroll] = useState(true);

  // Filter logs when filter or logLevel changes
  useEffect(() => {
    let result = [...logs];
    
    if (logLevel !== 'all') {
      result = result.filter(log => log.level.toLowerCase() === logLevel);
    }
    
    if (filter) {
      const searchTerm = filter.toLowerCase();
      result = result.filter(log => 
        log.message.toLowerCase().includes(searchTerm) || 
        log.timestamp.toLowerCase().includes(searchTerm)
      );
    }
    
    setFilteredLogs(result);
  }, [logs, filter, logLevel]);

  // Auto scroll to the bottom when new logs come in
  useEffect(() => {
    if (autoScroll && logContainerRef.current) {
      logContainerRef.current.scrollTop = logContainerRef.current.scrollHeight;
    }
  }, [filteredLogs, autoScroll]);

  const handleClearFilter = () => {
    setFilter('');
    setLogLevel('all');
  };

  const downloadLogs = () => {
    const logText = filteredLogs
      .map(log => `[${log.timestamp}] [${log.level}] ${log.message}`)
      .join('\n');
    
    const blob = new Blob([logText], { type: 'text/plain' });
    const href = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = href;
    link.download = `scraper-logs-${new Date().toISOString().slice(0, 10)}.txt`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(href);
  };

  // Function to get color based on log level
  const getLogColor = (level) => {
    switch(level.toLowerCase()) {
      case 'error':
        return 'error.main';
      case 'warning':
        return 'warning.main';
      case 'info':
        return 'info.main';
      case 'debug':
        return 'text.secondary';
      default:
        return 'text.primary';
    }
  };

  return (
    <Paper elevation={2}>
      <Box sx={{ p: 2 }}>
        <Stack direction="row" justifyContent="space-between" alignItems="center" mb={2}>
          <Typography variant="h6">Log Viewer</Typography>
          
          <Box>
            <IconButton onClick={downloadLogs} title="Download logs">
              <DownloadIcon />
            </IconButton>
          </Box>
        </Stack>
        
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} mb={2}>
          <TextField
            placeholder="Filter logs..."
            value={filter}
            onChange={(e) => setFilter(e.target.value)}
            size="small"
            fullWidth
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon fontSize="small" />
                </InputAdornment>
              ),
              endAdornment: filter && (
                <InputAdornment position="end">
                  <IconButton size="small" onClick={handleClearFilter}>
                    <ClearIcon fontSize="small" />
                  </IconButton>
                </InputAdornment>
              )
            }}
          />
          
          <FormControl sx={{ minWidth: 120 }} size="small">
            <Select
              value={logLevel}
              onChange={(e) => setLogLevel(e.target.value)}
              displayEmpty
              startAdornment={
                <InputAdornment position="start">
                  <FilterIcon fontSize="small" />
                </InputAdornment>
              }
            >
              <MenuItem value="all">All Levels</MenuItem>
              <MenuItem value="info">Info</MenuItem>
              <MenuItem value="warning">Warning</MenuItem>
              <MenuItem value="error">Error</MenuItem>
              <MenuItem value="debug">Debug</MenuItem>
            </Select>
            <FormHelperText>Log Level</FormHelperText>
          </FormControl>
        </Stack>
        
        <Divider />

        <Box 
          ref={logContainerRef}
          sx={{ 
            height: '300px', 
            overflowY: 'auto', 
            p: 1,
            fontFamily: 'monospace',
            fontSize: '0.85rem',
            backgroundColor: 'grey.100',
            borderRadius: 1,
            mt: 2
          }}
        >
          {filteredLogs.length === 0 ? (
            <Typography 
              variant="body2" 
              color="text.secondary" 
              sx={{ fontStyle: 'italic', textAlign: 'center', mt: 2 }}
            >
              No logs to display
            </Typography>
          ) : (
            filteredLogs.map((log, index) => (
              <Box key={index} sx={{ mb: 0.5, display: 'flex' }}>
                <Typography
                  component="span"
                  sx={{ color: 'text.secondary', mr: 1 }}
                >
                  [{log.timestamp}]
                </Typography>
                <Typography 
                  component="span" 
                  sx={{ 
                    color: getLogColor(log.level), 
                    fontWeight: log.level.toLowerCase() === 'error' ? 'bold' : 'normal',
                    mr: 1
                  }}
                >
                  [{log.level}]
                </Typography>
                <Typography component="span">
                  {log.message}
                </Typography>
              </Box>
            ))
          )}
        </Box>
      </Box>
    </Paper>
  );
};

export default LogViewer;