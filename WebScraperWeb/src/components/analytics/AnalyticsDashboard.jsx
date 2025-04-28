// src/pages/AnalyticsDashboard.jsx
import React, { useState, useEffect } from 'react';
import { 
  Container, Box, Paper, Typography, Grid, Card, CardContent, 
  CircularProgress, Divider, Tabs, Tab, ButtonGroup, Button,
  Tooltip, IconButton
} from '@mui/material';
import {
  Timeline as TimelineIcon,
  TrendingUp as TrendingUpIcon,
  Language as LanguageIcon,
  Storage as StorageIcon,
  Event as EventIcon,
  FileDownload as FileDownloadIcon,
  Refresh as RefreshIcon
} from '@mui/icons-material';
import { 
  LineChart, Line, BarChart, Bar, PieChart, Pie, 
  XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, 
  Legend, ResponsiveContainer, Cell
} from 'recharts';
import { getScraperAnalytics, getAnalyticsSummary, getPopularDomains, getContentChangeFrequency } from '../api/analytics';

// Tab panel component
function TabPanel(props) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`analytics-tabpanel-${index}`}
      aria-labelledby={`analytics-tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ p: 3 }}>
          {children}
        </Box>
      )}
    </div>
  );
}

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884d8', '#82ca9d'];

const AnalyticsDashboard = () => {
  const [loading, setLoading] = useState(true);
  const [summary, setSummary] = useState(null);
  const [activeTab, setActiveTab] = useState(0);
  const [timeRange, setTimeRange] = useState('week'); // 'day', 'week', 'month'
  const [popularDomains, setPopularDomains] = useState([]);
  const [changeFrequency, setChangeFrequency] = useState(null);
  const [scraperPerformance, setScraperPerformance] = useState([]);
  
  useEffect(() => {
    fetchAnalyticsData();
  }, [timeRange]);
  
  const fetchAnalyticsData = async () => {
    setLoading(true);
    try {
      // Fetch overall analytics summary
      const summaryData = await getAnalyticsSummary();
      setSummary(summaryData);
      
      // Fetch most popular domains
      const domainsData = await getPopularDomains(10);
      setPopularDomains(domainsData.PopularDomains || []);
      
      // Fetch content change frequency
      const since = getDateBySince(timeRange);
      const frequencyData = await getContentChangeFrequency({ since });
      setChangeFrequency(frequencyData.ChangeFrequency || []);
      
      // Create simulated performance data
      const performance = createSimulatedPerformanceData(timeRange);
      setScraperPerformance(performance);
    } catch (error) {
      console.error('Error fetching analytics data:', error);
    } finally {
      setLoading(false);
    }
  };
  
  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
  };
  
  const handleTimeRangeChange = (range) => {
    setTimeRange(range);
  };
  
  // Helper function to get date based on time range
  const getDateBySince = (range) => {
    const now = new Date();
    switch (range) {
      case 'day':
        return new Date(now.setDate(now.getDate() - 1));
      case 'week':
        return new Date(now.setDate(now.getDate() - 7));
      case 'month':
        return new Date(now.setMonth(now.getMonth() - 1));
      default:
        return new Date(now.setDate(now.getDate() - 7));
    }
  };
  
  // Create simulated performance data for demo purposes
  const createSimulatedPerformanceData = (range) => {
    const count = range === 'day' ? 24 : range === 'week' ? 7 : 30;
    return Array.from({ length: count }, (_, i) => {
      const factor = Math.random() * 0.5 + 0.75; // Random factor between 0.75 and 1.25
      return {
        name: range === 'day' ? `${i}:00` : (range === 'week' ? `Day ${i+1}` : `Day ${i+1}`),
        urlsProcessed: Math.floor(500 * factor),
        documentsExtracted: Math.floor(50 * factor),
        changesDetected: Math.floor(15 * factor),
      };
    });
  };
  
  if (loading) {
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
            Analytics Dashboard
          </Typography>
          <Box>
            <ButtonGroup variant="outlined" sx={{ mr: 2 }}>
              <Button 
                onClick={() => handleTimeRangeChange('day')}
                variant={timeRange === 'day' ? 'contained' : 'outlined'}
              >
                24 Hours
              </Button>
              <Button 
                onClick={() => handleTimeRangeChange('week')}
                variant={timeRange === 'week' ? 'contained' : 'outlined'}
              >
                Week
              </Button>
              <Button 
                onClick={() => handleTimeRangeChange('month')}
                variant={timeRange === 'month' ? 'contained' : 'outlined'}
              >
                Month
              </Button>
            </ButtonGroup>
            <Tooltip title="Refresh Data">
              <IconButton onClick={fetchAnalyticsData}>
                <RefreshIcon />
              </IconButton>
            </Tooltip>
          </Box>
        </Box>
        
        <Divider sx={{ mb: 3 }} />
        
        {/* Summary Cards */}
        <Grid container spacing={3} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Total Scrapers
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  <StorageIcon sx={{ mr: 1, color: 'primary.main' }} />
                  <Typography variant="h4" component="div">
                    {summary?.totalScrapers || 0}
                  </Typography>
                </Box>
                <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>
                  {summary?.activeScrapers || 0} currently active
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  URLs Processed
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  <LanguageIcon sx={{ mr: 1, color: 'success.main' }} />
                  <Typography variant="h4" component="div">
                    {summary?.totalUrlsProcessed?.toLocaleString() || 0}
                  </Typography>
                </Box>
                <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>
                  In the last {timeRange === 'day' ? '24 hours' : timeRange}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Documents Processed
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  <FileDownloadIcon sx={{ mr: 1, color: 'info.main' }} />
                  <Typography variant="h4" component="div">
                    {summary?.totalDocumentsProcessed?.toLocaleString() || 0}
                  </Typography>
                </Box>
                <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>
                  PDFs, Office docs, etc.
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Content Changes
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  <TimelineIcon sx={{ mr: 1, color: 'warning.main' }} />
                  <Typography variant="h4" component="div">
                    {summary?.totalContentChanges?.toLocaleString() || 0}
                  </Typography>
                </Box>
                <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>
                  Significant changes detected
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
        
        {/* Tabs for different chart views */}
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={activeTab} onChange={handleTabChange} aria-label="analytics tabs">
            <Tab icon={<TrendingUpIcon />} label="Performance" />
            <Tab icon={<LanguageIcon />} label="Domains" />
            <Tab icon={<TimelineIcon />} label="Changes" />
          </Tabs>
        </Box>
        
        {/* Performance Tab */}
        <TabPanel value={activeTab} index={0}>
          <Typography variant="h6" gutterBottom>
            Scraper Performance
          </Typography>
          <ResponsiveContainer width="100%" height={400}>
            <LineChart
              data={scraperPerformance}
              margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
            >
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="name" />
              <YAxis />
              <RechartsTooltip />
              <Legend />
              <Line type="monotone" dataKey="urlsProcessed" stroke="#8884d8" name="URLs Processed" />
              <Line type="monotone" dataKey="documentsExtracted" stroke="#82ca9d" name="Documents Extracted" />
              <Line type="monotone" dataKey="changesDetected" stroke="#ffc658" name="Changes Detected" />
            </LineChart>
          </ResponsiveContainer>
        </TabPanel>
        
        {/* Domains Tab */}
        <TabPanel value={activeTab} index={1}>
          <Typography variant="h6" gutterBottom>
            Most Popular Domains
          </Typography>
          <Grid container spacing={3}>
            <Grid item xs={12} md={7}>
              <ResponsiveContainer width="100%" height={400}>
                <BarChart
                  data={popularDomains}
                  margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                  layout="vertical"
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis type="number" />
                  <YAxis dataKey="domain" type="category" width={150} />
                  <RechartsTooltip />
                  <Legend />
                  <Bar dataKey="scraperCount" fill="#8884d8" name="Scraper Count" />
                  <Bar dataKey="totalUrlsProcessed" fill="#82ca9d" name="URLs Processed" />
                </BarChart>
              </ResponsiveContainer>
            </Grid>
            <Grid item xs={12} md={5}>
              <ResponsiveContainer width="100%" height={400}>
                <PieChart>
                  <Pie
                    data={popularDomains}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    label={({ name, percent }) => `${name} (${(percent * 100).toFixed(0)}%)`}
                    outerRadius={150}
                    fill="#8884d8"
                    dataKey="totalUrlsProcessed"
                    nameKey="domain"
                  >
                    {popularDomains.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <RechartsTooltip />
                </PieChart>
              </ResponsiveContainer>
            </Grid>
          </Grid>
        </TabPanel>
        
        {/* Changes Tab */}
        <TabPanel value={activeTab} index={2}>
          <Typography variant="h6" gutterBottom>
            Content Change Frequency
          </Typography>
          <ResponsiveContainer width="100%" height={400}>
            <BarChart
              data={changeFrequency?.changesByDay || []}
              margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
            >
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="date" tickFormatter={(date) => new Date(date).toLocaleDateString()} />
              <YAxis />
              <RechartsTooltip
                labelFormatter={(label) => new Date(label).toLocaleDateString()}
                formatter={(value) => [`${value} changes`, 'Count']}
              />
              <Legend />
              <Bar dataKey="count" fill="#ff8042" name="Changes Detected" />
            </BarChart>
          </ResponsiveContainer>
        </TabPanel>
      </Paper>
      
      {/* Recent Activity */}
      <Paper sx={{ p: 3 }}>
        <Typography variant="h5" gutterBottom>
          Recent Activity
        </Typography>
        <Divider sx={{ mb: 2 }} />
        
        {summary?.recentScrapers && summary.recentScrapers.length > 0 ? (
          <Grid container spacing={2}>
            {summary.recentScrapers.map((scraper, index) => (
              <Grid item xs={12} key={index}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', p: 2, bgcolor: 'background.default', borderRadius: 1 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center' }}>
                    <EventIcon sx={{ mr: 1, color: scraper.isRunning ? 'success.main' : 'text.secondary' }} />
                    <Box>
                      <Typography variant="subtitle1">{scraper.name}</Typography>
                      <Typography variant="body2" color="textSecondary">
                        {scraper.isRunning ? 'Currently running' : 'Last updated ' + new Date(scraper.lastUpdate).toLocaleString()}
                      </Typography>
                    </Box>
                  </Box>
                  <Typography>
                    {scraper.urlsProcessed.toLocaleString()} URLs processed
                  </Typography>
                </Box>
              </Grid>
            ))}
          </Grid>
        ) : (
          <Typography variant="body2" color="textSecondary">
            No recent activity found
          </Typography>
        )}
      </Paper>
    </Container>
  );
};

export default AnalyticsDashboard;