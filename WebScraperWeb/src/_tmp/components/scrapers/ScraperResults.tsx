import React, { useState, useEffect } from 'react';
import { 
  Box, Typography, Paper, Table, TableBody, TableCell, 
  TableContainer, TableHead, TableRow, Chip, Button,
  FormControl, InputLabel, Select, MenuItem, TextField,
  InputAdornment, IconButton, Tooltip, CircularProgress,
  Alert, Divider, Pagination, Card, CardContent, Grid,
  Dialog, DialogTitle, DialogContent, DialogActions,
  Tabs, Tab
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  FilterList as FilterIcon,
  Search as SearchIcon,
  Clear as ClearIcon,
  GetApp as DownloadIcon,
  Visibility as ViewIcon,
  Compare as CompareIcon,
  History as HistoryIcon,
  Link as LinkIcon,
  OpenInNew as OpenInNewIcon,
  ContentCopy as CopyIcon
} from '@mui/icons-material';
import { format } from 'date-fns';
import { getScraperResults, getScraperChanges, getProcessedDocuments } from '../../api/scrapers';
import { getUserFriendlyErrorMessage } from '../../utils/errorHandler';
import { extractDomain, extractFilename } from '../../utils/urlUtils';

// TabPanel component for tab content
function TabPanel(props) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`results-tabpanel-${index}`}
      aria-labelledby={`results-tab-${index}`}
      {...other}
      style={{ padding: '16px 0' }}
    >
      {value === index && children}
    </div>
  );
}

// Helper function for tab accessibility
function a11yProps(index) {
  return {
    id: `results-tab-${index}`,
    'aria-controls': `results-tabpanel-${index}`,
  };
}

