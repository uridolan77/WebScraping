import React, { useState, useEffect } from 'react';
import {
  Grid,
  Card,
  CardContent,
  Typography,
  Box,
  Button,
  Paper,
  Divider,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Chip,
  Collapse,
  IconButton,
  CircularProgress
} from '@mui/material';
import {
  Speed as SpeedIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Sync as SyncIcon,
  ArrowUpward as ArrowUpwardIcon,
  ArrowDownward as ArrowDownwardIcon,
  Web as WebIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  Link as LinkIcon
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useScraperContext } from '../contexts/ScraperContext';
import { formatDate, formatRelativeTime } from '../utils/formatters';
import { getStatusColor } from '../utils/helpers';
import PageHeader from '../components/common/PageHeader';
import LoadingSpinner from '../components/common/LoadingSpinner';
import axios from 'axios';

const Dashboard = () => {
  const navigate = useNavigate();
  const { scrapers, scraperStatus, loading, error, fetchScrapers } = useScraperContext();
  const [stats, setStats] = useState({
    total: 0,
    running: 0,
    completed: 0,
    failed: 0,
    scheduled: 0
  });
  const [expandedScraper, setExpandedScraper] = useState(null);
  const [processingLogs, setProcessingLogs] = useState({});
  const [loadingLogs, setLoadingLogs] = useState({});

  useEffect(() => {
    fetchScrapers();
    const interval = setInterval(fetchScrapers, 10000); // Refresh every 10 seconds
    return () => clearInterval(interval);
  }, [fetchScrapers]);

  // Calculate stats when scrapers or status changes
  useEffect(() => {
    if (scrapers && scraperStatus) {
      const newStats = {
        total: scrapers.length,
        running: 0,
        completed: 0,
        failed: 0,
        scheduled: 0
      };

      scrapers.forEach(scraper => {
        const status = scraperStatus[scraper.id];
        if (status) {
          if (status.isRunning) {
            newStats.running++;
          } else if (status.hasErrors) {
            newStats.failed++;
          } else if (status.lastRun) {
            newStats.completed++;
          }

          if (scraper.enableContinuousMonitoring) {
            newStats.scheduled++;
          }
        }
      });

      setStats(newStats);
    }
  }, [scrapers, scraperStatus]);

  const fetchScraperLogs = async (scraperId) => {
    try {
      setLoadingLogs(prev => ({ ...prev, [scraperId]: true }));
      const response = await axios.get(`/api/scrapers/${scraperId}/logs?limit=50`);
      setProcessingLogs(prev => ({ ...prev, [scraperId]: response.data }));
      setLoadingLogs(prev => ({ ...prev, [scraperId]: false }));
    } catch (err) {
      console.error(`Error fetching logs for scraper ${scraperId}:`, err);
      setLoadingLogs(prev => ({ ...prev, [scraperId]: false }));
      setProcessingLogs(prev => ({ ...prev, [scraperId]: [{ 
        id: Date.now(), 
        timestamp: new Date(),
        message: 'Error loading logs', 
        logLevel: 'Error' 
      }] }));
    }
  };

  const handleExpandScraper = (scraperId) => {
    if (expandedScraper === scraperId) {
      setExpandedScraper(null);
    } else {
      setExpandedScraper(scraperId);
      if (!processingLogs[scraperId]) {
        fetchScraperLogs(scraperId);
      }
    }
  };

  const handleCreateScraper = () => {
    navigate('/scrapers/create');
  };

  const handleViewScraper = (id) => {
    navigate(`/scrapers/${id}`);
  };

  const handleViewAllScrapers = () => {
    navigate('/scrapers');
  };

  if (loading) {
    return <LoadingSpinner />;
  }

  if (error) {
    return (
      <Box p={3}>
        <Typography color="error" variant="h6">
          Error loading dashboard: {error}
        </Typography>
        <Button variant="contained" onClick={fetchScrapers} sx={{ mt: 2 }}>
          Retry
        </Button>
      </Box>
    );
  }

  // Get recent scrapers (last 5)
  const recentScrapers = [...scrapers]
    .sort((a, b) => new Date(b.lastRun || 0) - new Date(a.lastRun || 0))
    .slice(0, 5);

  // Get currently running scrapers
  const runningScrapers = scrapers.filter(scraper => 
    scraperStatus[scraper.id]?.isRunning
  );

  return (
    <Box>
      <PageHeader
        title="Dashboard"
        subtitle="Overview of your web scraping operations"
        actionText="Create New Scraper"
        onActionClick={handleCreateScraper}
      />

      {/* Stats Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={2.4}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <WebIcon color="primary" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h4">{stats.total}</Typography>
              <Typography variant="body2" color="textSecondary">Total Scrapers</Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={2.4}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <SyncIcon color="info" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h4">{stats.running}</Typography>
              <Typography variant="body2" color="textSecondary">Running</Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={2.4}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <CheckCircleIcon color="success" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h4">{stats.completed}</Typography>
              <Typography variant="body2" color="textSecondary">Completed</Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={2.4}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <ErrorIcon color="error" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h4">{stats.failed}</Typography>
              <Typography variant="body2" color="textSecondary">Failed</Typography>
            </CardContent>
          </Card>
        </Grid>
                        primary={scraper.name}
                        secondary={`Last run: ${scraper.lastRun ? formatRelativeTime(new Date(scraper.lastRun)) : 'Never'}`}
                      />
                      <Chip
                        label={status.isRunning ? 'Running' : status.hasErrors ? 'Error' : 'Completed'}
                        color={statusColor}
                        size="small"
                      />
                    </ListItem>
                  );
                })}
              </List>
            ) : (
              <Typography variant="body2" color="textSecondary" align="center">
                No scrapers found. Create your first scraper to get started.
              </Typography>
            )}
          </Paper>
        </Grid>

        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 2, height: '100%' }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6">Recent Activity</Typography>
              <Button size="small" onClick={() => navigate('/monitoring')}>View All</Button>
            </Box>
            <Divider sx={{ mb: 2 }} />

            <List>
              {/* This would be populated with actual activity data from an API */}
              <ListItem sx={{ mb: 1 }}>
                <ListItemIcon>
                  <CheckCircleIcon color="success" />
                </ListItemIcon>
                <ListItemText
                  primary="UKGC Scraper completed successfully"
                  secondary={formatDate(new Date(), 'MMM d, yyyy HH:mm')}
                />
              </ListItem>

              <ListItem sx={{ mb: 1 }}>
                <ListItemIcon>
                  <ArrowUpwardIcon color="info" />
                </ListItemIcon>
                <ListItemText
                  primary="MGA Scraper started"
                  secondary={formatDate(new Date(Date.now() - 3600000), 'MMM d, yyyy HH:mm')}
                />
              </ListItem>

              <ListItem sx={{ mb: 1 }}>
                <ListItemIcon>
                  <ErrorIcon color="error" />
                </ListItemIcon>
                <ListItemText
                  primary="Gibraltar Scraper failed"
                  secondary={formatDate(new Date(Date.now() - 7200000), 'MMM d, yyyy HH:mm')}
                />
              </ListItem>

              <ListItem sx={{ mb: 1 }}>
                <ListItemIcon>
                  <ArrowDownwardIcon color="warning" />
                </ListItemIcon>
                <ListItemText
                  primary="UKGC Scraper stopped"
                  secondary={formatDate(new Date(Date.now() - 86400000), 'MMM d, yyyy HH:mm')}
                />
              </ListItem>
            </List>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default Dashboard;
