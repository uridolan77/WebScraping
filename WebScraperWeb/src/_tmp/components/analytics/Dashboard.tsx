import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
  Container, Grid, Paper, Typography, Box,
  Button, FormControl, InputLabel, Select, MenuItem,
  CircularProgress, Divider, Tabs, Tab, Alert,
  SelectChangeEvent
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  TrendingUp as TrendingUpIcon,
  DateRange as DateRangeIcon
} from '@mui/icons-material';
import { useAnalytics } from '../../hooks';
import { getUserFriendlyErrorMessage } from '../../utils/errorHandler';
import OverviewStats from './OverviewStats';
import ContentChangeChart from './ContentChangeChart';
import PerformanceMetrics from './PerformanceMetrics';
import ContentTypeDistribution from './ContentTypeDistribution';
import ScraperComparison from './ScraperComparison';
import TrendAnalysis from './TrendAnalysis';

// TabPanel component for tab content
interface TabPanelProps {
  children?: React.ReactNode;
  value: number;
  index: number;
  [key: string]: any;
}

const TabPanel = React.memo(function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`analytics-tabpanel-${index}`}
      aria-labelledby={`analytics-tab-${index}`}
      {...other}
      style={{ padding: '24px 0' }}
    >
      {value === index && children}
    </div>
  );
});

// Helper function for tab accessibility
function a11yProps(index: number) {
  return {
    id: `analytics-tab-${index}`,
    'aria-controls': `analytics-tabpanel-${index}`,
  };
}

const Dashboard = React.memo(() => {
  const {
    overallData,
    changeData,
    performanceData,
    contentTypeData,
    loading,
    error,
    fetchAllAnalytics,
    fetchContentChangeAnalytics,
    fetchPerformanceMetrics,
    fetchContentTypeDistribution,
    fetchTrendAnalysis,
    fetchScraperComparison
  } = useAnalytics();

  const [timeframe, setTimeframe] = useState('week');
  const [tabValue, setTabValue] = useState(0);
  const [isRefreshing, setIsRefreshing] = useState(false);

  // Fetch analytics data on component mount
  useEffect(() => {
    fetchAllAnalytics(timeframe);
  }, [timeframe, fetchAllAnalytics]);

  // Handle timeframe change
  const handleTimeframeChange = useCallback((event: SelectChangeEvent) => {
    setTimeframe(event.target.value);
  }, []);

  // Handle tab change
  const handleTabChange = useCallback((event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  }, []);

  // Handle refresh
  const handleRefresh = useCallback(async () => {
    setIsRefreshing(true);
    try {
      await fetchAllAnalytics(timeframe, true); // Force refresh from server
    } finally {
      setIsRefreshing(false);
    }
  }, [fetchAllAnalytics, timeframe]);

  // Memoize tab content to prevent unnecessary re-renders
  const contentChangesTab = useMemo(() => (
    <ContentChangeChart
      data={changeData}
      isLoading={loading}
      timeframe={timeframe}
      onRefresh={() => fetchContentChangeAnalytics(timeframe)}
    />
  ), [changeData, loading, timeframe, fetchContentChangeAnalytics]);

  const performanceTab = useMemo(() => (
    <PerformanceMetrics
      data={performanceData}
      isLoading={loading}
      timeframe={timeframe}
      onRefresh={() => fetchPerformanceMetrics(timeframe)}
    />
  ), [performanceData, loading, timeframe, fetchPerformanceMetrics]);

  const contentTypeTab = useMemo(() => (
    <ContentTypeDistribution
      data={contentTypeData}
      isLoading={loading}
      onRefresh={() => fetchContentTypeDistribution(null)}
    />
  ), [contentTypeData, loading, fetchContentTypeDistribution]);

  const trendAnalysisTab = useMemo(() => (
    <TrendAnalysis
      timeframe={timeframe}
      fetchTrendAnalysis={fetchTrendAnalysis}
    />
  ), [timeframe, fetchTrendAnalysis]);

  const scraperComparisonTab = useMemo(() => (
    <ScraperComparison
      timeframe={timeframe}
      fetchScraperComparison={fetchScraperComparison}
    />
  ), [timeframe, fetchScraperComparison]);

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">Analytics Dashboard</Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <FormControl variant="outlined" size="small" sx={{ minWidth: 150 }}>
            <InputLabel id="timeframe-select-label">Timeframe</InputLabel>
            <Select
              labelId="timeframe-select-label"
              value={timeframe}
              onChange={handleTimeframeChange}
              label="Timeframe"
              startAdornment={<DateRangeIcon sx={{ mr: 1 }} />}
            >
              <MenuItem value="day">Last 24 Hours</MenuItem>
              <MenuItem value="week">Last 7 Days</MenuItem>
              <MenuItem value="month">Last 30 Days</MenuItem>
              <MenuItem value="year">Last 12 Months</MenuItem>
            </Select>
          </FormControl>
          <Button
            variant="outlined"
            startIcon={isRefreshing ? <CircularProgress size={20} /> : <RefreshIcon />}
            onClick={handleRefresh}
            disabled={isRefreshing || loading}
          >
            Refresh
          </Button>
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {getUserFriendlyErrorMessage(error, 'Failed to load analytics data')}
        </Alert>
      )}

      {/* Overview Stats */}
      <OverviewStats
        data={overallData}
        isLoading={loading}
      />

      {/* Tabs */}
      <Paper sx={{ mt: 3 }}>
        <Tabs
          value={tabValue}
          onChange={handleTabChange}
          aria-label="analytics tabs"
          variant="scrollable"
          scrollButtons="auto"
          sx={{ borderBottom: 1, borderColor: 'divider' }}
        >
          <Tab label="Content Changes" {...a11yProps(0)} />
          <Tab label="Performance" {...a11yProps(1)} />
          <Tab label="Content Types" {...a11yProps(2)} />
          <Tab label="Trends" {...a11yProps(3)} />
          <Tab label="Scraper Comparison" {...a11yProps(4)} />
        </Tabs>

        <Box sx={{ p: 3 }}>
          <TabPanel value={tabValue} index={0}>
            {contentChangesTab}
          </TabPanel>

          <TabPanel value={tabValue} index={1}>
            {performanceTab}
          </TabPanel>

          <TabPanel value={tabValue} index={2}>
            {contentTypeTab}
          </TabPanel>

          <TabPanel value={tabValue} index={3}>
            {trendAnalysisTab}
          </TabPanel>

          <TabPanel value={tabValue} index={4}>
            {scraperComparisonTab}
          </TabPanel>
        </Box>
      </Paper>
    </Container>
  );
});

export default Dashboard;
