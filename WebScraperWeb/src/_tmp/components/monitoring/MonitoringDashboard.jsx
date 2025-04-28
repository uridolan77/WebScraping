// src/pages/MonitoringDashboard.jsx
import React, { useState, useEffect, useRef } from 'react';
import { 
  Container, Box, Paper, Typography, Grid, Card, CardContent, Divider,
  CircularProgress, Button, List, ListItem, ListItemText, Tabs, Tab,
  Chip, LinearProgress, IconButton, TextField, InputAdornment, TableContainer,
  Table, TableHead, TableBody, TableRow, TableCell, Dialog, DialogTitle,
  DialogContent, DialogActions
} from '@mui/material';
import {
  PlayArrow as PlayIcon,
  Stop as StopIcon,
  Speed as SpeedIcon,
  Refresh as RefreshIcon,
  Search as SearchIcon,
  Timer as TimerIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  InsertLink as LinkIcon,
  Link as LinkOffIcon,
  Warning as WarningIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { getScraperStatus, startScraper, stopScraper, getScraperLogs } from '../api/scrapers';

// Tab panel component
function TabPanel(props) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`monitoring-tabpanel-${index}`}
      aria-labelledby={`monitoring-tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ p: 2 }}>
          {children}
        </Box>
      )}
    </div>
  );
}

const MonitoringDashboard = () => {
  const [loading, setLoading] = useState(true);
  const [scrapers, setScrapers] = useState([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [activeTab, setActiveTab] = useState(0);
  const [selectedScraper, setSelectedScraper] = useState(null);
  const [scraperLogs, setScraperLogs] = useState([]);
  const [isLogsLoading, setIsLogsLoading] = useState(false);
  const [showLogDialog, setShowLogDialog] = useState(false);
  const [filterErrorsOnly, setFilterErrorsOnly] = useState(false);
  const [expandedScrapers, setExpandedScrapers] = useState({});
  const refreshTimerRef = useRef(null);
  const navigate = useNavigate();
  
  useEffect(() => {
    fetchScraperStatus();
    
    // Set up recurring refresh
    refreshTimerRef.current = setInterval(fetchScraperStatus, 10000); // Refresh every 10 seconds
    
    // Clean up on unmount
    return () => {
      if (refreshTimerRef.current) {
        clearInterval(refreshTimerRef.current);
      }
    };
  }, []);
  
  useEffect(() => {
    if (selectedScraper) {
      fetchScraperLogs(selectedScraper.id);
    }
  }, [selectedScraper]);
  
  const fetchScraperStatus = async () => {
    setLoading(true);
    try {
      // In a real app, this would call your API to get all scraper statuses
      const response = await fetch('/api/scrapers/status');
      const data = await response.json();
      
      // For demo purposes, let's create some simulated data
      const simulatedScrapers = generateSimulatedScrapers();
      setScrapers(simulatedScrapers);
    } catch (error) {
      console.error('Error fetching scraper status:', error);
    } finally {
      setLoading(false);
    }
  };
  
  const fetchScraperLogs = async (scraperId) => {
    setIsLogsLoading(true);
    try {
      // In a real app, call your API endpoint
      // const logs = await getScraperLogs(scraperId);
      
      // For demo purposes, use simulated logs
      const logs = generateSimulatedLogs(scraperId);
      setScraperLogs(logs);
    } catch (error) {
      console.error('Error fetching scraper logs:', error);
    } finally {
      setIsLogsLoading(false);
    }
  };
  
  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
  };
  
  const handleSearchChange = (event) => {
    setSearchTerm(event.target.value);
  };
  
  const handleStartScraper = async (scraperId) => {
    try {
      // In a real app, call your API endpoint
      // await startScraper(scraperId);
      
      // For the demo, let's update the local state
      setScrapers(prevScrapers => 
        prevScrapers.map(scraper => 
          scraper.id === scraperId 
            ? { ...scraper, isRunning: true, startTime: new Date().toISOString() }
            : scraper
        )
      );
    } catch (error) {
      console.error('Error starting scraper:', error);
    }
  };
  
  const handleStopScraper = async (scraperId) => {
    try {
      // In a real app, call your API endpoint
      // await stopScraper(scraperId);
      
      // For the demo, let's update the local state
      setScrapers(prevScrapers => 
        prevScrapers.map(scraper => 
          scraper.id === scraperId 
            ? { 
                ...scraper, 
                isRunning: false, 
                endTime: new Date().toISOString(),
                urlsProcessed: scraper.urlsProcessed + Math.floor(Math.random() * 50)
              }
            : scraper
        )
      );
    } catch (error) {
      console.error('Error stopping scraper:', error);
    }
  };
  
  const handleViewLogs = (scraper) => {
    setSelectedScraper(scraper);
    setShowLogDialog(true);
  };
  
  const handleCloseLogDialog = () => {
    setShowLogDialog(false);
  };
  
  const handleToggleExpand = (scraperId) => {
    setExpandedScrapers(prev => ({
      ...prev,
      [scraperId]: !prev[scraperId]
    }));
  };
  
  const handleFilterErrorsToggle = () => {
    setFilterErrorsOnly(!filterErrorsOnly);
  };
  
  const generateSimulatedScrapers = () => {
    return [
      {
        id: 'scraper-1',
        name: 'UKGC Monitor',
        startUrl: 'https://www.gamblingcommission.gov.uk/licensees-and-businesses',
        isRunning: true,
        startTime: new Date(Date.now() - 45 * 60000).toISOString(), // 45 minutes ago
        endTime: null,
        urlsProcessed: 243,
        urlsQueued: 67,
        documentsProcessed: 24,
        hasErrors: false,
        message: 'Scraping in progress',
        metrics: {
          processingItems: 3,
          queuedItems: 67,
          completedItems: 243,
          failedItems: 2,
          averageProcessingTimeMs: 1420
        }
      },
      {
        id: 'scraper-2',
        name: 'MGA Content Monitor',
        startUrl: 'https://www.mga.org.mt/licensee-hub/',
        isRunning: false,
        startTime: new Date(Date.now() - 3 * 3600000).toISOString(), // 3 hours ago
        endTime: new Date(Date.now() - 2 * 3600000).toISOString(), // 2 hours ago
        urlsProcessed: 518,
        urlsQueued: 0,
        documentsProcessed: 47,
        hasErrors: true,
        message: 'Stopped with errors: Connection timeout',
        metrics: {
          processingItems: 0,
          queuedItems: 0,
          completedItems: 518,
          failedItems: 12,
          averageProcessingTimeMs: 1850
        }
      },
      {
        id: 'scraper-3',
        name: 'Regulatory News Monitor',
        startUrl: 'https://example.com/regulatory-news',
        isRunning: false,
        startTime: new Date(Date.now() - 24 * 3600000).toISOString(), // 24 hours ago
        endTime: new Date(Date.now() - 23.5 * 3600000).toISOString(), // 23.5 hours ago
        urlsProcessed: 126,
        urlsQueued: 0,
        documentsProcessed: 18,
        hasErrors: false,
        message: 'Scraping completed successfully',
        metrics: {
          processingItems: 0,
          queuedItems: 0,
          completedItems: 126,
          failedItems: 0,
          averageProcessingTimeMs: 980
        }
      }
    ];
  };
  
  const generateSimulatedLogs = (scraperId) => {
    const baseTime = new Date();
    const logs = [];
    
    // Create logs going back in time
    for (let i = 0; i < 50; i++) {
      const logTime = new Date(baseTime.getTime() - i * 30000); // 30 seconds between logs
      const isError = Math.random() < 0.1; // 10% chance of error
      const isWarning = !isError && Math.random() < 0.15; // 15% chance of warning if not error
      
      logs.push({
        timestamp: logTime.toISOString(),
        message: isError 
          ? `Error processing URL: Connection timeout after 30 seconds` 
          : isWarning
            ? `Warning: Slow response time (5.2s) from server`
            : `Successfully processed URL: https://example.com/page-${Math.floor(Math.random() * 100)}`,
        level: isError ? 'Error' : isWarning ? 'Warning' : 'Info'
      });
    }
    
    return logs;
  };
  
  const formatDuration = (startTime, endTime) => {
    const start = new Date(startTime);
    const end = endTime ? new Date(endTime) : new Date();
    const durationMs = end - start;
    
    const seconds = Math.floor(durationMs / 1000) % 60;
    const minutes = Math.floor(durationMs / (1000 * 60)) % 60;
    const hours = Math.floor(durationMs / (1000 * 60 * 60));
    
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  };
  
  // Filter scrapers based on search term
  const filteredScrapers = scrapers.filter(scraper => 
    scraper.name.toLowerCase().includes(searchTerm.toLowerCase()) || 
    scraper.startUrl.toLowerCase().includes(searchTerm.toLowerCase())
  );
  
  // Filter logs based on errors only toggle
  const filteredLogs = filterErrorsOnly 
    ? scraperLogs.filter(log => log.level === 'Error')
    : scraperLogs;
  
  if (loading && scrapers.length === 0) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '80vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="h4" component="h1" gutterBottom>
            Monitoring Dashboard
          </Typography>
          <Box>
            <Button 
              variant="contained" 
              color="primary" 
              startIcon={<RefreshIcon />}
              onClick={fetchScraperStatus}
              sx={{ mr: 2 }}
            >
              Refresh
            </Button>
            <Button 
              variant="outlined"
              onClick={() => navigate('/scrapers/new')}
            >
              Create New Scraper
            </Button>
          </Box>
        </Box>
        
        <Divider sx={{ mb: 3 }} />
        
        {/* Status Summary Cards */}
        <Grid container spacing={3} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Total Scrapers
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  <Typography variant="h4" component="div">
                    {scrapers.length}
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>
          
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Active Scrapers
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  <Typography variant="h4" component="div" sx={{ color: 'success.main' }}>
                    {scrapers.filter(s => s.isRunning).length}
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>
          
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Scrapers with Errors
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  <Typography variant="h4" component="div" sx={{ color: 'error.main' }}>
                    {scrapers.filter(s => s.hasErrors).length}
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>
          
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Total URLs Processed
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  <Typography variant="h4" component="div">
                    {scrapers.reduce((sum, s) => sum + s.urlsProcessed, 0).toLocaleString()}
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
        
        {/* Search Box */}
        <Box sx={{ mb: 3 }}>
          <TextField
            fullWidth
            variant="outlined"
            placeholder="Search scrapers by name or URL..."
            value={searchTerm}
            onChange={handleSearchChange}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon />
                </InputAdornment>
              ),
            }}
          />
        </Box>
        
        {/* Scraper List */}
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={activeTab} onChange={handleTabChange} aria-label="scraper status tabs">
            <Tab label="All Scrapers" />
            <Tab label="Running" />
            <Tab label="With Errors" />
            <Tab label="Completed" />
          </Tabs>
        </Box>
        
        <TabPanel value={activeTab} index={0}>
          <ScraperList 
            scrapers={filteredScrapers}
            expandedScrapers={expandedScrapers}
            onToggleExpand={handleToggleExpand}
            onStart={handleStartScraper}
            onStop={handleStopScraper}
            onViewLogs={handleViewLogs}
            formatDuration={formatDuration}
          />
        </TabPanel>
        
        <TabPanel value={activeTab} index={1}>
          <ScraperList 
            scrapers={filteredScrapers.filter(s => s.isRunning)}
            expandedScrapers={expandedScrapers}
            onToggleExpand={handleToggleExpand}
            onStart={handleStartScraper}
            onStop={handleStopScraper}
            onViewLogs={handleViewLogs}
            formatDuration={formatDuration}
          />
        </TabPanel>
        
        <TabPanel value={activeTab} index={2}>
          <ScraperList 
            scrapers={filteredScrapers.filter(s => s.hasErrors)}
            expandedScrapers={expandedScrapers}
            onToggleExpand={handleToggleExpand}
            onStart={handleStartScraper}
            onStop={handleStopScraper}
            onViewLogs={handleViewLogs}
            formatDuration={formatDuration}
          />
        </TabPanel>
        
        <TabPanel value={activeTab} index={3}>
          <ScraperList 
            scrapers={filteredScrapers.filter(s => !s.isRunning && !s.hasErrors)}
            expandedScrapers={expandedScrapers}
            onToggleExpand={handleToggleExpand}
            onStart={handleStartScraper}
            onStop={handleStopScraper}
            onViewLogs={handleViewLogs}
            formatDuration={formatDuration}
          />
        </TabPanel>
      </Paper>
      
      {/* Logs Dialog */}
      <Dialog
        open={showLogDialog}
        onClose={handleCloseLogDialog}
        fullWidth
        maxWidth="md"
      >
        <DialogTitle>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h6">
              Logs: {selectedScraper?.name}
            </Typography>
            <Box>
              <Button
                color="primary"
                onClick={handleFilterErrorsToggle}
                startIcon={filterErrorsOnly ? <CheckCircleIcon /> : <ErrorIcon />}
                sx={{ mr: 1 }}
              >
                {filterErrorsOnly ? 'Show All' : 'Errors Only'}
              </Button>
              <Button
                color="primary"
                startIcon={<RefreshIcon />}
                onClick={() => selectedScraper && fetchScraperLogs(selectedScraper.id)}
              >
                Refresh
              </Button>
            </Box>
          </Box>
        </DialogTitle>
        <DialogContent dividers>
          {isLogsLoading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <CircularProgress />
            </Box>
          ) : (
            <List dense>
              {filteredLogs.map((log, index) => (
                <ListItem 
                  key={index}
                  sx={{ 
                    bgcolor: log.level === 'Error' 
                      ? 'error.light' 
                      : log.level === 'Warning' 
                        ? 'warning.light' 
                        : 'background.paper',
                    borderRadius: 1,
                    mb: 0.5
                  }}
                >
                  <ListItemText
                    primary={log.message}
                    secondary={new Date(log.timestamp).toLocaleString()}
                    primaryTypographyProps={{
                      color: log.level === 'Error' ? 'error.dark' : 'inherit'
                    }}
                  />
                </ListItem>
              ))}
            </List>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseLogDialog}>Close</Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

