import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  FormControlLabel,
  Switch,
  Divider,
  Button,
  Alert,
  CircularProgress,
  Tooltip,
  IconButton,
  Chip
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  Memory as MemoryIcon,
  Speed as SpeedIcon,
  Security as SecurityIcon,
  Psychology as PsychologyIcon,
  HelpOutline as HelpIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon
} from '@mui/icons-material';
import useEnhancedFeatures from '../../hooks/useEnhancedFeatures';

/**
 * Component for managing enhanced features in the scraper detail page
 */
const EnhancedFeaturesManager = ({ scraper }) => {
  const {
    metrics,
    isLoading,
    error,
    fetchMetrics,
    toggleContentExtraction,
    toggleCircuitBreaker,
    toggleSecurity,
    toggleMlClassification,
    resetCircuitBreaker
  } = useEnhancedFeatures(scraper?.id);

  const [features, setFeatures] = useState({
    enableEnhancedContentExtraction: scraper?.enableEnhancedContentExtraction || false,
    enableCircuitBreaker: scraper?.enableCircuitBreaker || false,
    enableSecurityValidation: scraper?.enableSecurityValidation || false,
    enableMachineLearningClassification: scraper?.enableMachineLearningClassification || false
  });

  // Fetch metrics on component mount
  useEffect(() => {
    if (scraper?.id) {
      fetchMetrics();
    }
  }, [scraper?.id, fetchMetrics]);

  // Update local state when scraper changes
  useEffect(() => {
    if (scraper) {
      setFeatures({
        enableEnhancedContentExtraction: scraper.enableEnhancedContentExtraction || false,
        enableCircuitBreaker: scraper.enableCircuitBreaker || false,
        enableSecurityValidation: scraper.enableSecurityValidation || false,
        enableMachineLearningClassification: scraper.enableMachineLearningClassification || false
      });
    }
  }, [scraper]);

  // Handle feature toggle
  const handleToggle = async (feature, enabled) => {
    setFeatures(prev => ({ ...prev, [feature]: enabled }));
    
    try {
      switch (feature) {
        case 'enableEnhancedContentExtraction':
          await toggleContentExtraction(enabled);
          break;
        case 'enableCircuitBreaker':
          await toggleCircuitBreaker(enabled);
          break;
        case 'enableSecurityValidation':
          await toggleSecurity(enabled);
          break;
        case 'enableMachineLearningClassification':
          await toggleMlClassification(enabled);
          break;
        default:
          break;
      }
    } catch (err) {
      console.error(`Error toggling ${feature}:`, err);
      // Revert the toggle if there was an error
      setFeatures(prev => ({ ...prev, [feature]: !enabled }));
    }
  };

  // Handle circuit breaker reset
  const handleResetCircuitBreaker = async () => {
    try {
      await resetCircuitBreaker();
    } catch (err) {
      console.error('Error resetting circuit breaker:', err);
    }
  };

  // Helper function to render feature status
  const renderFeatureStatus = (feature) => {
    const isEnabled = features[feature];
    const isActive = metrics?.features?.[feature]?.status === 'active';
    const hasError = metrics?.features?.[feature]?.status === 'error';
    
    if (!isEnabled) {
      return <Chip label="Disabled" color="default" size="small" />;
    }
    
    if (isActive) {
      return <Chip icon={<CheckCircleIcon />} label="Active" color="success" size="small" />;
    }
    
    if (hasError) {
      return <Chip icon={<ErrorIcon />} label="Error" color="error" size="small" />;
    }
    
    return <Chip label="Enabled" color="primary" size="small" />;
  };

  return (
    <Box>
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h6">
          Enhanced Features
        </Typography>
        <Button
          variant="outlined"
          startIcon={<RefreshIcon />}
          onClick={fetchMetrics}
          disabled={isLoading}
        >
          Refresh Metrics
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error.message || 'Error loading enhanced features metrics'}
        </Alert>
      )}

      {isLoading && !metrics ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
          <CircularProgress />
        </Box>
      ) : (
        <Grid container spacing={3}>
          {/* Content Extraction */}
          <Grid item xs={12} md={6}>
            <Paper sx={{ p: 3, height: '100%' }}>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <MemoryIcon color="primary" sx={{ mr: 1 }} />
                <Typography variant="h6">
                  Streaming Content Extraction
                </Typography>
                <Tooltip title="Uses streaming content extraction to reduce memory usage and improve performance for large pages">
                  <IconButton size="small">
                    <HelpIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
              </Box>
              <Divider sx={{ mb: 2 }} />
              
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={features.enableEnhancedContentExtraction}
                      onChange={(e) => handleToggle('enableEnhancedContentExtraction', e.target.checked)}
                    />
                  }
                  label="Enable"
                />
                {renderFeatureStatus('enableEnhancedContentExtraction')}
              </Box>
              
              <Typography variant="body2" color="text.secondary" paragraph>
                Optimizes memory usage by processing content in streams rather than loading entire pages into memory.
              </Typography>
              
              {metrics?.features?.enableEnhancedContentExtraction && (
                <Box sx={{ mt: 2 }}>
                  <Typography variant="subtitle2">
                    Memory Efficiency: {metrics.features.enableEnhancedContentExtraction.memoryEfficiency || 'N/A'}
                  </Typography>
                  <Typography variant="subtitle2">
                    Pages Processed: {metrics.features.enableEnhancedContentExtraction.pagesProcessed || 0}
                  </Typography>
                </Box>
              )}
            </Paper>
          </Grid>

          {/* Circuit Breaker */}
          <Grid item xs={12} md={6}>
            <Paper sx={{ p: 3, height: '100%' }}>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <SpeedIcon color="primary" sx={{ mr: 1 }} />
                <Typography variant="h6">
                  Circuit Breaker
                </Typography>
                <Tooltip title="Prevents cascading failures by temporarily stopping requests to domains that are failing">
                  <IconButton size="small">
                    <HelpIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
              </Box>
              <Divider sx={{ mb: 2 }} />
              
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={features.enableCircuitBreaker}
                      onChange={(e) => handleToggle('enableCircuitBreaker', e.target.checked)}
                    />
                  }
                  label="Enable"
                />
                {renderFeatureStatus('enableCircuitBreaker')}
              </Box>
              
              <Typography variant="body2" color="text.secondary" paragraph>
                Automatically stops requests to failing domains and gradually resumes after a cooldown period.
              </Typography>
              
              {metrics?.features?.enableCircuitBreaker && (
                <Box sx={{ mt: 2 }}>
                  <Typography variant="subtitle2">
                    Trip Count: {metrics.features.enableCircuitBreaker.tripCount || 0}
                  </Typography>
                  <Typography variant="subtitle2">
                    Last Trip: {metrics.features.enableCircuitBreaker.lastTripReason || 'None'}
                  </Typography>
                  
                  <Button
                    variant="outlined"
                    size="small"
                    onClick={handleResetCircuitBreaker}
                    disabled={!features.enableCircuitBreaker}
                    sx={{ mt: 1 }}
                  >
                    Reset Circuit Breaker
                  </Button>
                </Box>
              )}
            </Paper>
          </Grid>

          {/* Security Validation */}
          <Grid item xs={12} md={6}>
            <Paper sx={{ p: 3, height: '100%' }}>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <SecurityIcon color="primary" sx={{ mr: 1 }} />
                <Typography variant="h6">
                  Security Validation
                </Typography>
                <Tooltip title="Validates security headers and HTTPS usage of scraped sites">
                  <IconButton size="small">
                    <HelpIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
              </Box>
              <Divider sx={{ mb: 2 }} />
              
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={features.enableSecurityValidation}
                      onChange={(e) => handleToggle('enableSecurityValidation', e.target.checked)}
                    />
                  }
                  label="Enable"
                />
                {renderFeatureStatus('enableSecurityValidation')}
              </Box>
              
              <Typography variant="body2" color="text.secondary" paragraph>
                Checks for security headers, HTTPS usage, and other security best practices on scraped sites.
              </Typography>
              
              {metrics?.features?.enableSecurityValidation && (
                <Box sx={{ mt: 2 }}>
                  <Typography variant="subtitle2">
                    Checks Performed: {metrics.features.enableSecurityValidation.checksPerformed || 0}
                  </Typography>
                  <Typography variant="subtitle2">
                    Issues Found: {metrics.features.enableSecurityValidation.issuesFound || 0}
                  </Typography>
                </Box>
              )}
            </Paper>
          </Grid>

          {/* ML Content Classification */}
          <Grid item xs={12} md={6}>
            <Paper sx={{ p: 3, height: '100%' }}>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <PsychologyIcon color="primary" sx={{ mr: 1 }} />
                <Typography variant="h6">
                  ML Content Classification
                </Typography>
                <Tooltip title="Uses machine learning to classify content and extract entities">
                  <IconButton size="small">
                    <HelpIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
              </Box>
              <Divider sx={{ mb: 2 }} />
              
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={features.enableMachineLearningClassification}
                      onChange={(e) => handleToggle('enableMachineLearningClassification', e.target.checked)}
                    />
                  }
                  label="Enable"
                />
                {renderFeatureStatus('enableMachineLearningClassification')}
              </Box>
              
              <Typography variant="body2" color="text.secondary" paragraph>
                Automatically classifies content and extracts entities using machine learning algorithms.
              </Typography>
              
              {metrics?.features?.enableMachineLearningClassification && (
                <Box sx={{ mt: 2 }}>
                  <Typography variant="subtitle2">
                    Documents Classified: {metrics.features.enableMachineLearningClassification.documentsClassified || 0}
                  </Typography>
                  <Typography variant="subtitle2">
                    Average Confidence: {metrics.features.enableMachineLearningClassification.averageConfidence || 'N/A'}
                  </Typography>
                </Box>
              )}
            </Paper>
          </Grid>
        </Grid>
      )}
    </Box>
  );
};

export default EnhancedFeaturesManager;
