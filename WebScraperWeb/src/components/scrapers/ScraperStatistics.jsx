import React from 'react';
import { 
  Box, Typography, Grid, Card, CardContent, 
  CircularProgress, Divider, Paper, Tooltip,
  List, ListItem, ListItemText, ListItemIcon,
  Skeleton
} from '@mui/material';
import {
  Language as LanguageIcon,
  Storage as StorageIcon,
  Speed as SpeedIcon,
  Schedule as ScheduleIcon,
  Update as UpdateIcon,
  Link as LinkIcon,
  FindInPage as FindInPageIcon,
  ChangeHistory as ChangeHistoryIcon,
  Error as ErrorIcon,
  Warning as WarningIcon,
  Info as InfoIcon
} from '@mui/icons-material';
import { format, formatDistanceToNow } from 'date-fns';
import { 
  LineChart, Line, BarChart, Bar, PieChart, Pie, Cell,
  XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, 
  Legend, ResponsiveContainer 
} from 'recharts';

const StatCard = ({ title, value, icon, color, isLoading }) => {
  return (
    <Card variant="outlined">
      <CardContent>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
          <Box sx={{ 
            display: 'flex', 
            alignItems: 'center', 
            justifyContent: 'center',
            bgcolor: `${color}.100`, 
            color: `${color}.800`,
            borderRadius: '50%',
            p: 1,
            mr: 2
          }}>
            {icon}
          </Box>
          <Typography variant="subtitle1" color="text.secondary">
            {title}
          </Typography>
        </Box>
        {isLoading ? (
          <Skeleton variant="text" width="80%" height={40} />
        ) : (
          <Typography variant="h4" component="div" sx={{ fontWeight: 'medium' }}>
            {value}
          </Typography>
        )}
      </CardContent>
    </Card>
  );
};

