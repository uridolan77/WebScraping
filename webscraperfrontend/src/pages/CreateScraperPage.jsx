import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Box } from '@mui/material';
import PageHeader from '../components/Common/PageHeader';
import ScraperForm from '../features/scrapers/ScraperForm/ScraperForm';
import useApiClient from '../hooks/useApiClient';

const CreateScraperPage = () => {
  const navigate = useNavigate();
  const { api, loading, error, execute } = useApiClient();
  
  const handleSubmit = async (formData) => {
    try {
      const response = await execute(() => api.scrapers.create(formData));
      // Redirect to the scraper detail page after successful creation
      navigate(`/scrapers/${response.id}`);
    } catch (error) {
      console.error('Error creating scraper:', error);
      // Error is already handled by the useApiClient hook
    }
  };

  const handleCancel = () => {
    navigate('/scrapers');
  };

  const breadcrumbs = [
    { label: 'Scrapers', path: '/scrapers' },
    { label: 'New Scraper' }
  ];

  return (
    <Box>
      <PageHeader 
        title="Create New Scraper" 
        subtitle="Configure a new web scraper"
        breadcrumbs={breadcrumbs}
      />
      
      <ScraperForm
        onSubmit={handleSubmit}
        onCancel={handleCancel}
        loading={loading}
        error={error}
      />
    </Box>
  );
};

export default CreateScraperPage;