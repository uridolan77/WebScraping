import React, { useState, useEffect } from 'react';
import {
  Paper, Typography, Box, Button,
  CircularProgress, Grid, FormControl,
  InputLabel, Select, MenuItem, Tooltip,
  Card, CardContent, Divider, Alert,
  Chip, Autocomplete, TextField, Table,
  TableBody, TableCell, TableContainer,
  TableHead, TableRow, SelectChangeEvent
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  Compare as CompareIcon
} from '@mui/icons-material';
import {
  BarChart, Bar, RadarChart, Radar, PolarGrid,
  PolarAngleAxis, PolarRadiusAxis, Cell,
  XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip,
  Legend, ResponsiveContainer
} from 'recharts';
import { useScraperContext } from '../../contexts/ScraperContext';

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8', '#82CA9D'];

interface Scraper {
  id: string;
  name: string;
  [key: string]: any;
}

interface ComparisonMetric {
  scraperId: string;
  scraperName: string;
  [key: string]: any;
}

interface ComparisonData {
  metrics: ComparisonMetric[];
  insights?: string;
}

interface ScraperComparisonProps {
  timeframe: string;
  fetchScraperComparison: (scraperIds: string[], metric: string, timeframe: string) => Promise<any>;
}

const ScraperComparison: React.FC<ScraperComparisonProps> = ({ timeframe, fetchScraperComparison }) => {
  const { scrapers, loading: scrapersLoading } = useScraperContext();

  const [selectedScrapers, setSelectedScrapers] = useState<Scraper[]>([]);
  const [metric, setMetric] = useState<string>('performance');
  const [viewType, setViewType] = useState<string>('radar');
  const [comparisonData, setComparisonData] = useState<ComparisonData | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  // Fetch comparison data when selections change
  useEffect(() => {
    const fetchData = async () => {
      if (selectedScrapers.length < 2) return;

      setLoading(true);
      setError(null);

      try {
        const scraperIds = selectedScrapers.map(scraper => scraper.id);
        const data = await fetchScraperComparison(scraperIds, metric, timeframe);
        setComparisonData(data);
      } catch (err) {
        const errorMessage = err instanceof Error ? err.message : 'Failed to fetch comparison data';
        setError(errorMessage);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [selectedScrapers, metric, timeframe, fetchScraperComparison]);

  // Handle metric change
  const handleMetricChange = (event: SelectChangeEvent) => {
    setMetric(event.target.value);
  };

  // Handle view type change
  const handleViewTypeChange = (event: SelectChangeEvent) => {
    setViewType(event.target.value);
  };

  // Handle refresh
  const handleRefresh = async () => {
    if (selectedScrapers.length < 2) return;

    setLoading(true);
    setError(null);

    try {
      const scraperIds = selectedScrapers.map(scraper => scraper.id);
      const data = await fetchScraperComparison(scraperIds, metric, timeframe);
      setComparisonData(data);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to fetch comparison data';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  // Get metric name for display
  const getMetricName = (metricType: string): string => {
    switch (metricType) {
      case 'performance':
        return 'Performance';
      case 'pages':
        return 'Pages Processed';
      case 'changes':
        return 'Content Changes';
      case 'errors':
        return 'Errors';
      default:
        return metricType;
    }
  };

  // Prepare radar chart data
  const prepareRadarData = () => {
    if (!comparisonData || !comparisonData.metrics) return [];

    // Transform data for radar chart
    const metrics = Object.keys(comparisonData.metrics[0]).filter(key => key !== 'scraperId' && key !== 'scraperName');

    return metrics.map(metricKey => {
      const dataPoint: Record<string, any> = { metric: metricKey };

      comparisonData.metrics.forEach(scraperMetric => {
        dataPoint[scraperMetric.scraperName] = scraperMetric[metricKey];
      });

      return dataPoint;
    });
  };

  // Prepare bar chart data
  const prepareBarData = () => {
    if (!comparisonData || !comparisonData.metrics) return [];

    // Transform data for bar chart based on selected metric
    return comparisonData.metrics.map(scraperMetric => ({
      name: scraperMetric.scraperName,
      value: scraperMetric[metric]
    }));
  };

  // Radar chart data
  const radarData = prepareRadarData();

  // Bar chart data
  const barData = prepareBarData();

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h6">Scraper Comparison</Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <FormControl variant="outlined" size="small" sx={{ minWidth: 150 }}>
            <InputLabel id="metric-select-label">Metric</InputLabel>
            <Select
              labelId="metric-select-label"
              value={metric}
              onChange={handleMetricChange}
              label="Metric"
            >
              <MenuItem value="performance">Performance</MenuItem>
              <MenuItem value="pages">Pages Processed</MenuItem>
              <MenuItem value="changes">Content Changes</MenuItem>
              <MenuItem value="errors">Errors</MenuItem>
            </Select>
          </FormControl>
          <FormControl variant="outlined" size="small" sx={{ minWidth: 150 }}>
            <InputLabel id="view-type-label">View Type</InputLabel>
            <Select
              labelId="view-type-label"
              value={viewType}
              onChange={handleViewTypeChange}
              label="View Type"
            >
              <MenuItem value="radar">Radar Chart</MenuItem>
              <MenuItem value="bar">Bar Chart</MenuItem>
              <MenuItem value="table">Table</MenuItem>
            </Select>
          </FormControl>
          <Button
            variant="outlined"
            startIcon={loading ? <CircularProgress size={20} /> : <RefreshIcon />}
            onClick={handleRefresh}
            disabled={loading || selectedScrapers.length < 2}
          >
            Refresh
          </Button>
        </Box>
      </Box>

      {/* Scraper Selection */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="subtitle1" gutterBottom>
          Select Scrapers to Compare
        </Typography>
        <Autocomplete
          multiple
          options={scrapers || []}
          getOptionLabel={(option) => option.name}
          value={selectedScrapers}
          onChange={(event, newValue) => setSelectedScrapers(newValue)}
          loading={scrapersLoading}
          renderInput={(params) => (
            <TextField
              {...params}
              variant="outlined"
              label="Scrapers"
              placeholder="Select scrapers to compare"
              helperText="Select at least 2 scrapers to compare"
              InputProps={{
                ...params.InputProps,
                endAdornment: (
                  <>
                    {scrapersLoading ? <CircularProgress color="inherit" size={20} /> : null}
                    {params.InputProps.endAdornment}
                  </>
                ),
              }}
            />
          )}
          renderTags={(value, getTagProps) =>
            value.map((option, index) => (
              <Chip
                label={option.name}
                {...getTagProps({ index })}
                key={option.id}
              />
            ))
          }
          sx={{ mb: 2 }}
        />

        {selectedScrapers.length < 2 && (
          <Alert severity="info">
            Please select at least 2 scrapers to compare.
          </Alert>
        )}
      </Paper>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Comparison Chart */}
      {selectedScrapers.length >= 2 && (
        <Paper sx={{ p: 3 }}>
          <Typography variant="subtitle1" gutterBottom>
            {getMetricName(metric)} Comparison
          </Typography>
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 400 }}>
              <CircularProgress />
            </Box>
          ) : comparisonData ? (
            viewType === 'radar' ? (
              <ResponsiveContainer width="100%" height={500}>
                <RadarChart outerRadius={150} data={radarData}>
                  <PolarGrid />
                  <PolarAngleAxis dataKey="metric" />
                  <PolarRadiusAxis angle={30} domain={[0, 'auto']} />
                  {selectedScrapers.map((scraper, index) => (
                    <Radar
                      key={scraper.id}
                      name={scraper.name}
                      dataKey={scraper.name}
                      stroke={COLORS[index % COLORS.length]}
                      fill={COLORS[index % COLORS.length]}
                      fillOpacity={0.6}
                    />
                  ))}
                  <Legend />
                  <RechartsTooltip />
                </RadarChart>
              </ResponsiveContainer>
            ) : viewType === 'bar' ? (
              <ResponsiveContainer width="100%" height={400}>
                <BarChart
                  data={barData}
                  margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <RechartsTooltip formatter={(value) => [`${value}`, getMetricName(metric)]} />
                  <Bar dataKey="value" name={getMetricName(metric)}>
                    {barData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <TableContainer>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Scraper</TableCell>
                      {comparisonData.metrics[0] && Object.keys(comparisonData.metrics[0])
                        .filter(key => key !== 'scraperId' && key !== 'scraperName')
                        .map(metric => (
                          <TableCell key={metric} align="right">{metric}</TableCell>
                        ))
                      }
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {comparisonData.metrics.map((scraperMetric) => (
                      <TableRow key={scraperMetric.scraperId} hover>
                        <TableCell component="th" scope="row">
                          {scraperMetric.scraperName}
                        </TableCell>
                        {Object.keys(scraperMetric)
                          .filter(key => key !== 'scraperId' && key !== 'scraperName')
                          .map(metric => (
                            <TableCell key={metric} align="right">
                              {scraperMetric[metric]}
                            </TableCell>
                          ))
                        }
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )
          ) : (
            <Alert severity="info">
              No comparison data available. Try selecting different scrapers or metrics.
            </Alert>
          )}
        </Paper>
      )}

      {/* Insights */}
      {comparisonData?.insights && (
        <Paper sx={{ p: 3, mt: 3 }}>
          <Typography variant="subtitle1" gutterBottom>
            Comparison Insights
          </Typography>
          <Typography variant="body1">
            {comparisonData.insights}
          </Typography>
        </Paper>
      )}
    </Box>
  );
};

export default ScraperComparison;
