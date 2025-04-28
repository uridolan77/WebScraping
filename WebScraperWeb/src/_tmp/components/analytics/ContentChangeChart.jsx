import React, { useState } from 'react';
import { 
  Paper, Typography, Box, Button, 
  CircularProgress, Grid, FormControl,
  InputLabel, Select, MenuItem, Tooltip,
  Card, CardContent, Divider
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  TrendingUp as TrendingUpIcon,
  TrendingDown as TrendingDownIcon,
  TrendingFlat as TrendingFlatIcon
} from '@mui/icons-material';
import { 
  BarChart, Bar, LineChart, Line, PieChart, Pie, Cell,
  XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, 
  Legend, ResponsiveContainer, Area, AreaChart
} from 'recharts';
import { format } from 'date-fns';

const ContentChangeChart = ({ data, isLoading, timeframe, onRefresh }) => {
  const [chartType, setChartType] = useState('area');
  const [isRefreshing, setIsRefreshing] = useState(false);
  
  // Handle refresh
  const handleRefresh = async () => {
    setIsRefreshing(true);
    try {
      await onRefresh();
    } finally {
      setIsRefreshing(false);
    }
  };
  
  // Handle chart type change
  const handleChartTypeChange = (event) => {
    setChartType(event.target.value);
  };
  
  // Prepare chart data
  const chartData = data?.changesOverTime || [];
  
  // Calculate trend percentages
  const calculateTrend = (data, key) => {
    if (!data || data.length < 2) return { value: 0, direction: 'flat' };
    
    const firstValue = data[0][key] || 0;
    const lastValue = data[data.length - 1][key] || 0;
    
    if (firstValue === 0) return { value: lastValue > 0 ? 100 : 0, direction: lastValue > 0 ? 'up' : 'flat' };
    
    const percentChange = ((lastValue - firstValue) / firstValue) * 100;
    
    return {
      value: Math.abs(percentChange.toFixed(1)),
      direction: percentChange > 0 ? 'up' : percentChange < 0 ? 'down' : 'flat'
    };
  };
  
  const addedTrend = calculateTrend(chartData, 'added');
  const modifiedTrend = calculateTrend(chartData, 'modified');
  const removedTrend = calculateTrend(chartData, 'removed');
  
  // Render trend icon
  const renderTrendIcon = (direction) => {
    switch (direction) {
      case 'up':
        return <TrendingUpIcon color="success" />;
      case 'down':
        return <TrendingDownIcon color="error" />;
      default:
        return <TrendingFlatIcon color="action" />;
    }
  };
  
  // Calculate totals
  const totals = chartData.reduce((acc, item) => {
    acc.added += item.added || 0;
    acc.modified += item.modified || 0;
    acc.removed += item.removed || 0;
    return acc;
  }, { added: 0, modified: 0, removed: 0 });
  
  // Colors for charts
  const COLORS = {
    added: '#4caf50',
    modified: '#ff9800',
    removed: '#f44336'
  };
  
  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h6">Content Changes Over Time</Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <FormControl variant="outlined" size="small" sx={{ minWidth: 150 }}>
            <InputLabel id="chart-type-label">Chart Type</InputLabel>
            <Select
              labelId="chart-type-label"
              value={chartType}
              onChange={handleChartTypeChange}
              label="Chart Type"
            >
              <MenuItem value="area">Area Chart</MenuItem>
              <MenuItem value="bar">Bar Chart</MenuItem>
              <MenuItem value="line">Line Chart</MenuItem>
            </Select>
          </FormControl>
          <Button
            variant="outlined"
            startIcon={isRefreshing ? <CircularProgress size={20} /> : <RefreshIcon />}
            onClick={handleRefresh}
            disabled={isRefreshing || isLoading}
          >
            Refresh
          </Button>
        </Box>
      </Box>
      
      {/* Summary Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} md={4}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Typography variant="subtitle1" color="text.secondary">
                  Added Content
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  {renderTrendIcon(addedTrend.direction)}
                  <Typography variant="body2" color="text.secondary" sx={{ ml: 0.5 }}>
                    {addedTrend.value}%
                  </Typography>
                </Box>
              </Box>
              <Typography variant="h4" component="div" sx={{ fontWeight: 'medium', color: COLORS.added }}>
                {isLoading ? <CircularProgress size={24} /> : totals.added.toLocaleString()}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} md={4}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Typography variant="subtitle1" color="text.secondary">
                  Modified Content
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  {renderTrendIcon(modifiedTrend.direction)}
                  <Typography variant="body2" color="text.secondary" sx={{ ml: 0.5 }}>
                    {modifiedTrend.value}%
                  </Typography>
                </Box>
              </Box>
              <Typography variant="h4" component="div" sx={{ fontWeight: 'medium', color: COLORS.modified }}>
                {isLoading ? <CircularProgress size={24} /> : totals.modified.toLocaleString()}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} md={4}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Typography variant="subtitle1" color="text.secondary">
                  Removed Content
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  {renderTrendIcon(removedTrend.direction)}
                  <Typography variant="body2" color="text.secondary" sx={{ ml: 0.5 }}>
                    {removedTrend.value}%
                  </Typography>
                </Box>
              </Box>
              <Typography variant="h4" component="div" sx={{ fontWeight: 'medium', color: COLORS.removed }}>
                {isLoading ? <CircularProgress size={24} /> : totals.removed.toLocaleString()}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
      
      {/* Chart */}
      <Paper sx={{ p: 3 }}>
        {isLoading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 400 }}>
            <CircularProgress />
          </Box>
        ) : (
          <ResponsiveContainer width="100%" height={400}>
            {chartType === 'area' ? (
              <AreaChart
                data={chartData}
                margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis 
                  dataKey="date" 
                  tickFormatter={(value) => {
                    const date = new Date(value);
                    return format(date, timeframe === 'day' ? 'HH:mm' : timeframe === 'year' ? 'MMM' : 'MM/dd');
                  }}
                />
                <YAxis />
                <RechartsTooltip 
                  formatter={(value, name) => [value, name === 'added' ? 'Added' : name === 'modified' ? 'Modified' : 'Removed']}
                  labelFormatter={(label) => format(new Date(label), 'MMM dd, yyyy')}
                />
                <Legend />
                <Area type="monotone" dataKey="added" stackId="1" stroke={COLORS.added} fill={COLORS.added} name="Added" />
                <Area type="monotone" dataKey="modified" stackId="1" stroke={COLORS.modified} fill={COLORS.modified} name="Modified" />
                <Area type="monotone" dataKey="removed" stackId="1" stroke={COLORS.removed} fill={COLORS.removed} name="Removed" />
              </AreaChart>
            ) : chartType === 'bar' ? (
              <BarChart
                data={chartData}
                margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis 
                  dataKey="date" 
                  tickFormatter={(value) => {
                    const date = new Date(value);
                    return format(date, timeframe === 'day' ? 'HH:mm' : timeframe === 'year' ? 'MMM' : 'MM/dd');
                  }}
                />
                <YAxis />
                <RechartsTooltip 
                  formatter={(value, name) => [value, name === 'added' ? 'Added' : name === 'modified' ? 'Modified' : 'Removed']}
                  labelFormatter={(label) => format(new Date(label), 'MMM dd, yyyy')}
                />
                <Legend />
                <Bar dataKey="added" fill={COLORS.added} name="Added" />
                <Bar dataKey="modified" fill={COLORS.modified} name="Modified" />
                <Bar dataKey="removed" fill={COLORS.removed} name="Removed" />
              </BarChart>
            ) : (
              <LineChart
                data={chartData}
                margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis 
                  dataKey="date" 
                  tickFormatter={(value) => {
                    const date = new Date(value);
                    return format(date, timeframe === 'day' ? 'HH:mm' : timeframe === 'year' ? 'MMM' : 'MM/dd');
                  }}
                />
                <YAxis />
                <RechartsTooltip 
                  formatter={(value, name) => [value, name === 'added' ? 'Added' : name === 'modified' ? 'Modified' : 'Removed']}
                  labelFormatter={(label) => format(new Date(label), 'MMM dd, yyyy')}
                />
                <Legend />
                <Line type="monotone" dataKey="added" stroke={COLORS.added} name="Added" />
                <Line type="monotone" dataKey="modified" stroke={COLORS.modified} name="Modified" />
                <Line type="monotone" dataKey="removed" stroke={COLORS.removed} name="Removed" />
              </LineChart>
            )}
          </ResponsiveContainer>
        )}
      </Paper>
    </Box>
  );
};

export default ContentChangeChart;
