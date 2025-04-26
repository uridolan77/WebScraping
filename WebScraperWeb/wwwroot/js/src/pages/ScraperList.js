import React, { useState, useEffect } from 'react';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import {
  Typography,
  Paper,
  Grid,
  Button,
  Box,
  Card,
  CardContent,
  CardActions,
  IconButton,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  TextField,
  Stack,
  Alert,
  LinearProgress,
  Divider,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Tooltip
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import StopIcon from '@mui/icons-material/Stop';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import MonitorHeartIcon from '@mui/icons-material/MonitorHeart';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import TaskAltIcon from '@mui/icons-material/TaskAlt';
import LinkIcon from '@mui/icons-material/Link';
import VisibilityIcon from '@mui/icons-material/Visibility';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import RefreshIcon from '@mui/icons-material/Refresh';

import { 
  fetchAllScrapers, 
  createScraper, 
  deleteScraper, 
  startScraper, 
  stopScraper, 
  fetchScraperStatus,
  setMonitoring
} from '../services/api';

const ScraperList = () => {
  const navigate = useNavigate();
  const [scrapers, setScrapers] = useState([]);
  const [scraperStatuses, setScraperStatuses] = useState({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [newScraperOpen, setNewScraperOpen] = useState(false);
  const [newScraperName, setNewScraperName] = useState('');
  const [newScraperUrl, setNewScraperUrl] = useState('');
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [scraperToDelete, setScraperToDelete] = useState(null);
  const [monitoringDialogOpen, setMonitoringDialogOpen] = useState(false);
  const [scraperToMonitor, setScraperToMonitor] = useState(null);
  const [monitoringSettings, setMonitoringSettings] = useState({
    enabled: false,
    intervalMinutes: 1440, // 24 hours
    notifyOnChanges: false,
    notificationEmail: '',
    trackChangesHistory: true
  });
  const [anchorEl, setAnchorEl] = useState(null);
  const [menuScraperId, setMenuScraperId] = useState(null);
  
  // Fetch all scrapers and their status
  const fetchData = async () => {
    setLoading(true);
    setError(null);
    
    try {
      const scrapersData = await fetchAllScrapers();
      setScrapers(scrapersData);
      
      // Fetch status for each scraper
      const statuses = {};
      for (const scraper of scrapersData) {
        try {
          const status = await fetchScraperStatus(scraper.id);
          statuses[scraper.id] = status;
        } catch (err) {
          console.error(`Failed to fetch status for scraper ${scraper.id}:`, err);
        }
      }
      
      setScraperStatuses(statuses);
    } catch (err) {
      setError('Failed to load scrapers: ' + err.message);
    } finally {
      setLoading(false);
    }
  };
  
  useEffect(() => {
    fetchData();
    
    // Set up polling for status updates
    const interval = setInterval(() => {
      updateScraperStatuses();
    }, 3000);
    
    return () => clearInterval(interval);
  }, []);
  
  // Update status of all scrapers
  const updateScraperStatuses = async () => {
    try {
      const updatedStatuses = { ...scraperStatuses };
      let hasChanges = false;
      
      for (const scraper of scrapers) {
        try {
          const status = await fetchScraperStatus(scraper.id);
          if (JSON.stringify(status) !== JSON.stringify(updatedStatuses[scraper.id])) {
            updatedStatuses[scraper.id] = status;
            hasChanges = true;
          }
        } catch (err) {
          console.error(`Failed to update status for scraper ${scraper.id}:`, err);
        }
      }
      
      if (hasChanges) {
        setScraperStatuses(updatedStatuses);
      }
    } catch (err) {
      console.error('Failed to update scraper statuses:', err);
    }
  };
  
  // Open menu for a scraper
  const handleMenuOpen = (event, scraperId) => {
    setAnchorEl(event.currentTarget);
    setMenuScraperId(scraperId);
  };
  
  // Close menu
  const handleMenuClose = () => {
    setAnchorEl(null);
    setMenuScraperId(null);
  };
  
  // Handle creating a new scraper
  const handleCreateScraper = async () => {
    if (!newScraperUrl) {
      return;
    }
    
    try {
      // Parse the URL to get base URL
      let baseUrl;
      try {
        const url = new URL(newScraperUrl);
        baseUrl = `${url.protocol}//${url.hostname}`;
      } catch (err) {
        baseUrl = newScraperUrl;
      }
      
      const newScraper = {
        name: newScraperName || 'New Scraper',
        startUrl: newScraperUrl,
        baseUrl: baseUrl
      };
      
      await createScraper(newScraper);
      setNewScraperOpen(false);
      setNewScraperName('');
      setNewScraperUrl('');
      
      // Refresh the list
      fetchData();
    } catch (err) {
      setError('Failed to create scraper: ' + err.message);
    }
  };
  
  // Handle starting a scraper
  const handleStartScraper = async (id) => {
    try {
      await startScraper(id);
      
      // Update status
      const status = await fetchScraperStatus(id);
      setScraperStatuses(prev => ({
        ...prev,
        [id]: status
      }));
    } catch (err) {
      setError('Failed to start scraper: ' + err.message);
    }
  };
  
  // Handle stopping a scraper
  const handleStopScraper = async (id) => {
    try {
      await stopScraper(id);
      
      // Update status
      const status = await fetchScraperStatus(id);
      setScraperStatuses(prev => ({
        ...prev,
        [id]: status
      }));
    } catch (err) {
      setError('Failed to stop scraper: ' + err.message);
    }
  };
  
  // Handle deleting a scraper
  const handleDeleteClick = (scraper) => {
    setScraperToDelete(scraper);
    setDeleteConfirmOpen(true);
  };
  
  const handleDeleteConfirm = async () => {
    if (!scraperToDelete) return;
    
    try {
      await deleteScraper(scraperToDelete.id);
      setDeleteConfirmOpen(false);
      setScraperToDelete(null);
      
      // Refresh the list
      fetchData();
    } catch (err) {
      setError('Failed to delete scraper: ' + err.message);
    }
  };
  
  // Handle opening monitoring settings
  const handleMonitoringClick = (scraper) => {
    const status = scraperStatuses[scraper.id];
    
    setScraperToMonitor(scraper);
    setMonitoringSettings({
      enabled: status?.isMonitoring || false,
      intervalMinutes: status?.monitoringInterval || 1440,
      notifyOnChanges: scraper.notifyOnChanges || false,
      notificationEmail: scraper.notificationEmail || '',
      trackChangesHistory: scraper.trackChangesHistory !== false
    });
    setMonitoringDialogOpen(true);
  };
  
  // Handle saving monitoring settings
  const handleMonitoringSave = async () => {
    if (!scraperToMonitor) return;
    
    try {
      await setMonitoring(scraperToMonitor.id, monitoringSettings);
      setMonitoringDialogOpen(false);
      setScraperToMonitor(null);
      
      // Refresh the data
      fetchData();
    } catch (err) {
      setError('Failed to update monitoring settings: ' + err.message);
    }
  };
  
  // Format date
  const formatDate = (dateString) => {
    if (!dateString) return 'Never';
    return new Date(dateString).toLocaleString();
  };
  
  // Duplicate a scraper (creates a new scraper with the same settings)
  const handleDuplicate = async (scraper) => {
    try {
      const newScraper = { ...scraper };
      delete newScraper.id;
      delete newScraper.createdAt;
      delete newScraper.lastRun;
      
      newScraper.name = `${scraper.name} (Copy)`;
      
      await createScraper(newScraper);
      handleMenuClose();
      fetchData();
    } catch (err) {
      setError('Failed to duplicate scraper: ' + err.message);
    }
  };
  
  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">
          My Scrapers
        </Typography>
        
        <Button
          variant="contained"
          color="primary"
          startIcon={<AddIcon />}
          onClick={() => setNewScraperOpen(true)}
        >
          New Scraper
        </Button>
      </Box>
      
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}
      
      {loading && <LinearProgress sx={{ mb: 3 }} />}
      
      {/* No scrapers message */}
      {!loading && scrapers.length === 0 && (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Typography variant="h6" gutterBottom>
            No scrapers configured yet
          </Typography>
          <Typography variant="body1" color="textSecondary" paragraph>
            Create your first scraper to get started
          </Typography>
          <Button
            variant="contained"
            color="primary"
            startIcon={<AddIcon />}
            onClick={() => setNewScraperOpen(true)}
          >
            Create Scraper
          </Button>
        </Paper>
      )}
      
      {/* Scraper grid */}
      <Grid container spacing={3}>
        {scrapers.map(scraper => {
          const status = scraperStatuses[scraper.id] || {};
          const isRunning = status.isRunning || false;
          
          return (
            <Grid item xs={12} md={6} lg={4} key={scraper.id}>
              <Card>
                <CardContent>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                    <Typography variant="h6" noWrap>
                      {scraper.name}
                    </Typography>
                    <IconButton
                      size="small"
                      onClick={(e) => handleMenuOpen(e, scraper.id)}
                    >
                      <MoreVertIcon />
                    </IconButton>
                  </Box>
                  
                  {/* Status indicator */}
                  <Box sx={{ mb: 2, display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                    <Chip
                      size="small"
                      label={isRunning ? "Running" : "Idle"}
                      color={isRunning ? "success" : "default"}
                    />
                    
                    {status.isMonitoring && (
                      <Chip
                        size="small"
                        icon={<MonitorHeartIcon />}
                        label="Monitoring"
                        color="primary"
                      />
                    )}
                  </Box>
                  
                  <Box sx={{ mb: 1 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <LinkIcon fontSize="small" />
                      <Typography variant="body2" noWrap>
                        {scraper.startUrl}
                      </Typography>
                    </Box>
                  </Box>
                  
                  <Divider sx={{ my: 1 }} />
                  
                  <Stack spacing={1} sx={{ mt: 2 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="textSecondary">
                        Last run:
                      </Typography>
                      <Typography variant="body2">
                        {formatDate(scraper.lastRun)}
                      </Typography>
                    </Box>
                    
                    {status.lastMonitorCheck && (
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant="body2" color="textSecondary">
                          Last check:
                        </Typography>
                        <Typography variant="body2">
                          {formatDate(status.lastMonitorCheck)}
                        </Typography>
                      </Box>
                    )}
                    
                    {status.urlsProcessed > 0 && (
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant="body2" color="textSecondary">
                          URLs processed:
                        </Typography>
                        <Typography variant="body2">
                          {status.urlsProcessed}
                        </Typography>
                      </Box>
                    )}
                    
                    {status.isMonitoring && (
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant="body2" color="textSecondary">
                          Check interval:
                        </Typography>
                        <Typography variant="body2">
                          {status.monitoringInterval < 60 
                            ? `${status.monitoringInterval} minutes` 
                            : `${(status.monitoringInterval / 60).toFixed(1)} hours`}
                        </Typography>
                      </Box>
                    )}
                  </Stack>
                </CardContent>
                
                <CardActions sx={{ justifyContent: 'space-between', flexWrap: 'wrap' }}>
                  <Box>
                    <Tooltip title="View Dashboard">
                      <IconButton
                        size="small"
                        color="primary"
                        onClick={() => navigate(`/dashboard/${scraper.id}`)}
                      >
                        <VisibilityIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    
                    <Tooltip title="Configure">
                      <IconButton
                        size="small"
                        color="primary"
                        onClick={() => navigate(`/configure/${scraper.id}`)}
                        disabled={isRunning}
                      >
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    
                    <Tooltip title="Monitoring Settings">
                      <IconButton
                        size="small"
                        color={status.isMonitoring ? "secondary" : "default"}
                        onClick={() => handleMonitoringClick(scraper)}
                      >
                        <MonitorHeartIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </Box>
                  
                  <Box>
                    {isRunning ? (
                      <Button
                        size="small"
                        color="error"
                        startIcon={<StopIcon />}
                        onClick={() => handleStopScraper(scraper.id)}
                      >
                        Stop
                      </Button>
                    ) : (
                      <Button
                        size="small"
                        color="primary"
                        startIcon={<PlayArrowIcon />}
                        onClick={() => handleStartScraper(scraper.id)}
                      >
                        Start
                      </Button>
                    )}
                  </Box>
                </CardActions>
              </Card>
            </Grid>
          );
        })}
      </Grid>
      
      {/* Menu for scraper actions */}
      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleMenuClose}
      >
        <MenuItem onClick={() => {
          handleMenuClose();
          const scraper = scrapers.find(s => s.id === menuScraperId);
          if (scraper) {
            navigate(`/dashboard/${scraper.id}`);
          }
        }}>
          <ListItemIcon>
            <VisibilityIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>View Dashboard</ListItemText>
        </MenuItem>
        
        <MenuItem onClick={() => {
          handleMenuClose();
          const scraper = scrapers.find(s => s.id === menuScraperId);
          if (scraper) {
            navigate(`/configure/${scraper.id}`);
          }
        }}>
          <ListItemIcon>
            <EditIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Edit Configuration</ListItemText>
        </MenuItem>
        
        <MenuItem onClick={() => {
          handleMenuClose();
          const scraper = scrapers.find(s => s.id === menuScraperId);
          if (scraper) {
            navigate(`/results?scraperId=${scraper.id}`);
          }
        }}>
          <ListItemIcon>
            <TaskAltIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>View Results</ListItemText>
        </MenuItem>
        
        <Divider />
        
        <MenuItem onClick={() => {
          const scraper = scrapers.find(s => s.id === menuScraperId);
          if (scraper) {
            handleDuplicate(scraper);
          }
        }}>
          <ListItemIcon>
            <ContentCopyIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Duplicate</ListItemText>
        </MenuItem>
        
        <MenuItem onClick={() => {
          handleMenuClose();
          const scraper = scrapers.find(s => s.id === menuScraperId);
          if (scraper) {
            handleMonitoringClick(scraper);
          }
        }}>
          <ListItemIcon>
            <MonitorHeartIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Monitoring Settings</ListItemText>
        </MenuItem>
        
        <Divider />
        
        <MenuItem onClick={() => {
          handleMenuClose();
          const scraper = scrapers.find(s => s.id === menuScraperId);
          if (scraper) {
            handleDeleteClick(scraper);
          }
        }} sx={{ color: 'error.main' }}>
          <ListItemIcon sx={{ color: 'error.main' }}>
            <DeleteIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Delete</ListItemText>
        </MenuItem>
      </Menu>
      
      {/* New Scraper Dialog */}
      <Dialog open={newScraperOpen} onClose={() => setNewScraperOpen(false)}>
        <DialogTitle>Create New Scraper</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Enter a name and starting URL for your new scraper.
          </DialogContentText>
          <TextField
            autoFocus
            margin="dense"
            label="Scraper Name"
            fullWidth
            variant="outlined"
            value={newScraperName}
            onChange={(e) => setNewScraperName(e.target.value)}
            sx={{ mb: 2 }}
          />
          <TextField
            margin="dense"
            label="Start URL"
            fullWidth
            variant="outlined"
            value={newScraperUrl}
            onChange={(e) => setNewScraperUrl(e.target.value)}
            placeholder="https://example.com"
            required
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setNewScraperOpen(false)}>Cancel</Button>
          <Button onClick={handleCreateScraper} variant="contained" color="primary">
            Create
          </Button>
        </DialogActions>
      </Dialog>
      
      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteConfirmOpen} onClose={() => setDeleteConfirmOpen(false)}>
        <DialogTitle>Delete Scraper</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete the scraper "{scraperToDelete?.name}"? This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteConfirmOpen(false)}>Cancel</Button>
          <Button onClick={handleDeleteConfirm} color="error" variant="contained">
            Delete
          </Button>
        </DialogActions>
      </Dialog>
      
      {/* Monitoring Settings Dialog */}
      <Dialog open={monitoringDialogOpen} onClose={() => setMonitoringDialogOpen(false)}>
        <DialogTitle>Monitoring Settings</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Configure continuous monitoring to automatically check for changes on this website.
          </DialogContentText>
          
          <Box sx={{ mt: 2 }}>
            <Typography variant="subtitle2" gutterBottom>Enable Monitoring</Typography>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <Chip
                label={monitoringSettings.enabled ? "Enabled" : "Disabled"}
                color={monitoringSettings.enabled ? "success" : "default"}
                onClick={() => setMonitoringSettings(prev => ({
                  ...prev,
                  enabled: !prev.enabled
                }))}
                sx={{ mr: 1 }}
              />
              <Typography variant="body2" color="textSecondary">
                {monitoringSettings.enabled ? 
                  "Scraper will periodically check for changes" : 
                  "No automatic checking for changes"}
              </Typography>
            </Box>
          </Box>
          
          <Box sx={{ mt: 3 }}>
            <Typography variant="subtitle2" gutterBottom>Check Interval</Typography>
            <Grid container spacing={2} alignItems="center">
              <Grid item xs={8}>
                <TextField
                  margin="dense"
                  label="Interval (minutes)"
                  fullWidth
                  variant="outlined"
                  type="number"
                  value={monitoringSettings.intervalMinutes}
                  onChange={(e) => setMonitoringSettings(prev => ({
                    ...prev,
                    intervalMinutes: Math.max(1, parseInt(e.target.value) || 1440)
                  }))}
                  disabled={!monitoringSettings.enabled}
                  InputProps={{ inputProps: { min: 1 } }}
                />
              </Grid>
              <Grid item xs={4}>
                <Typography variant="body2" color="textSecondary">
                  {monitoringSettings.intervalMinutes >= 60 
                    ? `${(monitoringSettings.intervalMinutes / 60).toFixed(1)} hours` 
                    : `${monitoringSettings.intervalMinutes} minutes`}
                </Typography>
              </Grid>
            </Grid>
            
            <Box sx={{ mt: 1 }}>
              <Button
                size="small"
                onClick={() => setMonitoringSettings(prev => ({ ...prev, intervalMinutes: 60 }))}
                sx={{ mr: 1 }}
                disabled={!monitoringSettings.enabled}
              >
                1 hour
              </Button>
              <Button
                size="small"
                onClick={() => setMonitoringSettings(prev => ({ ...prev, intervalMinutes: 1440 }))}
                sx={{ mr: 1 }}
                disabled={!monitoringSettings.enabled}
              >
                24 hours
              </Button>
              <Button
                size="small"
                onClick={() => setMonitoringSettings(prev => ({ ...prev, intervalMinutes: 10080 }))}
                disabled={!monitoringSettings.enabled}
              >
                1 week
              </Button>
            </Box>
          </Box>
          
          <Box sx={{ mt: 3 }}>
            <Typography variant="subtitle2" gutterBottom>Notification Settings</Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <Chip
                label={monitoringSettings.notifyOnChanges ? "Enabled" : "Disabled"}
                color={monitoringSettings.notifyOnChanges ? "primary" : "default"}
                onClick={() => setMonitoringSettings(prev => ({
                  ...prev,
                  notifyOnChanges: !prev.notifyOnChanges
                }))}
                sx={{ mr: 1 }}
                disabled={!monitoringSettings.enabled}
              />
              <Typography variant="body2" color="textSecondary">
                Email notifications when changes are detected
              </Typography>
            </Box>
            
            <TextField
              margin="dense"
              label="Notification Email"
              fullWidth
              variant="outlined"
              value={monitoringSettings.notificationEmail}
              onChange={(e) => setMonitoringSettings(prev => ({
                ...prev,
                notificationEmail: e.target.value
              }))}
              disabled={!monitoringSettings.enabled || !monitoringSettings.notifyOnChanges}
            />
          </Box>
          
          <Box sx={{ mt: 3 }}>
            <Typography variant="subtitle2" gutterBottom>History Tracking</Typography>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <Chip
                label={monitoringSettings.trackChangesHistory ? "Enabled" : "Disabled"}
                color={monitoringSettings.trackChangesHistory ? "primary" : "default"}
                onClick={() => setMonitoringSettings(prev => ({
                  ...prev,
                  trackChangesHistory: !prev.trackChangesHistory
                }))}
                sx={{ mr: 1 }}
                disabled={!monitoringSettings.enabled}
              />
              <Typography variant="body2" color="textSecondary">
                Keep track of all content changes
              </Typography>
            </Box>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setMonitoringDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleMonitoringSave} variant="contained" color="primary">
            Save Settings
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ScraperList;