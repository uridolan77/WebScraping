import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import {
  Container,
  Typography,
  Box,
  Button,
  TextField,
  InputAdornment,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  CircularProgress,
  Tooltip,
  Card,
  CardContent,
  Grid
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
import { useScrapers } from '../hooks';

const ScraperList = () => {
  const navigate = useNavigate();
  const { 
    getScrapers, 
    startScraper, 
    stopScraper, 
    deleteScraper,
    getScraperStatus,
    loading, 
    error 
  } = useScrapers();
  
  const [scrapers, setScrapers] = useState([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [scraperStatus, setScraperStatus] = useState({});
  const [actionInProgress, setActionInProgress] = useState(null);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [scraperToDelete, setScraperToDelete] = useState(null);

  // Fetch all scrapers
  const fetchScrapers = async () => {
    try {
      const data = await getScrapers();
      setScrapers(data);
      // Get status for each scraper
      data.forEach(scraper => {
        fetchScraperStatus(scraper.id);
      });
    } catch (error) {
      console.error('Error fetching scrapers:', error);
    }
  };

  // Fetch status for a single scraper
  const fetchScraperStatus = async (id) => {
    try {
      const status = await getScraperStatus(id);
      setScraperStatus(prev => ({
        ...prev,
        [id]: status
      }));
    } catch (error) {
      console.error(`Error fetching status for scraper ${id}:`, error);
    }
  };

  // Initial load
  useEffect(() => {
    fetchScrapers();
    
    // Refresh status every 10 seconds
    const interval = setInterval(() => {
      scrapers.forEach(scraper => {
        if (!actionInProgress || actionInProgress !== scraper.id) {
          fetchScraperStatus(scraper.id);
        }
      });
    }, 10000);
    
    return () => clearInterval(interval);
  }, [scrapers.length]); // Only re-run when the number of scrapers changes

  // Filter scrapers based on search term
  const filteredScrapers = scrapers.filter(scraper => 
    scraper.name.toLowerCase().includes(searchTerm.toLowerCase()) || 
    scraper.baseUrl.toLowerCase().includes(searchTerm.toLowerCase())
  );

  // Handle start scraper
  const handleStartScraper = async (id) => {
    try {
      setActionInProgress(id);
      await startScraper(id);
      await fetchScraperStatus(id);
    } catch (error) {
      console.error(`Error starting scraper ${id}:`, error);
    } finally {
      setActionInProgress(null);
    }
  };

  // Handle stop scraper
  const handleStopScraper = async (id) => {
    try {
      setActionInProgress(id);
      await stopScraper(id);
      await fetchScraperStatus(id);
    } catch (error) {
      console.error(`Error stopping scraper ${id}:`, error);
    } finally {
      setActionInProgress(null);
    }
  };

  // Handle delete scraper
  const handleDeleteClick = (scraper) => {
    setScraperToDelete(scraper);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!scraperToDelete) return;
    
    try {
      setActionInProgress(scraperToDelete.id);
      await deleteScraper(scraperToDelete.id);
      setDeleteDialogOpen(false);
      setScraperToDelete(null);
      // Refresh the list
      fetchScrapers();
    } catch (error) {
      console.error(`Error deleting scraper ${scraperToDelete.id}:`, error);
    } finally {
      setActionInProgress(null);
    }
  };

  // Render status chip
  const renderStatusChip = (id) => {
    const status = scraperStatus[id];
    
    if (!status) {
      return <Chip label="Unknown" color="default" size="small" />;
    }
    
    if (status.isRunning) {
      return (
        <Tooltip title={`Processing: ${status.urlsProcessed || 0} URLs`}>
          <Chip
            label="Running"
            color="success"
            size="small"
          />
        </Tooltip>
      );
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

  // Format date
  const formatDate = (dateString) => {
    if (!dateString) return 'Never';
    
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).format(date);
  };

  if (loading && scrapers.length === 0) {
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
            <Grid item xs={12} md={9}>
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
              <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
                <Button
                  variant="outlined"
                  startIcon={<RefreshIcon />}
                  onClick={fetchScrapers}
                  disabled={loading}
                >
                  Refresh
                </Button>
              </Box>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Scrapers Table */}
      {loading && scrapers.length > 0 ? (
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
                        style={{ textDecoration: 'none', color: 'inherit', fontWeight: 500 }}
                      >
                        {scraper.name}
                      </Link>
                    </TableCell>
                    <TableCell>{scraper.baseUrl}</TableCell>
                    <TableCell>{renderStatusChip(scraper.id)}</TableCell>
                    <TableCell>
                      {formatDate(scraperStatus[scraper.id]?.lastRun || scraper.lastRun)}
                    </TableCell>
                    <TableCell>
                      {scraperStatus[scraper.id]?.urlsProcessed || scraper.urlsProcessed || 0}
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', gap: 1 }}>
                        {scraperStatus[scraper.id]?.isRunning ? (
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
                            component={Link}
                            to={`/scrapers/${scraper.id}/edit`}
                            size="small"
                          >
                            <EditIcon />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete Scraper">
                          <IconButton
                            color="error"
                            onClick={() => handleDeleteClick(scraper)}
                            disabled={actionInProgress === scraper.id || scraperStatus[scraper.id]?.isRunning}
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
                  <TableCell colSpan={6} align="center">
                    <Typography variant="body1" sx={{ py: 2 }}>
                      {scrapers.length === 0
                        ? "No scrapers found. Create your first scraper to get started."
                        : "No scrapers match your search criteria."}
                    </Typography>
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
            Are you sure you want to delete the scraper "{scraperToDelete?.name}"? This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>
            Cancel
          </Button>
          <Button 
            onClick={handleDeleteConfirm} 
            color="error" 
            variant="contained"
            disabled={actionInProgress === scraperToDelete?.id}
          >
            {actionInProgress === scraperToDelete?.id ? (
              <CircularProgress size={24} />
            ) : (
              'Delete'
            )}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default ScraperList;