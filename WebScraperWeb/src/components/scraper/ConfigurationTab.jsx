import React from 'react';
import { 
  Card, 
  CardContent, 
  Typography, 
  Divider, 
  Grid, 
  Box, 
  Chip 
} from '@mui/material';

/**
 * Component for the Configuration tab in the scraper details page
 */
const ConfigurationTab = ({ scraper }) => {
  return (
    <Card>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Scraper Configuration
        </Typography>
        <Divider sx={{ mb: 2 }} />

        <Grid container spacing={2}>
          <Grid item xs={12} md={6}>
            <Typography variant="subtitle2" color="textSecondary">
              Name
            </Typography>
            <Typography variant="body1" sx={{ mb: 2 }}>
              {scraper?.name || 'N/A'}
            </Typography>

            <Typography variant="subtitle2" color="textSecondary">
              Base URL
            </Typography>
            <Typography variant="body1" sx={{ mb: 2 }}>
              {scraper?.baseUrl || 'N/A'}
            </Typography>

            <Typography variant="subtitle2" color="textSecondary">
              Start URL
            </Typography>
            <Typography variant="body1" sx={{ mb: 2 }}>
              {scraper?.startUrl || 'N/A'}
            </Typography>

            <Typography variant="subtitle2" color="textSecondary">
              Output Directory
            </Typography>
            <Typography variant="body1" sx={{ mb: 2 }}>
              {scraper?.outputDirectory || 'Default'}
            </Typography>
          </Grid>

          <Grid item xs={12} md={6}>
            <Typography variant="subtitle2" color="textSecondary">
              Max Depth
            </Typography>
            <Typography variant="body1" sx={{ mb: 2 }}>
              {scraper?.maxDepth || 'N/A'}
            </Typography>

            <Typography variant="subtitle2" color="textSecondary">
              Max Pages
            </Typography>
            <Typography variant="body1" sx={{ mb: 2 }}>
              {scraper?.maxPages || 'N/A'}
            </Typography>

            <Typography variant="subtitle2" color="textSecondary">
              Max Concurrent Requests
            </Typography>
            <Typography variant="body1" sx={{ mb: 2 }}>
              {scraper?.maxConcurrentRequests || 'N/A'}
            </Typography>

            <Typography variant="subtitle2" color="textSecondary">
              Delay Between Requests (ms)
            </Typography>
            <Typography variant="body1" sx={{ mb: 2 }}>
              {scraper?.delayBetweenRequests || 'N/A'}
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Divider sx={{ my: 2 }} />
            <Typography variant="h6" gutterBottom>
              Features
            </Typography>

            <Grid container spacing={2}>
              <FeatureItem 
                label="Follow Links" 
                enabled={scraper?.followLinks} 
              />
              
              <FeatureItem 
                label="Follow External Links" 
                enabled={scraper?.followExternalLinks} 
              />
              
              <FeatureItem 
                label="Respect Robots.txt" 
                enabled={scraper?.respectRobotsTxt} 
              />
              
              <FeatureItem 
                label="Change Detection" 
                enabled={scraper?.enableChangeDetection} 
              />
              
              <FeatureItem 
                label="Track Content Versions" 
                enabled={scraper?.trackContentVersions} 
              />
              
              <FeatureItem 
                label="Adaptive Crawling" 
                enabled={scraper?.enableAdaptiveCrawling} 
              />
            </Grid>
          </Grid>
        </Grid>
      </CardContent>
    </Card>
  );
};

/**
 * Helper component for displaying feature settings
 */
const FeatureItem = ({ label, enabled }) => {
  return (
    <>
      <Grid item xs={6} sm={4}>
        <Box>{label}:</Box>
      </Grid>
      <Grid item xs={6} sm={2}>
        <Chip
          label={enabled ? 'Enabled' : 'Disabled'}
          color={enabled ? 'success' : 'default'}
          size="small"
        />
      </Grid>
    </>
  );
};

export default ConfigurationTab;