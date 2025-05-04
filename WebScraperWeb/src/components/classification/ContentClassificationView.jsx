import React, { useState, useEffect } from 'react';
import { 
  Box, 
  Card, 
  CardContent, 
  Typography, 
  Chip, 
  Grid, 
  CircularProgress, 
  Alert, 
  Divider,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  LinearProgress
} from '@mui/material';
import { getContentClassification } from '../../api/contentClassification';

/**
 * Component for displaying content classification results
 */
const ContentClassificationView = ({ scraperId, url }) => {
  const [classification, setClassification] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchClassification = async () => {
      if (!scraperId || !url) return;
      
      setLoading(true);
      setError(null);
      
      try {
        const result = await getContentClassification(scraperId, url);
        setClassification(result);
      } catch (err) {
        setError(err.message || 'Failed to fetch classification');
      } finally {
        setLoading(false);
      }
    };
    
    fetchClassification();
  }, [scraperId, url]);

  // Helper function to get color for document type
  const getDocumentTypeColor = (type) => {
    switch (type) {
      case 'Regulation':
        return 'error';
      case 'Guidance':
        return 'primary';
      case 'News':
        return 'success';
      default:
        return 'default';
    }
  };

  // Helper function to get color for sentiment
  const getSentimentColor = (sentiment) => {
    switch (sentiment) {
      case 'Positive':
        return 'success';
      case 'Negative':
        return 'error';
      case 'Neutral':
        return 'info';
      default:
        return 'default';
    }
  };

  // Helper function to format confidence as percentage
  const formatConfidence = (confidence) => {
    return `${Math.round(confidence * 100)}%`;
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mb: 2 }}>
        {error}
      </Alert>
    );
  }

  if (!classification) {
    return (
      <Alert severity="info" sx={{ mb: 2 }}>
        No classification data available for this URL.
      </Alert>
    );
  }

  return (
    <Card variant="outlined" sx={{ mb: 3 }}>
      <CardContent>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="h6">Content Classification</Typography>
          <Chip 
            label={classification.documentType} 
            color={getDocumentTypeColor(classification.documentType)}
            size="small"
          />
        </Box>
        
        <Divider sx={{ mb: 2 }} />
        
        <Grid container spacing={2}>
          {/* Content metrics */}
          <Grid item xs={12} md={6}>
            <Typography variant="subtitle2" color="text.secondary" gutterBottom>
              Content Metrics
            </Typography>
            <Box sx={{ mb: 2 }}>
              <Grid container spacing={1}>
                <Grid item xs={6}>
                  <Typography variant="body2" color="text.secondary">Content Length:</Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="body2">{classification.contentLength} characters</Typography>
                </Grid>
                
                <Grid item xs={6}>
                  <Typography variant="body2" color="text.secondary">Sentences:</Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="body2">{classification.sentenceCount}</Typography>
                </Grid>
                
                <Grid item xs={6}>
                  <Typography variant="body2" color="text.secondary">Paragraphs:</Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="body2">{classification.paragraphCount}</Typography>
                </Grid>
                
                <Grid item xs={6}>
                  <Typography variant="body2" color="text.secondary">Readability Score:</Typography>
                </Grid>
                <Grid item xs={6}>
                  <Typography variant="body2">{classification.readabilityScore.toFixed(2)}</Typography>
                </Grid>
              </Grid>
            </Box>
          </Grid>
          
          {/* Sentiment analysis */}
          <Grid item xs={12} md={6}>
            <Typography variant="subtitle2" color="text.secondary" gutterBottom>
              Sentiment Analysis
            </Typography>
            <Box sx={{ mb: 2 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <Typography variant="body2" color="text.secondary" sx={{ mr: 1 }}>
                  Overall Sentiment:
                </Typography>
                <Chip 
                  label={classification.overallSentiment} 
                  color={getSentimentColor(classification.overallSentiment)}
                  size="small"
                />
              </Box>
              
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Sentiment Scores:
              </Typography>
              
              <Box sx={{ mb: 1 }}>
                <Typography variant="body2" color="text.secondary">
                  Positive: {classification.positiveScore}%
                </Typography>
                <LinearProgress 
                  variant="determinate" 
                  value={classification.positiveScore} 
                  color="success"
                  sx={{ height: 8, borderRadius: 1 }}
                />
              </Box>
              
              <Box>
                <Typography variant="body2" color="text.secondary">
                  Negative: {classification.negativeScore}%
                </Typography>
                <LinearProgress 
                  variant="determinate" 
                  value={classification.negativeScore} 
                  color="error"
                  sx={{ height: 8, borderRadius: 1 }}
                />
              </Box>
            </Box>
          </Grid>
          
          {/* Classification confidence */}
          <Grid item xs={12}>
            <Typography variant="subtitle2" color="text.secondary" gutterBottom>
              Classification Confidence: {formatConfidence(classification.confidence)}
            </Typography>
            <LinearProgress 
              variant="determinate" 
              value={classification.confidence * 100} 
              color="primary"
              sx={{ height: 10, borderRadius: 1 }}
            />
          </Grid>
          
          {/* Entities */}
          {classification.entities && classification.entities.length > 0 && (
            <Grid item xs={12}>
              <Typography variant="subtitle2" color="text.secondary" gutterBottom sx={{ mt: 2 }}>
                Extracted Entities
              </Typography>
              <TableContainer component={Paper} variant="outlined">
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Type</TableCell>
                      <TableCell>Value</TableCell>
                      <TableCell align="right">Confidence</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {classification.entities.map((entity, index) => (
                      <TableRow key={index}>
                        <TableCell>
                          <Chip 
                            label={entity.type} 
                            size="small"
                            color={
                              entity.type === 'Organization' ? 'primary' :
                              entity.type === 'Date' ? 'secondary' :
                              entity.type === 'Money' ? 'success' :
                              entity.type === 'Percentage' ? 'warning' :
                              'default'
                            }
                          />
                        </TableCell>
                        <TableCell>{entity.value}</TableCell>
                        <TableCell align="right">{entity.confidence ? formatConfidence(entity.confidence) : 'N/A'}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Grid>
          )}
        </Grid>
      </CardContent>
    </Card>
  );
};

export default ContentClassificationView;
