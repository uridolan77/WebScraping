import React, { useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import {
  Paper,
  Typography,
  Box,
  Button,
  Divider,
  CircularProgress
} from '@mui/material';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import StopIcon from '@mui/icons-material/Stop';
import SettingsIcon from '@mui/icons-material/Settings';
import FolderIcon from '@mui/icons-material/Folder';
import MonitorHeartIcon from '@mui/icons-material/MonitorHeart';

import MonitoringDialog from '../MonitoringDialog';

/**
 * Component for scraper actions (start, stop, configure, etc.)
 */
const ActionPanel = ({ 
  scraperId, 
  isRunning, 
  isLoading = false,
  onStart,
  onStop,
  onMonitoringUpdated
}) => {
  const [monitoringDialogOpen, setMonitoringDialogOpen] = useState(false);

  const handleMonitoringSave = () => {
    setMonitoringDialogOpen(false);
    if (onMonitoringUpdated) {
      onMonitoringUpdated();
    }
  };

  return (
    <>
      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          Actions
        </Typography>

        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          {!isRunning ? (
            <Button
              variant="contained"
              color="primary"
              startIcon={<PlayArrowIcon />}
              onClick={onStart}
              disabled={isLoading}
              fullWidth
            >
              {isLoading ? (
                <>
                  <CircularProgress size={24} color="inherit" sx={{ mr: 1 }} />
                  Starting...
                </>
              ) : (
                'Start Scraping'
              )}
            </Button>
          ) : (
            <Button
              variant="contained"
              color="error"
              startIcon={<StopIcon />}
              onClick={onStop}
              disabled={isLoading}
              fullWidth
            >
              {isLoading ? (
                <>
                  <CircularProgress size={24} color="inherit" sx={{ mr: 1 }} />
                  Stopping...
                </>
              ) : (
                'Stop Scraping'
              )}
            </Button>
          )}

          <Divider />

          <Button
            component={RouterLink}
            to={`/configure/${scraperId}`}
            variant="outlined"
            color="primary"
            startIcon={<SettingsIcon />}
            disabled={isRunning}
            fullWidth
          >
            Configure Scraper
          </Button>

          <Button
            component={RouterLink}
            to={`/results?scraperId=${scraperId}`}
            variant="outlined"
            startIcon={<FolderIcon />}
            fullWidth
          >
            View Results
          </Button>

          <Button
            variant="outlined"
            startIcon={<MonitorHeartIcon />}
            onClick={() => setMonitoringDialogOpen(true)}
            fullWidth
          >
            Configure Monitoring
          </Button>
        </Box>
      </Paper>

      {/* Monitoring Dialog */}
      <MonitoringDialog
        open={monitoringDialogOpen}
        onClose={() => setMonitoringDialogOpen(false)}
        onSave={handleMonitoringSave}
        scraperId={scraperId}
      />
    </>
  );
};

export default ActionPanel;