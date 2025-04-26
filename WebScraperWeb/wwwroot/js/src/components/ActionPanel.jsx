import React, { useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import {
  Paper,
  Typography,
  Button,
  Box,
  Divider,
  Alert,
  CircularProgress
} from '@mui/material';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import StopIcon from '@mui/icons-material/Stop';
import SettingsIcon from '@mui/icons-material/Settings';
import FolderIcon from '@mui/icons-material/Folder';
import MonitorHeartIcon from '@mui/icons-material/MonitorHeart';

import { startScraper, stopScraper } from '../services/api';
import MonitoringDialog from './MonitoringDialog';

/**
 * ActionPanel component that provides controls for a scraper
 * 
 * @param {Object} props - Component properties
 * @param {string} props.scraperId - ID of the scraper
 * @param {boolean} props.isRunning - Whether the scraper is currently running
 * @param {Function} props.onStatusChange - Callback when scraper status changes
 * @param {Object} props.monitoringSettings - Current monitoring settings
 */
const ActionPanel = ({ 
  scraperId, 
  isRunning, 
  onStatusChange,
  monitoringSettings = {}
}) => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [showMonitoringDialog, setShowMonitoringDialog] = useState(false);

  // Start the scraper
  const handleStart = async () => {
    setLoading(true);
    setError(null);
    
    try {
      await startScraper(scraperId);
      if (onStatusChange) onStatusChange();
    } catch (err) {
      setError(`Failed to start scraper: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };

  // Stop the scraper
  const handleStop = async () => {
    setLoading(true);
    setError(null);
    
    try {
      await stopScraper(scraperId);
      if (onStatusChange) onStatusChange();
    } catch (err) {
      setError(`Failed to stop scraper: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };

  // Handle monitoring dialog save
  const handleMonitoringSave = () => {
    setShowMonitoringDialog(false);
    if (onStatusChange) onStatusChange();
  };

  return (
    <Paper sx={{ p: 2 }}>
      <Typography variant="h6" gutterBottom>
        Actions
      </Typography>
      
      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}
      
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        {isRunning ? (
          <Button
            variant="contained"
            color="error"
            startIcon={<StopIcon />}
            onClick={handleStop}
            disabled={loading}
            fullWidth
          >
            {loading ? (
              <CircularProgress size={24} color="inherit" />
            ) : (
              'Stop Scraping'
            )}
          </Button>
        ) : (
          <Button
            variant="contained"
            color="primary"
            startIcon={<PlayArrowIcon />}
            onClick={handleStart}
            disabled={loading}
            fullWidth
          >
            {loading ? (
              <CircularProgress size={24} color="inherit" />
            ) : (
              'Start Scraping'
            )}
          </Button>
        )}

        <Button
          component={RouterLink}
          to={`/configure/${scraperId}`}
          variant="outlined"
          startIcon={<SettingsIcon />}
          disabled={isRunning}
          fullWidth
        >
          Configure
        </Button>
        
        <Divider sx={{ my: 1 }} />
        
        <Button
          variant="outlined"
          color="secondary"
          startIcon={<MonitorHeartIcon />}
          onClick={() => setShowMonitoringDialog(true)}
          fullWidth
        >
          {monitoringSettings?.enabled ? 'Edit Monitoring' : 'Setup Monitoring'}
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
      </Box>
      
      {/* Monitoring Dialog */}
      <MonitoringDialog
        open={showMonitoringDialog}
        onClose={() => setShowMonitoringDialog(false)}
        onSave={handleMonitoringSave}
        scraperId={scraperId}
        initialValues={monitoringSettings}
      />
    </Paper>
  );
};

export default ActionPanel;