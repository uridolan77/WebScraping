import React, { useState, useEffect, useRef } from 'react';
import { 
  Box, Typography, Paper, Table, TableBody, TableCell, 
  TableContainer, TableHead, TableRow, Chip, Button,
  FormControl, InputLabel, Select, MenuItem, TextField,
  InputAdornment, IconButton, Tooltip, CircularProgress,
  Alert, Divider
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  FilterList as FilterIcon,
  Search as SearchIcon,
  Clear as ClearIcon,
  GetApp as DownloadIcon,
  Error as ErrorIcon,
  Warning as WarningIcon,
  Info as InfoIcon,
  CheckCircle as SuccessIcon
} from '@mui/icons-material';
import { useScrapers } from '../../contexts/ScraperContext';
import { formatDistanceToNow, format } from 'date-fns';

const LogLevelChip = ({ level }) => {
  switch (level.toLowerCase()) {
    case 'error':
      return <Chip label="Error" color="error" size="small" icon={<ErrorIcon />} />;
    case 'warning':
      return <Chip label="Warning" color="warning" size="small" icon={<WarningIcon />} />;
    case 'info':
      return <Chip label="Info" color="info" size="small" icon={<InfoIcon />} />;
    case 'success':
      return <Chip label="Success" color="success" size="small" icon={<SuccessIcon />} />;
    default:
      return <Chip label={level} size="small" />;
  }
};

const ScraperLogs = ({ logs = [], isRunning = false }) => {
  const { fetchScraperLogs } = useScrapers();
  const [filteredLogs, setFilteredLogs] = useState([]);
  const [levelFilter, setLevelFilter] = useState('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [autoScroll, setAutoScroll] = useState(true);
  const tableEndRef = useRef(null);

  // Filter logs based on level and search term
  useEffect(() => {
    let filtered = [...logs];
    
    // Apply level filter
    if (levelFilter !== 'all') {
      filtered = filtered.filter(log => 
        log.level && log.level.toLowerCase() === levelFilter.toLowerCase()
      );
    }
    
    // Apply search filter
    if (searchTerm) {
      const term = searchTerm.toLowerCase();
      filtered = filtered.filter(log => 
        (log.message && log.message.toLowerCase().includes(term)) ||
        (log.source && log.source.toLowerCase().includes(term))
      );
    }
    
    setFilteredLogs(filtered);
  }, [logs, levelFilter, searchTerm]);

  // Auto-scroll to bottom when new logs arrive if autoScroll is enabled
  useEffect(() => {
    if (autoScroll && tableEndRef.current && isRunning) {
      tableEndRef.current.scrollIntoView({ behavior: 'smooth' });
    }
  }, [filteredLogs, autoScroll, isRunning]);

  // Handle refresh
  const handleRefresh = async (scraperId) => {
    if (!scraperId) return;
    
    setIsRefreshing(true);
    try {
      await fetchScraperLogs(scraperId, 100);
    } finally {
      setIsRefreshing(false);
    }
  };

  // Handle clear filters
  const handleClearFilters = () => {
    setLevelFilter('all');
    setSearchTerm('');
  };

  // Handle download logs
  const handleDownloadLogs = () => {
    if (!logs || logs.length === 0) return;
    
    // Create CSV content
    const headers = ['Timestamp', 'Level', 'Source', 'Message'];
    const csvContent = [
      headers.join(','),
      ...logs.map(log => [
        log.timestamp ? format(new Date(log.timestamp), 'yyyy-MM-dd HH:mm:ss') : '',
        log.level || '',
        log.source || '',
        `"${(log.message || '').replace(/"/g, '""')}"`
      ].join(','))
    ].join('\n');
    
    // Create and download file
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', `scraper-logs-${new Date().toISOString().slice(0, 10)}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h6">Scraper Logs</Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Button
            variant="outlined"
            startIcon={isRefreshing ? <CircularProgress size={20} /> : <RefreshIcon />}
            onClick={() => handleRefresh(logs[0]?.scraperId)}
            disabled={isRefreshing}
          >
            Refresh
          </Button>
          <Button
            variant="outlined"
            startIcon={<DownloadIcon />}
            onClick={handleDownloadLogs}
            disabled={!logs || logs.length === 0}
          >
            Download
          </Button>
        </Box>
      </Box>
      
      {/* Filters */}
      <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
        <FormControl variant="outlined" size="small" sx={{ minWidth: 150 }}>
          <InputLabel id="log-level-filter-label">Log Level</InputLabel>
          <Select
            labelId="log-level-filter-label"
            value={levelFilter}
            onChange={(e) => setLevelFilter(e.target.value)}
            label="Log Level"
          >
            <MenuItem value="all">All Levels</MenuItem>
            <MenuItem value="error">Error</MenuItem>
            <MenuItem value="warning">Warning</MenuItem>
            <MenuItem value="info">Info</MenuItem>
            <MenuItem value="success">Success</MenuItem>
          </Select>
        </FormControl>
        
        <TextField
          size="small"
          placeholder="Search logs..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
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
        
        {(levelFilter !== 'all' || searchTerm) && (
          <Button
            variant="text"
            onClick={handleClearFilters}
            startIcon={<ClearIcon />}
          >
            Clear Filters
          </Button>
        )}
      </Box>
      
      {/* Auto-scroll toggle */}
      {isRunning && (
        <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 1 }}>
          <Button
            variant="text"
            size="small"
            onClick={() => setAutoScroll(!autoScroll)}
          >
            {autoScroll ? 'Disable Auto-scroll' : 'Enable Auto-scroll'}
          </Button>
        </Box>
      )}
      
      {/* Logs table */}
      {filteredLogs.length > 0 ? (
        <TableContainer component={Paper} sx={{ maxHeight: 500, overflow: 'auto' }}>
          <Table stickyHeader size="small">
            <TableHead>
              <TableRow>
                <TableCell>Timestamp</TableCell>
                <TableCell>Level</TableCell>
                <TableCell>Source</TableCell>
                <TableCell>Message</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredLogs.map((log, index) => (
                <TableRow key={index} hover>
                  <TableCell>
                    <Tooltip title={log.timestamp ? format(new Date(log.timestamp), 'yyyy-MM-dd HH:mm:ss') : ''}>
                      <Typography variant="body2">
                        {log.timestamp ? formatDistanceToNow(new Date(log.timestamp), { addSuffix: true }) : ''}
                      </Typography>
                    </Tooltip>
                  </TableCell>
                  <TableCell>
                    <LogLevelChip level={log.level || 'info'} />
                  </TableCell>
                  <TableCell>{log.source || ''}</TableCell>
                  <TableCell sx={{ maxWidth: 500, wordBreak: 'break-word' }}>{log.message || ''}</TableCell>
                </TableRow>
              ))}
              <TableRow ref={tableEndRef}>
                <TableCell colSpan={4} sx={{ height: 1, p: 0, border: 'none' }} />
              </TableRow>
            </TableBody>
          </Table>
        </TableContainer>
      ) : (
        <Alert severity="info" sx={{ mt: 2 }}>
          {logs.length === 0 ? 
            'No logs available for this scraper yet.' : 
            'No logs match the current filters.'}
        </Alert>
      )}
      
      {isRunning && (
        <Alert severity="info" sx={{ mt: 2 }}>
          Scraper is currently running. Logs will update automatically.
        </Alert>
      )}
    </Box>
  );
};

export default ScraperLogs;
