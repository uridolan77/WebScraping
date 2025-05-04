import React, { useState } from 'react';
import { 
  Box, Typography, Card, CardContent, Grid, Button,
  Tabs, Tab, CircularProgress, Alert, Divider,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
  Chip, Tooltip, IconButton
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  Visibility as ViewIcon,
  OpenInNew as OpenInNewIcon
} from '@mui/icons-material';
import { format } from 'date-fns';
import ClassificationStatistics from '../../classification/ClassificationStatistics';
import ContentClassificationView from '../../classification/ContentClassificationView';
import { getContentClassifications, getClassificationStatistics } from '../../../api/contentClassification';

// TabPanel component for nested tabs
function NestedTabPanel(props) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`classification-tabpanel-${index}`}
      aria-labelledby={`classification-tab-${index}`}
      {...other}
      style={{ padding: '16px 0' }}
    >
      {value === index && children}
    </div>
  );
}

// Helper function for nested tab accessibility
function nestedA11yProps(index) {
  return {
    id: `classification-tab-${index}`,
    'aria-controls': `classification-tabpanel-${index}`,
  };
}

const ResultsClassificationTab = ({ 
  classifications, 
  statistics, 
  loading, 
  error, 
  scraperId,
  handleViewItem
}) => {
  const [nestedTabValue, setNestedTabValue] = useState(0);
  const [selectedUrl, setSelectedUrl] = useState(null);
  const [refreshing, setRefreshing] = useState(false);

  // Handle nested tab change
  const handleNestedTabChange = (event, newValue) => {
    setNestedTabValue(newValue);
  };

  // Handle refresh
  const handleRefresh = async () => {
    if (!scraperId) return;
    
    setRefreshing(true);
    
    try {
      await getContentClassifications(scraperId, 50, true);
      await getClassificationStatistics(scraperId, true);
      // The parent component will re-fetch the data due to the cache being invalidated
    } catch (err) {
      console.error('Error refreshing classification data:', err);
    } finally {
      setRefreshing(false);
    }
  };

  // Handle view classification
  const handleViewClassification = (url) => {
    setSelectedUrl(url);
    setNestedTabValue(2); // Switch to the Detail tab
  };

  // Render document type chip
  const renderDocumentTypeChip = (type) => {
    switch (type) {
      case 'Regulation':
        return <Chip label={type} size="small" color="error" />;
      case 'Guidance':
        return <Chip label={type} size="small" color="primary" />;
      case 'News':
        return <Chip label={type} size="small" color="success" />;
      default:
        return <Chip label={type || 'Unknown'} size="small" color="default" />;
    }
  };

  // Render sentiment chip
  const renderSentimentChip = (sentiment) => {
    switch (sentiment) {
      case 'Positive':
        return <Chip label={sentiment} size="small" color="success" />;
      case 'Negative':
        return <Chip label={sentiment} size="small" color="error" />;
      case 'Neutral':
        return <Chip label={sentiment} size="small" color="info" />;
      default:
        return <Chip label={sentiment || 'Unknown'} size="small" color="default" />;
    }
  };

  return (
    <>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h6">Content Classification</Typography>
        <Button
          variant="outlined"
          startIcon={(loading || refreshing) ? <CircularProgress size={20} /> : <RefreshIcon />}
          onClick={handleRefresh}
          disabled={loading || refreshing}
        >
          Refresh
        </Button>
      </Box>
      
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
        <Tabs 
          value={nestedTabValue} 
          onChange={handleNestedTabChange} 
          aria-label="classification tabs"
        >
          <Tab label="Overview" {...nestedA11yProps(0)} />
          <Tab label="Classifications" {...nestedA11yProps(1)} />
          <Tab label="Detail" {...nestedA11yProps(2)} disabled={!selectedUrl} />
        </Tabs>
      </Box>
      
      {/* Overview Tab */}
      <NestedTabPanel value={nestedTabValue} index={0}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : error ? (
          <Alert severity="error" sx={{ mb: 3 }}>
            {error}
          </Alert>
        ) : (
          <ClassificationStatistics scraperId={scraperId} />
        )}
      </NestedTabPanel>
      
      {/* Classifications Tab */}
      <NestedTabPanel value={nestedTabValue} index={1}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : error ? (
          <Alert severity="error" sx={{ mb: 3 }}>
            {error}
          </Alert>
        ) : classifications.length > 0 ? (
          <TableContainer component={Paper}>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>URL</TableCell>
                  <TableCell>Document Type</TableCell>
                  <TableCell>Sentiment</TableCell>
                  <TableCell>Confidence</TableCell>
                  <TableCell>Classified At</TableCell>
                  <TableCell>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {classifications.map((classification) => (
                  <TableRow key={classification.id} hover>
                    <TableCell>
                      <Tooltip title={classification.url}>
                        <Typography 
                          variant="body2" 
                          sx={{ 
                            maxWidth: 250, 
                            overflow: 'hidden', 
                            textOverflow: 'ellipsis', 
                            whiteSpace: 'nowrap' 
                          }}
                        >
                          {classification.url}
                        </Typography>
                      </Tooltip>
                    </TableCell>
                    <TableCell>{renderDocumentTypeChip(classification.documentType)}</TableCell>
                    <TableCell>{renderSentimentChip(classification.overallSentiment)}</TableCell>
                    <TableCell>{Math.round(classification.confidence * 100)}%</TableCell>
                    <TableCell>
                      {classification.classifiedAt ? format(new Date(classification.classifiedAt), 'yyyy-MM-dd HH:mm') : ''}
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', gap: 1 }}>
                        <Tooltip title="View Classification">
                          <IconButton 
                            size="small" 
                            onClick={() => handleViewClassification(classification.url)}
                          >
                            <ViewIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Open URL">
                          <IconButton 
                            size="small"
                            component="a"
                            href={classification.url}
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
            No classifications found. Process some content to generate classifications.
          </Alert>
        )}
      </NestedTabPanel>
      
      {/* Detail Tab */}
      <NestedTabPanel value={nestedTabValue} index={2}>
        {selectedUrl ? (
          <ContentClassificationView scraperId={scraperId} url={selectedUrl} />
        ) : (
          <Alert severity="info">
            Select a classification to view details.
          </Alert>
        )}
      </NestedTabPanel>
    </>
  );
};

export default ResultsClassificationTab;
