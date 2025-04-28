// src/pages/ContentBrowser.jsx
import React, { useState, useEffect } from 'react';
import { 
  Container, Box, Paper, Typography, Grid, Card, CardContent, Divider,
  CircularProgress, Button, List, ListItem, ListItemText, Tabs, Tab,
  Chip, LinearProgress, IconButton, TextField, InputAdornment, TableContainer,
  Table, TableHead, TableBody, TableRow, TableCell, Dialog, DialogTitle,
  DialogContent, DialogActions, Pagination, Alert, Menu, MenuItem, Link,
  Accordion, AccordionSummary, AccordionDetails, FormControl, InputLabel,
  Select, OutlinedInput
} from '@mui/material';
import {
  Search as SearchIcon,
  FilterList as FilterListIcon,
  Article as ArticleIcon,
  Description as DescriptionIcon,
  PictureAsPdf as PdfIcon,
  InsertDriveFile as FileIcon,
  FolderOpen as FolderIcon,
  CloudDownload as DownloadIcon,
  OpenInNew as OpenInNewIcon,
  ExpandMore as ExpandMoreIcon,
  Compare as CompareIcon,
  WebAsset as WebAssetIcon,
  Code as CodeIcon,
  Timer as TimerIcon,
} from '@mui/icons-material';

// Simulated API methods
const getScrapedContent = async (scraperId, searchTerm, contentType, page, pageSize) => {
  // This would be your actual API call
  return simulateScrapedContent(scraperId, searchTerm, contentType, page, pageSize);
};

const getScrapersList = async () => {
  // This would be your actual API call
  return [
    { id: 'scraper-1', name: 'UKGC Monitor' },
    { id: 'scraper-2', name: 'MGA Content Monitor' },
    { id: 'scraper-3', name: 'Regulatory News Monitor' }
  ];
};

