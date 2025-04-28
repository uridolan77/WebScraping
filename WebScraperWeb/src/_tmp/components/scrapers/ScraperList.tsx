// src/components/scrapers/ScraperList.tsx
import React, { useState, useEffect } from 'react';
import {
  Container, Typography, Box, Button, TextField, InputAdornment,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Paper, Chip, IconButton, Dialog, DialogTitle, DialogContent,
  DialogContentText, DialogActions, CircularProgress, Tooltip,
  Card, CardContent, Grid, MenuItem, Select, FormControl, InputLabel,
  Alert, Snackbar, SelectChangeEvent
} from '@mui/material';
import {
  Add as AddIcon,
  Search as SearchIcon,
  PlayArrow as PlayIcon,
  Stop as StopIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
  Refresh as RefreshIcon,
  Error as ErrorIcon
} from '@mui/icons-material';
import { Link, useNavigate } from 'react-router-dom';
import { useScraperContext } from '../../contexts/ScraperContext';
import { getUserFriendlyErrorMessage } from '../../utils/errorHandler';
import { Scraper, ScraperStatus } from '../../types';

interface NotificationState {
  open: boolean;
  message: string;
  severity: 'success' | 'info' | 'warning' | 'error';
}

const ScraperList: React.FC = () => {
  const navigate = useNavigate();
  const {
    scrapers,
    loading: isLoading,
    error,
    fetchScrapers,
    start: startScraper,
    stop: stopScraper,
    deleteScraper: removeScraper,
    refreshAll
  } = useScraperContext();

  const [filteredScrapers, setFilteredScrapers] = useState<Scraper[]>([]);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [deleteDialogOpen, setDeleteDialogOpen] = useState<boolean>(false);
  const [scraperToDelete, setScraperToDelete] = useState<Scraper | null>(null);
  const [actionInProgress, setActionInProgress] = useState<string | null>(null);
  const [notification, setNotification] = useState<NotificationState>({ 
    open: false, 
    message: '', 
    severity: 'info' 
  });

  // Show notification
  const showNotification = (message: string, severity: 'success' | 'info' | 'warning' | 'error' = 'info') => {
    setNotification({
      open: true,
      message,
      severity
    });
  };

  // Handle notification close
  const handleNotificationClose = () => {
    setNotification(prev => ({ ...prev, open: false }));
  };

  // Initial load
  useEffect(() => {
    fetchScrapers();

    // Auto-refresh every 30 seconds
    const interval = setInterval(refreshAll, 30000);
    return () => clearInterval(interval);
  }, [fetchScrapers, refreshAll]);

  // Show error notification if there's an error
  useEffect(() => {
    if (error) {
      showNotification(getUserFriendlyErrorMessage(error), 'error');
    }
  }, [error]);

  // Filter scrapers based on search term and status filter
  useEffect(() => {
    let filtered = [...scrapers];

    // Apply search term filter
    if (searchTerm) {
      const term = searchTerm.toLowerCase();
      filtered = filtered.filter(
        scraper =>
          scraper.name.toLowerCase().includes(term) ||
          (scraper.baseUrl && scraper.baseUrl.toLowerCase().includes(term)) ||
          scraper.id.toLowerCase().includes(term)
      );
    }

    // Apply status filter
    if (statusFilter !== 'all') {
      if (statusFilter === 'running') {
        filtered = filtered.filter(scraper => scraper.status?.isRunning);
      } else if (statusFilter === 'idle') {
        filtered = filtered.filter(scraper => !scraper.status?.isRunning && !scraper.status?.hasErrors);
      } else if (statusFilter === 'error') {
        filtered = filtered.filter(scraper => scraper.status?.hasErrors);
      }
    }

    setFilteredScrapers(filtered);
  }, [scrapers, searchTerm, statusFilter]);

  // Handle scraper actions
  const handleStartScraper = async (id: string) => {
    try {
      setActionInProgress(id);
      await startScraper(id);
      showNotification('Scraper started successfully', 'success');
    } catch (error) {
      showNotification(getUserFriendlyErrorMessage(error, `Failed to start scraper with ID ${id}`), 'error');
    } finally {
      setActionInProgress(null);
    }
  };

  const handleStopScraper = async (id: string) => {
    try {
      setActionInProgress(id);
      await stopScraper(id);
      showNotification('Scraper stopped successfully', 'success');
    } catch (error) {
      showNotification(getUserFriendlyErrorMessage(error, `Failed to stop scraper with ID ${id}`), 'error');
    } finally {
      setActionInProgress(null);
    }
  };

  const handleOpenDeleteDialog = (scraper: Scraper) => {
    setScraperToDelete(scraper);
    setDeleteDialogOpen(true);
  };

  const handleDeleteScraper = async () => {
    if (!scraperToDelete) return;

    try {
      setActionInProgress(scraperToDelete.id);
      await removeScraper(scraperToDelete.id);
      setDeleteDialogOpen(false);
      setScraperToDelete(null);
      showNotification(`Scraper "${scraperToDelete.name}" deleted successfully`, 'success');
    } catch (error) {
      showNotification(getUserFriendlyErrorMessage(error, `Failed to delete scraper "${scraperToDelete.name}"`), 'error');
    } finally {
      setActionInProgress(null);
    }
  };

  const handleStatusFilterChange = (event: SelectChangeEvent) => {
    setStatusFilter(event.target.value);
  };

  // Render status chip with appropriate color
  const renderStatusChip = (scraper: Scraper) => {
    const status = scraper.status;

    if (!status) {
      return <Chip label="Unknown" color="default" size="small" />;
    }

    if (status.isRunning) {
      return <Chip label="Running" color="success" size="small" />;
    } else if (status.hasErrors) {
      return (
        <Tooltip title={status.errorMessage || 'An error occurred'}>
          <Chip
            label="Error"
            color="error"
            size="small"
            icon={<ErrorIcon fontSize="small" />}
          />
        </Tooltip>
      );
    } else {
      return <Chip label="Idle" color="default" size="small" />;
    }
  };

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">Scrapers</Typography>
        <Button
          variant="contained"
          color="primary"
          startIcon={<AddIcon />}
          component={Link}
          to="/scrapers/create"
        >
          New Scraper
        </Button>
      </Box>

      {/* Filters */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder="Search scrapers..."
                variant="outlined"
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <SearchIcon />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid item xs={12} md={3}>
              <FormControl fullWidth variant="outlined">
                <InputLabel id="status-filter-label">Status</InputLabel>
                <Select
                  labelId="status-filter-label"
                  value={statusFilter}
                  onChange={handleStatusFilterChange}
                  label="Status"
                >
                  <MenuItem value="all">All Status</MenuItem>
                  <MenuItem value="running">Running</MenuItem>
                  <MenuItem value="idle">Idle</MenuItem>
                  <MenuItem value="error">Error</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={3}>
              <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
                <Button
                  variant="outlined"
                  startIcon={<RefreshIcon />}
                  onClick={refreshAll}
                  disabled={isLoading}
                >
                  Refresh
                </Button>
              </Box>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Scrapers Table */}
      {isLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Base URL</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Last Run</TableCell>
                <TableCell>URLs Processed</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredScrapers.length > 0 ? (
                filteredScrapers.map((scraper) => (
                  <TableRow key={scraper.id}>
                    <TableCell>
                      <Link
                        to={`/scrapers/${scraper.id}`}
                        style={{ textDecoration: 'none', color: 'inherit', fontWeight: 'bold' }}
                      >
                        {scraper.name}
                      </Link>
                    </TableCell>
                    <TableCell>{scraper.baseUrl}</TableCell>
                    <TableCell>{renderStatusChip(scraper)}</TableCell>
                    <TableCell>
                      {scraper.lastRun
                        ? new Date(scraper.lastRun).toLocaleString()
                        : 'Never'}
                    </TableCell>
                    <TableCell>
                      {scraper.status?.urlsProcessed || scraper.urlsProcessed || 0}
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', gap: 1 }}>
                        {scraper.status?.isRunning ? (
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
                        <Tooltip title="Edit Scraper">
                          <IconButton
                            color="primary"
                            onClick={() => navigate(`/scrapers/${scraper.id}/edit`)}
                            size="small"
                          >
                            <EditIcon />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete Scraper">
                          <IconButton
                            color="error"
                            onClick={() => handleOpenDeleteDialog(scraper)}
                            disabled={actionInProgress === scraper.id || scraper.status?.isRunning}
                            size="small"
                          >
                            <DeleteIcon />
                          </IconButton>
                        </Tooltip>
                      </Box>
                    </TableCell>
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell colSpan={6} align="center" sx={{ py: 3 }}>
                    {searchTerm || statusFilter !== 'all' ? (
                      <>
                        <Typography variant="body1" gutterBottom>
                          No scrapers found matching your filters.
                        </Typography>
                        <Button
                          variant="text"
                          onClick={() => {
                            setSearchTerm('');
                            setStatusFilter('all');
                          }}
                        >
                          Clear Filters
                        </Button>
                      </>
                    ) : (
                      <>
                        <Typography variant="body1" gutterBottom>
                          No scrapers found. Create your first scraper to get started.
                        </Typography>
                        <Button
                          variant="contained"
                          startIcon={<AddIcon />}
                          component={Link}
                          to="/scrapers/create"
                          sx={{ mt: 1 }}
                        >
                          New Scraper
                        </Button>
                      </>
                    )}
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {/* Delete Confirmation Dialog */}
      <Dialog
        open={deleteDialogOpen}
        onClose={() => setDeleteDialogOpen(false)}
      >
        <DialogTitle>Delete Scraper</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete the scraper "{scraperToDelete?.name}"?
            This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button
            onClick={handleDeleteScraper}
            color="error"
            variant="contained"
            disabled={actionInProgress === scraperToDelete?.id}
          >
            {actionInProgress === scraperToDelete?.id ? (
              <CircularProgress size={24} color="inherit" />
            ) : (
              'Delete'
            )}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Notification Snackbar */}
      <Snackbar
        open={notification.open}
        autoHideDuration={6000}
        onClose={handleNotificationClose}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert
          onClose={handleNotificationClose}
          severity={notification.severity}
          variant="filled"
          sx={{ width: '100%' }}
        >
          {notification.message}
        </Alert>
      </Snackbar>
    </Container>
  );
};

export default ScraperList;
