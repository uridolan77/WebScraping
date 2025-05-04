import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Divider,
  Grid,
  Box,
  Tooltip,
  LinearProgress,
  Chip
} from '@mui/material';
import {
  Memory as MemoryIcon,
  Speed as SpeedIcon,
  Security as SecurityIcon,
  Psychology as PsychologyIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Warning as WarningIcon
} from '@mui/icons-material';

/**
 * Component for displaying enhanced metrics in the monitor tab
 */
const EnhancedMetricsCard = ({ monitorData }) => {
  // Check if enhanced features are enabled
  const hasEnhancedFeatures = monitorData?.enhancedFeatures?.enabled || false;
  
  // Get enhanced metrics
  const enhancedMetrics = monitorData?.enhancedFeatures?.metrics || {};
  
  // Helper function to render status chip
  const renderStatusChip = (status) => {
    if (status === 'active') {
      return <Chip 
        icon={<CheckCircleIcon />} 
        label="Active" 
        color="success" 
        size="small" 
        variant="outlined"
      />;
    } else if (status === 'error') {
      return <Chip 
        icon={<ErrorIcon />} 
        label="Error" 
        color="error" 
        size="small" 
        variant="outlined"
      />;
    } else if (status === 'warning') {
      return <Chip 
        icon={<WarningIcon />} 
        label="Warning" 
        color="warning" 
        size="small" 
        variant="outlined"
      />;
    } else {
      return <Chip 
        label="Inactive" 
        color="default" 
        size="small" 
        variant="outlined"
      />;
    }
  };

  if (!hasEnhancedFeatures) {
    return (
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Enhanced Features
          </Typography>
          <Divider sx={{ mb: 2 }} />
          <Typography variant="body2" color="text.secondary" align="center" sx={{ py: 2 }}>
            Enhanced features are not enabled for this scraper.
          </Typography>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Enhanced Features
        </Typography>
        <Divider sx={{ mb: 2 }} />

        <Grid container spacing={2}>
          {/* Content Extraction */}
          <Grid item xs={12}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <MemoryIcon color="primary" sx={{ mr: 1 }} />
                <Typography variant="subtitle2">
                  Streaming Content Extraction
                </Typography>
              </Box>
              {renderStatusChip(enhancedMetrics.contentExtraction?.status || 'inactive')}
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center', mt: 1 }}>
              <Box sx={{ width: '100%', mr: 1 }}>
                <LinearProgress 
                  variant="determinate" 
                  value={enhancedMetrics.contentExtraction?.memoryEfficiency || 0} 
                  color="success"
                />
              </Box>
              <Box sx={{ minWidth: 35 }}>
                <Typography variant="body2" color="text.secondary">
                  {`${Math.round(enhancedMetrics.contentExtraction?.memoryEfficiency || 0)}%`}
                </Typography>
              </Box>
            </Box>
            <Typography variant="caption" color="text.secondary">
              Memory Efficiency: {enhancedMetrics.contentExtraction?.memoryUsage || 'N/A'}
            </Typography>
          </Grid>

          {/* Circuit Breaker */}
          <Grid item xs={12} sm={6}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <SpeedIcon color="primary" sx={{ mr: 1 }} />
                <Typography variant="subtitle2">
                  Circuit Breaker
                </Typography>
              </Box>
              {renderStatusChip(enhancedMetrics.circuitBreaker?.status || 'inactive')}
            </Box>
            <Typography variant="body2">
              Trips: {enhancedMetrics.circuitBreaker?.tripCount || 0}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Last Trip: {enhancedMetrics.circuitBreaker?.lastTripReason || 'None'}
            </Typography>
          </Grid>

          {/* Security Validation */}
          <Grid item xs={12} sm={6}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <SecurityIcon color="primary" sx={{ mr: 1 }} />
                <Typography variant="subtitle2">
                  Security Validation
                </Typography>
              </Box>
              {renderStatusChip(enhancedMetrics.securityValidation?.status || 'inactive')}
            </Box>
            <Typography variant="body2">
              Checks: {enhancedMetrics.securityValidation?.checksPerformed || 0}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Issues Found: {enhancedMetrics.securityValidation?.issuesFound || 0}
            </Typography>
          </Grid>

          {/* ML Classification */}
          <Grid item xs={12}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <PsychologyIcon color="primary" sx={{ mr: 1 }} />
                <Typography variant="subtitle2">
                  ML Content Classification
                </Typography>
              </Box>
              {renderStatusChip(enhancedMetrics.mlClassification?.status || 'inactive')}
            </Box>
            <Typography variant="body2">
              Documents Classified: {enhancedMetrics.mlClassification?.documentsClassified || 0}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Confidence: {enhancedMetrics.mlClassification?.averageConfidence || 'N/A'}
            </Typography>
          </Grid>
        </Grid>
      </CardContent>
    </Card>
  );
};

export default EnhancedMetricsCard;
