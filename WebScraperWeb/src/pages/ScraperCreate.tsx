import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Box,
  Typography,
  Button,
  Paper,
  Stepper,
  Step,
  StepLabel,
  Alert
} from '@mui/material';
import { ArrowBack as ArrowBackIcon } from '@mui/icons-material';
import { useScraperContext } from '../contexts/ScraperContext';
import PageHeader from '../components/common/PageHeader';
import LoadingSpinner from '../components/common/LoadingSpinner';
import ScraperForm from '../components/scrapers/ScraperForm';

const ScraperCreate = () => {
  const navigate = useNavigate();
  const { addScraper } = useScraperContext();

  const [activeStep, setActiveStep] = useState(0);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const [createdScraperId, setCreatedScraperId] = useState(null);

  const steps = ['Configure Scraper', 'Success'];

  const handleNext = () => {
    setActiveStep((prevActiveStep) => prevActiveStep + 1);
  };

  const handleBack = () => {
    setActiveStep((prevActiveStep) => prevActiveStep - 1);
  };

  const handleSubmit = async (formData) => {
    try {
      setIsSubmitting(true);
      setError(null);

      // The form already handles default values, so we can use formData directly

      const result = await addScraper(formData);

      if (result && result.id) {
        setCreatedScraperId(result.id);
        handleNext(); // Move to success step
      } else {
        setError('Failed to create scraper. Please try again.');
      }
    } catch (err) {
      setError(err.message || 'An error occurred while creating the scraper');
      console.error('Error creating scraper:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const renderStepContent = (step) => {
    switch (step) {
      case 0:
        return (
          <ScraperForm
            onSubmit={handleSubmit}
            isSubmitting={isSubmitting}
          />
        );
      case 1:
        return (
          <Box sx={{ textAlign: 'center', py: 4 }}>
            <Typography variant="h6" gutterBottom color="success.main">
              Scraper Created Successfully!
            </Typography>
            <Typography variant="body1" paragraph>
              Your new scraper has been created with ID: {createdScraperId}
            </Typography>
            <Box sx={{ mt: 3, display: 'flex', justifyContent: 'center', gap: 2 }}>
              <Button
                variant="contained"
                onClick={() => navigate(`/scrapers/${createdScraperId}`)}
              >
                View Scraper
              </Button>
              <Button
                variant="outlined"
                onClick={() => navigate('/scrapers')}
              >
                Back to Scrapers List
              </Button>
            </Box>
          </Box>
        );
      default:
        return <Typography>Unknown step</Typography>;
    }
  };

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      {/* Header */}
      <PageHeader
        title="Create New Scraper"
        subtitle="Configure a new web scraper"
        breadcrumbs={[
          { text: 'Dashboard', path: '/' },
          { text: 'Scrapers', path: '/scrapers' },
          { text: 'Create New Scraper' }
        ]}
      />

      {/* Back Button */}
      <Box sx={{ mb: 3 }}>
        <Button
          variant="outlined"
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/scrapers')}
        >
          Back to Scrapers
        </Button>
      </Box>

      {/* Error Alert */}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Stepper */}
      <Paper sx={{ mb: 3, p: 3 }}>
        {activeStep === 0 && (
          <Typography variant="h6" gutterBottom>
            Configure your new scraper
          </Typography>
        )}

        {/* Step Content */}
        {renderStepContent(activeStep)}

        {/* Navigation Buttons */}
        {activeStep === 0 && (
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 3 }}>
            <Button
              variant="contained"
              onClick={handleNext}
              disabled={isSubmitting}
              sx={{ display: 'none' }} // Hide this button as we're using the form submit button
            >
              Next
            </Button>
          </Box>
        )}
      </Paper>
    </Container>
  );
};

export default ScraperCreate;
