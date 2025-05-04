import React from 'react';
import { Typography, Alert, Card, CardContent } from '@mui/material';

/**
 * Component for the Results tab in the scraper details page
 */
const ResultsTab = () => {
  return (
    <>
      <Typography variant="h6" gutterBottom>
        Scraped Results
      </Typography>

      <Alert severity="info" sx={{ mb: 3 }}>
        This section will display the most recent results from this scraper. You can view more detailed results and analysis in the Analytics section.
      </Alert>

      <Card>
        <CardContent>
          <Typography variant="body1" sx={{ p: 2, textAlign: 'center' }}>
            No results available yet
          </Typography>
        </CardContent>
      </Card>
    </>
  );
};

export default ResultsTab;