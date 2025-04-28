import React, { useState } from 'react';
import {
  Paper, Typography, Box, Button,
  CircularProgress, Grid, FormControl,
  InputLabel, Select, MenuItem, Tooltip,
  Card, CardContent, Divider, List,
  ListItem, ListItemText, ListItemIcon,
  Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow, SelectChangeEvent
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  PieChart as PieChartIcon,
  BarChart as BarChartIcon,
  TableChart as TableChartIcon
} from '@mui/icons-material';
import {
  PieChart, Pie, Cell, BarChart, Bar,
  XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip,
  Legend, ResponsiveContainer, Treemap
} from 'recharts';

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8', '#82CA9D', '#A4DE6C', '#D0ED57'];

interface ContentTypeItem {
  name: string;
  value: number;
}

interface ContentTypeData {
  contentTypes?: ContentTypeItem[];
  domainDistribution?: ContentTypeItem[];
  sizeDistribution?: ContentTypeItem[];
}

interface ContentTypeDistributionProps {
  data: ContentTypeData | null;
  isLoading: boolean;
  onRefresh: () => Promise<any>;
}

const ContentTypeDistribution: React.FC<ContentTypeDistributionProps> = ({ data, isLoading, onRefresh }) => {
  const [viewType, setViewType] = useState<string>('pie');
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

  // Handle view type change
  const handleViewTypeChange = (event: SelectChangeEvent) => {
    setViewType(event.target.value);
  };

  // Prepare chart data
  const contentTypeData = data?.contentTypes || [];
  const domainData = data?.domainDistribution || [];

  // Custom pie chart label
  interface LabelProps {
    cx: number;
    cy: number;
    midAngle: number;
    innerRadius: number;
    outerRadius: number;
    percent: number;
    index: number;
    name: string;
  }

  const renderCustomizedLabel = ({ cx, cy, midAngle, innerRadius, outerRadius, percent, index, name }: LabelProps) => {
    const RADIAN = Math.PI / 180;
    const radius = innerRadius + (outerRadius - innerRadius) * 0.5;
    const x = cx + radius * Math.cos(-midAngle * RADIAN);
    const y = cy + radius * Math.sin(-midAngle * RADIAN);

    return percent > 0.05 ? (
      <text x={x} y={y} fill="white" textAnchor={x > cx ? 'start' : 'end'} dominantBaseline="central">
        {`${name} (${(percent * 100).toFixed(0)}%)`}
      </text>
    ) : null;
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h6">Content Type Distribution</Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <FormControl variant="outlined" size="small" sx={{ minWidth: 150 }}>
            <InputLabel id="view-type-label">View Type</InputLabel>
            <Select
              labelId="view-type-label"
              value={viewType}
              onChange={handleViewTypeChange}
              label="View Type"
            >
              <MenuItem value="pie">Pie Chart</MenuItem>
              <MenuItem value="bar">Bar Chart</MenuItem>
              <MenuItem value="treemap">Treemap</MenuItem>
              <MenuItem value="table">Table</MenuItem>
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

      <Grid container spacing={3}>
        {/* Content Type Distribution */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3, height: '100%' }}>
            <Typography variant="subtitle1" gutterBottom>
              Content Type Distribution
            </Typography>
            {isLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 400 }}>
                <CircularProgress />
              </Box>
            ) : viewType === 'pie' ? (
              <ResponsiveContainer width="100%" height={400}>
                <PieChart>
                  <Pie
                    data={contentTypeData}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    label={renderCustomizedLabel}
                    outerRadius={150}
                    fill="#8884d8"
                    dataKey="value"
                  >
                    {contentTypeData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <RechartsTooltip formatter={(value, name) => [`${value} URLs`, name]} />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            ) : viewType === 'bar' ? (
              <ResponsiveContainer width="100%" height={400}>
                <BarChart
                  data={contentTypeData}
                  margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <RechartsTooltip formatter={(value) => [`${value} URLs`, 'Count']} />
                  <Bar dataKey="value" name="URLs">
                    {contentTypeData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            ) : viewType === 'treemap' ? (
              <ResponsiveContainer width="100%" height={400}>
                <Treemap
                  data={contentTypeData}
                  dataKey="value"
                  nameKey="name"
                  aspectRatio={4/3}
                  stroke="#fff"
                  fill="#8884d8"
                >
                  {contentTypeData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Treemap>
              </ResponsiveContainer>
            ) : (
              <TableContainer sx={{ maxHeight: 400 }}>
                <Table stickyHeader size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Content Type</TableCell>
                      <TableCell align="right">Count</TableCell>
                      <TableCell align="right">Percentage</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {contentTypeData.map((row, index) => {
                      const total = contentTypeData.reduce((sum, item) => sum + item.value, 0);
                      const percentage = total > 0 ? (row.value / total) * 100 : 0;

                      return (
                        <TableRow key={index} hover>
                          <TableCell component="th" scope="row">
                            {row.name}
                          </TableCell>
                          <TableCell align="right">{row.value}</TableCell>
                          <TableCell align="right">{percentage.toFixed(1)}%</TableCell>
                        </TableRow>
                      );
                    })}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </Paper>
        </Grid>

        {/* Domain Distribution */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3, height: '100%' }}>
            <Typography variant="subtitle1" gutterBottom>
              Domain Distribution
            </Typography>
            {isLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 400 }}>
                <CircularProgress />
              </Box>
            ) : viewType === 'pie' ? (
              <ResponsiveContainer width="100%" height={400}>
                <PieChart>
                  <Pie
                    data={domainData}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    label={renderCustomizedLabel}
                    outerRadius={150}
                    fill="#8884d8"
                    dataKey="value"
                  >
                    {domainData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <RechartsTooltip formatter={(value, name) => [`${value} URLs`, name]} />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            ) : viewType === 'bar' ? (
              <ResponsiveContainer width="100%" height={400}>
                <BarChart
                  data={domainData}
                  margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
                  layout="vertical"
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis type="number" />
                  <YAxis dataKey="name" type="category" width={150} />
                  <RechartsTooltip formatter={(value) => [`${value} URLs`, 'Count']} />
                  <Bar dataKey="value" name="URLs">
                    {domainData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            ) : viewType === 'treemap' ? (
              <ResponsiveContainer width="100%" height={400}>
                <Treemap
                  data={domainData}
                  dataKey="value"
                  nameKey="name"
                  aspectRatio={4/3}
                  stroke="#fff"
                  fill="#8884d8"
                >
                  {domainData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Treemap>
              </ResponsiveContainer>
            ) : (
              <TableContainer sx={{ maxHeight: 400 }}>
                <Table stickyHeader size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Domain</TableCell>
                      <TableCell align="right">Count</TableCell>
                      <TableCell align="right">Percentage</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {domainData.map((row, index) => {
                      const total = domainData.reduce((sum, item) => sum + item.value, 0);
                      const percentage = total > 0 ? (row.value / total) * 100 : 0;

                      return (
                        <TableRow key={index} hover>
                          <TableCell component="th" scope="row">
                            {row.name}
                          </TableCell>
                          <TableCell align="right">{row.value}</TableCell>
                          <TableCell align="right">{percentage.toFixed(1)}%</TableCell>
                        </TableRow>
                      );
                    })}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </Paper>
        </Grid>

        {/* Content Size Distribution */}
        <Grid item xs={12}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="subtitle1" gutterBottom>
              Content Size Distribution
            </Typography>
            {isLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 300 }}>
                <CircularProgress />
              </Box>
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <BarChart
                  data={data?.sizeDistribution || []}
                  margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <RechartsTooltip formatter={(value) => [`${value} URLs`, 'Count']} />
                  <Bar dataKey="value" name="URLs" fill="#8884d8" />
                </BarChart>
              </ResponsiveContainer>
            )}
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default ContentTypeDistribution;
