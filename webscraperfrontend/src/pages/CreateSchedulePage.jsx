import { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Box } from '@mui/material';
import PageHeader from '../components/Common/PageHeader';
import ScheduleForm from '../features/schedules/ScheduleForm/ScheduleForm';
import LoadingSpinner from '../components/Common/LoadingSpinner';
import ErrorMessage from '../components/Common/ErrorMessage';
import useApiClient from '../hooks/useApiClient';

const CreateSchedulePage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { api, loading, error, execute } = useApiClient();
  const [scrapers, setScrapers] = useState([]);
  
  // Parse query parameters to extract scraperId if provided
  const queryParams = new URLSearchParams(location.search);
  const scraperId = queryParams.get('scraperId');
  
  // Fetch scrapers on component mount
  useEffect(() => {
    fetchScrapers();
  }, []);
  
  const fetchScrapers = async () => {
    try {
      const data = await execute(() => api.scrapers.getAll());
      setScrapers(data || []);
    } catch (error) {
      console.error('Error fetching scrapers:', error);
    }
  };

  const handleSubmit = async (formData) => {
    try {
      const response = await execute(() => api.scheduling.create(formData.scraperId, formData));
      // Redirect to the schedules list after successful creation
      navigate('/schedules');
    } catch (error) {
      console.error('Error creating schedule:', error);
      // Error is already handled by the useApiClient hook
    }
  };

  const handleCancel = () => {
    navigate('/schedules');
  };

  const breadcrumbs = [
    { label: 'Schedules', path: '/schedules' },
    { label: 'New Schedule' }
  ];

  if (loading && scrapers.length === 0) {
    return <LoadingSpinner message="Loading scrapers..." />;
  }

  if (error && scrapers.length === 0) {
    return (
      <ErrorMessage 
        title="Failed to load scrapers" 
        message={error}
        onRetry={fetchScrapers}
      />
    );
  }

  return (
    <Box>
      <PageHeader 
        title="Create New Schedule" 
        subtitle="Schedule automated runs of your scrapers"
        breadcrumbs={breadcrumbs}
      />
      
      <ScheduleForm
        scraperId={scraperId}
        scraperOptions={scrapers}
        onSubmit={handleSubmit}
        onCancel={handleCancel}
        loading={loading}
        error={error}
      />
    </Box>
  );
};

export default CreateSchedulePage;