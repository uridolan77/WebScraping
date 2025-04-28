import React, { useState } from 'react';
import {
  Paper, Typography, Box, Button,
  CircularProgress, Grid, FormControl,
  InputLabel, Select, MenuItem, Tooltip,
  Card, CardContent, Divider, List,
  ListItem, ListItemText, ListItemIcon,
  SelectChangeEvent
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  Speed as SpeedIcon,
  Memory as MemoryIcon,
  Storage as StorageIcon,
  CloudDownload as CloudDownloadIcon,
  Schedule as ScheduleIcon,
  Error as ErrorIcon,
  CheckCircle as CheckCircleIcon,
  Info as InfoIcon
} from '@mui/icons-material';
import {
  LineChart, Line, BarChart, Bar,
  XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip,
  Legend, ResponsiveContainer, Area, AreaChart,
  RadarChart, Radar, PolarGrid, PolarAngleAxis, PolarRadiusAxis
} from 'recharts';
import { format } from 'date-fns';

interface TimeSeriesDataPoint {
  timestamp: string;
  requestTime?: number;
  requestsPerMinute?: number;
  memoryUsage?: number;
  cpuUsage?: number;
  successRate?: number;
  [key: string]: any;
}

interface ResourceUsageDataPoint {
  name: string;
  value: number;
}

interface PerformanceInsight {
  type: 'warning' | 'success' | 'info';
  title: string;
  description: string;
}

interface PerformanceData {
  avgRequestTime?: number;
  avgRequestsPerMinute?: number;
  avgMemoryUsage?: number;
  successRate?: number;
  timeSeriesData?: TimeSeriesDataPoint[];
  resourceUsageData?: ResourceUsageDataPoint[];
  insights?: PerformanceInsight[];
}

interface PerformanceMetricsProps {
  data: PerformanceData | null;
  isLoading: boolean;
  timeframe: string;
  onRefresh: () => Promise<any>;
}

