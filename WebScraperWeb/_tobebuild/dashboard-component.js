// src/pages/Dashboard.jsx
import React, { useState, useEffect } from 'react';
import { Container, Grid, Card, CardContent, Typography, Box, Button, CircularProgress } from '@mui/material';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import { Link } from 'react-router-dom';
import { getAllScrapers } from '../api/scrapers';

// Mock data for analytics summary
const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8'];

const Dashboard = () => {
  const [isLoading, setIsLoading] = useState(true);
  const [scrapers, setScrapers] = useState([]);
  const [activeScrapers, setActiveScrapers] = useState(0);
  const [errorScrapers, setErrorScrapers] = useState(0);
  const [totalUrls, setTotalUrls] = useState(0);
  const [statusData, setStatusData] = useState([]);
  const [recentScrapers, setRecentScrapers] = useState([]);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setIsLoading(true);
        const data = await getAllScrapers();
        setScrapers(data || []);
        
        // Calculate dashboard metrics
        const active = data.filter(s => s.isRunning).length;
        const withErrors = data.filter(s => s.hasErrors).length;
        const urlsProcessed = data.reduce((sum, s) => sum + (s.urlsProcessed || 0), 0);
        
        setActiveScrapers(active);
        setErrorScrapers(withErrors);
        setTotalUrls(urlsProcessed);
        
        // Prepare status data for pie chart
        setStatusData([
          { name: 'Running', value: active },
          { name: 'Idle', value: data.length - active - withErrors },
          { name: 'Error', value: withErrors }
        ]);
        
        // Recent scrapers (sort by lastUpdate)
        const recent = [...data]
          .sort((a, b) => new Date(b.lastRun || 0) - new Date(a.lastRun || 0))
          .slice(0, 5);
        setRecentScrapers(recent);
      } catch (error) {
        console.error('Error fetching dashboard data:', error);
      } finally {
        setIsLoading(false);
      }
    };
    
    fetchData();
    
    // Refresh data every 30 seconds
    const interval = setInterval(fetchData, 30000);
    return () => clearInterval(interval);
  }, []);

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Typography variant="h4" gutterBottom>
        Dashboard
      </Typography>
      
      {/* Summary Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card sx={{ bgcolor: '#f5f5f5' }}>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Total Scrapers
              </Typography>
              <Typography variant="h4" component="div">
                {scrapers.length}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card sx={{ bgcolor: '#e8f5e9' }}>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Active Scrapers
              </Typography>
              <Typography variant="h4" component="div" color="success.main">
                {activeScrapers}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card sx={{ bgcolor: '#fff8e1' }}>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                URLs Processed
              </Typography>
              <Typography variant="h4" component="div">
                {totalUrls.toLocaleString()}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card sx={{ bgcolor: errorScrapers > 0 ? '#ffebee' : '#f5f5f5' }}>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Scrapers with Errors
              </Typography>
              <Typography variant="h4" component="div" color={errorScrapers > 0 ? "error" : "textPrimary"}>
                {errorScrapers}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
      
      {/* Charts Row */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} md={8}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Scraper Status
              </Typography>
              <Box sx={{ height: 300 }}>
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart
                    data={scrapers}
                    margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
                  >
                    <XAxis dataKey="name" />
                    <YAxis />
                    <Tooltip />
                    <Bar dataKey="urlsProcessed" name="URLs Processed" fill="#8884d8" />
                  </BarChart>
                </ResponsiveContainer>
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Scraper Status Distribution
              </Typography>
              <Box sx={{ height: 300, display: 'flex', justifyContent: 'center' }}>
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={statusData}
                      cx="50%"
                      cy="50%"
                      labelLine={false}
                      label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
                      outerRadius={80}
                      fill="#8884d8"
                      dataKey="value"
                    >
                      {statusData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
      
      {/* Recent Scrapers Table */}
      <Card>
        <CardContent>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6">
              Recent Scrapers
            </Typography>
            <Button component={Link} to="/scrapers" variant="outlined" color="primary">
              View All
            </Button>
          </Box>
          
          {recentScrapers.length > 0 ? (
            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
              <thead>
                <tr style={{ borderBottom: '1px solid #ddd' }}>
                  <th style={{ textAlign: 'left', padding: '8px' }}>Name</th>
                  <th style={{ textAlign: 'left', padding: '8px' }}>Status</th>
                  <th style={{ textAlign: 'left', padding: '8px' }}>Last Run</th>
                  <th style={{ textAlign: 'left', padding: '8px' }}>URLs Processed</th>
                  <th style={{ textAlign: 'left', padding: '8px' }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {recentScrapers.map((scraper) => (
                  <tr key={scraper.id} style={{ borderBottom: '1px solid #ddd' }}>
                    <td style={{ padding: '8px' }}>{scraper.name}</td>
                    <td style={{ padding: '8px' }}>
                      <Box
                        sx={{
                          display: 'inline-block',
                          px: 1,
                          py: 0.5,
                          borderRadius: 1,
                          bgcolor: scraper.isRunning 
                            ? 'success.light' 
                            : scraper.hasErrors 
                              ? 'error.light' 
                              : 'grey.300',
                          color: scraper.isRunning 
                            ? 'success.contrastText' 
                            : scraper.hasErrors 
                              ? 'error.contrastText' 
                              : 'grey.contrastText',
                        }}
                      >
                        {scraper.isRunning ? 'Running' : scraper.hasErrors ? 'Error' : 'Idle'}
                      </Box>
                    </td>
                    <td style={{ padding: '8px' }}>
                      {scraper.lastRun 
                        ? new Date(scraper.lastRun).toLocaleString() 
                        : 'Never'}
                    </td>
                    <td style={{ padding: '8px' }}>{scraper.urlsProcessed || 0}</td>
                    <td style={{ padding: '8px' }}>
                      <Button
                        component={Link}
                        to={`/scrapers/${scraper.id}`}
                        variant="text"
                        size="small"
                        sx={{ mr: 1 }}
                      >
                        View
                      </Button>
                      <Button
                        component={Link}
                        to={`/scrapers/${scraper.id}/edit`}
                        variant="text"
                        size="small"
                        color="secondary"
                      >
                        Edit
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <Typography variant="body1" sx={{ textAlign: 'center', py: 4 }}>
              No scrapers found. Create your first scraper to get started.
            </Typography>
          )}
        </CardContent>
      </Card>
    </Container>
  );
};

export default Dashboard;