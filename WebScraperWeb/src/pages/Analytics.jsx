import React, { useState, useEffect } from 'react';
import { 
  Container, 
  Box, 
  Typography, 
  Paper, 
  Grid, 
  Card, 
  CardContent,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Divider,
  Alert
} from '@mui/material';
import { 
  BarChart, 
  Bar, 
  LineChart, 
  Line, 
  PieChart, 
  Pie, 
  Cell, 
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Tooltip, 
  Legend, 
  ResponsiveContainer 
} from 'recharts';
import PageHeader from '../components/common/PageHeader';
import LoadingSpinner from '../components/common/LoadingSpinner';
import { useAnalytics } from '../hooks/useAnalytics';

// Mock data for charts
const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8'];

const Analytics = () => {
  const { 
    fetchOverallAnalytics, 
    fetchContentChangeAnalytics, 
    fetchPerformanceMetrics, 
    fetchContentTypeDistribution, 
    fetchRegulatoryImpactAnalysis,
    overallData,
    changeData,
    performanceData,
    contentTypeData,
    regulatoryImpactData,
    loading,
    error
  } = useAnalytics();
  
  const [timeframe, setTimeframe] = useState('week');

  // Fetch analytics data
  useEffect(() => {
    fetchOverallAnalytics(timeframe);
    fetchContentChangeAnalytics(timeframe);
    fetchPerformanceMetrics(timeframe);
    fetchContentTypeDistribution();
    fetchRegulatoryImpactAnalysis(timeframe);
  }, [
    timeframe, 
    fetchOverallAnalytics, 
    fetchContentChangeAnalytics, 
    fetchPerformanceMetrics, 
    fetchContentTypeDistribution, 
    fetchRegulatoryImpactAnalysis
  ]);

  const handleTimeframeChange = (event) => {
    setTimeframe(event.target.value);
  };

  if (loading && !overallData) {
    return <LoadingSpinner message="Loading analytics data..." />;
  }

  // Mock data for demonstration
  const mockOverallData = [
    { name: 'UKGC', pages: 120, changes: 15 },
    { name: 'MGA', pages: 85, changes: 8 },
    { name: 'Gibraltar', pages: 65, changes: 5 },
    { name: 'Isle of Man', pages: 45, changes: 3 },
    { name: 'Alderney', pages: 30, changes: 2 },
  ];

  const mockChangeData = [
    { date: '2023-01-01', changes: 5 },
    { date: '2023-02-01', changes: 8 },
    { date: '2023-03-01', changes: 12 },
    { date: '2023-04-01', changes: 7 },
    { date: '2023-05-01', changes: 15 },
    { date: '2023-06-01', changes: 9 },
  ];

  const mockContentTypeData = [
    { name: 'HTML', value: 65 },
    { name: 'PDF', value: 20 },
    { name: 'Images', value: 10 },
    { name: 'Other', value: 5 },
  ];

  const mockPerformanceData = [
    { date: '2023-01-01', crawlTime: 120, processingTime: 45 },
    { date: '2023-02-01', crawlTime: 110, processingTime: 40 },
    { date: '2023-03-01', crawlTime: 130, processingTime: 50 },
    { date: '2023-04-01', crawlTime: 100, processingTime: 35 },
    { date: '2023-05-01', crawlTime: 140, processingTime: 55 },
    { date: '2023-06-01', crawlTime: 125, processingTime: 48 },
  ];

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      {/* Header */}
      <PageHeader
        title="Analytics"
        subtitle="Insights and statistics about your web scrapers"
        breadcrumbs={[
          { text: 'Dashboard', path: '/' },
          { text: 'Analytics' }
        ]}
      />

      {/* Error Alert */}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          Error loading analytics data: {error}
        </Alert>
      )}

      {/* Timeframe Selector */}
      <Paper sx={{ p: 2, mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Typography variant="h6">Analytics Dashboard</Typography>
          <FormControl variant="outlined" size="small" sx={{ minWidth: 150 }}>
            <InputLabel id="timeframe-select-label">Timeframe</InputLabel>
            <Select
              labelId="timeframe-select-label"
              value={timeframe}
              onChange={handleTimeframeChange}
              label="Timeframe"
            >
              <MenuItem value="day">Last 24 Hours</MenuItem>
              <MenuItem value="week">Last 7 Days</MenuItem>
              <MenuItem value="month">Last 30 Days</MenuItem>
              <MenuItem value="quarter">Last 90 Days</MenuItem>
              <MenuItem value="year">Last Year</MenuItem>
            </Select>
          </FormControl>
        </Box>
      </Paper>

      {/* Summary Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" gutterBottom>
                Total Pages Scraped
              </Typography>
              <Typography variant="h4">
                345
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" gutterBottom>
                Content Changes Detected
              </Typography>
              <Typography variant="h4" color="primary">
                33
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" gutterBottom>
                Documents Processed
              </Typography>
              <Typography variant="h4">
                42
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" gutterBottom>
                Regulatory Changes
              </Typography>
              <Typography variant="h4" color="error">
                7
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Charts */}
      <Grid container spacing={3}>
        {/* Pages Scraped by Source */}
        <Grid item xs={12} md={8}>
          <Paper sx={{ p: 2, height: '100%' }}>
            <Typography variant="h6" gutterBottom>
              Pages Scraped by Source
            </Typography>
            <Divider sx={{ mb: 2 }} />
            
            <Box sx={{ height: 300 }}>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart
                  data={mockOverallData}
                  margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Bar dataKey="pages" name="Pages Scraped" fill="#8884d8" />
                  <Bar dataKey="changes" name="Changes Detected" fill="#82ca9d" />
                </BarChart>
              </ResponsiveContainer>
            </Box>
          </Paper>
        </Grid>
        
        {/* Content Type Distribution */}
        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 2, height: '100%' }}>
            <Typography variant="h6" gutterBottom>
              Content Type Distribution
            </Typography>
            <Divider sx={{ mb: 2 }} />
            
            <Box sx={{ height: 300 }}>
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={mockContentTypeData}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
                    outerRadius={80}
                    fill="#8884d8"
                    dataKey="value"
                  >
                    {mockContentTypeData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
            </Box>
          </Paper>
        </Grid>
        
        {/* Content Changes Over Time */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Content Changes Over Time
            </Typography>
            <Divider sx={{ mb: 2 }} />
            
            <Box sx={{ height: 300 }}>
              <ResponsiveContainer width="100%" height="100%">
                <LineChart
                  data={mockChangeData}
                  margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="date" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Line 
                    type="monotone" 
                    dataKey="changes" 
                    name="Content Changes" 
                    stroke="#8884d8" 
                    activeDot={{ r: 8 }} 
                  />
                </LineChart>
              </ResponsiveContainer>
            </Box>
          </Paper>
        </Grid>
        
        {/* Performance Metrics */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Performance Metrics
            </Typography>
            <Divider sx={{ mb: 2 }} />
            
            <Box sx={{ height: 300 }}>
              <ResponsiveContainer width="100%" height="100%">
                <LineChart
                  data={mockPerformanceData}
                  margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="date" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Line 
                    type="monotone" 
                    dataKey="crawlTime" 
                    name="Crawl Time (s)" 
                    stroke="#8884d8" 
                  />
                  <Line 
                    type="monotone" 
                    dataKey="processingTime" 
                    name="Processing Time (s)" 
                    stroke="#82ca9d" 
                  />
                </LineChart>
              </ResponsiveContainer>
            </Box>
          </Paper>
        </Grid>
      </Grid>
    </Container>
  );
};

export default Analytics;
