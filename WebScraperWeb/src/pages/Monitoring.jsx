import React, { useState, useEffect } from 'react';
import {
  Container,
  Typography,
  Box,
  Paper,
  Grid,
  Card,
  CardContent,
  Tabs,
  Tab,
  Divider,
  Alert,
  CircularProgress,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  Chip,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  LinearProgress,
  Tooltip,
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  Error as ErrorIcon,
  CheckCircle as CheckCircleIcon,
  Warning as WarningIcon,
  Info as InfoIcon,
  PlayArrow as PlayArrowIcon,
  Stop as StopIcon,
  Visibility as VisibilityIcon,
} from '@mui/icons-material';
import { useScrapers } from '../hooks';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, Legend, ResponsiveContainer } from 'recharts';

// Tab Panel component
function TabPanel(props) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`monitoring-tab-${index}`}
      aria-labelledby={`monitoring-tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ p: 3 }}>
          {children}
        </Box>
      )}
    </div>
  );
}

// Generate sample data for usage charts
const generateSampleUsageData = (hours = 24) => {
  const data = [];
  const now = new Date();
  
  for (let i = 0; i < hours; i++) {
    const time = new Date(now);
    time.setHours(time.getHours() - i);
    data.push({
      time: time.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      cpu: Math.floor(Math.random() * 60) + 10,
      memory: Math.floor(Math.random() * 40) + 30,
      requests: Math.floor(Math.random() * 100) + 50,
    });
  }
  
  return data.reverse();
};

const Monitoring = () => {
  const { getScrapers, getScraperStatus, startScraper, stopScraper } = useScrapers();
  const [activeTab, setActiveTab] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [scrapers, setScrapers] = useState([]);
  const [scrapersStatus, setScrapersStatus] = useState({});
  const [systemMetrics, setSystemMetrics] = useState({
    cpu: 32,
    memory: 68,
    disk: 47,
    network: 23,
    uptime: '14 days, 7 hours',
    activeScrapers: 3,
    queuedJobs: 2,
    requestsPerMinute: 128
  });
  const [usageData, setUsageData] = useState([]);
  const [recentEvents, setRecentEvents] = useState([]);
  const [actionInProgress, setActionInProgress] = useState(null);
  const [timeRange, setTimeRange] = useState('24h');
  
  // Load initial data
  useEffect(() => {
    const fetchData = async () => {
      try {
        setIsLoading(true);
        
        // Fetch scrapers
        const scrapersData = await getScrapers();
        setScrapers(scrapersData || []);
        
        // Fetch status for each scraper
        const statusMap = {};
        for (const scraper of scrapersData || []) {
          try {
            const status = await getScraperStatus(scraper.id);
            statusMap[scraper.id] = status;
          } catch (err) {
            console.error(`Error fetching status for scraper ${scraper.id}:`, err);
            statusMap[scraper.id] = { 
              isRunning: false, 
              hasErrors: true, 
              errorMessage: "Failed to fetch status" 
            };
          }
        }
        setScrapersStatus(statusMap);
        
        // Set sample usage data
        setUsageData(generateSampleUsageData(24));
        
        // Set sample recent events
        setRecentEvents([
          { 
            id: 1, 
            timestamp: new Date(new Date().setHours(new Date().getHours() - 1)), 
            type: 'error', 
            message: 'Scraper "Regulatory News" encountered 403 Forbidden error on page /restricted-content',
            scraperId: scrapersData?.[1]?.id
          },
          { 
            id: 2, 
            timestamp: new Date(new Date().setHours(new Date().getHours() - 3)), 
            type: 'info', 
            message: 'Scraper "UKGC Website" completed successfully, 238 pages processed',
            scraperId: scrapersData?.[0]?.id
          },
          { 
            id: 3, 
            timestamp: new Date(new Date().setHours(new Date().getHours() - 5)), 
            type: 'warning', 
            message: 'High memory usage detected (87%), consider optimizing scrapers',
            scraperId: null
          },
          { 
            id: 4, 
            timestamp: new Date(new Date().setHours(new Date().getHours() - 12)), 
            type: 'info', 
            message: 'System update applied successfully, restarted all services',
            scraperId: null
          },
          { 
            id: 5, 
            timestamp: new Date(new Date().setHours(new Date().getHours() - 18)), 
            type: 'error', 
            message: 'Failed to connect to database, retrying... (Resolved after 3 attempts)',
            scraperId: null
          },
        ]);
        
      } catch (error) {
        console.error('Error loading monitoring data:', error);
      } finally {
        setIsLoading(false);
      }
    };
    
    fetchData();
    
    // Set up polling for real-time updates (every 30 seconds)
    const intervalId = setInterval(() => {
      // Update system metrics with slight variations
      setSystemMetrics(prev => ({
        ...prev,
        cpu: Math.min(100, Math.max(10, prev.cpu + (Math.random() * 10 - 5))),
        memory: Math.min(100, Math.max(20, prev.memory + (Math.random() * 6 - 3))),
        disk: prev.disk,
        network: Math.min(100, Math.max(5, prev.network + (Math.random() * 8 - 4))),
        requestsPerMinute: Math.floor(Math.max(50, prev.requestsPerMinute + (Math.random() * 40 - 20)))
      }));
      
      // Add new usage datapoint
      setUsageData(prevData => {
        const newData = [...prevData.slice(1)];
        const now = new Date();
        newData.push({
          time: now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
          cpu: Math.floor(Math.random() * 60) + 10,
          memory: Math.floor(Math.random() * 40) + 30,
          requests: Math.floor(Math.random() * 100) + 50,
        });
        return newData;
      });
      
    }, 30000);
    
    return () => clearInterval(intervalId);
  }, [getScrapers, getScraperStatus]);
  
  // Handle tab change
  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
  };
  
  // Handle time range change
  const handleTimeRangeChange = (event) => {
    const range = event.target.value;
    setTimeRange(range);
    
    // Update usage data based on selected time range
    const hours = range === '1h' ? 1 : range === '6h' ? 6 : range === '24h' ? 24 : 168;
    setUsageData(generateSampleUsageData(hours));
  };
  
  // Handle scraper action (start/stop)
  const handleScraperAction = async (scraperId, action) => {
    try {
      setActionInProgress(scraperId);
      
      if (action === 'start') {
        await startScraper(scraperId);
      } else if (action === 'stop') {
        await stopScraper(scraperId);
      }
      
      // Update scraper status
      const newStatus = await getScraperStatus(scraperId);
      setScrapersStatus(prev => ({
        ...prev,
        [scraperId]: newStatus
      }));
      
    } catch (error) {
      console.error(`Error ${action}ing scraper:`, error);
    } finally {
      setActionInProgress(null);
    }
  };
  
  // Handle refresh
  const handleRefresh = async () => {
    setIsLoading(true);
    
    try {
      // Fetch scrapers
      const scrapersData = await getScrapers();
      setScrapers(scrapersData || []);
      
      // Fetch status for each scraper
      const statusMap = {};
      for (const scraper of scrapersData || []) {
        try {
          const status = await getScraperStatus(scraper.id);
          statusMap[scraper.id] = status;
        } catch (err) {
          console.error(`Error fetching status for scraper ${scraper.id}:`, err);
          statusMap[scraper.id] = { 
            isRunning: false, 
            hasErrors: true, 
            errorMessage: "Failed to fetch status" 
          };
        }
      }
      setScrapersStatus(statusMap);
      
      // Update usage data
      const hours = timeRange === '1h' ? 1 : timeRange === '6h' ? 6 : timeRange === '24h' ? 24 : 168;
      setUsageData(generateSampleUsageData(hours));
      
    } catch (error) {
      console.error('Error refreshing monitoring data:', error);
    } finally {
      setIsLoading(false);
    }
  };
  
  // Format timestamp
  const formatTimestamp = (timestamp) => {
    const date = new Date(timestamp);
    return date.toLocaleString();
  };
  
  // Format time ago
  const timeAgo = (timestamp) => {
    const seconds = Math.floor((new Date() - new Date(timestamp)) / 1000);
    
    let interval = Math.floor(seconds / 31536000);
    if (interval >= 1) return `${interval} year${interval > 1 ? 's' : ''} ago`;
    
    interval = Math.floor(seconds / 2592000);
    if (interval >= 1) return `${interval} month${interval > 1 ? 's' : ''} ago`;
    
    interval = Math.floor(seconds / 86400);
    if (interval >= 1) return `${interval} day${interval > 1 ? 's' : ''} ago`;
    
    interval = Math.floor(seconds / 3600);
    if (interval >= 1) return `${interval} hour${interval > 1 ? 's' : ''} ago`;
    
    interval = Math.floor(seconds / 60);
    if (interval >= 1) return `${interval} minute${interval > 1 ? 's' : ''} ago`;
    
    return `${Math.floor(seconds)} second${seconds !== 1 ? 's' : ''} ago`;
  };
  
  if (isLoading && !scrapers.length) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }
  
  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      {/* Header */}
      <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h4" gutterBottom>
            System Monitoring
          </Typography>
          <Typography variant="body1" color="textSecondary">
            Monitor your web scraping system performance and status
          </Typography>
        </Box>
        <Button
          variant="outlined"
          startIcon={<RefreshIcon />}
          onClick={handleRefresh}
          disabled={isLoading}
        >
          Refresh
        </Button>
      </Box>
      
      {isLoading && (
        <LinearProgress sx={{ mb: 4 }} />
      )}
      
      {/* System Overview Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                CPU Usage
              </Typography>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <Typography variant="h5" component="div" sx={{ fontWeight: 'bold', mr: 1 }}>
                  {Math.round(systemMetrics.cpu)}%
                </Typography>
                <LinearProgress 
                  variant="determinate" 
                  value={systemMetrics.cpu} 
                  sx={{ 
                    flexGrow: 1, 
                    height: 8, 
                    borderRadius: 5,
                    bgcolor: 'background.paper',
                    '& .MuiLinearProgress-bar': {
                      bgcolor: systemMetrics.cpu > 80 ? 'error.main' : 
                              systemMetrics.cpu > 60 ? 'warning.main' : 'success.main',
                    }
                  }} 
                />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                Memory Usage
              </Typography>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <Typography variant="h5" component="div" sx={{ fontWeight: 'bold', mr: 1 }}>
                  {Math.round(systemMetrics.memory)}%
                </Typography>
                <LinearProgress 
                  variant="determinate" 
                  value={systemMetrics.memory} 
                  sx={{ 
                    flexGrow: 1, 
                    height: 8, 
                    borderRadius: 5,
                    bgcolor: 'background.paper',
                    '& .MuiLinearProgress-bar': {
                      bgcolor: systemMetrics.memory > 80 ? 'error.main' : 
                              systemMetrics.memory > 60 ? 'warning.main' : 'success.main',
                    }
                  }} 
                />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                Active Scrapers
              </Typography>
              <Typography variant="h5" component="div" sx={{ fontWeight: 'bold' }}>
                {Object.values(scrapersStatus).filter(status => status.isRunning).length} / {scrapers.length}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                System Uptime
              </Typography>
              <Typography variant="h5" component="div" sx={{ fontWeight: 'bold' }}>
                {systemMetrics.uptime}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
      
      {/* Tabs */}
      <Paper sx={{ mb: 4 }}>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs
            value={activeTab}
            onChange={handleTabChange}
            indicatorColor="primary"
            textColor="primary"
            variant="scrollable"
            scrollButtons="auto"
          >
            <Tab label="Scraper Status" />
            <Tab label="System Performance" />
            <Tab label="Event Log" />
          </Tabs>
        </Box>
        
        {/* Scraper Status Tab */}
        <TabPanel value={activeTab} index={0}>
          <Typography variant="h6" gutterBottom>
            Scraper Status Overview
          </Typography>
          <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
            View and manage the status of all configured scrapers in the system.
          </Typography>
          
          <TableContainer component={Paper} variant="outlined" sx={{ mb: 4 }}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Scraper Name</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Last Run</TableCell>
                  <TableCell>URLs Processed</TableCell>
                  <TableCell>Last Error</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {scrapers.map((scraper) => {
                  const status = scrapersStatus[scraper.id] || {};
                  return (
                    <TableRow key={scraper.id}>
                      <TableCell>{scraper.name}</TableCell>
                      <TableCell>
                        <Chip 
                          label={
                            status.isRunning ? 'Running' : 
                            status.hasErrors ? 'Error' : 
                            'Idle'
                          }
                          color={
                            status.isRunning ? 'success' : 
                            status.hasErrors ? 'error' : 
                            'default'
                          }
                          size="small"
                          icon={
                            status.isRunning ? <PlayArrowIcon /> : 
                            status.hasErrors ? <ErrorIcon /> : 
                            <CheckCircleIcon />
                          }
                        />
                      </TableCell>
                      <TableCell>
                        {status.lastRun ? timeAgo(status.lastRun) : 'Never'}
                      </TableCell>
                      <TableCell>
                        {status.urlsProcessed || 0}
                      </TableCell>
                      <TableCell>
                        {status.hasErrors ? (
                          <Tooltip title={status.errorMessage || 'Unknown error'}>
                            <span style={{ cursor: 'help' }}>
                              {(status.errorMessage || 'Unknown error').substring(0, 30)}
                              {(status.errorMessage || 'Unknown error').length > 30 ? '...' : ''}
                            </span>
                          </Tooltip>
                        ) : (
                          'None'
                        )}
                      </TableCell>
                      <TableCell align="right">
                        <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
                          <Tooltip title="View Details">
                            <IconButton 
                              size="small"
                              onClick={() => window.location.href = `/scrapers/${scraper.id}`}
                            >
                              <VisibilityIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          
                          {status.isRunning ? (
                            <Tooltip title="Stop Scraper">
                              <IconButton 
                                size="small"
                                color="warning"
                                onClick={() => handleScraperAction(scraper.id, 'stop')}
                                disabled={actionInProgress === scraper.id}
                              >
                                {actionInProgress === scraper.id ? (
                                  <CircularProgress size={20} />
                                ) : (
                                  <StopIcon fontSize="small" />
                                )}
                              </IconButton>
                            </Tooltip>
                          ) : (
                            <Tooltip title="Start Scraper">
                              <IconButton 
                                size="small"
                                color="success"
                                onClick={() => handleScraperAction(scraper.id, 'start')}
                                disabled={actionInProgress === scraper.id}
                              >
                                {actionInProgress === scraper.id ? (
                                  <CircularProgress size={20} />
                                ) : (
                                  <PlayArrowIcon fontSize="small" />
                                )}
                              </IconButton>
                            </Tooltip>
                          )}
                        </Box>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </TableContainer>
        </TabPanel>
        
        {/* System Performance Tab */}
        <TabPanel value={activeTab} index={1}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
            <Typography variant="h6">
              System Resource Usage
            </Typography>
            <FormControl sx={{ width: 120 }}>
              <InputLabel id="time-range-label">Time Range</InputLabel>
              <Select
                labelId="time-range-label"
                id="time-range-select"
                value={timeRange}
                label="Time Range"
                onChange={handleTimeRangeChange}
                size="small"
              >
                <MenuItem value="1h">1 Hour</MenuItem>
                <MenuItem value="6h">6 Hours</MenuItem>
                <MenuItem value="24h">24 Hours</MenuItem>
                <MenuItem value="7d">7 Days</MenuItem>
              </Select>
            </FormControl>
          </Box>
          
          <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
            Monitor system resource usage over time to identify performance bottlenecks.
          </Typography>
          
          <Box sx={{ height: 400, mb: 4 }}>
            <ResponsiveContainer width="100%" height="100%">
              <LineChart
                data={usageData}
                margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="time" />
                <YAxis />
                <RechartsTooltip />
                <Legend />
                <Line 
                  type="monotone" 
                  dataKey="cpu" 
                  name="CPU Usage (%)" 
                  stroke="#8884d8" 
                  activeDot={{ r: 8 }} 
                />
                <Line 
                  type="monotone" 
                  dataKey="memory" 
                  name="Memory Usage (%)" 
                  stroke="#82ca9d" 
                />
                <Line 
                  type="monotone" 
                  dataKey="requests" 
                  name="Requests/min" 
                  stroke="#ffc658" 
                />
              </LineChart>
            </ResponsiveContainer>
          </Box>
          
          <Divider sx={{ my: 4 }} />
          
          <Typography variant="h6" gutterBottom>
            System Details
          </Typography>
          
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <TableContainer component={Paper} variant="outlined">
                <Table size="small">
                  <TableBody>
                    <TableRow>
                      <TableCell component="th" scope="row">
                        CPU Usage
                      </TableCell>
                      <TableCell align="right">
                        {Math.round(systemMetrics.cpu)}%
                      </TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell component="th" scope="row">
                        Memory Usage
                      </TableCell>
                      <TableCell align="right">
                        {Math.round(systemMetrics.memory)}%
                      </TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell component="th" scope="row">
                        Disk Usage
                      </TableCell>
                      <TableCell align="right">
                        {Math.round(systemMetrics.disk)}%
                      </TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell component="th" scope="row">
                        Network Usage
                      </TableCell>
                      <TableCell align="right">
                        {Math.round(systemMetrics.network)}%
                      </TableCell>
                    </TableRow>
                  </TableBody>
                </Table>
              </TableContainer>
            </Grid>
            <Grid item xs={12} md={6}>
              <TableContainer component={Paper} variant="outlined">
                <Table size="small">
                  <TableBody>
                    <TableRow>
                      <TableCell component="th" scope="row">
                        System Uptime
                      </TableCell>
                      <TableCell align="right">
                        {systemMetrics.uptime}
                      </TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell component="th" scope="row">
                        Active Scrapers
                      </TableCell>
                      <TableCell align="right">
                        {Object.values(scrapersStatus).filter(status => status.isRunning).length} / {scrapers.length}
                      </TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell component="th" scope="row">
                        Queued Jobs
                      </TableCell>
                      <TableCell align="right">
                        {systemMetrics.queuedJobs}
                      </TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell component="th" scope="row">
                        Requests Per Minute
                      </TableCell>
                      <TableCell align="right">
                        {systemMetrics.requestsPerMinute}
                      </TableCell>
                    </TableRow>
                  </TableBody>
                </Table>
              </TableContainer>
            </Grid>
          </Grid>
        </TabPanel>
        
        {/* Event Log Tab */}
        <TabPanel value={activeTab} index={2}>
          <Typography variant="h6" gutterBottom>
            System Event Log
          </Typography>
          <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
            Review system events, warnings, and errors for troubleshooting.
          </Typography>
          
          {recentEvents.map((event) => (
            <Paper 
              key={event.id} 
              variant="outlined" 
              sx={{ 
                p: 2, 
                mb: 2, 
                borderLeft: 6,
                borderColor: 
                  event.type === 'error' ? 'error.main' : 
                  event.type === 'warning' ? 'warning.main' : 
                  event.type === 'info' ? 'info.main' : 
                  'grey.500'
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                {event.type === 'error' && <ErrorIcon color="error" sx={{ mr: 1 }} />}
                {event.type === 'warning' && <WarningIcon color="warning" sx={{ mr: 1 }} />}
                {event.type === 'info' && <InfoIcon color="info" sx={{ mr: 1 }} />}
                <Typography variant="subtitle1" sx={{ fontWeight: 'medium' }}>
                  {event.type === 'error' ? 'Error' : event.type === 'warning' ? 'Warning' : 'Information'}
                </Typography>
                <Typography variant="body2" color="textSecondary" sx={{ ml: 'auto' }}>
                  {timeAgo(event.timestamp)}
                </Typography>
              </Box>
              <Typography variant="body1" sx={{ mb: 1 }}>
                {event.message}
              </Typography>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Typography variant="caption" color="textSecondary">
                  {formatTimestamp(event.timestamp)}
                </Typography>
                {event.scraperId && (
                  <Button
                    size="small"
                    onClick={() => window.location.href = `/scrapers/${event.scraperId}`}
                  >
                    View Scraper
                  </Button>
                )}
              </Box>
            </Paper>
          ))}
          
          <Box sx={{ display: 'flex', justifyContent: 'center', mt: 2 }}>
            <Button variant="outlined">
              View All Events
            </Button>
          </Box>
        </TabPanel>
      </Paper>
    </Container>
  );
};

export default Monitoring;