import React, { useState, useEffect } from 'react';
import {
  Box, Typography, Paper, Grid, CircularProgress,
  Card, CardContent, LinearProgress, Tooltip,
  List, ListItem, ListItemText, ListItemIcon,
  Divider, Button, Alert
} from '@mui/material';
import {
  Memory as MemoryIcon,
  Storage as StorageIcon,
  Speed as SpeedIcon,
  CloudQueue as CloudIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Warning as WarningIcon,
  Refresh as RefreshIcon,
  Info as InfoIcon
} from '@mui/icons-material';
import {
  LineChart, Line, AreaChart, Area,
  XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip,
  Legend, ResponsiveContainer
} from 'recharts';

interface SystemDataPoint {
  time: string;
  value: number;
}

interface NetworkDataPoint {
  time: string;
  download: number;
  upload: number;
}

interface SystemService {
  name: string;
  status: 'running' | 'warning' | 'error';
  uptime: string;
  message?: string;
}

interface SystemIssue {
  type: 'warning' | 'error' | 'info';
  message: string;
  timestamp: Date;
}

interface SystemData {
  cpu: {
    usage: number;
    temperature: number;
    cores: number;
    history: SystemDataPoint[];
  };
  memory: {
    usage: number;
    total: number;
    available: number;
    history: SystemDataPoint[];
  };
  disk: {
    usage: number;
    total: number;
    available: number;
    history: SystemDataPoint[];
  };
  network: {
    status: 'online' | 'offline';
    latency: number;
    bandwidth: number;
    history: NetworkDataPoint[];
  };
  services: SystemService[];
  issues: SystemIssue[];
}

