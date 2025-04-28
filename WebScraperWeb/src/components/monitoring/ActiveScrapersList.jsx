import React, { useState } from 'react';
import {
  Box, Typography, Paper, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, Chip, Button,
  LinearProgress, IconButton, Tooltip, CircularProgress,
  Alert, Card, CardContent, Grid
} from '@mui/material';
import {
  Stop as StopIcon,
  Visibility as VisibilityIcon,
  Timeline as TimelineIcon,
  Speed as SpeedIcon,
  Schedule as ScheduleIcon
} from '@mui/icons-material';
import { Link as RouterLink } from 'react-router-dom';
import { formatDistanceToNow } from 'date-fns';

const ActiveScrapersList = ({ activeScrapers, isLoading, onStop }) => {
  const [actionInProgress, setActionInProgress] = useState(null);

  // Handle stop scraper
  const handleStopScraper = async (id) => {
    setActionInProgress(id);
    try {
      await onStop(id);
    } finally {
      setActionInProgress(null);
    }
  };

  // Format duration
  const formatDuration = (startTime) => {
    if (!startTime) return 'Unknown';

    const start = new Date(startTime);
    const now = new Date();
    const durationMs = now - start;

    const seconds = Math.floor(durationMs / 1000) % 60;
    const minutes = Math.floor(durationMs / (1000 * 60)) % 60;
    const hours = Math.floor(durationMs / (1000 * 60 * 60));

    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  };

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (activeScrapers.length === 0) {
    return (
      <Alert severity="info">
        No active scrapers. Start a scraper to see real-time monitoring data.
      </Alert>
    );
  }

  return (
    <Box>
      <Typography variant="subtitle1" gutterBottom>
        Currently Running Scrapers: {activeScrapers.length}
      </Typography>

      <TableContainer component={Paper} variant="outlined">
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Scraper</TableCell>
              <TableCell>Progress</TableCell>
              <TableCell>Started</TableCell>
              <TableCell>Duration</TableCell>
              <TableCell>URLs Processed</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {activeScrapers.map((scraper) => (
              <TableRow key={scraper.id} hover>
                <TableCell>
                  <Box>
                    <Typography variant="subtitle2">{scraper.name}</Typography>
                    <Typography variant="caption" color="text.secondary">
                      {scraper.baseUrl}
                    </Typography>
                  </Box>
                </TableCell>
                <TableCell>
                  <Box sx={{ width: '100%', mr: 1 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                      <Box sx={{ width: '100%', mr: 1 }}>
                        <LinearProgress
                          variant={scraper.status.progress ? "determinate" : "indeterminate"}
                          value={scraper.status.progress || 0}
                        />
                      </Box>
                      <Box minWidth={35}>
                        <Typography variant="body2" color="text.secondary">
                          {scraper.status.progress ? `${Math.round(scraper.status.progress)}%` : ''}
                        </Typography>
                      </Box>
                    </Box>
                    <Typography variant="caption" color="text.secondary">
                      {scraper.status.currentUrl ? `Processing: ${scraper.status.currentUrl}` : 'Initializing...'}
                    </Typography>
                  </Box>
                </TableCell>
                <TableCell>
                  {scraper.status.startTime ? (
                    <Tooltip title={new Date(scraper.status.startTime).toLocaleString()}>
                      <Typography variant="body2">
                        {formatDistanceToNow(new Date(scraper.status.startTime), { addSuffix: true })}
                      </Typography>
                    </Tooltip>
                  ) : (
                    'Unknown'
                  )}
                </TableCell>
                <TableCell>
                  {formatDuration(scraper.status.startTime)}
                </TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center' }}>
                    <Typography variant="body2">
                      {scraper.status.urlsProcessed || 0}
                    </Typography>
                    {scraper.status.urlsQueued > 0 && (
                      <Typography variant="caption" color="text.secondary" sx={{ ml: 1 }}>
                        ({scraper.status.urlsQueued} queued)
                      </Typography>
                    )}
                  </Box>
                </TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    <Button
                      variant="outlined"
                      color="error"
                      size="small"
                      startIcon={actionInProgress === scraper.id ? <CircularProgress size={20} /> : <StopIcon />}
                      onClick={() => handleStopScraper(scraper.id)}
                      disabled={actionInProgress === scraper.id}
                    >
                      Stop
                    </Button>
                    <Tooltip title="View Details">
                      <IconButton
                        size="small"
                        component={RouterLink}
                        to={`/scrapers/${scraper.id}`}
                      >
                        <VisibilityIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </Box>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Performance Metrics */}
      <Typography variant="subtitle1" sx={{ mt: 4, mb: 2 }}>
        Real-time Performance Metrics
      </Typography>

      <Grid container spacing={3}>
        {activeScrapers.map((scraper) => (
          <Grid item xs={12} md={6} key={`metrics-${scraper.id}`}>
            <Card variant="outlined">
              <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                  <Typography variant="subtitle2">{scraper.name}</Typography>
                  <Chip
                    label="Active"
                    color="success"
                    size="small"
                  />
                </Box>

                <Grid container spacing={2}>
                  <Grid item xs={6}>
                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                      <SpeedIcon color="primary" sx={{ mr: 1 }} fontSize="small" />
                      <Box>
                        <Typography variant="caption" color="text.secondary">
                          Processing Speed
                        </Typography>
                        <Typography variant="body2">
                          {scraper.status.requestsPerMinute || 0} URLs/min
                        </Typography>
                      </Box>
                    </Box>
                  </Grid>

                  <Grid item xs={6}>
                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                      <TimelineIcon color="info" sx={{ mr: 1 }} fontSize="small" />
                      <Box>
                        <Typography variant="caption" color="text.secondary">
                          Avg. Response Time
                        </Typography>
                        <Typography variant="body2">
                          {scraper.status.avgResponseTime || 0} ms
                        </Typography>
                      </Box>
                    </Box>
                  </Grid>

                  <Grid item xs={6}>
                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                      <ScheduleIcon color="warning" sx={{ mr: 1 }} fontSize="small" />
                      <Box>
                        <Typography variant="caption" color="text.secondary">
                          Estimated Completion
                        </Typography>
                        <Typography variant="body2">
                          {scraper.status.estimatedCompletion
                            ? formatDistanceToNow(new Date(scraper.status.estimatedCompletion), { addSuffix: true })
                            : 'Unknown'}
                        </Typography>
                      </Box>
                    </Box>
                  </Grid>

                  <Grid item xs={6}>
                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                      <Box>
                        <Typography variant="caption" color="text.secondary">
                          Memory Usage
                        </Typography>
                        <Typography variant="body2">
                          {scraper.status.memoryUsage
                            ? `${(scraper.status.memoryUsage / (1024 * 1024)).toFixed(2)} MB`
                            : 'Unknown'}
                        </Typography>
                      </Box>
                    </Box>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
};

export default ActiveScrapersList;
