import React from 'react';
import {
  Grid, Card, CardContent, Typography, Box,
  Skeleton, Divider, Tooltip, Chip
} from '@mui/material';
import {
  PlayArrow as PlayIcon,
  Stop as StopIcon,
  Error as ErrorIcon,
  CheckCircle as CheckCircleIcon,
  Link as LinkIcon,
  FindInPage as FindInPageIcon,
  Storage as StorageIcon,
  Speed as SpeedIcon
} from '@mui/icons-material';

interface StatusCardProps {
  title: string;
  value: React.ReactNode;
  icon: React.ReactNode;
  color: string;
  isLoading?: boolean;
  tooltip?: string;
  secondaryValue?: string;
}

const StatusCard: React.FC<StatusCardProps> = ({ title, value, icon, color, isLoading, tooltip, secondaryValue }) => {
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
          <Tooltip title={tooltip || ''}>
            <Typography variant="subtitle1" color="text.secondary">
              {title}
            </Typography>
          </Tooltip>
        </Box>
        {isLoading ? (
          <Skeleton variant="text" width="80%" height={40} />
        ) : (
          <Box>
            <Typography variant="h4" component="div" sx={{ fontWeight: 'medium' }}>
              {value}
            </Typography>
            {secondaryValue && (
              <Typography variant="caption" color="text.secondary">
                {secondaryValue}
              </Typography>
            )}
          </Box>
        )}
      </CardContent>
    </Card>
  );
};

interface ScraperData {
  id: string;
  name: string;
  urlsProcessed?: number;
  documentsProcessed?: number;
  status?: {
    isRunning?: boolean;
    hasErrors?: boolean;
    urlsProcessed?: number;
    documentsProcessed?: number;
    avgResponseTime?: number;
  };
}

interface ScraperStatusCardsProps {
  scrapers: ScraperData[];
  scraperStatus: Record<string, any>;
  isLoading?: boolean;
}

const ScraperStatusCards: React.FC<ScraperStatusCardsProps> = ({ scrapers, scraperStatus, isLoading }) => {
  // Calculate summary statistics
  const totalScrapers = scrapers.length;
  const activeScrapers = scrapers.filter(scraper =>
    scraperStatus[scraper.id]?.isRunning
  ).length;
  const scrapersWithErrors = scrapers.filter(scraper =>
    scraperStatus[scraper.id]?.hasErrors
  ).length;
  const totalUrlsProcessed = scrapers.reduce((total, scraper) =>
    total + (scraperStatus[scraper.id]?.urlsProcessed || scraper.urlsProcessed || 0),
    0
  );
  const totalDocumentsProcessed = scrapers.reduce((total, scraper) =>
    total + (scraperStatus[scraper.id]?.documentsProcessed || scraper.documentsProcessed || 0),
    0
  );
  const averageRequestTime = scrapers.reduce((total, scraper) => {
    const status = scraperStatus[scraper.id];
    if (status?.avgResponseTime) {
      return total + status.avgResponseTime;
    }
    return total;
  }, 0) / (scrapers.filter(scraper => scraperStatus[scraper.id]?.avgResponseTime).length || 1);

  return (
    <Grid container spacing={3} sx={{ mb: 3 }}>
      <Grid item xs={12} sm={6} md={3}>
        <StatusCard
          title="Total Scrapers"
          value={totalScrapers}
          icon={<LinkIcon />}
          color="primary"
          isLoading={isLoading}
          tooltip="Total number of configured scrapers"
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <StatusCard
          title="Active Scrapers"
          value={activeScrapers}
          icon={<PlayIcon />}
          color="success"
          isLoading={isLoading}
          tooltip="Number of scrapers currently running"
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <StatusCard
          title="Scrapers with Errors"
          value={scrapersWithErrors}
          icon={<ErrorIcon />}
          color="error"
          isLoading={isLoading}
          tooltip="Number of scrapers with errors"
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <StatusCard
          title="Completed Scrapers"
          value={totalScrapers - activeScrapers - scrapersWithErrors}
          icon={<CheckCircleIcon />}
          color="info"
          isLoading={isLoading}
          tooltip="Number of scrapers that completed successfully"
        />
      </Grid>

      <Grid item xs={12}>
        <Divider sx={{ my: 1 }} />
      </Grid>

      <Grid item xs={12} sm={6} md={3}>
        <StatusCard
          title="URLs Processed"
          value={totalUrlsProcessed.toLocaleString()}
          icon={<LinkIcon />}
          color="primary"
          isLoading={isLoading}
          tooltip="Total number of URLs processed across all scrapers"
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <StatusCard
          title="Documents Processed"
          value={totalDocumentsProcessed.toLocaleString()}
          icon={<FindInPageIcon />}
          color="secondary"
          isLoading={isLoading}
          tooltip="Total number of documents processed across all scrapers"
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <StatusCard
          title="Avg. Response Time"
          value={`${averageRequestTime.toFixed(0)} ms`}
          icon={<SpeedIcon />}
          color="warning"
          isLoading={isLoading}
          tooltip="Average response time across all active scrapers"
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <StatusCard
          title="Storage Used"
          value={`${((Math.random() * 100) + 50).toFixed(2)} MB`} // Placeholder value
          icon={<StorageIcon />}
          color="info"
          isLoading={isLoading}
          tooltip="Total storage used by all scrapers"
          secondaryValue="Across all scrapers"
        />
      </Grid>
    </Grid>
  );
};

export default ScraperStatusCards;
