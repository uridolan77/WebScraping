import React from 'react';
import { 
  Typography, 
  Box, 
  Button, 
  Paper, 
  Grid, 
  Chip, 
  CircularProgress,
  IconButton, 
  Tooltip 
} from '@mui/material';
import {
  PlayArrow as PlayIcon,
  Stop as StopIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import { Link } from 'react-router-dom';
import StatusChip from './StatusChip';
import { formatDistanceToNow } from 'date-fns';

// Helper function for time ago display
const timeAgo = (dateString) => {
  if (!dateString) return 'Never';
  return formatDistanceToNow(new Date(dateString), { addSuffix: true });
};

const ScraperHeader = ({ 
  scraper, 
  status, 
  isStatusLoading, 
  isActionInProgress,
  onRefresh, 
  onStart, 
  onStop, 
  onDelete 
}) => {
  return (
    <Paper sx={{ p: 3, mb: 3 }}>
      <Grid container spacing={2} alignItems="center">
        <Grid item xs={12} md={8}>
          <Typography variant="h4" gutterBottom>
            {scraper?.name || 'Loading...'}
          </Typography>
          <Typography variant="subtitle1" color="textSecondary" gutterBottom>
            {scraper?.baseUrl || ''}
          </Typography>
          <Box sx={{ mt: 1, display: 'flex', gap: 1, flexWrap: 'wrap' }}>
            {isStatusLoading ? (
              <CircularProgress size={24} />
            ) : (
              <>
                <StatusChip status={status} />
                {status?.lastRun && (
                  <Chip
                    label={`Last run: ${timeAgo(status.lastRun)}`}
                    variant="outlined"
                    size="small"
                  />
                )}
                {status?.urlsProcessed > 0 && (
                  <Chip
                    label={`URLs processed: ${status.urlsProcessed}`}
                    variant="outlined"
                    size="small"
                  />
                )}
              </>
            )}
          </Box>
        </Grid>
        <Grid item xs={12} md={4}>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1, flexWrap: 'wrap' }}>
            <Tooltip title="Refresh">
              <IconButton
                onClick={onRefresh}
                disabled={isActionInProgress}
              >
                <RefreshIcon />
              </IconButton>
            </Tooltip>

            {status?.isRunning ? (
              <Tooltip title="Stop Scraper">
                <Button
                  variant="contained"
                  color="warning"
                  startIcon={<StopIcon />}
                  onClick={onStop}
                  disabled={isActionInProgress || isStatusLoading}
                >
                  Stop
                </Button>
              </Tooltip>
            ) : (
              <Tooltip title="Start Scraper">
                <Button
                  variant="contained"
                  color="success"
                  startIcon={<PlayIcon />}
                  onClick={onStart}
                  disabled={isActionInProgress || isStatusLoading}
                >
                  Start
                </Button>
              </Tooltip>
            )}

            <Tooltip title="Edit Scraper">
              <Button
                variant="outlined"
                color="primary"
                startIcon={<EditIcon />}
                component={Link}
                to={`/scrapers/${scraper?.id}/edit`}
                disabled={isActionInProgress}
              >
                Edit
              </Button>
            </Tooltip>

            <Tooltip title="Delete Scraper">
              <Button
                variant="outlined"
                color="error"
                startIcon={<DeleteIcon />}
                onClick={onDelete}
                disabled={isActionInProgress || status?.isRunning}
              >
                Delete
              </Button>
            </Tooltip>
          </Box>
        </Grid>
      </Grid>
    </Paper>
  );
};

export default ScraperHeader;