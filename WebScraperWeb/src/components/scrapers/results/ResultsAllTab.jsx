import React from 'react';
import { 
  Box, Typography, Paper, Table, TableBody, TableCell, 
  TableContainer, TableHead, TableRow, Chip, Button,
  FormControl, InputLabel, Select, MenuItem, TextField,
  InputAdornment, IconButton, Tooltip, CircularProgress,
  Alert, Pagination
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  Search as SearchIcon,
  Clear as ClearIcon,
  Visibility as ViewIcon,
  OpenInNew as OpenInNewIcon
} from '@mui/icons-material';
import { format } from 'date-fns';

const ResultsAllTab = ({ 
  results, 
  loading, 
  error, 
  page, 
  totalPages, 
  searchTerm, 
  sortOrder, 
  handlePageChange, 
  handleSearch, 
  handleSortOrderChange, 
  handleViewItem,
  setPage,
  setSearchTerm
}) => {
  // Render content type chip
  const renderContentTypeChip = (contentType) => {
    switch (contentType?.toLowerCase()) {
      case 'html':
        return <Chip label="HTML" size="small" color="primary" />;
      case 'pdf':
        return <Chip label="PDF" size="small" color="secondary" />;
      case 'json':
        return <Chip label="JSON" size="small" color="success" />;
      case 'xml':
        return <Chip label="XML" size="small" color="info" />;
      case 'text':
        return <Chip label="Text" size="small" color="default" />;
      default:
        return <Chip label={contentType || 'Unknown'} size="small" />;
    }
  };

  return (
    <>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h6">Scraped URLs</Typography>
        <Button
          variant="outlined"
          startIcon={loading ? <CircularProgress size={20} /> : <RefreshIcon />}
          onClick={() => {
            setPage(1);
            setSearchTerm('');
          }}
          disabled={loading}
        >
          Refresh
        </Button>
      </Box>
      
      {/* Filters */}
      <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
        <TextField
          size="small"
          placeholder="Search URLs..."
          value={searchTerm}
          onChange={handleSearch}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon fontSize="small" />
              </InputAdornment>
            ),
            endAdornment: searchTerm && (
              <InputAdornment position="end">
                <IconButton
                  size="small"
                  onClick={() => setSearchTerm('')}
                  edge="end"
                >
                  <ClearIcon fontSize="small" />
                </IconButton>
              </InputAdornment>
            )
          }}
          sx={{ flexGrow: 1 }}
        />
        
        <FormControl variant="outlined" size="small" sx={{ minWidth: 150 }}>
          <InputLabel id="sort-order-label">Sort By</InputLabel>
          <Select
            labelId="sort-order-label"
            value={sortOrder}
            onChange={handleSortOrderChange}
            label="Sort By"
          >
            <MenuItem value="newest">Newest First</MenuItem>
            <MenuItem value="oldest">Oldest First</MenuItem>
          </Select>
        </FormControl>
      </Box>
      
      {/* Results table */}
      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : error ? (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      ) : results.length > 0 ? (
        <>
          <TableContainer component={Paper}>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>URL</TableCell>
                  <TableCell>Content Type</TableCell>
                  <TableCell>Discovered</TableCell>
                  <TableCell>Last Processed</TableCell>
                  <TableCell>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {results.map((result) => (
                  <TableRow key={result.id} hover>
                    <TableCell>
                      <Tooltip title={result.url}>
                        <Typography 
                          variant="body2" 
                          sx={{ 
                            maxWidth: 300, 
                            overflow: 'hidden', 
                            textOverflow: 'ellipsis', 
                            whiteSpace: 'nowrap' 
                          }}
                        >
                          {result.url}
                        </Typography>
                      </Tooltip>
                    </TableCell>
                    <TableCell>{renderContentTypeChip(result.contentType)}</TableCell>
                    <TableCell>
                      {result.discoveredAt ? format(new Date(result.discoveredAt), 'yyyy-MM-dd HH:mm') : ''}
                    </TableCell>
                    <TableCell>
                      {result.lastProcessedAt ? format(new Date(result.lastProcessedAt), 'yyyy-MM-dd HH:mm') : ''}
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', gap: 1 }}>
                        <Tooltip title="View Content">
                          <IconButton 
                            size="small" 
                            onClick={() => handleViewItem(result)}
                          >
                            <ViewIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Open URL">
                          <IconButton 
                            size="small"
                            component="a"
                            href={result.url}
                            target="_blank"
                            rel="noopener noreferrer"
                          >
                            <OpenInNewIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </Box>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
          
          <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3 }}>
            <Pagination 
              count={totalPages} 
              page={page} 
              onChange={handlePageChange} 
              color="primary" 
            />
          </Box>
        </>
      ) : (
        <Alert severity="info">
          No results found. {searchTerm ? 'Try a different search term.' : ''}
        </Alert>
      )}
    </>
  );
};

export default ResultsAllTab;
