import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { 
  Button,
  IconButton,
  Tooltip,
  Box,
  Menu,
  MenuItem
} from '@mui/material';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import VisibilityIcon from '@mui/icons-material/Visibility';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import StopIcon from '@mui/icons-material/Stop';

import PageHeader from '../components/Common/PageHeader';
import DataTable from '../components/Common/DataTable/DataTable';
import StatusBadge from '../components/Common/StatusBadge';
import ErrorMessage from '../components/Common/ErrorMessage';
import useApiClient from '../hooks/useApiClient';
import { formatDate, formatUrl } from '../utils/formatters';

const ScraperListPage = () => {
  const navigate = useNavigate();
  const { api, loading, error, execute } = useApiClient();
  const [scrapers, setScrapers] = useState([]);
  const [menuAnchorEl, setMenuAnchorEl] = useState(null);
  const [selectedScraper, setSelectedScraper] = useState(null);

  // Fetch scrapers on component mount
  useEffect(() => {
    fetchScrapers();
  }, []);
  
  const fetchScrapers = async () => {
    try {
      const data = await execute(() => api.scrapers.getAll());
      setScrapers(data || []);
    } catch (error) {
      console.error('Error fetching scrapers:', error);
    }
  };

  const handleMenuOpen = (event, scraper) => {
    setMenuAnchorEl(event.currentTarget);
    setSelectedScraper(scraper);
  };

  const handleMenuClose = () => {
    setMenuAnchorEl(null);
    setSelectedScraper(null);
  };

  const handleViewDetails = () => {
    handleMenuClose();
    navigate(`/scrapers/${selectedScraper.id}`);
  };

  const handleEdit = () => {
    handleMenuClose();
    navigate(`/scrapers/${selectedScraper.id}/edit`);
  };

  const handleDelete = async () => {
    if (!selectedScraper) return;
    
    // Ask for confirmation (could use a dialog component)
    if (window.confirm(`Are you sure you want to delete the scraper "${selectedScraper.name}"?`)) {
      try {
        await execute(() => api.scrapers.delete(selectedScraper.id));
        setScrapers(scrapers.filter(s => s.id !== selectedScraper.id));
        handleMenuClose();
      } catch (error) {
        console.error('Error deleting scraper:', error);
      }
    } else {
      handleMenuClose();
    }
  };

  const handleStartScraper = async (scraperId) => {
    try {
      await execute(() => api.scrapers.start(scraperId));
      fetchScrapers(); // Refresh the list to update status
    } catch (error) {
      console.error('Error starting scraper:', error);
    }
  };

  const handleStopScraper = async (scraperId) => {
    try {
      await execute(() => api.scrapers.stop(scraperId));
      fetchScrapers(); // Refresh the list to update status
    } catch (error) {
      console.error('Error stopping scraper:', error);
    }
  };

  const columns = [
    { 
      id: 'name', 
      label: 'Scraper Name' 
    },
    { 
      id: 'url', 
      label: 'Target URL',
      render: (row) => formatUrl(row.url),
    },
    { 
      id: 'status', 
      label: 'Status',
      render: (row) => <StatusBadge status={row.status} />,
    },
    { 
      id: 'lastRun', 
      label: 'Last Run',
      render: (row) => formatDate(row.lastRun),
    },
    { 
      id: 'actions', 
      label: 'Actions',
      sortable: false,
      align: 'right',
      render: (row) => (
        <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
          {row.status === 'running' ? (
            <Tooltip title="Stop">
              <IconButton 
                size="small"
                onClick={(e) => {
                  e.stopPropagation();
                  handleStopScraper(row.id);
                }}
              >
                <StopIcon />
              </IconButton>
            </Tooltip>
          ) : (
            <Tooltip title="Start">
              <IconButton 
                size="small"
                onClick={(e) => {
                  e.stopPropagation();
                  handleStartScraper(row.id);
                }}
              >
                <PlayArrowIcon />
              </IconButton>
            </Tooltip>
          )}
          
          <Tooltip title="View">
            <IconButton 
              size="small"
              onClick={(e) => {
                e.stopPropagation();
                navigate(`/scrapers/${row.id}`);
              }}
            >
              <VisibilityIcon />
            </IconButton>
          </Tooltip>
          
          <Tooltip title="Options">
            <IconButton 
              size="small"
              onClick={(e) => {
                e.stopPropagation();
                handleMenuOpen(e, row);
              }}
            >
              <MoreVertIcon />
            </IconButton>
          </Tooltip>
        </Box>
      ),
    },
  ];

  // Context menu for more options
  const renderMenu = (
    <Menu
      anchorEl={menuAnchorEl}
      open={Boolean(menuAnchorEl)}
      onClose={handleMenuClose}
    >
      <MenuItem onClick={handleViewDetails}>
        <VisibilityIcon fontSize="small" sx={{ mr: 1 }} />
        View Details
      </MenuItem>
      <MenuItem onClick={handleEdit}>
        <EditIcon fontSize="small" sx={{ mr: 1 }} />
        Edit
      </MenuItem>
      <MenuItem onClick={handleDelete} sx={{ color: 'error.main' }}>
        <DeleteIcon fontSize="small" sx={{ mr: 1 }} />
        Delete
      </MenuItem>
    </Menu>
  );

  return (
    <>
      <PageHeader 
        title="Scrapers" 
        subtitle="Manage your web scrapers"
      />
      
      {error && (
        <ErrorMessage 
          title="Failed to load scrapers" 
          message={error}
          onRetry={fetchScrapers}
        />
      )}
      
      <DataTable
        columns={columns}
        data={scrapers}
        loading={loading}
        emptyMessage="No scrapers found. Create your first scraper to get started."
        onRowClick={(row) => navigate(`/scrapers/${row.id}`)}
      />
      
      {renderMenu}
    </>
  );
};

export default ScraperListPage;