const SystemHealthMonitor: React.FC = () => {
  const [loading, setLoading] = useState<boolean>(true);
  const [systemData, setSystemData] = useState<SystemData | null>(null);
  const [isRefreshing, setIsRefreshing] = useState<boolean>(false);

  // Fetch system health data
  useEffect(() => {
    // Simulate API call
    const fetchData = () => {
      setLoading(true);

      // Mock data
      setTimeout(() => {
        setSystemData({
          cpu: {
            usage: 45,
            temperature: 65,
            cores: 8,
            history: Array.from({ length: 24 }).map((_, i) => ({
              time: new Date(Date.now() - (23 - i) * 5 * 60000).toISOString(),
              value: 30 + Math.random() * 40
            }))
          },
          memory: {
            usage: 62,
            total: 16384, // MB
            available: 6226, // MB
            history: Array.from({ length: 24 }).map((_, i) => ({
              time: new Date(Date.now() - (23 - i) * 5 * 60000).toISOString(),
              value: 50 + Math.random() * 30
            }))
          },
          disk: {
            usage: 78,
            total: 512, // GB
            available: 112, // GB
            history: Array.from({ length: 24 }).map((_, i) => ({
              time: new Date(Date.now() - (23 - i) * 5 * 60000).toISOString(),
              value: 70 + Math.random() * 15
            }))
          },
          network: {
            status: 'online',
            latency: 45, // ms
            bandwidth: 95, // Mbps
            history: Array.from({ length: 24 }).map((_, i) => ({
              time: new Date(Date.now() - (23 - i) * 5 * 60000).toISOString(),
              download: 80 + Math.random() * 40,
              upload: 20 + Math.random() * 15
            }))
          },
          services: [
            { name: 'Web Server', status: 'running', uptime: '14d 6h 32m' },
            { name: 'Database', status: 'running', uptime: '14d 6h 30m' },
            { name: 'Scraper Service', status: 'running', uptime: '5d 12h 45m' },
            { name: 'Notification Service', status: 'warning', uptime: '2h 15m', message: 'High memory usage' }
          ],
          issues: [
            { type: 'warning', message: 'Disk space is running low (22% available)', timestamp: new Date(Date.now() - 120 * 60000) },
            { type: 'info', message: 'System update available', timestamp: new Date(Date.now() - 360 * 60000) }
          ]
        });
        setLoading(false);
      }, 1500);
    };

    fetchData();
  }, []);

  // Handle refresh
  const handleRefresh = () => {
    setIsRefreshing(true);

    // Simulate API call
    setTimeout(() => {
      // Update with slightly different values
      if (systemData) {
        const updatedData: SystemData = {
          cpu: {
            ...systemData.cpu,
            usage: Math.min(100, Math.max(10, systemData.cpu.usage + (Math.random() * 10 - 5))),
            temperature: Math.min(90, Math.max(50, systemData.cpu.temperature + (Math.random() * 6 - 3))),
            history: [...systemData.cpu.history.slice(1), {
              time: new Date().toISOString(),
              value: 30 + Math.random() * 40
            }]
          },
          memory: {
            ...systemData.memory,
            usage: Math.min(95, Math.max(30, systemData.memory.usage + (Math.random() * 8 - 4))),
            available: Math.max(1024, Math.min(systemData.memory.total, systemData.memory.available + (Math.random() * 1024 - 512))),
            history: [...systemData.memory.history.slice(1), {
              time: new Date().toISOString(),
              value: 50 + Math.random() * 30
            }]
          },
          disk: {
            ...systemData.disk,
            usage: Math.min(95, Math.max(60, systemData.disk.usage + (Math.random() * 2 - 1))),
            available: Math.max(10, Math.min(systemData.disk.total, systemData.disk.available + (Math.random() * 5 - 2.5))),
            history: [...systemData.disk.history.slice(1), {
              time: new Date().toISOString(),
              value: 70 + Math.random() * 15
            }]
          },
          network: {
            ...systemData.network,
            latency: Math.max(10, Math.min(200, systemData.network.latency + (Math.random() * 20 - 10))),
            bandwidth: Math.max(50, Math.min(150, systemData.network.bandwidth + (Math.random() * 20 - 10))),
            history: [...systemData.network.history.slice(1), {
              time: new Date().toISOString(),
              download: 80 + Math.random() * 40,
              upload: 20 + Math.random() * 15
            }]
          },
          services: systemData.services,
          issues: systemData.issues
        };
        setSystemData(updatedData);
      }
      setIsRefreshing(false);
    }, 1000);
  };

  // Get status color
  const getStatusColor = (usage: number): 'error' | 'warning' | 'success' => {
    if (usage >= 90) return 'error';
    if (usage >= 70) return 'warning';
    return 'success';
  };

  // Get service status icon
  const getServiceStatusIcon = (status: string): React.ReactNode => {
    switch (status) {
      case 'running':
        return <CheckCircleIcon color="success" />;
      case 'warning':
        return <WarningIcon color="warning" />;
      case 'error':
        return <ErrorIcon color="error" />;
      default:
        return <CheckCircleIcon color="success" />;
    }
  };

  if (loading || !systemData) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h6">System Health</Typography>
        <Button
          variant="outlined"
          startIcon={isRefreshing ? <CircularProgress size={20} /> : <RefreshIcon />}
          onClick={handleRefresh}
          disabled={isRefreshing}
        >
          Refresh
        </Button>
      </Box>

      {/* Resource Usage Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <Box sx={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  bgcolor: `${getStatusColor(systemData.cpu.usage)}.100`,
                  color: `${getStatusColor(systemData.cpu.usage)}.800`,
                  borderRadius: '50%',
                  p: 1,
                  mr: 2
                }}>
                  <SpeedIcon />
                </Box>
                <Typography variant="subtitle1" color="text.secondary">
                  CPU Usage
                </Typography>
              </Box>
              <Typography variant="h4" component="div" sx={{ mb: 1 }}>
                {systemData.cpu.usage.toFixed(1)}%
              </Typography>
              <LinearProgress
                variant="determinate"
                value={systemData.cpu.usage}
                color={getStatusColor(systemData.cpu.usage)}
                sx={{ height: 8, borderRadius: 4 }}
              />
              <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                Temperature: {systemData.cpu.temperature}°C | {systemData.cpu.cores} Cores
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <Box sx={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  bgcolor: `${getStatusColor(systemData.memory.usage)}.100`,
                  color: `${getStatusColor(systemData.memory.usage)}.800`,
                  borderRadius: '50%',
                  p: 1,
                  mr: 2
                }}>
                  <MemoryIcon />
                </Box>
                <Typography variant="subtitle1" color="text.secondary">
                  Memory Usage
                </Typography>
              </Box>
              <Typography variant="h4" component="div" sx={{ mb: 1 }}>
                {systemData.memory.usage.toFixed(1)}%
              </Typography>
              <LinearProgress
                variant="determinate"
                value={systemData.memory.usage}
                color={getStatusColor(systemData.memory.usage)}
                sx={{ height: 8, borderRadius: 4 }}
              />
              <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                {(systemData.memory.available / 1024).toFixed(1)} GB free of {(systemData.memory.total / 1024).toFixed(1)} GB
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <Box sx={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  bgcolor: `${getStatusColor(systemData.disk.usage)}.100`,
                  color: `${getStatusColor(systemData.disk.usage)}.800`,
                  borderRadius: '50%',
                  p: 1,
                  mr: 2
                }}>
                  <StorageIcon />
                </Box>
                <Typography variant="subtitle1" color="text.secondary">
                  Disk Usage
                </Typography>
              </Box>
              <Typography variant="h4" component="div" sx={{ mb: 1 }}>
                {systemData.disk.usage.toFixed(1)}%
              </Typography>
              <LinearProgress
                variant="determinate"
                value={systemData.disk.usage}
                color={getStatusColor(systemData.disk.usage)}
                sx={{ height: 8, borderRadius: 4 }}
              />
              <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                {systemData.disk.available.toFixed(1)} GB free of {systemData.disk.total} GB
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <Box sx={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  bgcolor: 'info.100',
                  color: 'info.800',
                  borderRadius: '50%',
                  p: 1,
                  mr: 2
                }}>
                  <CloudIcon />
                </Box>
                <Typography variant="subtitle1" color="text.secondary">
                  Network
                </Typography>
              </Box>
              <Typography variant="h4" component="div" sx={{ mb: 1 }}>
                {systemData.network.status === 'online' ? 'Online' : 'Offline'}
              </Typography>
              <Typography variant="body2" sx={{ mb: 1 }}>
                Latency: {systemData.network.latency} ms
              </Typography>
              <Typography variant="body2">
                Bandwidth: {systemData.network.bandwidth} Mbps
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Charts and Services */}
      <Grid container spacing={3}>
        {/* CPU Usage Chart */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="subtitle1" gutterBottom>
              CPU Usage History
            </Typography>
            <ResponsiveContainer width="100%" height={250}>
              <AreaChart
                data={systemData.cpu.history}
                margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis
                  dataKey="time"
                  tickFormatter={(time) => {
                    const date = new Date(time);
                    return `${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
                  }}
                />
                <YAxis domain={[0, 100]} />
                <RechartsTooltip
                  formatter={(value: number) => [`${value.toFixed(1)}%`, 'CPU Usage']}
                  labelFormatter={(time: string) => {
                    const date = new Date(time);
                    return `${date.toLocaleTimeString()}`;
                  }}
                />
                <Area
                  type="monotone"
                  dataKey="value"
                  stroke="#8884d8"
                  fill="#8884d8"
                  name="CPU Usage"
                />
              </AreaChart>
            </ResponsiveContainer>
          </Paper>
        </Grid>

        {/* Memory Usage Chart */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="subtitle1" gutterBottom>
              Memory Usage History
            </Typography>
            <ResponsiveContainer width="100%" height={250}>
              <AreaChart
                data={systemData.memory.history}
                margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis
                  dataKey="time"
                  tickFormatter={(time) => {
                    const date = new Date(time);
                    return `${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
                  }}
                />
                <YAxis domain={[0, 100]} />
                <RechartsTooltip
                  formatter={(value: number) => [`${value.toFixed(1)}%`, 'Memory Usage']}
                  labelFormatter={(time: string) => {
                    const date = new Date(time);
                    return `${date.toLocaleTimeString()}`;
                  }}
                />
                <Area
                  type="monotone"
                  dataKey="value"
                  stroke="#82ca9d"
                  fill="#82ca9d"
                  name="Memory Usage"
                />
              </AreaChart>
            </ResponsiveContainer>
          </Paper>
        </Grid>

        {/* Network Usage Chart */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="subtitle1" gutterBottom>
              Network Traffic
            </Typography>
            <ResponsiveContainer width="100%" height={250}>
              <LineChart
                data={systemData.network.history}
                margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis
                  dataKey="time"
                  tickFormatter={(time) => {
                    const date = new Date(time);
                    return `${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
                  }}
                />
                <YAxis />
                <RechartsTooltip
                  formatter={(value: number) => [`${value.toFixed(1)} Mbps`, '']}
                  labelFormatter={(time: string) => {
                    const date = new Date(time);
                    return `${date.toLocaleTimeString()}`;
                  }}
                />
                <Legend />
                <Line
                  type="monotone"
                  dataKey="download"
                  stroke="#8884d8"
                  name="Download"
                />
                <Line
                  type="monotone"
                  dataKey="upload"
                  stroke="#82ca9d"
                  name="Upload"
                />
              </LineChart>
            </ResponsiveContainer>
          </Paper>
        </Grid>

        {/* Services Status */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="subtitle1" gutterBottom>
              Services Status
            </Typography>
            <List>
              {systemData.services.map((service, index) => (
                <React.Fragment key={service.name}>
                  <ListItem>
                    <ListItemIcon>
                      {getServiceStatusIcon(service.status)}
                    </ListItemIcon>
                    <ListItemText
                      primary={service.name}
                      secondary={`Uptime: ${service.uptime}${service.message ? ` | ${service.message}` : ''}`}
                    />
                  </ListItem>
                  {index < systemData.services.length - 1 && <Divider component="li" />}
                </React.Fragment>
              ))}
            </List>
          </Paper>
        </Grid>

        {/* System Issues */}
        <Grid item xs={12}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="subtitle1" gutterBottom>
              System Issues
            </Typography>
            {systemData.issues.length > 0 ? (
              <List>
                {systemData.issues.map((issue, index) => (
                  <React.Fragment key={index}>
                    <ListItem>
                      <ListItemIcon>
                        {issue.type === 'error' ? (
                          <ErrorIcon color="error" />
                        ) : issue.type === 'warning' ? (
                          <WarningIcon color="warning" />
                        ) : (
                          <InfoIcon color="info" />
                        )}
                      </ListItemIcon>
                      <ListItemText
                        primary={issue.message}
                        secondary={issue.timestamp.toLocaleString()}
                      />
                    </ListItem>
                    {index < systemData.issues.length - 1 && <Divider component="li" />}
                  </React.Fragment>
                ))}
              </List>
            ) : (
              <Alert severity="success">
                No system issues detected. All systems are operating normally.
              </Alert>
            )}
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default SystemHealthMonitor;
