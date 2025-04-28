import React, { useState, useEffect } from 'react';
import {
  Paper, Typography, Box, Button,
  CircularProgress, Grid, FormControl,
  InputLabel, Select, MenuItem, Tooltip,
  Card, CardContent, Divider, Alert,
  SelectChangeEvent
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  TrendingUp as TrendingUpIcon,
  TrendingDown as TrendingDownIcon,
  TrendingFlat as TrendingFlatIcon
} from '@mui/icons-material';
import {
  LineChart, Line, AreaChart, Area,
  XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip,
  Legend, ResponsiveContainer
} from 'recharts';
import { format } from 'date-fns';

interface TrendAnalysisProps {
  timeframe: string;
  fetchTrendAnalysis: (metric: string, timeframe: string) => Promise<any>;
}

interface TrendData {
  data: Array<{
    date: string;
    value: number;
    forecast?: number;
  }>;
  insights?: string;
  seasonality?: {
    daily?: string;
    weekly?: string;
    monthly?: string;
    yearly?: string;
  };
  forecast?: boolean;
}

interface TrendResult {
  value: number;
  direction: 'up' | 'down' | 'flat';
}

const TrendAnalysis: React.FC<TrendAnalysisProps> = ({ timeframe, fetchTrendAnalysis }) => {
  const [metric, setMetric] = useState<string>('changes');
  const [trendData, setTrendData] = useState<TrendData | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  // Fetch trend data when metric or timeframe changes
  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      setError(null);

      try {
        const data = await fetchTrendAnalysis(metric, timeframe);
        setTrendData(data);
      } catch (err) {
        const errorMessage = err instanceof Error ? err.message : 'Failed to fetch trend data';
        setError(errorMessage);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [metric, timeframe, fetchTrendAnalysis]);

  // Handle metric change
  const handleMetricChange = (event: SelectChangeEvent) => {
    setMetric(event.target.value);
  };

  // Handle refresh
  const handleRefresh = async () => {
    setLoading(true);
    setError(null);

    try {
      const data = await fetchTrendAnalysis(metric, timeframe);
      setTrendData(data);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to fetch trend data';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  // Calculate trend
  const calculateTrend = (data: Array<{ value: number }> | undefined): TrendResult => {
    if (!data || data.length < 2) return { value: 0, direction: 'flat' };

    const firstValue = data[0].value || 0;
    const lastValue = data[data.length - 1].value || 0;

    if (firstValue === 0) return { value: lastValue > 0 ? 100 : 0, direction: lastValue > 0 ? 'up' : 'flat' };

    const percentChange = ((lastValue - firstValue) / firstValue) * 100;

    return {
      value: Math.abs(parseFloat(percentChange.toFixed(1))),
      direction: percentChange > 0 ? 'up' : percentChange < 0 ? 'down' : 'flat'
    };
  };

  // Render trend icon
  const renderTrendIcon = (direction: 'up' | 'down' | 'flat') => {
    switch (direction) {
      case 'up':
        return <TrendingUpIcon color="success" />;
      case 'down':
        return <TrendingDownIcon color="error" />;
      default:
        return <TrendingFlatIcon color="action" />;
    }
  };

  // Get metric name for display
  const getMetricName = (metricType: string): string => {
    switch (metricType) {
      case 'changes':
        return 'Content Changes';
      case 'pages':
        return 'Pages Processed';
      case 'errors':
        return 'Errors';
      default:
        return metricType;
    }
  };

  // Get trend description
  const getTrendDescription = (trend: TrendResult, metricType: string): string => {
    if (trend.direction === 'flat') return `No significant change in ${getMetricName(metricType).toLowerCase()}`;

    return `${trend.direction === 'up' ? 'Increase' : 'Decrease'} of ${trend.value}% in ${getMetricName(metricType).toLowerCase()}`;
  };

  // Calculate trend from data
  const trend: TrendResult = trendData ? calculateTrend(trendData.data) : { value: 0, direction: 'flat' };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h6">Trend Analysis</Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <FormControl variant="outlined" size="small" sx={{ minWidth: 150 }}>
            <InputLabel id="metric-select-label">Metric</InputLabel>
            <Select
              labelId="metric-select-label"
              value={metric}
              onChange={handleMetricChange}
              label="Metric"
            >
              <MenuItem value="changes">Content Changes</MenuItem>
              <MenuItem value="pages">Pages Processed</MenuItem>
              <MenuItem value="errors">Errors</MenuItem>
            </Select>
          </FormControl>
          <Button
            variant="outlined"
            startIcon={loading ? <CircularProgress size={20} /> : <RefreshIcon />}
            onClick={handleRefresh}
            disabled={loading}
          >
            Refresh
          </Button>
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Trend Summary Card */}
      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2}>
            <Grid item xs={12} md={4}>
              <Box sx={{ display: 'flex', alignItems: 'center', height: '100%' }}>
                <Box sx={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  bgcolor: trend.direction === 'up' ? 'success.100' :
                           trend.direction === 'down' ? 'error.100' : 'grey.100',
                  color: trend.direction === 'up' ? 'success.800' :
                         trend.direction === 'down' ? 'error.800' : 'grey.800',
                  borderRadius: '50%',
                  p: 2,
                  mr: 2,
                  width: 60,
                  height: 60
                }}>
                  {renderTrendIcon(trend.direction)}
                </Box>
                <Box>
                  <Typography variant="h5" component="div">
                    {trend.value}%
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {trend.direction === 'up' ? 'Increase' : trend.direction === 'down' ? 'Decrease' : 'No Change'}
                  </Typography>
                </Box>
              </Box>
            </Grid>
            <Grid item xs={12} md={8}>
              <Typography variant="h6" gutterBottom>
                {getMetricName(metric)} Trend
              </Typography>
              <Typography variant="body1" paragraph>
                {getTrendDescription(trend, metric)}
              </Typography>
              {trendData?.insights && (
                <Typography variant="body2" color="text.secondary">
                  {trendData.insights}
                </Typography>
              )}
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Trend Chart */}
      <Paper sx={{ p: 3 }}>
        <Typography variant="subtitle1" gutterBottom>
          {getMetricName(metric)} Over Time
        </Typography>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 400 }}>
            <CircularProgress />
          </Box>
        ) : trendData?.data && trendData.data.length > 0 ? (
          <ResponsiveContainer width="100%" height={400}>
            <AreaChart
              data={trendData.data}
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
                formatter={(value) => [`${value}`, getMetricName(metric)]}
                labelFormatter={(label) => format(new Date(label), 'MMM dd, yyyy')}
              />
              <Area
                type="monotone"
                dataKey="value"
                stroke="#8884d8"
                fill="#8884d8"
                name={getMetricName(metric)}
              />
              {trendData.forecast && (
                <Area
                  type="monotone"
                  dataKey="forecast"
                  stroke="#82ca9d"
                  fill="#82ca9d"
                  strokeDasharray="5 5"
                  name="Forecast"
                />
              )}
              <Legend />
            </AreaChart>
          </ResponsiveContainer>
        ) : (
          <Alert severity="info" sx={{ mt: 2 }}>
            No trend data available for the selected metric and timeframe.
          </Alert>
        )}
      </Paper>

      {/* Seasonality Analysis */}
      {trendData?.seasonality && (
        <Paper sx={{ p: 3, mt: 3 }}>
          <Typography variant="subtitle1" gutterBottom>
            Seasonality Analysis
          </Typography>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Typography variant="body1" gutterBottom>
                <strong>Daily Pattern:</strong> {trendData.seasonality.daily || 'No significant daily pattern detected'}
              </Typography>
              <Typography variant="body1" gutterBottom>
                <strong>Weekly Pattern:</strong> {trendData.seasonality.weekly || 'No significant weekly pattern detected'}
              </Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="body1" gutterBottom>
                <strong>Monthly Pattern:</strong> {trendData.seasonality.monthly || 'No significant monthly pattern detected'}
              </Typography>
              <Typography variant="body1" gutterBottom>
                <strong>Yearly Pattern:</strong> {trendData.seasonality.yearly || 'No significant yearly pattern detected'}
              </Typography>
            </Grid>
          </Grid>
        </Paper>
      )}
    </Box>
  );
};

export default TrendAnalysis;