const ScraperStatistics = ({ scraper, status, analyticsData, isLoading }) => {
  // Prepare chart data
  const urlsOverTimeData = analyticsData?.urlsOverTime || [
    { date: '2023-01-01', count: 0 },
    { date: '2023-01-02', count: 0 },
    { date: '2023-01-03', count: 0 },
    { date: '2023-01-04', count: 0 },
    { date: '2023-01-05', count: 0 },
    { date: '2023-01-06', count: 0 },
    { date: '2023-01-07', count: 0 }
  ];
  
  const contentTypeData = analyticsData?.contentTypes || [
    { name: 'HTML', value: 0 },
    { name: 'PDF', value: 0 },
    { name: 'Other', value: 0 }
  ];
  
  const changesOverTimeData = analyticsData?.changesOverTime || [
    { date: '2023-01-01', added: 0, modified: 0, removed: 0 },
    { date: '2023-01-02', added: 0, modified: 0, removed: 0 },
    { date: '2023-01-03', added: 0, modified: 0, removed: 0 },
    { date: '2023-01-04', added: 0, modified: 0, removed: 0 },
    { date: '2023-01-05', added: 0, modified: 0, removed: 0 },
    { date: '2023-01-06', added: 0, modified: 0, removed: 0 },
    { date: '2023-01-07', added: 0, modified: 0, removed: 0 }
  ];
  
  // Colors for pie chart
  const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8'];
  
  return (
    <Box>
      <Grid container spacing={3}>
        {/* Stats Cards */}
        <Grid item xs={12} md={3}>
          <StatCard 
            title="Total URLs" 
            value={status?.urlsProcessed || scraper?.urlsProcessed || 0}
            icon={<LanguageIcon />}
            color="primary"
            isLoading={isLoading}
          />
        </Grid>
        <Grid item xs={12} md={3}>
          <StatCard 
            title="Content Changes" 
            value={analyticsData?.changesDetected || 0}
            icon={<ChangeHistoryIcon />}
            color="warning"
            isLoading={isLoading}
          />
        </Grid>
        <Grid item xs={12} md={3}>
          <StatCard 
            title="Storage Used" 
            value={analyticsData?.storageUsed ? `${(analyticsData.storageUsed / (1024 * 1024)).toFixed(2)} MB` : '0 MB'}
            icon={<StorageIcon />}
            color="info"
            isLoading={isLoading}
          />
        </Grid>
        <Grid item xs={12} md={3}>
          <StatCard 
            title="Avg. Request Time" 
            value={analyticsData?.avgRequestTime ? `${analyticsData.avgRequestTime.toFixed(2)} ms` : '0 ms'}
            icon={<SpeedIcon />}
            color="success"
            isLoading={isLoading}
          />
        </Grid>
        
        {/* URLs Over Time Chart */}
        <Grid item xs={12} md={8}>
          <Paper sx={{ p: 3, height: '100%' }}>
            <Typography variant="h6" gutterBottom>URLs Processed Over Time</Typography>
            {isLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 300 }}>
                <CircularProgress />
              </Box>
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <LineChart
                  data={urlsOverTimeData}
                  margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis 
                    dataKey="date" 
                    tickFormatter={(value) => {
                      const date = new Date(value);
                      return format(date, 'MM/dd');
                    }}
                  />
                  <YAxis />
                  <RechartsTooltip 
                    formatter={(value, name) => [`${value} URLs`, 'Processed']}
                    labelFormatter={(label) => format(new Date(label), 'MMM dd, yyyy')}
                  />
                  <Line 
                    type="monotone" 
                    dataKey="count" 
                    stroke="#8884d8" 
                    activeDot={{ r: 8 }} 
                    name="URLs Processed"
                  />
                </LineChart>
              </ResponsiveContainer>
            )}
          </Paper>
        </Grid>
        
        {/* Content Type Distribution */}
        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 3, height: '100%' }}>
            <Typography variant="h6" gutterBottom>Content Type Distribution</Typography>
            {isLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 300 }}>
                <CircularProgress />
              </Box>
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie
                    data={contentTypeData}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    outerRadius={80}
                    fill="#8884d8"
                    dataKey="value"
                    label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
                  >
                    {contentTypeData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <RechartsTooltip formatter={(value) => [`${value} URLs`, 'Count']} />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            )}
          </Paper>
        </Grid>
        
        {/* Changes Over Time Chart */}
        <Grid item xs={12}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>Content Changes Over Time</Typography>
            {isLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 300 }}>
                <CircularProgress />
              </Box>
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <BarChart
                  data={changesOverTimeData}
                  margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis 
                    dataKey="date" 
                    tickFormatter={(value) => {
                      const date = new Date(value);
                      return format(date, 'MM/dd');
                    }}
                  />
                  <YAxis />
                  <RechartsTooltip 
                    formatter={(value, name) => [`${value} changes`, name]}
                    labelFormatter={(label) => format(new Date(label), 'MMM dd, yyyy')}
                  />
                  <Legend />
                  <Bar dataKey="added" name="Added" fill="#4caf50" />
                  <Bar dataKey="modified" name="Modified" fill="#ff9800" />
                  <Bar dataKey="removed" name="Removed" fill="#f44336" />
                </BarChart>
              </ResponsiveContainer>
            )}
          </Paper>
        </Grid>
        
        {/* Scraper Details */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>Scraper Configuration</Typography>
            <List dense>
              <ListItem>
                <ListItemIcon>
                  <LinkIcon />
                </ListItemIcon>
                <ListItemText 
                  primary="Start URL" 
                  secondary={scraper?.startUrl || 'Not set'} 
                />
              </ListItem>
              <Divider component="li" />
              <ListItem>
                <ListItemIcon>
                  <FindInPageIcon />
                </ListItemIcon>
                <ListItemText 
                  primary="Max Depth" 
                  secondary={scraper?.maxDepth || 'Not set'} 
                />
              </ListItem>
              <Divider component="li" />
              <ListItem>
                <ListItemIcon>
                  <SpeedIcon />
                </ListItemIcon>
                <ListItemText 
                  primary="Max Concurrent Requests" 
                  secondary={scraper?.maxConcurrentRequests || 'Not set'} 
                />
              </ListItem>
              <Divider component="li" />
              <ListItem>
                <ListItemIcon>
                  <ScheduleIcon />
                </ListItemIcon>
                <ListItemText 
                  primary="Delay Between Requests" 
                  secondary={scraper?.delayBetweenRequests ? `${scraper.delayBetweenRequests} ms` : 'Not set'} 
                />
              </ListItem>
              <Divider component="li" />
              <ListItem>
                <ListItemIcon>
                  <StorageIcon />
                </ListItemIcon>
                <ListItemText 
                  primary="Output Directory" 
                  secondary={scraper?.outputDirectory || 'Not set'} 
                />
              </ListItem>
            </List>
          </Paper>
        </Grid>
        
        {/* Recent Activity */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>Recent Activity</Typography>
            {isLoading ? (
              <Box>
                {[1, 2, 3, 4, 5].map((i) => (
                  <Box key={i} sx={{ mb: 2 }}>
                    <Skeleton variant="text" width="60%" />
                    <Skeleton variant="text" width="40%" />
                  </Box>
                ))}
              </Box>
            ) : analyticsData?.recentActivity && analyticsData.recentActivity.length > 0 ? (
              <List dense>
                {analyticsData.recentActivity.map((activity, index) => (
                  <React.Fragment key={index}>
                    <ListItem>
                      <ListItemIcon>
                        {activity.type === 'error' ? (
                          <ErrorIcon color="error" />
                        ) : activity.type === 'warning' ? (
                          <WarningIcon color="warning" />
                        ) : (
                          <InfoIcon color="info" />
                        )}
                      </ListItemIcon>
                      <ListItemText 
                        primary={activity.message} 
                        secondary={activity.timestamp ? formatDistanceToNow(new Date(activity.timestamp), { addSuffix: true }) : ''} 
                      />
                    </ListItem>
                    {index < analyticsData.recentActivity.length - 1 && <Divider component="li" />}
                  </React.Fragment>
                ))}
              </List>
            ) : (
              <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
                No recent activity recorded.
              </Typography>
            )}
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default ScraperStatistics;
