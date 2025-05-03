import React, { useState, useCallback, useMemo } from 'react';
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
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { 
  getAllScrapers, 
  getScraperStatus, 
  startScraper, 
  stopScraper, 
  deleteScraper 
} from '../api/scrapers';

// Simple status chip component that doesn't use hooks
const StatusChip = ({ status, isLoading, isError }) => {
  if (isLoading && !status) {
    return <Chip label="Loading..." color="default" size="small" />;
  }

  if (isError && !status) {
    return (
      <Tooltip title="Unable to fetch status">
        <Chip
          label="Unknown"
          color="default"
          size="small"
          icon={<ErrorIcon fontSize="small" />}
        />
      </Tooltip>
    );
  }

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

// Format date helper function
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

// Simple row component that doesn't use hooks
const ScraperRow = ({ 
  scraper, 
  status, 
  isStatusLoading, 
  isStatusError,
  isPending,
  onStart,
  onStop,
  onDelete,
  onEdit
}) => {
  const isRunning = !isStatusError && status?.isRunning === true;
  
  return (
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
      <TableCell>
        <StatusChip 
          status={status} 
          isLoading={isStatusLoading} 
          isError={isStatusError} 
        />
      </TableCell>
      <TableCell>
        {formatDate(status?.lastRun || scraper.lastRun)}
      </TableCell>
      <TableCell>
        {status?.urlsProcessed || scraper.urlsProcessed || 0}
      </TableCell>
      <TableCell>
        <Box sx={{ display: 'flex', gap: 1 }}>
          {isRunning ? (
            <Tooltip title="Stop Scraper">
              <IconButton
                color="warning"
                onClick={() => onStop(scraper.id)}
                disabled={isPending || isStatusError}
                size="small"
              >
                {isPending ? (
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
                onClick={() => onStart(scraper.id)}
                disabled={isPending || isStatusError}
                size="small"
              >
                {isPending ? (
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
              onClick={() => onDelete(scraper)}
              disabled={isPending || isRunning}
              size="small"
            >
              <DeleteIcon />
            </IconButton>
          </Tooltip>
        </Box>
      </TableCell>
    </TableRow>
  );
};

const ScraperList = () => {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  
  // State for UI interactions
  const [searchTerm, setSearchTerm] = useState('');
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [scraperToDelete, setScraperToDelete] = useState(null);

  // Fetch all scrapers
  const { 
    data: scrapers = [], 
    isLoading: isScrapersLoading, 
    error: scrapersError,
    refetch: refetchScrapers
  } = useQuery({
    queryKey: ['scrapers'],
    queryFn: async () => {
      try {
        return await getAllScrapers();
      } catch (error) {
        console.error('Error fetching scrapers:', error);
        return [];
      }
    },
    staleTime: 60 * 1000, // 1 minute
    refetchOnWindowFocus: false,
    keepPreviousData: true
  });

  // Fetch status for all scrapers at once
  const { 
    data: allStatuses = {}, 
    isLoading: isStatusesLoading,
    error: statusesError,
    refetch: refetchStatuses
  } = useQuery({
    queryKey: ['scraperStatuses'],
    queryFn: async () => {
      try {
        // Fetch status for each scraper in parallel
        const statusPromises = scrapers.map(scraper => 
          getScraperStatus(scraper.id)
            .then(status => ({ id: scraper.id, status }))
            .catch(error => {
              console.error(`Error fetching status for scraper ${scraper.id}:`, error);
              return { 
                id: scraper.id, 
                status: { isRunning: false, hasErrors: false, errorMessage: 'Unable to fetch status' },
                error: true
              };
            })
        );
        
        const results = await Promise.all(statusPromises);
        
        // Convert array of results to an object keyed by scraper ID
        return results.reduce((acc, { id, status, error }) => {
          acc[id] = { 
            data: status,
            isError: !!error
          };
          return acc;
        }, {});
      } catch (error) {
        console.error('Error fetching scraper statuses:', error);
        return {};
      }
    },
    enabled: scrapers.length > 0,
    staleTime: 30 * 1000, // 30 seconds
    refetchInterval: 30 * 1000, // Poll every 30 seconds
    refetchIntervalInBackground: false,
    keepPreviousData: true
  });

  // Setup mutations for actions
  const startScraperMutation = useMutation({
    mutationFn: (id) => startScraper(id),
    onSuccess: () => {
      // Invalidate all statuses to trigger a refetch
      queryClient.invalidateQueries({ queryKey: ['scraperStatuses'] });
    }
  });

  const stopScraperMutation = useMutation({
    mutationFn: (id) => stopScraper(id),
    onSuccess: () => {
      // Invalidate all statuses to trigger a refetch
      queryClient.invalidateQueries({ queryKey: ['scraperStatuses'] });
    }
  });

  const deleteScraperMutation = useMutation({
    mutationFn: (id) => deleteScraper(id),
    onSuccess: () => {
      // Invalidate scrapers list to trigger a refetch
      queryClient.invalidateQueries({ queryKey: ['scrapers'] });
      setDeleteDialogOpen(false);
      setScraperToDelete(null);
    }
  });

  // Filter scrapers based on search term
  const filteredScrapers = useMemo(() => {
    return scrapers.filter(scraper => 
      scraper.name.toLowerCase().includes(searchTerm.toLowerCase()) || 
      scraper.baseUrl.toLowerCase().includes(searchTerm.toLowerCase())
    );
  }, [scrapers, searchTerm]);

  // Action handlers
  const handleStartScraper = useCallback((id) => {
    startScraperMutation.mutate(id);
  }, [startScraperMutation]);

  const handleStopScraper = useCallback((id) => {
    stopScraperMutation.mutate(id);
  }, [stopScraperMutation]);

  const handleDeleteClick = useCallback((scraper) => {
    setScraperToDelete(scraper);
    setDeleteDialogOpen(true);
  }, []);

  const handleDeleteConfirm = useCallback(() => {
    if (!scraperToDelete) return;
    deleteScraperMutation.mutate(scraperToDelete.id);
  }, [deleteScraperMutation, scraperToDelete]);

  // Refresh all data
  const handleRefresh = useCallback(() => {
    refetchScrapers();
    refetchStatuses();
  }, [refetchScrapers, refetchStatuses]);

  if (isScrapersLoading && scrapers.length === 0) {
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
                  onClick={handleRefresh}
                  disabled={isScrapersLoading || isStatusesLoading}
                >
                  Refresh
                </Button>
              </Box>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Scrapers Table */}
      {isScrapersLoading && scrapers.length > 0 ? (
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
                filteredScrapers.map((scraper) => {
                  // Get status for this specific scraper
                  const scraperStatus = allStatuses[scraper.id] || {};
                  const status = scraperStatus.data;
                  const isStatusError = scraperStatus.isError;
                  
                  // Check if this specific scraper has an action in progress
                  const isPending =
                    (startScraperMutation.isPending && startScraperMutation.variables === scraper.id) ||
                    (stopScraperMutation.isPending && stopScraperMutation.variables === scraper.id) ||
                    (deleteScraperMutation.isPending && deleteScraperMutation.variables === scraper.id);
                  
                  return (
                    <ScraperRow
                      key={scraper.id}
                      scraper={scraper}
                      status={status}
                      isStatusLoading={isStatusesLoading}
                      isStatusError={isStatusError}
                      isPending={isPending}
                      onStart={handleStartScraper}
                      onStop={handleStopScraper}
                      onDelete={handleDeleteClick}
                    />
                  );
                })
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
            disabled={deleteScraperMutation.isPending}
          >
            {deleteScraperMutation.isPending ? (
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