const ContentBrowser = () => {
  const [loading, setLoading] = useState(true);
  const [scrapers, setScrapers] = useState([]);
  const [selectedScraperId, setSelectedScraperId] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [contentType, setContentType] = useState('all');
  const [content, setContent] = useState({ items: [], totalCount: 0 });
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [selectedContent, setSelectedContent] = useState(null);
  const [showContentDialog, setShowContentDialog] = useState(false);
  
  useEffect(() => {
    // Load scrapers list
    loadScrapers();
  }, []);
  
  useEffect(() => {
    if (selectedScraperId) {
      loadContent();
    }
  }, [selectedScraperId, contentType, page, pageSize]);
  
  const loadScrapers = async () => {
    try {
      const scrapersList = await getScrapersList();
      setScrapers(scrapersList);
      if (scrapersList.length > 0) {
        setSelectedScraperId(scrapersList[0].id);
      }
    } catch (error) {
      console.error('Error loading scrapers:', error);
    } finally {
      setLoading(false);
    }
  };
  
  const loadContent = async () => {
    setLoading(true);
    try {
      const contentData = await getScrapedContent(
        selectedScraperId,
        searchTerm,
        contentType,
        page,
        pageSize
      );
      setContent(contentData);
    } catch (error) {
      console.error('Error loading content:', error);
    } finally {
      setLoading(false);
    }
  };
  
  const handleScraperChange = (event) => {
    setSelectedScraperId(event.target.value);
    setPage(1); // Reset to first page when changing scrapers
  };
  
  const handleContentTypeChange = (event) => {
    setContentType(event.target.value);
    setPage(1); // Reset to first page when changing content type
  };
  
  const handlePageChange = (event, value) => {
    setPage(value);
  };
  
  const handleSearchChange = (event) => {
    setSearchTerm(event.target.value);
  };
  
  const handleSearch = () => {
    setPage(1); // Reset to first page when searching
    loadContent();
  };
  
  const handleKeyPress = (event) => {
    if (event.key === 'Enter') {
      handleSearch();
    }
  };
  
  const handleViewContent = (item) => {
    setSelectedContent(item);
    setShowContentDialog(true);
  };
  
  const handleCloseContentDialog = () => {
    setShowContentDialog(false);
  };
  
  // Get icon based on content type
  const getContentTypeIcon = (type) => {
    switch (type.toLowerCase()) {
      case 'pdf':
        return <PdfIcon />;
      case 'html':
        return <WebAssetIcon />;
      case 'doc':
      case 'docx':
        return <DescriptionIcon />;
      case 'xls':
      case 'xlsx':
        return <ArticleIcon />;
      default:
        return <FileIcon />;
    }
  };
  
  // Format content summary
  const formatContentSummary = (content, maxLength = 150) => {
    if (!content) return '';
    
    const text = content.replace(/<[^>]*>/g, ''); // Remove HTML tags
    if (text.length <= maxLength) return text;
    
    return text.substring(0, maxLength) + '...';
  };
  
  if (loading && scrapers.length === 0) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '80vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="h4" component="h1" gutterBottom>
            Content Browser
          </Typography>
        </Box>
        
        <Divider sx={{ mb: 3 }} />
        
        {/* Filters Section */}
        <Box sx={{ mb: 3 }}>
          <Grid container spacing={2}>
            <Grid item xs={12} md={3}>
              <FormControl fullWidth variant="outlined">
                <InputLabel id="scraper-select-label">Scraper</InputLabel>
                <Select
                  labelId="scraper-select-label"
                  id="scraper-select"
                  value={selectedScraperId}
                  onChange={handleScraperChange}
                  label="Scraper"
                >
                  {scrapers.map((scraper) => (
                    <MenuItem key={scraper.id} value={scraper.id}>
                      {scraper.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            
            <Grid item xs={12} md={3}>
              <FormControl fullWidth variant="outlined">
                <InputLabel id="content-type-label">Content Type</InputLabel>
                <Select
                  labelId="content-type-label"
                  id="content-type"
                  value={contentType}
                  onChange={handleContentTypeChange}
                  label="Content Type"
                >
                  <MenuItem value="all">All Content</MenuItem>
                  <MenuItem value="html">Web Pages</MenuItem>
                  <MenuItem value="pdf">PDF Documents</MenuItem>
                  <MenuItem value="doc">Word Documents</MenuItem>
                  <MenuItem value="regulatory">Regulatory Content</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                variant="outlined"
                placeholder="Search content..."
                value={searchTerm}
                onChange={handleSearchChange}
                onKeyPress={handleKeyPress}
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton onClick={handleSearch} edge="end">
                        <SearchIcon />
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
          </Grid>
        </Box>
        
        {/* Results Count */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="subtitle1">
            {content.totalCount} results found
          </Typography>
          <Box>
            {content.totalCount > 0 && (
              <Pagination 
                count={Math.ceil(content.totalCount / pageSize)} 
                page={page} 
                onChange={handlePageChange} 
                color="primary" 
              />
            )}
          </Box>
        </Box>
        
        {/* Content List */}
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 5 }}>
            <CircularProgress />
          </Box>
        ) : content.items.length === 0 ? (
          <Alert severity="info">
            No content found. Try changing your search criteria or select a different scraper.
          </Alert>
        ) : (
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Type</TableCell>
                  <TableCell>Title</TableCell>
                  <TableCell>URL</TableCell>
                  <TableCell>Captured</TableCell>
                  <TableCell>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {content.items.map((item) => (
                  <TableRow key={item.id} hover>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        {getContentTypeIcon(item.contentType)}
                        <Typography variant="body2" sx={{ ml: 1 }}>
                          {item.contentType.toUpperCase()}
                        </Typography>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Typography variant="subtitle2">{item.title}</Typography>
                      <Typography variant="body2" color="textSecondary">
                        {formatContentSummary(item.summary)}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Link href={item.url} target="_blank" sx={{ display: 'flex', alignItems: 'center' }}>
                        <Typography variant="body2" noWrap sx={{ maxWidth: 250 }}>
                          {item.url}
                        </Typography>
                        <OpenInNewIcon fontSize="small" sx={{ ml: 0.5 }} />
                      </Link>
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <TimerIcon fontSize="small" sx={{ mr: 0.5 }} />
                        <Typography variant="body2">
                          {new Date(item.capturedAt).toLocaleString()}
                        </Typography>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Button
                        variant="outlined"
                        size="small"
                        startIcon={<ArticleIcon />}
                        onClick={() => handleViewContent(item)}
                        sx={{ mr: 1 }}
                      >
                        View
                      </Button>
                      {item.hasChanges && (
                        <IconButton 
                          color="warning" 
                          size="small"
                          onClick={() => handleViewContent(item)}
                          title="View changes"
                        >
                          <CompareIcon />
                        </IconButton>
                      )}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}
        
        {/* Pagination at bottom */}
        {content.totalCount > pageSize && (
          <Box sx={{ display: 'flex', justifyContent: 'center', mt: 2 }}>
            <Pagination 
              count={Math.ceil(content.totalCount / pageSize)} 
              page={page} 
              onChange={handlePageChange} 
              color="primary" 
            />
          </Box>
        )}
      </Paper>
      
      {/* Content Viewer Dialog */}
      <Dialog
        open={showContentDialog}
        onClose={handleCloseContentDialog}
        fullWidth
        maxWidth="lg"
      >
        {selectedContent && (
          <>
            <DialogTitle>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Typography variant="h6">
                  {selectedContent.title}
                </Typography>
                <Box>
                  <Chip 
                    icon={getContentTypeIcon(selectedContent.contentType)} 
                    label={selectedContent.contentType.toUpperCase()} 
                    size="small"
                    color="primary"
                    sx={{ mr: 1 }}
                  />
                  {selectedContent.hasChanges && (
                    <Chip 
                      icon={<CompareIcon />} 
                      label="Has Changes" 
                      size="small"
                      color="warning"
                    />
                  )}
                </Box>
              </Box>
            </DialogTitle>
            <DialogContent dividers>
              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" gutterBottom>
                  Source URL:
                </Typography>
                <Link href={selectedContent.url} target="_blank" sx={{ display: 'flex', alignItems: 'center' }}>
                  {selectedContent.url}
                  <OpenInNewIcon fontSize="small" sx={{ ml: 0.5 }} />
                </Link>
              </Box>
              
              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" gutterBottom>
                  Captured: {new Date(selectedContent.capturedAt).toLocaleString()}
                </Typography>
              </Box>
              
              <Divider sx={{ mb: 2 }} />
              
              {/* Content Tabs */}
              <Box sx={{ mb: 2 }}>
                <Tabs value={0}>
                  <Tab label="Processed Content" />
                  <Tab label="Raw Content" />
                  {selectedContent.hasChanges && <Tab label="Changes" />}
                </Tabs>
              </Box>
              
              {/* Content Display */}
              <Box sx={{ 
                maxHeight: '50vh', 
                overflow: 'auto', 
                p: 2, 
                bgcolor: 'background.default',
                borderRadius: 1
              }}>
                {selectedContent.contentType.toLowerCase() === 'html' ? (
                  <div dangerouslySetInnerHTML={{ __html: selectedContent.processedContent }} />
                ) : (
                  <Typography sx={{ whiteSpace: 'pre-wrap' }}>
                    {selectedContent.processedContent}
                  </Typography>
                )}
              </Box>
              
              {/* Metadata Section */}
              {selectedContent.metadata && Object.keys(selectedContent.metadata).length > 0 && (
                <Box sx={{ mt: 3 }}>
                  <Accordion>
                    <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                      <Typography variant="subtitle2">
                        Metadata
                      </Typography>
                    </AccordionSummary>
                    <AccordionDetails>
                      <TableContainer>
                        <Table size="small">
                          <TableBody>
                            {Object.entries(selectedContent.metadata).map(([key, value]) => (
                              <TableRow key={key}>
                                <TableCell component="th" scope="row" sx={{ fontWeight: 'bold' }}>
                                  {key}
                                </TableCell>
                                <TableCell>{value?.toString() || ''}</TableCell>
                              </TableRow>
                            ))}
                          </TableBody>
                        </Table>
                      </TableContainer>
                    </AccordionDetails>
                  </Accordion>
                </Box>
              )}
            </DialogContent>
            <DialogActions>
              <Button 
                startIcon={<DownloadIcon />}
                onClick={() => {
                  // Logic to download content
                  alert('Download functionality would be implemented here');
                }}
              >
                Download
              </Button>
              <Button onClick={handleCloseContentDialog}>Close</Button>
            </DialogActions>
          </>
        )}
      </Dialog>
    </Container>
  );
};

// Simulate API response for content
const simulateScrapedContent = (scraperId, searchTerm, contentType, page, pageSize) => {
  const totalItems = 48;
  const filteredItems = Array.from({ length: totalItems }, (_, i) => {
    const id = `content-${i + 1}`;
    const isHtml = i % 3 === 0;
    const isPdf = i % 3 === 1;
    const isDoc = i % 3 === 2;
    
    let type = isHtml ? 'html' : isPdf ? 'pdf' : 'doc';
    
    // Filter by content type if specified
    if (contentType !== 'all') {
      if (contentType === 'regulatory') {
        // For regulatory content filter
        if (i % 4 !== 0) return null; // Only keep 25% as regulatory
      } else if (type !== contentType) {
        return null;
      }
    }
    
    // Filter by search term if specified
    if (searchTerm && !`Page ${i + 1} Title`.toLowerCase().includes(searchTerm.toLowerCase())) {
      return null;
    }
    
    return {
      id,
      title: `Page ${i + 1} Title`,
      url: `https://example.com/page-${i + 1}`,
      contentType: type,
      capturedAt: new Date(Date.now() - i * 3600000).toISOString(), // Hours ago
      summary: `This is a summary of the content for page ${i + 1}. It provides a brief overview of what can be found in the full content.`,
      processedContent: `<h1>Page ${i + 1} Title</h1>
<p>This is the content of page ${i + 1}. It has been processed and extracted for easier reading.</p>
<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam auctor, nisl eget ultricies tincidunt, 
nisl nisl aliquam nisl, eget aliquam nisl nisl sit amet nisl. Nullam auctor, nisl eget ultricies tincidunt,
nisl nisl aliquam nisl, eget aliquam nisl nisl sit amet nisl.</p>
<h2>Section 1</h2>
<p>This is section 1 of the document. It contains important information about the topic.</p>
<h2>Section 2</h2>
<p>This is section 2 of the document. It provides additional details and context.</p>`,
      rawContent: `<html><body><h1>Page ${i + 1} Title</h1>
<p>This is the content of page ${i + 1}. It has been processed and extracted for easier reading.</p>
<!-- Raw HTML includes all the source HTML -->
</body></html>`,
      hasChanges: i % 5 === 0, // Some items have changes
      metadata: {
        contentType: isHtml ? 'text/html' : isPdf ? 'application/pdf' : 'application/msword',
        contentLength: Math.floor(Math.random() * 50000) + 5000,
        lastModified: new Date(Date.now() - i * 86400000).toISOString(), // Days ago
        isRegulatoryContent: i % 4 === 0
      }
    };
  }).filter(Boolean); // Remove null items
  
  // Paginate results
  const startIndex = (page - 1) * pageSize;
  const endIndex = startIndex + pageSize;
  const paginatedItems = filteredItems.slice(startIndex, endIndex);
  
  return {
    items: paginatedItems,
    totalCount: filteredItems.length,
    page,
    pageSize,
    totalPages: Math.ceil(filteredItems.length / pageSize)
  };
};

export default ContentBrowser;