// Scraper List Component
const ScraperList = ({ 
  scrapers, 
  expandedScrapers, 
  onToggleExpand, 
  onStart, 
  onStop, 
  onViewLogs, 
  formatDuration 
}) => {
  const navigate = useNavigate();
  
  const getStatusChip = (scraper) => {
    if (scraper.isRunning) {
      return <Chip 
        icon={<PlayIcon />} 
        label="Running" 
        color="primary" 
        size="small" 
      />;
    } else if (scraper.hasErrors) {
      return <Chip 
        icon={<ErrorIcon />} 
        label="Error" 
        color="error" 
        size="small" 
      />;
    } else {
      return <Chip 
        icon={<CheckCircleIcon />} 
        label="Completed" 
        color="success" 
        size="small" 
      />;
    }
  };
  
  if (scrapers.length === 0) {
    return (
      <Typography variant="body2" color="textSecondary" sx={{ p: 3, textAlign: 'center' }}>
        No scrapers found matching the current filter.
      </Typography>
    );
  }
  
  return (
    <TableContainer>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell></TableCell>
            <TableCell>Name</TableCell>
            <TableCell>Status</TableCell>
            <TableCell>Progress</TableCell>
            <TableCell>Started</TableCell>
            <TableCell>Duration</TableCell>
            <TableCell>Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {scrapers.map((scraper) => (
            <React.Fragment key={scraper.id}>
              <TableRow
                sx={{ 
                  '&:hover': { bgcolor: 'action.hover' },
                  cursor: 'pointer'
                }}
              >
                <TableCell>
                  <IconButton 
                    size="small" 
                    onClick={() => onToggleExpand(scraper.id)}
                    aria-label="expand row"
                  >
                    {expandedScrapers[scraper.id] ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                  </IconButton>
                </TableCell>
                <TableCell 
                  component="th" 
                  scope="row"
                  onClick={() => navigate(`/scrapers/${scraper.id}`)}
                  sx={{ cursor: 'pointer' }}
                >
                  <Typography variant="subtitle2">{scraper.name}</Typography>
                  <Typography variant="caption" color="textSecondary">
                    <LinkIcon fontSize="inherit" sx={{ verticalAlign: 'text-bottom', mr: 0.5 }} />
                    {scraper.startUrl}
                  </Typography>
                </TableCell>
                <TableCell>{getStatusChip(scraper)}</TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', width: '100%' }}>
                    <Box sx={{ width: '100%', mr: 1 }}>
                      <LinearProgress 
                        variant={scraper.isRunning ? "indeterminate" : "determinate"} 
                        value={scraper.isRunning ? undefined : 100} 
                        color={scraper.hasErrors ? "error" : "primary"}
                      />
                    </Box>
                    <Box>
                      <Typography variant="body2" color="text.secondary">
                        {scraper.urlsProcessed} / {scraper.urlsProcessed + scraper.urlsQueued}
                      </Typography>
                    </Box>
                  </Box>
                </TableCell>
                <TableCell>
                  {new Date(scraper.startTime).toLocaleString()}
                </TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center' }}>
                    <TimerIcon fontSize="small" sx={{ mr: 0.5 }} />
                    {formatDuration(scraper.startTime, scraper.endTime)}
                  </Box>
                </TableCell>
                <TableCell>
                  <Box>
                    {scraper.isRunning ? (
                      <Button
                        variant="outlined"
                        color="error"
                        size="small"
                        startIcon={<StopIcon />}
                        onClick={() => onStop(scraper.id)}
                        sx={{ mr: 1 }}
                      >
                        Stop
                      </Button>
                    ) : (
                      <Button
                        variant="outlined"
                        color="primary"
                        size="small"
                        startIcon={<PlayIcon />}
                        onClick={() => onStart(scraper.id)}
                        sx={{ mr: 1 }}
                      >
                        Start
                      </Button>
                    )}
                    <Button
                      variant="outlined"
                      size="small"
                      onClick={() => onViewLogs(scraper)}
                    >
                      Logs
                    </Button>
                  </Box>
                </TableCell>
              </TableRow>
              {expandedScrapers[scraper.id] && (
                <TableRow>
                  <TableCell colSpan={7} sx={{ pb: 2, pt: 0 }}>
                    <Box sx={{ pl: 4 }}>
                      <Grid container spacing={2}>
                        <Grid item xs={12} md={6}>
                          <Card variant="outlined">
                            <CardContent>
                              <Typography variant="subtitle2" gutterBottom>
                                Performance Metrics
                              </Typography>
                              <Grid container spacing={1}>
                                <Grid item xs={6}>
                                  <Typography variant="body2" color="textSecondary">
                                    URLs Processed:
                                  </Typography>
                                </Grid>
                                <Grid item xs={6}>
                                  <Typography variant="body2">
                                    {scraper.urlsProcessed}
                                  </Typography>
                                </Grid>
                                <Grid item xs={6}>
                                  <Typography variant="body2" color="textSecondary">
                                    Documents Processed:
                                  </Typography>
                                </Grid>
                                <Grid item xs={6}>
                                  <Typography variant="body2">
                                    {scraper.documentsProcessed}
                                  </Typography>
                                </Grid>
                                <Grid item xs={6}>
                                  <Typography variant="body2" color="textSecondary">
                                    Avg. Processing Time:
                                  </Typography>
                                </Grid>
                                <Grid item xs={6}>
                                  <Typography variant="body2">
                                    {scraper.metrics.averageProcessingTimeMs.toFixed(0)} ms
                                  </Typography>
                                </Grid>
                                <Grid item xs={6}>
                                  <Typography variant="body2" color="textSecondary">
                                    Failed Items:
                                  </Typography>
                                </Grid>
                                <Grid item xs={6}>
                                  <Typography variant="body2" color={scraper.metrics.failedItems > 0 ? "error" : "inherit"}>
                                    {scraper.metrics.failedItems}
                                  </Typography>
                                </Grid>
                              </Grid>
                            </CardContent>
                          </Card>
                        </Grid>
                        <Grid item xs={12} md={6}>
                          <Card variant="outlined">
                            <CardContent>
                              <Typography variant="subtitle2" gutterBottom>
                                Status Information
                              </Typography>
                              <Typography variant="body2" sx={{ mb: 1 }}>
                                {scraper.message}
                              </Typography>
                              {scraper.isRunning && (
                                <Box sx={{ mt: 2 }}>
                                  <Typography variant="body2" color="textSecondary" gutterBottom>
                                    Pipeline Status:
                                  </Typography>
                                  <Grid container spacing={1}>
                                    <Grid item xs={4}>
                                      <Box sx={{ textAlign: 'center', p: 1, bgcolor: 'primary.light', borderRadius: 1 }}>
                                        <Typography variant="h6">{scraper.metrics.processingItems}</Typography>
                                        <Typography variant="caption">Processing</Typography>
                                      </Box>
                                    </Grid>
                                    <Grid item xs={4}>
                                      <Box sx={{ textAlign: 'center', p: 1, bgcolor: 'info.light', borderRadius: 1 }}>
                                        <Typography variant="h6">{scraper.metrics.queuedItems}</Typography>
                                        <Typography variant="caption">Queued</Typography>
                                      </Box>
                                    </Grid>
                                    <Grid item xs={4}>
                                      <Box sx={{ textAlign: 'center', p: 1, bgcolor: 'success.light', borderRadius: 1 }}>
                                        <Typography variant="h6">{scraper.metrics.completedItems}</Typography>
                                        <Typography variant="caption">Completed</Typography>
                                      </Box>
                                    </Grid>
                                  </Grid>
                                </Box>
                              )}
                            </CardContent>
                          </Card>
                        </Grid>
                      </Grid>
                    </Box>
                  </TableCell>
                </TableRow>
              )}
            </React.Fragment>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
};

export default MonitoringDashboard;