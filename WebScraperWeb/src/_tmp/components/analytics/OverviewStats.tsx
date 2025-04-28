import React from 'react';
import {
  Grid, Paper, Typography, Box,
  Skeleton, Divider, Tooltip
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
  Info as InfoIcon,
  CheckCircle as CheckCircleIcon
} from '@mui/icons-material';

interface StatCardProps {
  title: string;
  value: string | number;
  icon: React.ReactNode;
  color: string;
  isLoading: boolean;
  tooltip?: string;
}

const StatCard: React.FC<StatCardProps> = ({ title, value, icon, color, isLoading, tooltip }) => {
  return (
    <Paper sx={{ p: 2, display: 'flex', flexDirection: 'column', height: '100%' }}>
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
        <Tooltip title={tooltip || ''}>
          <Typography variant="subtitle1" color="text.secondary">
            {title}
          </Typography>
        </Tooltip>
      </Box>
      {isLoading ? (
        <Skeleton variant="text" width="80%" height={40} />
      ) : (
        <Typography variant="h4" component="div" sx={{ fontWeight: 'medium' }}>
          {value}
        </Typography>
      )}
    </Paper>
  );
};

interface OverviewData {
  totalScrapers?: number;
  activeScrapers?: number;
  totalUrlsProcessed?: number;
  totalChangesDetected?: number;
  avgRequestTime?: number;
  storageUsed?: number;
  totalErrors?: number;
  successRate?: number;
}

interface OverviewStatsProps {
  data: OverviewData | null;
  isLoading: boolean;
}

const OverviewStats: React.FC<OverviewStatsProps> = ({ data, isLoading }) => {
  return (
    <Grid container spacing={3}>
      <Grid item xs={12} sm={6} md={3}>
        <StatCard
          title="Total Scrapers"
          value={data?.totalScrapers || 0}
          icon={<LanguageIcon />}
          color="primary"
          isLoading={isLoading}
          tooltip="Total number of configured scrapers"
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <StatCard
          title="Active Scrapers"
          value={data?.activeScrapers || 0}
          icon={<CheckCircleIcon />}
          color="success"
          isLoading={isLoading}
          tooltip="Number of scrapers currently running"
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <StatCard
          title="URLs Processed"
          value={data?.totalUrlsProcessed?.toLocaleString() || 0}
          icon={<LinkIcon />}
          color="info"
          isLoading={isLoading}
          tooltip="Total number of URLs processed across all scrapers"
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <StatCard
          title="Content Changes"
          value={data?.totalChangesDetected?.toLocaleString() || 0}
          icon={<ChangeHistoryIcon />}
          color="warning"
          isLoading={isLoading}
          tooltip="Total number of content changes detected"
        />
      </Grid>

      <Grid item xs={12}>
        <Divider sx={{ my: 1 }} />
      </Grid>

      <Grid item xs={12} sm={6} md={3}>
        <StatCard
          title="Avg. Request Time"
          value={data?.avgRequestTime ? `${data.avgRequestTime.toFixed(2)} ms` : '0 ms'}
          icon={<SpeedIcon />}
          color="success"
          isLoading={isLoading}
          tooltip="Average time per request across all scrapers"
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <StatCard
          title="Storage Used"
          value={data?.storageUsed ? `${(data.storageUsed / (1024 * 1024)).toFixed(2)} MB` : '0 MB'}
          icon={<StorageIcon />}
          color="primary"
          isLoading={isLoading}
          tooltip="Total storage used by all scrapers"
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <StatCard
          title="Errors"
          value={data?.totalErrors || 0}
          icon={<ErrorIcon />}
          color="error"
          isLoading={isLoading}
          tooltip="Total number of errors across all scrapers"
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <StatCard
          title="Success Rate"
          value={data?.successRate ? `${data.successRate.toFixed(1)}%` : '0%'}
          icon={<CheckCircleIcon />}
          color="success"
          isLoading={isLoading}
          tooltip="Percentage of successful requests"
        />
      </Grid>
    </Grid>
  );
};

export default OverviewStats;
