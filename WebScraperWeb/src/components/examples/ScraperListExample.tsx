import React, { useState, useCallback, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Box, Button, Chip, IconButton, Tooltip, CircularProgress } from '@mui/material';
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  PlayArrow as PlayIcon,
  Stop as StopIcon
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import VirtualizedTable from '../common/VirtualizedTable';
import { getAllScrapers } from '../../api/scrapers';

interface Scraper {
  id: string;
  name: string;
  baseUrl: string;
  status?: {
    isRunning: boolean;
    hasErrors: boolean;
  };
  lastRun?: string;
  urlsProcessed?: number;
}

const ScraperListExample: React.FC = () => {
  const navigate = useNavigate();
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [sortBy, setSortBy] = useState('name');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');
  const [searchTerm, setSearchTerm] = useState('');

  // Fetch scrapers with React Query
  const { data: scrapers = [], isLoading, error, refetch } = useQuery({
    queryKey: ['scrapers', { page, rowsPerPage, sortBy, sortDirection, searchTerm }],
    queryFn: () => getAllScrapers(),
  });

  // Filter and sort scrapers
  const filteredScrapers = useMemo(() => {
    let filtered = [...scrapers];
    
    // Apply search filter
    if (searchTerm) {
      const term = searchTerm.toLowerCase();
      filtered = filtered.filter(
        scraper => 
          scraper.name.toLowerCase().includes(term) || 
          scraper.baseUrl.toLowerCase().includes(term)
      );
    }
    
    // Apply sorting
    filtered.sort((a, b) => {
      const aValue = a[sortBy as keyof Scraper];
      const bValue = b[sortBy as keyof Scraper];
      
      if (aValue < bValue) return sortDirection === 'asc' ? -1 : 1;
      if (aValue > bValue) return sortDirection === 'asc' ? 1 : -1;
      return 0;
    });
    
    return filtered;
  }, [scrapers, searchTerm, sortBy, sortDirection]);

  // Paginate scrapers
  const paginatedScrapers = useMemo(() => {
    const startIndex = page * rowsPerPage;
    return filteredScrapers.slice(startIndex, startIndex + rowsPerPage);
  }, [filteredScrapers, page, rowsPerPage]);

  // Handle page change
  const handlePageChange = useCallback((newPage: number) => {
    setPage(newPage);
  }, []);

  // Handle rows per page change
  const handleRowsPerPageChange = useCallback((newRowsPerPage: number) => {
    setRowsPerPage(newRowsPerPage);
    setPage(0);
  }, []);

  // Handle sort
  const handleSort = useCallback((column: string, direction: 'asc' | 'desc') => {
    setSortBy(column);
    setSortDirection(direction);
  }, []);

  // Handle search
  const handleSearch = useCallback((term: string) => {
    setSearchTerm(term);
    setPage(0);
  }, []);

  // Render status chip
  const renderStatusChip = useCallback((status?: { isRunning: boolean; hasErrors: boolean }) => {
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
  const columns = useMemo(() => [
    {
      id: 'name',
      label: 'Name',
      sortable: true,
      minWidth: 150
    },
    {
      id: 'baseUrl',
      label: 'Base URL',
      sortable: true,
      minWidth: 200
    },
    {
      id: 'status',
      label: 'Status',
      sortable: false,
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
      align: 'right',
      render: (value) => value || 0
    }
  ], [renderStatusChip]);

  // Define row actions
  const renderRowActions = useCallback((row: Scraper) => (
    <Box sx={{ display: 'flex', gap: 1 }}>
      {row.status?.isRunning ? (
        <Tooltip title="Stop Scraper">
          <IconButton 
            color="warning"
            size="small"
          >
            <StopIcon />
          </IconButton>
        </Tooltip>
      ) : (
        <Tooltip title="Start Scraper">
          <IconButton 
            color="success"
            size="small"
          >
            <PlayIcon />
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
          size="small"
        >
          <DeleteIcon />
        </IconButton>
      </Tooltip>
    </Box>
  ), [navigate]);

  // Define toolbar actions
  const toolbarActions = useMemo(() => (
    <Button
      variant="contained"
      onClick={() => navigate('/scrapers/create')}
    >
      New Scraper
    </Button>
  ), [navigate]);

  return (
    <VirtualizedTable
      columns={columns}
      data={paginatedScrapers}
      totalCount={filteredScrapers.length}
      page={page}
      rowsPerPage={rowsPerPage}
      onPageChange={handlePageChange}
      onRowsPerPageChange={handleRowsPerPageChange}
      onSort={handleSort}
      onSearch={handleSearch}
      onRefresh={refetch}
      sortBy={sortBy}
      sortDirection={sortDirection}
      searchTerm={searchTerm}
      isLoading={isLoading}
      error={error?.message}
      emptyMessage="No scrapers found. Create your first scraper to get started."
      title="Scrapers"
      searchPlaceholder="Search scrapers..."
      renderRowActions={renderRowActions}
      toolbarActions={toolbarActions}
      onRowClick={(row) => navigate(`/scrapers/${row.id}`)}
      rowKey="id"
    />
  );
};

export default React.memo(ScraperListExample);
