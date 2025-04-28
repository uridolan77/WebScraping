import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Container,
  Box,
  Typography,
  Button,
  Paper,
  Alert,
  CircularProgress
} from '@mui/material';
import { ArrowBack as ArrowBackIcon } from '@mui/icons-material';
import { useScraperContext } from '../contexts/ScraperContext';
import PageHeader from '../components/common/PageHeader';
import LoadingSpinner from '../components/common/LoadingSpinner';
import ScraperForm from '../components/scrapers/ScraperForm';

const ScraperEdit = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const {
    fetchScraper,
    selectedScraper,
    loading,
    error,
    editScraper
  } = useScraperContext();

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState(null);
  const [submitSuccess, setSubmitSuccess] = useState(false);

  // Fetch scraper data
  useEffect(() => {
    if (id) {
      fetchScraper(id);
    }
  }, [id, fetchScraper]);

  const handleSubmit = async (formData) => {
    if (!id) return;

    try {
      setIsSubmitting(true);
      setSubmitError(null);
      setSubmitSuccess(false);

      const result = await editScraper(id, formData);

      if (result) {
        setSubmitSuccess(true);
        // Navigate back to detail page after a short delay
        setTimeout(() => {
          navigate(`/scrapers/${id}`);
        }, 1500);
      } else {
        setSubmitError('Failed to update scraper. Please try again.');
      }
    } catch (err) {
      setSubmitError(err.message || 'An error occurred while updating the scraper');
      console.error('Error updating scraper:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  if (loading && !selectedScraper) {
    return <LoadingSpinner message="Loading scraper details..." />;
  }

  if (error) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Alert severity="error" sx={{ mb: 2 }}>
          Error loading scraper: {error}
        </Alert>
        <Button
          variant="contained"
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/scrapers')}
        >
          Back to Scrapers
        </Button>
      </Container>
    );
  }

  if (!selectedScraper) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Alert severity="warning" sx={{ mb: 2 }}>
          Scraper not found
        </Alert>
        <Button
          variant="contained"
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/scrapers')}
        >
          Back to Scrapers
        </Button>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      {/* Header */}
      <PageHeader
        title={`Edit ${selectedScraper.name}`}
        subtitle="Modify scraper configuration"
        breadcrumbs={[
          { text: 'Dashboard', path: '/' },
          { text: 'Scrapers', path: '/scrapers' },
          { text: selectedScraper.name, path: `/scrapers/${id}` },
          { text: 'Edit' }
        ]}
      />

      {/* Back Button */}
      <Box sx={{ mb: 3 }}>
        <Button
          variant="outlined"
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate(`/scrapers/${id}`)}
        >
          Back to Scraper Details
        </Button>
      </Box>

      {/* Success Alert */}
      {submitSuccess && (
        <Alert severity="success" sx={{ mb: 3 }}>
          Scraper updated successfully! Redirecting...
        </Alert>
      )}

      {/* Error Alert */}
      {submitError && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {submitError}
        </Alert>
      )}

      {/* Form */}
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>Edit Scraper Configuration</Typography>

        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <ScraperForm
            initialValues={selectedScraper}
            onSubmit={handleSubmit}
            isSubmitting={isSubmitting}
            isEditMode={true}
          />
        )}
      </Paper>
    </Container>
  );
};

export default ScraperEdit;
