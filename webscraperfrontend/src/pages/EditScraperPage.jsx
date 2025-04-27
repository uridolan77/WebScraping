import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Box } from '@mui/material';
import PageHeader from '../components/Common/PageHeader';
import ScraperForm from '../features/scrapers/ScraperForm/ScraperForm';
import LoadingSpinner from '../components/Common/LoadingSpinner';
import ErrorMessage from '../components/Common/ErrorMessage';
import useApiClient from '../hooks/useApiClient';

const EditScraperPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { api, loading, error, execute } = useApiClient();
  const [scraper, setScraper] = useState(null);
  const [isDeleting, setIsDeleting] = useState(false);
  
  // Fetch scraper data on component mount
  useEffect(() => {
    const fetchScraper = async () => {
      try {
        const data = await execute(() => api.scrapers.getById(id));
        setScraper(data);
      } catch (err) {
        console.error('Error fetching scraper:', err);
      }
    };
    
    fetchScraper();
  }, [id, execute, api.scrapers]);
  
  const handleSubmit = async (formData) => {
    try {
      await execute(() => api.scrapers.update(id, formData));
      // Redirect to the scraper detail page after successful update
      navigate(`/scrapers/${id}`);
    } catch (error) {
      console.error('Error updating scraper:', error);
    }
  };

  const handleCancel = () => {
    navigate(`/scrapers/${id}`);
  };

  const handleDelete = async () => {
    if (window.confirm(`Are you sure you want to delete the scraper "${scraper.name}"?`)) {
      setIsDeleting(true);
      try {
        await execute(() => api.scrapers.delete(id));
        navigate('/scrapers');
      } catch (error) {
        console.error('Error deleting scraper:', error);
        setIsDeleting(false);
      }
    }
  };
  
  const breadcrumbs = [
    { label: 'Scrapers', path: '/scrapers' },
    { label: scraper?.name || 'Scraper Details', path: `/scrapers/${id}` },
    { label: 'Edit' }
  ];

  if (loading && !scraper) {
    return <LoadingSpinner message="Loading scraper data..." />;
  }

  if (error && !scraper) {
    return (
      <ErrorMessage 
        title="Failed to load scraper" 
        message={error}
        onRetry={() => window.location.reload()}
      />
    );
  }

  if (!scraper) {
    return (
      <ErrorMessage 
        title="Scraper not found" 
        message="The scraper you're looking for does not exist or has been deleted."
      />
    );
  }

  return (
    <Box>
      <PageHeader 
        title={`Edit ${scraper.name}`} 
        subtitle="Modify scraper configuration"
        breadcrumbs={breadcrumbs}
      />
      
      <ScraperForm
        initialData={scraper}
        onSubmit={handleSubmit}
        onCancel={handleCancel}
        onDelete={handleDelete}
        loading={loading || isDeleting}
        error={error}
      />
    </Box>
  );
};

export default EditScraperPage;