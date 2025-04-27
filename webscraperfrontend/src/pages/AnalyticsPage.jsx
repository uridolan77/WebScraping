import { useState, useEffect } from 'react';
import { 
  Grid, 
  Paper, 
  Typography, 
  Box, 
  Card, 
  CardContent, 
  MenuItem,
  FormControl,
  Select,
  InputLabel
} from '@mui/material';
import PageHeader from '../components/Common/PageHeader';
import LoadingSpinner from '../components/Common/LoadingSpinner';
import ErrorMessage from '../components/Common/ErrorMessage';
import useApiClient from '../hooks/useApiClient';

const AnalyticsPage = () => {
  const { api, loading, error, execute } = useApiClient();
  const [analyticsData, setAnalyticsData] = useState({
    summary: null,
    popularDomains: [],
    errorDistribution: [],
    contentChangeFrequency: []
  });
  
  const [timeRange, setTimeRange] = useState('month');
  
  // Fetch analytics data on component mount and when time range changes
  useEffect(() => {
    fetchAnalyticsData();
  }, [timeRange]);
  
  const fetchAnalyticsData = async () => {
    try {
      // Fetch summary data
      const summary = await execute(() => api.analytics.getSummary());
      
      // Fetch popular domains data
      const popularDomains = await execute(() => api.analytics.getPopularDomains(), { showLoading: false });
      
      // Fetch error distribution data
      const errorDistribution = await execute(() => api.analytics.getErrorDistribution(), { showLoading: false });
      
      // Fetch content change frequency data
      const contentChangeFrequency = await execute(() => api.analytics.getContentChangeFrequency(), { showLoading: false });
      
      // Fetch usage statistics data
      const usageStatistics = await execute(() => api.analytics.getUsageStatistics({ 
        timeRange 
      }), { showLoading: false });
      
      setAnalyticsData({
        summary,
        popularDomains: popularDomains || [],
        errorDistribution: errorDistribution || [],
        contentChangeFrequency: contentChangeFrequency || [],
        usageStatistics
      });
    } catch (error) {
      console.error('Error fetching analytics data:', error);
    }
  };
  
  const handleTimeRangeChange = (event) => {
    setTimeRange(event.target.value);
  };
  
  if (loading && !analyticsData.summary) {
    return <LoadingSpinner message="Loading analytics data..." />;
  }

  if (error && !analyticsData.summary) {
    return (
      <ErrorMessage 
        title="Analytics Error" 
        message="Failed to load analytics data. Please try again later."
        onRetry={fetchAnalyticsData}
      />
    );
  }
  
  // Get data for the summary stats
  const stats = analyticsData.summary || {
    totalScrapers: 0,
    activeScrapers: 0,
    totalPagesScraped: 0,
    averageScrapingTime: 0,
    errorRate: 0,
    changesDetected: 0
  };

  return (
    <>
      <PageHeader 
        title="Analytics" 
        subtitle="Performance insights for your web scraping operations"
      />
      
      {/* Time range filter */}
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'flex-end' }}>
        <FormControl sx={{ minWidth: 200 }}>
          <InputLabel>Time Range</InputLabel>
          <Select
            value={timeRange}
            label="Time Range"
            onChange={handleTimeRangeChange}
          >
            <MenuItem value="day">Last 24 hours</MenuItem>
            <MenuItem value="week">Last 7 days</MenuItem>
            <MenuItem value="month">Last 30 days</MenuItem>
            <MenuItem value="quarter">Last 3 months</MenuItem>
            <MenuItem value="year">Last 12 months</MenuItem>
          </Select>
        </FormControl>
      </Box>
      
      {/* Summary Statistics */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard 
            title="Total Scrapers" 
            value={stats.totalScrapers || 0} 
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard 
            title="Active Scrapers" 
            value={stats.activeScrapers || 0}
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard 
            title="Pages Scraped" 
            value={stats.totalPagesScraped || 0} 
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard 
            title="Error Rate" 
            value={`${stats.errorRate || 0}%`}
          />
        </Grid>
      </Grid>
      
      {/* Charts Section */}
      <Grid container spacing={3}>
        <Grid item xs={12}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Scraping Activity Over Time
            </Typography>
            <Box sx={{ height: 300, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <Typography color="text.secondary">
                Scraping activity chart will be displayed here
              </Typography>
            </Box>
          </Paper>
        </Grid>
        
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Popular Domains
            </Typography>
            <Box sx={{ height: 300, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <Typography color="text.secondary">
                Domain distribution chart will be displayed here
              </Typography>
            </Box>
          </Paper>
        </Grid>
        
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Error Distribution
            </Typography>
            <Box sx={{ height: 300, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <Typography color="text.secondary">
                Error chart will be displayed here
              </Typography>
            </Box>
          </Paper>
        </Grid>
        
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Content Change Frequency
            </Typography>
            <Box sx={{ height: 300, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <Typography color="text.secondary">
                Change frequency chart will be displayed here
              </Typography>
            </Box>
          </Paper>
        </Grid>
        
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Performance Metrics
            </Typography>
            <Box sx={{ height: 300, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <Typography color="text.secondary">
                Performance metrics chart will be displayed here
              </Typography>
            </Box>
          </Paper>
        </Grid>
      </Grid>
    </>
  );
};

// Helper component for stat cards
const StatCard = ({ title, value, subtitle, color }) => {
  return (
    <Card sx={{ height: '100%' }}>
      <CardContent>
        <Typography variant="h6" color="text.secondary" gutterBottom>
          {title}
        </Typography>
        <Typography variant="h3" component="div" color={color}>
          {value}
        </Typography>
        {subtitle && (
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            {subtitle}
          </Typography>
        )}
      </CardContent>
    </Card>
  );
};

export default AnalyticsPage;