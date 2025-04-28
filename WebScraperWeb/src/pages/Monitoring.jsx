import React, { useState, useEffect } from 'react';
import { 
  Container, 
  Box, 
  Typography, 
  Paper, 
  Grid, 
  Card, 
  CardContent,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Button,
  IconButton,
  Tooltip,
  Alert,
  CircularProgress,
  Divider
} from '@mui/material';
import { 
  Refresh as RefreshIcon,
  Visibility as VisibilityIcon,
  PlayArrow as PlayIcon,
  Stop as StopIcon,
  Warning as WarningIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  AccessTime as AccessTimeIcon
} from '@mui/icons-material';
import { Link } from 'react-router-dom';
import PageHeader from '../components/common/PageHeader';
import LoadingSpinner from '../components/common/LoadingSpinner';
import { useScrapers } from '../contexts/ScraperContext';
import { formatRelativeTime } from '../utils/formatters';

const Monitoring = () => {
  const { 
    scrapers, 
    scraperStatus, 
    loading, 
    error, 
    fetchScrapers, 
    start, 
    stop 
  } = useScrapers();
  
  const [actionInProgress, setActionInProgress] = useState(null);
  const [refreshing, setRefreshing] = useState(false);

  // Fetch scrapers data
  useEffect(() => {
    fetchScrapers();
    
    // Auto-refresh every 30 seconds
    const interval = setInterval(fetchScrapers, 30000);
    return () => clearInterval(interval);
  }, [fetchScrapers]);

  const handleRefresh = async () => {
    setRefreshing(true);
    await fetchScrapers();
    setRefreshing(false);
  };

  const handleStartScraper = async (id) => {
    try {
      setActionInProgress(id);
      await start(id);
      fetchScrapers(); // Refresh the list
    } catch (error) {
      console.error(`Error starting scraper ${id}:`, error);
    } finally {
      setActionInProgress(null);
    }
  };

  const handleStopScraper = async (id) => {
    try {
      setActionInProgress(id);
      await stop(id);
      fetchScrapers(); // Refresh the list
    } catch (error) {
      console.error(`Error stopping scraper ${id}:`, error);
    } finally {
      setActionInProgress(null);
    }
  };

  // Calculate system status
  const calculateSystemStatus = () => {
    if (!scrapers || scrapers.length === 0) {
      return { status: 'unknown', message: 'No scrapers configured' };
    }
    
    const runningCount = Object.values(scraperStatus).filter(status => status?.isRunning).length;
    const errorCount = Object.values(scraperStatus).filter(status => status?.hasErrors).length;
    
    if (errorCount > 0) {
      return { 
        status: 'error', 
        message: `${errorCount} scraper${errorCount > 1 ? 's' : ''} with errors` 
      };
    }
    
    if (runningCount > 0) {
      return { 
        status: 'running', 
        message: `${runningCount} scraper${runningCount > 1 ? 's' : ''} running` 
      };
    }
    
    return { status: 'idle', message: 'All scrapers idle' };
  };

  const systemStatus = calculateSystemStatus();

  // Get status icon based on status
  const getStatusIcon = (status) => {
    switch (status) {
      case 'running':
        return <PlayIcon sx={{ color: 'success.main' }} />;
      case 'error':
        return <ErrorIcon sx={{ color: 'error.main' }} />;
      case 'idle':
        return <AccessTimeIcon sx={{ color: 'info.main' }} />;
      default:
        return <WarningIcon sx={{ color: 'warning.main' }} />;
    }
  };

  if (loading && scrapers.length === 0) {
    return <LoadingSpinner message="Loading monitoring data..." />;
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      {/* Header */}
      <PageHeader
        title="Monitoring"
        subtitle="Monitor the status and performance of your scrapers"
        breadcrumbs={[
          { text: 'Dashboard', path: '/' },
          { text: 'Monitoring' }
        ]}
      />

      {/* Error Alert */}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          Error loading monitoring data: {error}
        </Alert>
      )}

      {/* Refresh Button */}
      <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 3 }}>
        <Button
          variant="outlined"
          startIcon={refreshing ? <CircularProgress size={20} /> : <RefreshIcon />}
          onClick={handleRefresh}
          disabled={refreshing}
        >
          {refreshing ? 'Refreshing...' : 'Refresh'}
        </Button>
      </Box>

      {/* System Status */}
      <Paper sx={{ p: 2, mb: 3 }}>
        <Typography variant="h6" gutterBottom>System Status</Typography>
        <Divider sx={{ mb: 2 }} />
        
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          {getStatusIcon(systemStatus.status)}
          <Typography variant="body1" sx={{ ml: 1 }}>
            {systemStatus.message}
          </Typography>
        </Box>
      </Paper>

      {/* Status Summary Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <CheckCircleIcon color="success" sx={{ mr: 1 }} />
                <Typography variant="h6">Active Scrapers</Typography>
              </Box>
              <Typography variant="h4">
                {Object.values(scraperStatus).filter(status => status?.isRunning).length}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <ErrorIcon color="error" sx={{ mr: 1 }} />
                <Typography variant="h6">Scrapers with Errors</Typography>
              </Box>
              <Typography variant="h4">
                {Object.values(scraperStatus).filter(status => status?.hasErrors).length}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <AccessTimeIcon color="info" sx={{ mr: 1 }} />
                <Typography variant="h6">Idle Scrapers</Typography>
              </Box>
              <Typography variant="h4">
                {Object.values(scraperStatus).filter(status => !status?.isRunning && !status?.hasErrors).length}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <WarningIcon color="warning" sx={{ mr: 1 }} />
                <Typography variant="h6">Scheduled Scrapers</Typography>
              </Box>
              <Typography variant="h4">
                {scrapers.filter(scraper => scraper.enableContinuousMonitoring).length}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Scrapers Status Table */}
      <Paper sx={{ mb: 3 }}>
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Scraper</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Last Run</TableCell>
                <TableCell>Next Run</TableCell>
                <TableCell>URLs Processed</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={6} align="center">
                    <CircularProgress size={24} sx={{ my: 2 }} />
                  </TableCell>
                </TableRow>
              ) : scrapers.length > 0 ? (
                scrapers.map((scraper) => {
                  const status = scraperStatus[scraper.id] || {};
                  
                  return (
                    <TableRow key={scraper.id}>
                      <TableCell>
                        <Link 
                          to={`/scrapers/${scraper.id}`} 
                          style={{ textDecoration: 'none', color: 'inherit', fontWeight: 'bold' }}
                        >
                          {scraper.name}
                        </Link>
                      </TableCell>
                      <TableCell>
                        <Chip 
                          label={status.isRunning ? 'Running' : status.hasErrors ? 'Error' : 'Idle'} 
                          color={status.isRunning ? 'success' : status.hasErrors ? 'error' : 'default'}
                          size="small"
                        />
                      </TableCell>
                      <TableCell>
                        {scraper.lastRun ? formatRelativeTime(new Date(scraper.lastRun)) : 'Never'}
                      </TableCell>
                      <TableCell>
                        {scraper.enableContinuousMonitoring ? 
                          (scraper.nextScheduledRun ? 
                            formatRelativeTime(new Date(scraper.nextScheduledRun)) : 
                            'Not scheduled') : 
                          'Not scheduled'}
                      </TableCell>
                      <TableCell>{scraper.urlsProcessed || 0}</TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', gap: 1 }}>
                          <Tooltip title="View Details">
                            <IconButton 
                              component={Link} 
                              to={`/scrapers/${scraper.id}`}
                              size="small"
                            >
                              <VisibilityIcon />
                            </IconButton>
                          </Tooltip>
                          
                          {status.isRunning ? (
                            <Tooltip title="Stop Scraper">
                              <IconButton 
                                color="warning"
                                onClick={() => handleStopScraper(scraper.id)}
                                disabled={actionInProgress === scraper.id}
                                size="small"
                              >
                                {actionInProgress === scraper.id ? (
                                  <CircularProgress size={24} />
                                ) : (
                                  <StopIcon />
                                )}
                              </IconButton>
                            </Tooltip>
                          ) : (
                            <Tooltip title="Start Scraper">
                              <IconButton 
                                color="success"
                                onClick={() => handleStartScraper(scraper.id)}
                                disabled={actionInProgress === scraper.id}
                                size="small"
                              >
                                {actionInProgress === scraper.id ? (
                                  <CircularProgress size={24} />
                                ) : (
                                  <PlayIcon />
                                )}
                              </IconButton>
                            </Tooltip>
                          )}
                        </Box>
                      </TableCell>
                    </TableRow>
                  );
                })
              ) : (
                <TableRow>
                  <TableCell colSpan={6} align="center" sx={{ py: 3 }}>
                    <Typography variant="body1" gutterBottom>
                      No scrapers found. Create your first scraper to get started.
                    </Typography>
                    <Button 
                      variant="contained" 
                      component={Link}
                      to="/scrapers/create"
                      sx={{ mt: 1 }}
                    >
                      Create Scraper
                    </Button>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>

      {/* Recent Logs */}
      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>Recent System Logs</Typography>
        <Divider sx={{ mb: 2 }} />
        
        <Box 
          sx={{ 
            maxHeight: '300px', 
            overflow: 'auto', 
            bgcolor: '#f5f5f5', 
            p: 2, 
            borderRadius: 1,
            fontFamily: 'monospace'
          }}
        >
          {/* Mock log entries */}
          <Box sx={{ mb: 1 }}>
            <Typography variant="body2" component="span" sx={{ mr: 2, color: 'text.secondary' }}>
              {new Date().toLocaleString()}
            </Typography>
            <Typography variant="body2" component="span" sx={{ mr: 2, fontWeight: 'bold', color: 'success.main' }}>
              [INFO]
            </Typography>
            <Typography variant="body2" component="span">
              UKGC scraper completed successfully
            </Typography>
          </Box>
          
          <Box sx={{ mb: 1 }}>
            <Typography variant="body2" component="span" sx={{ mr: 2, color: 'text.secondary' }}>
              {new Date(Date.now() - 3600000).toLocaleString()}
            </Typography>
            <Typography variant="body2" component="span" sx={{ mr: 2, fontWeight: 'bold', color: 'info.main' }}>
              [INFO]
            </Typography>
            <Typography variant="body2" component="span">
              MGA scraper started
            </Typography>
          </Box>
          
          <Box sx={{ mb: 1 }}>
            <Typography variant="body2" component="span" sx={{ mr: 2, color: 'text.secondary' }}>
              {new Date(Date.now() - 7200000).toLocaleString()}
            </Typography>
            <Typography variant="body2" component="span" sx={{ mr: 2, fontWeight: 'bold', color: 'error.main' }}>
              [ERROR]
            </Typography>
            <Typography variant="body2" component="span">
              Gibraltar scraper failed: Connection timeout
            </Typography>
          </Box>
          
          <Box sx={{ mb: 1 }}>
            <Typography variant="body2" component="span" sx={{ mr: 2, color: 'text.secondary' }}>
              {new Date(Date.now() - 10800000).toLocaleString()}
            </Typography>
            <Typography variant="body2" component="span" sx={{ mr: 2, fontWeight: 'bold', color: 'warning.main' }}>
              [WARN]
            </Typography>
            <Typography variant="body2" component="span">
              Rate limiting detected on UKGC website
            </Typography>
          </Box>
        </Box>
      </Paper>
    </Container>
  );
};

export default Monitoring;
