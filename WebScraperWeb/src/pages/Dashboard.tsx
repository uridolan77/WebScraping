import React, { useState } from 'react';
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
  Chip
} from '@mui/material';
import {
  Speed as SpeedIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Sync as SyncIcon,
  ArrowUpward as ArrowUpwardIcon,
  ArrowDownward as ArrowDownwardIcon,
  Web as WebIcon
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import PageHeader from '../components/common/PageHeader';

// Mock data for the dashboard
const mockStats = {
  total: 3,
  running: 1,
  completed: 1,
  failed: 1,
  scheduled: 2
};

const mockScrapers = [
  {
    id: '1',
    name: 'UKGC Scraper',
    url: 'https://www.gamblingcommission.gov.uk',
    lastRun: new Date().toISOString(),
    status: { isRunning: false, hasErrors: false }
  },
  {
    id: '2',
    name: 'MGA Scraper',
    url: 'https://www.mga.org.mt',
    lastRun: new Date(Date.now() - 3600000).toISOString(),
    status: { isRunning: true, hasErrors: false }
  },
  {
    id: '3',
    name: 'Gibraltar Scraper',
    url: 'https://www.gibraltar.gov.gi',
    lastRun: new Date(Date.now() - 7200000).toISOString(),
    status: { isRunning: false, hasErrors: true }
  }
];

const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const { currentUser } = useAuth();
  const [stats] = useState(mockStats);
  const [recentScrapers] = useState(mockScrapers);

  const handleCreateScraper = () => {
    // This would navigate to the scraper creation page
    alert('Create scraper functionality will be implemented soon');
  };

  const handleViewScraper = (id: string) => {
    // This would navigate to the scraper detail page
    alert(`View scraper ${id} functionality will be implemented soon`);
  };

  const handleViewAllScrapers = () => {
    // This would navigate to the scrapers list page
    alert('View all scrapers functionality will be implemented soon');
  };

  // Helper function to format dates
  const formatDate = (date: Date, format: string = 'MMM d, yyyy HH:mm'): string => {
    return date.toLocaleString();
  };

  // Helper function to format relative time
  const formatRelativeTime = (date: Date): string => {
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} minutes ago`;

    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours} hours ago`;

    const diffDays = Math.floor(diffHours / 24);
    return `${diffDays} days ago`;
  };

  // Helper function to get status color
  const getStatusColor = (status: string): 'success' | 'error' | 'warning' | 'info' => {
    switch (status) {
      case 'running':
        return 'info';
      case 'error':
        return 'error';
      case 'completed':
        return 'success';
      default:
        return 'warning';
    }
  };

  return (
    <Box>
      <PageHeader
        title={`Welcome, ${currentUser?.name || 'User'}`}
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

        <Grid item xs={12} sm={6} md={2.4}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <SpeedIcon color="warning" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h4">{stats.scheduled}</Typography>
              <Typography variant="body2" color="textSecondary">Scheduled</Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Recent Scrapers */}
      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 2, height: '100%' }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6">Recent Scrapers</Typography>
              <Button size="small" onClick={handleViewAllScrapers}>View All</Button>
            </Box>
            <Divider sx={{ mb: 2 }} />

            {recentScrapers.length > 0 ? (
              <List>
                {recentScrapers.map((scraper) => {
                  const status = scraper.status || { isRunning: false, hasErrors: false };
                  const statusColor = getStatusColor(status.isRunning ? 'running' : status.hasErrors ? 'error' : 'completed');

                  return (
                    <ListItem
                      key={scraper.id}
                      button
                      onClick={() => handleViewScraper(scraper.id)}
                      sx={{
                        borderLeft: `4px solid ${statusColor === 'success' ? 'green' :
                                            statusColor === 'error' ? 'red' :
                                            statusColor === 'warning' ? 'orange' : 'blue'}`,
                        mb: 1,
                        borderRadius: 1,
                        '&:hover': { backgroundColor: 'rgba(0, 0, 0, 0.04)' }
                      }}
                    >
                      <ListItemIcon>
                        <WebIcon />
                      </ListItemIcon>
                      <ListItemText
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
              <Button size="small" onClick={() => alert('Monitoring functionality will be implemented soon')}>View All</Button>
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
                  secondary={formatDate(new Date())}
                />
              </ListItem>

              <ListItem sx={{ mb: 1 }}>
                <ListItemIcon>
                  <ArrowUpwardIcon color="info" />
                </ListItemIcon>
                <ListItemText
                  primary="MGA Scraper started"
                  secondary={formatDate(new Date(Date.now() - 3600000))}
                />
              </ListItem>

              <ListItem sx={{ mb: 1 }}>
                <ListItemIcon>
                  <ErrorIcon color="error" />
                </ListItemIcon>
                <ListItemText
                  primary="Gibraltar Scraper failed"
                  secondary={formatDate(new Date(Date.now() - 7200000))}
                />
              </ListItem>

              <ListItem sx={{ mb: 1 }}>
                <ListItemIcon>
                  <ArrowDownwardIcon color="warning" />
                </ListItemIcon>
                <ListItemText
                  primary="UKGC Scraper stopped"
                  secondary={formatDate(new Date(Date.now() - 86400000))}
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
