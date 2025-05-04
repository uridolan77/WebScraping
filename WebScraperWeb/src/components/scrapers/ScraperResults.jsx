import React, { useState, useEffect } from 'react';
import { 
  Box, Typography, Tabs, Tab, CircularProgress, Alert
} from '@mui/material';
import ResultsAllTab from './results/ResultsAllTab';
import ResultsChangesTab from './results/ResultsChangesTab';
import ResultsDocumentsTab from './results/ResultsDocumentsTab';
import ResultsClassificationTab from './results/ResultsClassificationTab';
import ViewItemDialog from './dialogs/ViewItemDialog';
import CompareVersionsDialog from './dialogs/CompareVersionsDialog';
import { getScraperResults, getScraperChanges, getProcessedDocuments } from '../../api/scrapers';
import { getContentClassifications, getClassificationStatistics } from '../../api/contentClassification';
import { getUserFriendlyErrorMessage } from '../../utils/errorHandler';

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
  const [classifications, setClassifications] = useState([]);
  const [classificationStats, setClassificationStats] = useState(null);
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
          case 3: // Classifications
            const classificationsData = await getContentClassifications(scraperId, 50, true);
            setClassifications(classificationsData || []);
            
            // Also fetch classification statistics
            const statsData = await getClassificationStatistics(scraperId, true);
            setClassificationStats(statsData);
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
          <Tab label="Classification" {...a11yProps(3)} />
        </Tabs>
      </Box>
      
      {/* All Results Tab */}
      <TabPanel value={tabValue} index={0}>
        <ResultsAllTab 
          results={results}
          loading={loading}
          error={error}
          page={page}
          totalPages={totalPages}
          searchTerm={searchTerm}
          sortOrder={sortOrder}
          handlePageChange={handlePageChange}
          handleSearch={handleSearch}
          handleSortOrderChange={handleSortOrderChange}
          handleViewItem={handleViewItem}
          setPage={setPage}
          setSearchTerm={setSearchTerm}
        />
      </TabPanel>
      
      {/* Changes Tab */}
      <TabPanel value={tabValue} index={1}>
        <ResultsChangesTab 
          changes={changes}
          loading={loading}
          error={error}
          handleCompareItems={handleCompareItems}
          setTabValue={setTabValue}
        />
      </TabPanel>
      
      {/* Documents Tab */}
      <TabPanel value={tabValue} index={2}>
        <ResultsDocumentsTab 
          documents={documents}
          loading={loading}
          error={error}
          page={page}
          totalPages={totalPages}
          searchTerm={searchTerm}
          contentTypeFilter={contentTypeFilter}
          handlePageChange={handlePageChange}
          handleSearch={handleSearch}
          handleContentTypeFilterChange={handleContentTypeFilterChange}
          handleViewItem={handleViewItem}
          setPage={setPage}
          setContentTypeFilter={setContentTypeFilter}
        />
      </TabPanel>
      
      {/* Classification Tab */}
      <TabPanel value={tabValue} index={3}>
        <ResultsClassificationTab 
          classifications={classifications}
          statistics={classificationStats}
          loading={loading}
          error={error}
          scraperId={scraperId}
          handleViewItem={handleViewItem}
        />
      </TabPanel>
      
      {/* View Item Dialog */}
      <ViewItemDialog 
        open={dialogOpen}
        onClose={handleDialogClose}
        selectedItem={selectedItem}
      />
      
      {/* Compare Dialog */}
      <CompareVersionsDialog 
        open={compareDialogOpen}
        onClose={handleCompareDialogClose}
        compareItems={compareItems}
      />
    </Box>
  );
};

export default ScraperResults;
