import React from 'react';
import { 
  Box, Typography, Card, CardContent, Grid, Button,
  FormControl, InputLabel, Select, MenuItem, TextField,
  InputAdornment, IconButton, Tooltip, CircularProgress,
  Alert, Pagination, Chip
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  Search as SearchIcon,
  Clear as ClearIcon,
  Visibility as ViewIcon,
  OpenInNew as OpenInNewIcon,
  GetApp as DownloadIcon
} from '@mui/icons-material';
import { format } from 'date-fns';
import { extractDomain, extractFilename } from '../../../utils/urlUtils';

const ResultsDocumentsTab = ({ 
  documents, 
  loading, 
  error, 
  page, 
  totalPages, 
  searchTerm, 
  contentTypeFilter, 
  handlePageChange, 
  handleSearch, 
  handleContentTypeFilterChange, 
  handleViewItem,
  setPage,
  setContentTypeFilter
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
        <Typography variant="h6">Processed Documents</Typography>
        <Button
          variant="outlined"
          startIcon={loading ? <CircularProgress size={20} /> : <RefreshIcon />}
          onClick={() => {
            setPage(1);
            setContentTypeFilter('all');
          }}
          disabled={loading}
        >
          Refresh
        </Button>
      </Box>
      
      {/* Filters */}
      <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
        <FormControl variant="outlined" size="small" sx={{ minWidth: 150 }}>
          <InputLabel id="content-type-filter-label">Content Type</InputLabel>
          <Select
            labelId="content-type-filter-label"
            value={contentTypeFilter}
            onChange={handleContentTypeFilterChange}
            label="Content Type"
          >
            <MenuItem value="all">All Types</MenuItem>
            <MenuItem value="html">HTML</MenuItem>
            <MenuItem value="pdf">PDF</MenuItem>
            <MenuItem value="json">JSON</MenuItem>
            <MenuItem value="xml">XML</MenuItem>
            <MenuItem value="text">Text</MenuItem>
          </Select>
        </FormControl>
        
        <TextField
          size="small"
          placeholder="Search documents..."
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
      </Box>
      
      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : error ? (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      ) : documents.length > 0 ? (
        <>
          <Grid container spacing={2}>
            {documents.map((doc) => (
              <Grid item xs={12} sm={6} md={4} key={doc.id}>
                <Card variant="outlined">
                  <CardContent>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
                      <Typography variant="subtitle1" noWrap sx={{ maxWidth: '70%' }}>
                        {extractFilename(doc.url) || 'Document'}
                      </Typography>
                      {renderContentTypeChip(doc.contentType)}
                    </Box>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      {extractDomain(doc.url)}
                    </Typography>
                    <Typography variant="caption" display="block" color="text.secondary">
                      Processed: {doc.processedAt ? format(new Date(doc.processedAt), 'yyyy-MM-dd HH:mm') : 'Unknown'}
                    </Typography>
                    <Typography variant="caption" display="block" color="text.secondary" gutterBottom>
                      Size: {doc.size ? `${Math.round(doc.size / 1024)} KB` : 'Unknown'}
                    </Typography>
                    <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 1 }}>
                      <Tooltip title="View Document">
                        <IconButton 
                          size="small" 
                          onClick={() => handleViewItem(doc)}
                        >
                          <ViewIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Open URL">
                        <IconButton 
                          size="small"
                          component="a"
                          href={doc.url}
                          target="_blank"
                          rel="noopener noreferrer"
                        >
                          <OpenInNewIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Download">
                        <IconButton 
                          size="small"
                          component="a"
                          href={doc.downloadUrl || doc.url}
                          download
                        >
                          <DownloadIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </Box>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>
          
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
          No documents found. {contentTypeFilter !== 'all' ? 'Try a different content type filter.' : ''}
        </Alert>
      )}
    </>
  );
};

export default ResultsDocumentsTab;