const ScraperResults = ({ scraperId }) => {
  const [tabValue, setTabValue] = useState(0);
  const [results, setResults] = useState([]);
  const [changes, setChanges] = useState([]);
  const [documents, setDocuments] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [contentTypeFilter, setContentTypeFilter] = useState('all');
  const [sortOrder, setSortOrder] = useState('newest');
  const [selectedItem, setSelectedItem] = useState(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [compareDialogOpen, setCompareDialogOpen] = useState(false);
  const [compareItems, setCompareItems] = useState({ old: null, new: null });

  // Fetch results based on current tab
  useEffect(() => {
    const fetchData = async () => {
      if (!scraperId) return;
      
      setLoading(true);
      setError(null);
      
      try {
        switch (tabValue) {
          case 0: // All Results
            const resultsData = await getScraperResults(page, pageSize, searchTerm, scraperId);
            setResults(resultsData.items || []);
            setTotalPages(resultsData.totalPages || 1);
            break;
          case 1: // Changes
            const changesData = await getScraperChanges(scraperId, null, 100);
            setChanges(changesData || []);
            break;
          case 2: // Documents
            const documentsData = await getProcessedDocuments(scraperId, contentTypeFilter === 'all' ? null : contentTypeFilter, page, pageSize);
            setDocuments(documentsData.items || []);
            setTotalPages(documentsData.totalPages || 1);
            break;
          default:
            break;
        }
      } catch (err) {
        setError(getUserFriendlyErrorMessage(err, 'Failed to fetch data'));
      } finally {
        setLoading(false);
      }
    };
    
    fetchData();
  }, [scraperId, tabValue, page, pageSize, searchTerm, contentTypeFilter]);

  // Handle tab change
  const handleTabChange = (event, newValue) => {
    setTabValue(newValue);
    setPage(1); // Reset to first page when changing tabs
  };

  // Handle page change
  const handlePageChange = (event, value) => {
    setPage(value);
  };

  // Handle search
  const handleSearch = (event) => {
    setSearchTerm(event.target.value);
    setPage(1); // Reset to first page when searching
  };

  // Handle content type filter change
  const handleContentTypeFilterChange = (event) => {
    setContentTypeFilter(event.target.value);
    setPage(1); // Reset to first page when changing filter
  };

  // Handle sort order change
  const handleSortOrderChange = (event) => {
    setSortOrder(event.target.value);
  };

  // Handle view item
  const handleViewItem = (item) => {
    setSelectedItem(item);
    setDialogOpen(true);
  };

  // Handle compare items
  const handleCompareItems = (oldItem, newItem) => {
    setCompareItems({ old: oldItem, new: newItem });
    setCompareDialogOpen(true);
  };

  // Handle dialog close
  const handleDialogClose = () => {
    setDialogOpen(false);
    setSelectedItem(null);
  };

  // Handle compare dialog close
  const handleCompareDialogClose = () => {
    setCompareDialogOpen(false);
    setCompareItems({ old: null, new: null });
  };

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

  // Render change type chip
  const renderChangeTypeChip = (changeType) => {
    switch (changeType?.toLowerCase()) {
      case 'added':
        return <Chip label="Added" size="small" color="success" />;
      case 'removed':
        return <Chip label="Removed" size="small" color="error" />;
      case 'modified':
        return <Chip label="Modified" size="small" color="warning" />;
      default:
        return <Chip label={changeType || 'Unknown'} size="small" />;
    }
  };

  return (
    <Box>
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 3 }}>
        <Tabs 
          value={tabValue} 
          onChange={handleTabChange} 
          aria-label="scraper results tabs"
        >
          <Tab label="All Results" {...a11yProps(0)} />
          <Tab label="Changes" {...a11yProps(1)} />
          <Tab label="Documents" {...a11yProps(2)} />
        </Tabs>
      </Box>
      
      {/* All Results Tab */}
      <TabPanel value={tabValue} index={0}>
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
      </TabPanel>
      
      {/* Changes Tab */}
      <TabPanel value={tabValue} index={1}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h6">Content Changes</Typography>
          <Button
            variant="outlined"
            startIcon={loading ? <CircularProgress size={20} /> : <RefreshIcon />}
            onClick={() => setTabValue(1)} // Refresh by re-setting the tab value
            disabled={loading}
          >
            Refresh
          </Button>
        </Box>
        
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : error ? (
          <Alert severity="error" sx={{ mb: 3 }}>
            {error}
          </Alert>
        ) : changes.length > 0 ? (
          <TableContainer component={Paper}>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>URL</TableCell>
                  <TableCell>Change Type</TableCell>
                  <TableCell>Detected At</TableCell>
                  <TableCell>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {changes.map((change, index) => (
                  <TableRow key={index} hover>
                    <TableCell>
                      <Tooltip title={change.url}>
                        <Typography 
                          variant="body2" 
                          sx={{ 
                            maxWidth: 300, 
                            overflow: 'hidden', 
                            textOverflow: 'ellipsis', 
                            whiteSpace: 'nowrap' 
                          }}
                        >
                          {change.url}
                        </Typography>
                      </Tooltip>
                    </TableCell>
                    <TableCell>{renderChangeTypeChip(change.changeType)}</TableCell>
                    <TableCell>
                      {change.detectedAt ? format(new Date(change.detectedAt), 'yyyy-MM-dd HH:mm') : ''}
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', gap: 1 }}>
                        <Tooltip title="Compare Versions">
                          <IconButton 
                            size="small"
                            onClick={() => handleCompareItems(change.previousVersion, change.currentVersion)}
                            disabled={!change.previousVersion || !change.currentVersion}
                          >
                            <CompareIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Open URL">
                          <IconButton 
                            size="small"
                            component="a"
                            href={change.url}
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
        ) : (
          <Alert severity="info">
            No content changes detected yet.
          </Alert>
        )}
      </TabPanel>
      
      {/* Documents Tab */}
      <TabPanel value={tabValue} index={2}>
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
      </TabPanel>
      
      {/* View Item Dialog */}
      <Dialog
        open={dialogOpen}
        onClose={handleDialogClose}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h6">
              {selectedItem?.url ? extractFilename(selectedItem.url) || 'Content View' : 'Content View'}
            </Typography>
            <Box>
              <Tooltip title="Copy URL">
                <IconButton 
                  size="small"
                  onClick={() => {
                    if (selectedItem?.url) {
                      navigator.clipboard.writeText(selectedItem.url);
                    }
                  }}
                >
                  <CopyIcon fontSize="small" />
                </IconButton>
              </Tooltip>
              <Tooltip title="Open in New Tab">
                <IconButton 
                  size="small"
                  component="a"
                  href={selectedItem?.url}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  <OpenInNewIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            </Box>
          </Box>
        </DialogTitle>
        <DialogContent dividers>
          {selectedItem?.contentType?.toLowerCase() === 'html' ? (
            <iframe
              src={selectedItem.url}
              title="Content Preview"
              width="100%"
              height="500px"
              style={{ border: 'none' }}
            />
          ) : selectedItem?.contentType?.toLowerCase() === 'pdf' ? (
            <embed
              src={selectedItem.url}
              type="application/pdf"
              width="100%"
              height="500px"
            />
          ) : (
            <Box 
              component="pre" 
              sx={{ 
                p: 2, 
                bgcolor: 'grey.100', 
                borderRadius: 1, 
                overflow: 'auto',
                maxHeight: '500px',
                whiteSpace: 'pre-wrap',
                wordBreak: 'break-word'
              }}
            >
              {selectedItem?.content || 'No content available'}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleDialogClose}>Close</Button>
          {selectedItem?.downloadUrl && (
            <Button 
              component="a"
              href={selectedItem.downloadUrl}
              download
              startIcon={<DownloadIcon />}
            >
              Download
            </Button>
          )}
        </DialogActions>
      </Dialog>
      
      {/* Compare Dialog */}
      <Dialog
        open={compareDialogOpen}
        onClose={handleCompareDialogClose}
        maxWidth="lg"
        fullWidth
      >
        <DialogTitle>Compare Versions</DialogTitle>
        <DialogContent dividers>
          <Grid container spacing={2}>
            <Grid item xs={6}>
              <Typography variant="subtitle1" gutterBottom>Previous Version</Typography>
              <Box 
                component="pre" 
                sx={{ 
                  p: 2, 
                  bgcolor: 'grey.100', 
                  borderRadius: 1, 
                  overflow: 'auto',
                  height: '500px',
                  whiteSpace: 'pre-wrap',
                  wordBreak: 'break-word'
                }}
              >
                {compareItems.old?.content || 'No previous content available'}
              </Box>
            </Grid>
            <Grid item xs={6}>
              <Typography variant="subtitle1" gutterBottom>Current Version</Typography>
              <Box 
                component="pre" 
                sx={{ 
                  p: 2, 
                  bgcolor: 'grey.100', 
                  borderRadius: 1, 
                  overflow: 'auto',
                  height: '500px',
                  whiteSpace: 'pre-wrap',
                  wordBreak: 'break-word'
                }}
              >
                {compareItems.new?.content || 'No current content available'}
              </Box>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCompareDialogClose}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ScraperResults;
