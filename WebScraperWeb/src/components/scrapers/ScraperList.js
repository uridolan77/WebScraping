// src/pages/ScraperList.jsx
import React, { useState, useEffect } from 'react';
import { 
  Container, Typography, Box, Button, TextField, InputAdornment,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow, 
  Paper, Chip, IconButton, Dialog, DialogTitle, DialogContent, 
  DialogContentText, DialogActions, CircularProgress, Tooltip,
  Card, CardContent, Grid, MenuItem, Select, FormControl, InputLabel
} from '@mui/material';
import { 
  Add as AddIcon, 
  Search as SearchIcon, 
  PlayArrow as PlayIcon, 
  Stop as StopIcon, 
  Delete as DeleteIcon,
  Edit as EditIcon,
  Refresh as RefreshIcon,
  FilterList as FilterIcon,
  MoreVert as MoreVertIcon
} from '@mui/icons-material';
import { Link, useNavigate } from 'react-router-dom';
import { getAllScrapers, startScraper, stopScraper, deleteScraper } from '../api/scrapers';

const ScraperList = () => {
  const navigate = useNavigate();
  const [scrapers, setScrapers] = useState([]);
  const [filteredScrapers, setFilteredScrapers] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [scraperToDelete, setScraperToDelete] = useState(null);
  const [actionInProgress, setActionInProgress] = useState(null);

  // Fetch scrapers
  const fetchScrapers = async () => {
    try {
      setIsLoading(true);
      const data = await getAllScrapers();
      setScrapers(data || []);
    } catch (error) {
      console.error('Error fetching scrapers:', error);
    } finally {
      setIsLoading(false);
    }
  };

  // Initial load
  useEffect(() => {
    fetchScrapers();
    
    // Auto-refresh every 30 seconds
    const interval = setInterval(fetchScrapers, 30000);
    return () => clearInterval(interval);
  }, []);

  // Filter scrapers based on search term and status filter
  useEffect(() => {
    let filtered = [...scrapers];
    
    // Apply search term filter
    if (searchTerm) {
      const term = searchTerm.toLowerCase();
      filtered = filtered.filter(
        scraper => 
          scraper.name.toLowerCase().includes(term) || 
          scraper.baseUrl.toLowerCase().includes(term) ||
          scraper.id.toLowerCase().includes(term)
      );
    }
    
    // Apply status filter
    if (statusFilter !== 'all') {
      if (statusFilter === 'running') {
        filtered = filtered.filter(scraper => scraper.isRunning);
      } else if (statusFilter === 'idle') {
        filtered = filtered.filter(scraper => !scraper.isRunning && !scraper.hasErrors);
      } else if (statusFilter === 'error') {
        filtered = filtered.filter(scraper => scraper.hasErrors);
      }
    }
    
    setFilteredScrapers(filtered);
  }, [scrapers, searchTerm, statusFilter]);

  // Handle scraper actions
  const handleStartScraper = async (id) => {
    try {
      setActionInProgress(id);
      await startScraper(id);
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
      await stopScraper(id);
      fetchScrapers(); // Refresh the list
    } catch (error) {
      console.error(`Error stopping scraper ${id}:`, error);
    } finally {
      setActionInProgress(null);
    }
  };

  const handleOpenDeleteDialog = (scraper) => {
    setScraperToDelete(scraper);
    setDeleteDialogOpen(true);
  };

  const handleDeleteScraper = async () => {
    if (!scraperToDelete) return;
    
    try {
      setActionInProgress(scraperToDelete.id);
      await deleteScraper(scraperToDelete.id);
      setDeleteDialogOpen(false);
      setScraperToDelete(null);
      fetchScrapers(); // Refresh the list
    } catch (error) {
      console.error(`Error deleting scraper ${scraperToDelete.id}:`, error);
    } finally {
      setActionInProgress(null);
    }
  };

  // Render status chip with appropriate color
  const renderStatusChip = (scraper) => {
    if (scraper.isRunning) {
      return <Chip label="Running" color="success" size="small" />;
    } else if (scraper.hasErrors) {
      return <Chip label="Error" color="error" size="small" />;
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
                  onChange={(e) => setStatusFilter(e.target.value)}
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
                  onClick={fetchScrapers}
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
                    <TableCell>{scraper.urlsProcessed || 0}</TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', gap: 1 }}>
                        {scraper.isRunning ? (
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
                            disabled={actionInProgress === scraper.id || scraper.isRunning}
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
    </Container>
  );
};

export default ScraperList;