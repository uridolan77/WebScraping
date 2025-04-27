import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Box } from '@mui/material';
import PageHeader from '../components/Common/PageHeader';
import ScheduleForm from '../features/schedules/ScheduleForm/ScheduleForm';
import LoadingSpinner from '../components/Common/LoadingSpinner';
import ErrorMessage from '../components/Common/ErrorMessage';
import useApiClient from '../hooks/useApiClient';

const EditSchedulePage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { api, loading, error, execute } = useApiClient();
  const [schedule, setSchedule] = useState(null);
  const [scrapers, setScrapers] = useState([]);
  const [isDeleting, setIsDeleting] = useState(false);
  
  // Fetch schedule data and scrapers on component mount
  useEffect(() => {
    const fetchData = async () => {
      try {
        // Fetch the schedule data
        const scheduleData = await execute(() => api.scheduling.getAll());
        const foundSchedule = scheduleData.find(s => s.id === id);
        if (foundSchedule) {
          setSchedule(foundSchedule);
        }
        
        // Fetch all scrapers for reference
        const scrapersData = await execute(() => api.scrapers.getAll());
        setScrapers(scrapersData || []);
      } catch (error) {
        console.error('Error fetching data:', error);
      }
    };
    
    fetchData();
  }, [id, execute, api]);
  
  const handleSubmit = async (formData) => {
    try {
      await execute(() => api.scheduling.update(id, formData));
      // Redirect to the schedules list after successful update
      navigate('/schedules');
    } catch (error) {
      console.error('Error updating schedule:', error);
    }
  };

  const handleCancel = () => {
    navigate('/schedules');
  };

  const handleDelete = async () => {
    if (window.confirm(`Are you sure you want to delete this schedule?`)) {
      setIsDeleting(true);
      try {
        await execute(() => api.scheduling.delete(id));
        navigate('/schedules');
      } catch (error) {
        console.error('Error deleting schedule:', error);
        setIsDeleting(false);
      }
    }
  };
  
  // Format breadcrumbs for navigation
  const breadcrumbs = [
    { label: 'Schedules', path: '/schedules' },
    { label: schedule?.name || 'Edit Schedule' }
  ];

  if ((loading && !schedule) || isDeleting) {
    return <LoadingSpinner message={isDeleting ? "Deleting schedule..." : "Loading schedule data..."} />;
  }

  if (error && !schedule) {
    return (
      <ErrorMessage 
        title="Failed to load schedule" 
        message={error}
        onRetry={() => window.location.reload()}
      />
    );
  }

  if (!schedule) {
    return (
      <ErrorMessage 
        title="Schedule not found" 
        message="The schedule you're looking for does not exist or has been deleted."
      />
    );
  }

  return (
    <Box>
      <PageHeader 
        title={`Edit ${schedule.name}`} 
        subtitle={`Schedule for ${schedule.scraperName}`}
        breadcrumbs={breadcrumbs}
      />
      
      <ScheduleForm
        initialData={schedule}
        scraperOptions={scrapers}
        onSubmit={handleSubmit}
        onCancel={handleCancel}
        onDelete={handleDelete}
        loading={loading}
        error={error}
      />
    </Box>
  );
};

export default EditSchedulePage;