import React from 'react';
import { 
  Box, 
  Typography, 
  Grid, 
  FormControlLabel, 
  Switch, 
  Tooltip, 
  IconButton,
  Divider,
  Paper
} from '@mui/material';
import HelpIcon from '@mui/icons-material/Help';

/**
 * Component for the Enhanced Features tab in the scraper form
 */
const EnhancedFeaturesTab = ({ scraper, handleChange }) => {
  return (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom>
        Enhanced Performance Features
      </Typography>
      <Typography variant="body2" color="text.secondary" paragraph>
        These advanced features improve performance, memory usage, and resilience of your scraper.
      </Typography>
      <Divider sx={{ my: 2 }} />

      {/* Enhanced Content Extraction */}
      <Box sx={{ mb: 3 }}>
        <Typography variant="subtitle1" gutterBottom>
          Content Extraction Enhancements
        </Typography>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <FormControlLabel
              control={
                <Switch
                  checked={scraper.enableEnhancedContentExtraction || false}
                  onChange={(e) => handleChange('enableEnhancedContentExtraction', e.target.checked)}
                />
              }
              label={
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  Enable Enhanced Content Extraction
                  <Tooltip title="Uses streaming content extraction to reduce memory usage and improve performance for large pages">
                    <IconButton size="small">
                      <HelpIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </Box>
              }
            />
          </Grid>
        </Grid>
      </Box>

      {/* Resilience Features */}
      <Box sx={{ mb: 3 }}>
        <Typography variant="subtitle1" gutterBottom>
          Resilience Features
        </Typography>
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <FormControlLabel
              control={
                <Switch
                  checked={scraper.enableCircuitBreaker || false}
                  onChange={(e) => handleChange('enableCircuitBreaker', e.target.checked)}
                />
              }
              label={
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  Enable Circuit Breaker
                  <Tooltip title="Prevents cascading failures by temporarily stopping requests to domains that are failing">
                    <IconButton size="small">
                      <HelpIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </Box>
              }
            />
          </Grid>
          <Grid item xs={12} md={6}>
            <FormControlLabel
              control={
                <Switch
                  checked={scraper.enableRetryWithBackoff || false}
                  onChange={(e) => handleChange('enableRetryWithBackoff', e.target.checked)}
                />
              }
              label={
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  Enable Retry with Backoff
                  <Tooltip title="Automatically retries failed requests with exponential backoff">
                    <IconButton size="small">
                      <HelpIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </Box>
              }
            />
          </Grid>
        </Grid>
      </Box>

      {/* Security Features */}
      <Box sx={{ mb: 3 }}>
        <Typography variant="subtitle1" gutterBottom>
          Security Features
        </Typography>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <FormControlLabel
              control={
                <Switch
                  checked={scraper.enableSecurityValidation || false}
                  onChange={(e) => handleChange('enableSecurityValidation', e.target.checked)}
                />
              }
              label={
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  Enable Security Validation
                  <Tooltip title="Validates security headers and HTTPS usage of scraped sites">
                    <IconButton size="small">
                      <HelpIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </Box>
              }
            />
          </Grid>
        </Grid>
      </Box>

      {/* Advanced Content Processing */}
      <Box>
        <Typography variant="subtitle1" gutterBottom>
          Advanced Content Processing
        </Typography>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <FormControlLabel
              control={
                <Switch
                  checked={scraper.enableMachineLearningClassification || false}
                  onChange={(e) => handleChange('enableMachineLearningClassification', e.target.checked)}
                />
              }
              label={
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  Enable ML Content Classification
                  <Tooltip title="Uses machine learning to classify content and extract entities">
                    <IconButton size="small">
                      <HelpIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </Box>
              }
            />
          </Grid>
        </Grid>
      </Box>
    </Paper>
  );
};

export default EnhancedFeaturesTab;
