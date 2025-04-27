import { useState, useEffect } from 'react';
import { 
  Grid, 
  Paper, 
  Typography, 
  Box,
  Card,
  CardContent
} from '@mui/material';
import PageHeader from '../components/Common/PageHeader';
import LoadingSpinner from '../components/Common/LoadingSpinner';
import ErrorMessage from '../components/Common/ErrorMessage';
import useApiClient from '../hooks/useApiClient';

const OverviewPage = () => {
  const { api, loading, error, execute } = useApiClient();
  const [dashboardData, setDashboardData] = useState({
    summary: null,
    popularDomains: [],
    contentChangeFrequency: [],
    errorDistribution: []
  });

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        // Fetch summary data
        const summary = await execute(() => api.analytics.getSummary());
        
        // For now, we'll use placeholder data until the actual API is ready
        // You can replace these with actual API calls once they're implemented
        
        setDashboardData({
          summary: summary,
          popularDomains: [], // To be populated from actual API
          contentChangeFrequency: [], // To be populated from actual API
          errorDistribution: [] // To be populated from actual API
        });
      } catch (error) {
        console.error('Error fetching dashboard data:', error);
      }
    };
    
    fetchDashboardData();
  }, [execute, api.analytics]);

  if (loading && !dashboardData.summary) {
    return <LoadingSpinner message="Loading dashboard data..." />;
  }

  if (error && !dashboardData.summary) {
    return (
      <ErrorMessage 
        title="Dashboard Error" 
        message="Failed to load dashboard data. Please try again later."
      />
    );
  }

  // Placeholder data until we connect to real API
  const stats = dashboardData.summary || {
    totalScrapers: 0,
    activeScrapers: 0,
    totalSchedules: 0,
    totalPagesScraped: 0,
    changesDetected: 0,
    errorRate: 0
  };

  return (
    <>
      <PageHeader 
        title="Dashboard" 
        subtitle="Overview of your web scraping operations"
      />

      <Grid container spacing={3}>
        {/* Summary Stats */}
        <Grid item xs={12} md={4}>
          <StatCard 
            title="Total Scrapers" 
            value={stats.totalScrapers || 0}
            subtitle="Configured scrapers"
          />
        </Grid>
        <Grid item xs={12} md={4}>
          <StatCard 
            title="Active Scrapers" 
            value={stats.activeScrapers || 0} 
            subtitle="Currently running"
          />
        </Grid>
        <Grid item xs={12} md={4}>
          <StatCard 
            title="Total Schedules" 
            value={stats.totalSchedules || 0}
            subtitle="Configured schedules" 
          />
        </Grid>
        <Grid item xs={12} md={4}>
          <StatCard 
            title="Pages Scraped" 
            value={stats.totalPagesScraped || 0} 
            subtitle="Total pages processed"
          />
        </Grid>
        <Grid item xs={12} md={4}>
          <StatCard 
            title="Changes Detected" 
            value={stats.changesDetected || 0} 
            subtitle="Content changes found"
          />
        </Grid>
        <Grid item xs={12} md={4}>
          <StatCard 
            title="Error Rate" 
            value={`${stats.errorRate || 0}%`} 
            subtitle="Average error rate"
          />
        </Grid>

        {/* Charts Section */}
        <Grid item xs={12}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Recent Activity
            </Typography>
            <Box sx={{ height: 300, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <Typography color="text.secondary">
                Activity chart will be displayed here
              </Typography>
            </Box>
          </Paper>
        </Grid>
        
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 2 }}>
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
          <Paper sx={{ p: 2 }}>
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
      </Grid>
    </>
  );
};

// Helper component for stat cards
const StatCard = ({ title, value, subtitle }) => {
  return (
    <Card sx={{ height: '100%' }}>
      <CardContent>
        <Typography variant="h6" color="text.secondary" gutterBottom>
          {title}
        </Typography>
        <Typography variant="h3" component="div">
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

export default OverviewPage;