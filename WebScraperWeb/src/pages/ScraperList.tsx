import React, { useState, useCallback, useMemo } from 'react';
import { 
  Container, Typography, Box, Button, TextField, InputAdornment,
  Chip, IconButton, Dialog, DialogTitle, DialogContent, 
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
  Refresh as RefreshIcon
} from '@mui/icons-material';
import { Link, useNavigate } from 'react-router-dom';
import { useScrapers, useScraperStatus, useStartScraper, useStopScraper, useDeleteScraper } from '../hooks/queries/useScraperQueries';
import PageHeader from '../components/common/PageHeader';
import AsyncWrapper from '../components/common/AsyncWrapper';
import VirtualizedTable from '../components/common/VirtualizedTable';
import { Scraper, ScraperStatus, TableColumn } from '../types';

const ScraperList: React.FC = () => {
  const navigate = useNavigate();
  
  // React Query hooks
  const { 
    data: scrapers = [], 
    isLoading: isScrapersLoading, 
    error: scrapersError,
    refetch: refetchScrapers
  } = useScrapers();
  
  const startScraperMutation = useStartScraper();
  const stopScraperMutation = useStopScraper();
  const deleteScraperMutation = useDeleteScraper();
  
  // Local state
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [deleteDialogOpen, setDeleteDialogOpen] = useState<boolean>(false);
  const [scraperToDelete, setScraperToDelete] = useState<Scraper | null>(null);
  
  // Compute loading and error states
  const isLoading = isScrapersLoading || 
    startScraperMutation.isPending || 
    stopScraperMutation.isPending || 
    deleteScraperMutation.isPending;
  
  const error = scrapersError?.message || 
    startScraperMutation.error?.message || 
    stopScraperMutation.error?.message || 
    deleteScraperMutation.error?.message;
  
  // Filter scrapers based on search term and status filter
  const filteredScrapers = useMemo(() => {
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
        filtered = filtered.filter(scraper => {
          const status = scraper.status;
          return status && status.isRunning;
        });
      } else if (statusFilter === 'idle') {
        filtered = filtered.filter(scraper => {
          const status = scraper.status;
          return status && !status.isRunning && !status.hasErrors;
        });
      } else if (statusFilter === 'error') {
        filtered = filtered.filter(scraper => {
          const status = scraper.status;
          return status && status.hasErrors;
        });
      }
    }
    
    return filtered;
  }, [scrapers, searchTerm, statusFilter]);
  
  // Handle scraper actions
  const handleStartScraper = useCallback(async (id: string) => {
    try {
      await startScraperMutation.mutateAsync(id);
    } catch (error) {
      console.error(`Error starting scraper ${id}:`, error);
    }
  }, [startScraperMutation]);

  const handleStopScraper = useCallback(async (id: string) => {
    try {
      await stopScraperMutation.mutateAsync(id);
    } catch (error) {
      console.error(`Error stopping scraper ${id}:`, error);
    }
  }, [stopScraperMutation]);

  const handleOpenDeleteDialog = useCallback((scraper: Scraper) => {
    setScraperToDelete(scraper);
    setDeleteDialogOpen(true);
  }, []);

  const handleDeleteScraper = useCallback(async () => {
    if (!scraperToDelete) return;
    
    try {
      await deleteScraperMutation.mutateAsync(scraperToDelete.id);
      setDeleteDialogOpen(false);
      setScraperToDelete(null);
    } catch (error) {
      console.error(`Error deleting scraper ${scraperToDelete.id}:`, error);
    }
  }, [deleteScraperMutation, scraperToDelete]);

  // Render status chip with appropriate color
  const renderStatusChip = useCallback((status?: ScraperStatus) => {
    if (!status) {
      return <Chip label="Unknown" color="default" size="small" />;
    }
    
    if (status.isRunning) {
      return <Chip label="Running" color="success" size="small" />;
    } else if (status.hasErrors) {
      return <Chip label="Error" color="error" size="small" />;
    } else {
      return <Chip label="Idle" color="default" size="small" />;
    }
  }, []);

  // Define table columns
  const columns = useMemo<TableColumn[]>(() => [
    {
      id: 'name',
      label: 'Name',
      sortable: true,
      render: (value, row) => (
        <Link 
          to={`/scrapers/${row.id}`} 
          style={{ textDecoration: 'none', color: 'inherit', fontWeight: 'bold' }}
        >
          {value}
        </Link>
      )
    },
    {
      id: 'baseUrl',
      label: 'Base URL',
      sortable: true
    },
    {
      id: 'status',
      label: 'Status',
      sortable: true,
      render: (_, row) => renderStatusChip(row.status)
    },
    {
      id: 'lastRun',
      label: 'Last Run',
      sortable: true,
      render: (value) => value ? new Date(value).toLocaleString() : 'Never'
    },
    {
      id: 'urlsProcessed',
      label: 'URLs Processed',
      sortable: true,
      render: (value) => value || 0
    }
  ], [renderStatusChip]);

  // Define row actions
  const renderRowActions = useCallback((row: Scraper) => {
    const status = row.status;
    const isActionInProgress = 
      startScraperMutation.isPending && startScraperMutation.variables === row.id ||
      stopScraperMutation.isPending && stopScraperMutation.variables === row.id ||
      deleteScraperMutation.isPending && deleteScraperMutation.variables === row.id;
    
    return (
      <Box sx={{ display: 'flex', gap: 1 }}>
        {status?.isRunning ? (
          <Tooltip title="Stop Scraper">
            <IconButton 
              color="warning"
              onClick={() => handleStopScraper(row.id)}
              disabled={isActionInProgress}
              size="small"
            >
              {isActionInProgress ? (
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
              onClick={() => handleStartScraper(row.id)}
              disabled={isActionInProgress}
              size="small"
            >
              {isActionInProgress ? (
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
            onClick={() => navigate(`/scrapers/${row.id}/edit`)}
            size="small"
          >
            <EditIcon />
          </IconButton>
        </Tooltip>
        <Tooltip title="Delete Scraper">
          <IconButton 
            color="error"
            onClick={() => handleOpenDeleteDialog(row)}
            disabled={isActionInProgress || status?.isRunning}
            size="small"
          >
            <DeleteIcon />
          </IconButton>
        </Tooltip>
      </Box>
    );
  }, [
    navigate, 
    handleStartScraper, 
    handleStopScraper, 
    handleOpenDeleteDialog, 
    startScraperMutation.isPending,
    startScraperMutation.variables,
    stopScraperMutation.isPending,
    stopScraperMutation.variables,
    deleteScraperMutation.isPending,
    deleteScraperMutation.variables
  ]);

  // Toolbar actions
  const toolbarActions = useMemo(() => (
    <Button
      variant="contained"
      startIcon={<AddIcon />}
      onClick={() => navigate('/scrapers/create')}
    >
      New Scraper
    </Button>
  ), [navigate]);

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      {/* Header */}
      <PageHeader
        title="Scrapers"
        subtitle="Manage your web scrapers"
        actionText="New Scraper"
        onActionClick={() => navigate('/scrapers/create')}
        breadcrumbs={[
          { text: 'Dashboard', path: '/' },
          { text: 'Scrapers' }
        ]}
      />

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
                  onClick={() => refetchScrapers()}
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
      <AsyncWrapper
        loading={isLoading && scrapers.length === 0}
        error={error}
        onRetry={() => refetchScrapers()}
      >
        <VirtualizedTable
          columns={columns}
          data={filteredScrapers}
          isLoading={isLoading && scrapers.length > 0}
          error={error}
          onRefresh={() => refetchScrapers()}
          renderRowActions={renderRowActions}
          toolbarActions={toolbarActions}
          searchTerm={searchTerm}
          onSearch={setSearchTerm}
          emptyMessage={
            searchTerm || statusFilter !== 'all'
              ? 'No scrapers found matching your filters.'
              : 'No scrapers found. Create your first scraper to get started.'
          }
          title="Scrapers"
          rowKey="id"
        />
      </AsyncWrapper>
      
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
            disabled={deleteScraperMutation.isPending}
          >
            {deleteScraperMutation.isPending ? (
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

export default React.memo(ScraperList);