const PerformanceMetrics: React.FC<PerformanceMetricsProps> = ({ data, isLoading, timeframe, onRefresh }) => {
  const [chartType, setChartType] = useState<string>('line');
  const [metricType, setMetricType] = useState<string>('requestTime');
  const [isRefreshing, setIsRefreshing] = useState<boolean>(false);

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
  const handleChartTypeChange = (event: SelectChangeEvent) => {
    setChartType(event.target.value);
  };

  // Handle metric type change
  const handleMetricTypeChange = (event: SelectChangeEvent) => {
    setMetricType(event.target.value);
  };

  // Prepare chart data
  const timeSeriesData = data?.timeSeriesData || [];
  const resourceUsageData = data?.resourceUsageData || [];

  // Get metric name for display
  const getMetricName = (metric: string): string => {
    switch (metric) {
      case 'requestTime':
        return 'Request Time (ms)';
      case 'requestsPerMinute':
        return 'Requests Per Minute';
      case 'memoryUsage':
        return 'Memory Usage (MB)';
      case 'cpuUsage':
        return 'CPU Usage (%)';
      case 'successRate':
        return 'Success Rate (%)';
      default:
        return metric;
    }
  };

  // Colors for charts
  const COLORS: Record<string, string> = {
    requestTime: '#8884d8',
    requestsPerMinute: '#82ca9d',
    memoryUsage: '#ffc658',
    cpuUsage: '#ff8042',
    successRate: '#0088fe'
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h6">Performance Metrics</Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <FormControl variant="outlined" size="small" sx={{ minWidth: 150 }}>
            <InputLabel id="metric-type-label">Metric</InputLabel>
            <Select
              labelId="metric-type-label"
              value={metricType}
              onChange={handleMetricTypeChange}
              label="Metric"
            >
              <MenuItem value="requestTime">Request Time</MenuItem>
              <MenuItem value="requestsPerMinute">Requests Per Minute</MenuItem>
              <MenuItem value="memoryUsage">Memory Usage</MenuItem>
              <MenuItem value="cpuUsage">CPU Usage</MenuItem>
              <MenuItem value="successRate">Success Rate</MenuItem>
            </Select>
          </FormControl>
          <FormControl variant="outlined" size="small" sx={{ minWidth: 150 }}>
            <InputLabel id="chart-type-label">Chart Type</InputLabel>
            <Select
              labelId="chart-type-label"
              value={chartType}
              onChange={handleChartTypeChange}
              label="Chart Type"
            >
              <MenuItem value="line">Line Chart</MenuItem>
              <MenuItem value="area">Area Chart</MenuItem>
              <MenuItem value="bar">Bar Chart</MenuItem>
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
        <Grid item xs={12} md={3}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <SpeedIcon color="primary" sx={{ mr: 1 }} />
                <Typography variant="subtitle1" color="text.secondary">
                  Avg. Request Time
                </Typography>
              </Box>
              <Typography variant="h5" component="div">
                {isLoading ? <CircularProgress size={24} /> : `${data?.avgRequestTime?.toFixed(2) || 0} ms`}
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={3}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <CloudDownloadIcon color="info" sx={{ mr: 1 }} />
                <Typography variant="subtitle1" color="text.secondary">
                  Requests Per Minute
                </Typography>
              </Box>
              <Typography variant="h5" component="div">
                {isLoading ? <CircularProgress size={24} /> : data?.avgRequestsPerMinute?.toFixed(1) || 0}
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={3}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <MemoryIcon color="warning" sx={{ mr: 1 }} />
                <Typography variant="subtitle1" color="text.secondary">
                  Memory Usage
                </Typography>
              </Box>
              <Typography variant="h5" component="div">
                {isLoading ? <CircularProgress size={24} /> : `${data?.avgMemoryUsage?.toFixed(1) || 0} MB`}
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={3}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <CheckCircleIcon color="success" sx={{ mr: 1 }} />
                <Typography variant="subtitle1" color="text.secondary">
                  Success Rate
                </Typography>
              </Box>
              <Typography variant="h5" component="div">
                {isLoading ? <CircularProgress size={24} /> : `${data?.successRate?.toFixed(1) || 0}%`}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Main Chart */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="subtitle1" gutterBottom>
          {getMetricName(metricType)} Over Time
        </Typography>
        {isLoading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 400 }}>
            <CircularProgress />
          </Box>
        ) : (
          <ResponsiveContainer width="100%" height={400}>
            {chartType === 'line' ? (
              <LineChart
                data={timeSeriesData}
                margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis
                  dataKey="timestamp"
                  tickFormatter={(value) => {
                    const date = new Date(value);
                    return format(date, timeframe === 'day' ? 'HH:mm' : timeframe === 'year' ? 'MMM' : 'MM/dd');
                  }}
                />
                <YAxis />
                <RechartsTooltip
                  formatter={(value) => [`${value}`, getMetricName(metricType)]}
                  labelFormatter={(label) => format(new Date(label), 'MMM dd, yyyy HH:mm')}
                />
                <Line
                  type="monotone"
                  dataKey={metricType}
                  stroke={COLORS[metricType]}
                  activeDot={{ r: 8 }}
                  name={getMetricName(metricType)}
                />
              </LineChart>
            ) : chartType === 'area' ? (
              <AreaChart
                data={timeSeriesData}
                margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis
                  dataKey="timestamp"
                  tickFormatter={(value) => {
                    const date = new Date(value);
                    return format(date, timeframe === 'day' ? 'HH:mm' : timeframe === 'year' ? 'MMM' : 'MM/dd');
                  }}
                />
                <YAxis />
                <RechartsTooltip
                  formatter={(value) => [`${value}`, getMetricName(metricType)]}
                  labelFormatter={(label) => format(new Date(label), 'MMM dd, yyyy HH:mm')}
                />
                <Area
                  type="monotone"
                  dataKey={metricType}
                  stroke={COLORS[metricType]}
                  fill={COLORS[metricType]}
                  name={getMetricName(metricType)}
                />
              </AreaChart>
            ) : (
              <BarChart
                data={timeSeriesData}
                margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis
                  dataKey="timestamp"
                  tickFormatter={(value) => {
                    const date = new Date(value);
                    return format(date, timeframe === 'day' ? 'HH:mm' : timeframe === 'year' ? 'MMM' : 'MM/dd');
                  }}
                />
                <YAxis />
                <RechartsTooltip
                  formatter={(value) => [`${value}`, getMetricName(metricType)]}
                  labelFormatter={(label) => format(new Date(label), 'MMM dd, yyyy HH:mm')}
                />
                <Bar
                  dataKey={metricType}
                  fill={COLORS[metricType]}
                  name={getMetricName(metricType)}
                />
              </BarChart>
            )}
          </ResponsiveContainer>
        )}
      </Paper>

      {/* Resource Usage Radar Chart */}
      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3, height: '100%' }}>
            <Typography variant="subtitle1" gutterBottom>
              Resource Usage
            </Typography>
            {isLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 300 }}>
                <CircularProgress />
              </Box>
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <RadarChart outerRadius={90} data={resourceUsageData}>
                  <PolarGrid />
                  <PolarAngleAxis dataKey="name" />
                  <PolarRadiusAxis angle={30} domain={[0, 100]} />
                  <Radar
                    name="Resource Usage (%)"
                    dataKey="value"
                    stroke="#8884d8"
                    fill="#8884d8"
                    fillOpacity={0.6}
                  />
                  <RechartsTooltip />
                </RadarChart>
              </ResponsiveContainer>
            )}
          </Paper>
        </Grid>

        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3, height: '100%' }}>
            <Typography variant="subtitle1" gutterBottom>
              Performance Insights
            </Typography>
            {isLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 300 }}>
                <CircularProgress />
              </Box>
            ) : (
              <List>
                {data?.insights?.map((insight, index) => (
                  <ListItem key={index} alignItems="flex-start">
                    <ListItemIcon>
                      {insight.type === 'warning' ? (
                        <ErrorIcon color="warning" />
                      ) : insight.type === 'success' ? (
                        <CheckCircleIcon color="success" />
                      ) : (
                        <InfoIcon color="info" />
                      )}
                    </ListItemIcon>
                    <ListItemText
                      primary={insight.title}
                      secondary={insight.description}
                    />
                  </ListItem>
                ))}
                {(!data?.insights || data.insights.length === 0) && (
                  <ListItem>
                    <ListItemText
                      primary="No performance insights available"
                      secondary="Run more scrapers to generate performance insights"
                    />
                  </ListItem>
                )}
              </List>
            )}
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default PerformanceMetrics;
