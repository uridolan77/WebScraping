// src/hooks/useScrapers.js
import { useState, useEffect, useCallback, useRef } from 'react';
import {
  getAllScrapers,
  getScraper,
  createScraper,
  updateScraper,
  deleteScraper,
  startScraper,
  stopScraper,
  getScraperStatus,
  getScraperLogs
} from '../api/scrapers';

const useScrapers = () => {
  const [scrapers, setScrapers] = useState([]);
  const [selectedScraper, setSelectedScraper] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [logs, setLogs] = useState([]);
  const [status, setStatus] = useState(null);
  const [scraperStatus, setScraperStatus] = useState({});
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  // Use a ref to track if the component is mounted
  const isMounted = useRef(true);

  // Set isMounted to false when the component unmounts
  useEffect(() => {
    return () => {
      isMounted.current = false;
    };
  }, []);

  // Fetch all scrapers
  const fetchScrapers = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getAllScrapers();

      // Only update state if the component is still mounted
      if (isMounted.current) {
        setScrapers(data);

        // Fetch status for all scrapers
        const statusPromises = data.map(scraper =>
          getScraperStatus(scraper.id)
            .then(statusData => ({ id: scraper.id, status: statusData }))
            .catch(() => ({ id: scraper.id, status: { isRunning: false, hasErrors: false } }))
        );

        const statuses = await Promise.all(statusPromises);
        const statusMap = statuses.reduce((acc, { id, status }) => {
          acc[id] = status;
          return acc;
        }, {});

        setScraperStatus(statusMap);
      }

      return data;
    } catch (err) {
      if (isMounted.current) {
        setError(err.response?.data?.message || 'Failed to fetch scrapers');
        console.error('Error fetching scrapers:', err);
      }
      return [];
    } finally {
      if (isMounted.current) {
        setLoading(false);
      }
    }
  }, [refreshTrigger]);

  // Fetch a single scraper by ID
  const fetchScraper = useCallback(async (id) => {
    try {
      setLoading(true);
      setError(null);
      const data = await getScraper(id);

      if (isMounted.current) {
        setSelectedScraper(data);
      }

      return data;
    } catch (err) {
      if (isMounted.current) {
        setError(err.response?.data?.message || `Failed to fetch scraper with ID ${id}`);
        console.error('Error fetching scraper:', err);
      }
      return null;
    } finally {
      if (isMounted.current) {
        setLoading(false);
      }
    }
  }, []);

  // Create a new scraper
  const addScraper = useCallback(async (scraperData) => {
    try {
      setLoading(true);
      setError(null);
      const data = await createScraper(scraperData);

      if (isMounted.current) {
        setScrapers(prev => [...prev, data]);
        // Trigger a refresh to update the list
        setRefreshTrigger(prev => prev + 1);
      }

      return data;
    } catch (err) {
      if (isMounted.current) {
        setError(err.response?.data?.message || 'Failed to create scraper');
        console.error('Error creating scraper:', err);
      }
      return null;
    } finally {
      if (isMounted.current) {
        setLoading(false);
      }
    }
  }, []);

  // Update an existing scraper
  const editScraper = useCallback(async (id, scraperData) => {
    try {
      setLoading(true);
      setError(null);
      const data = await updateScraper(id, scraperData);

      if (isMounted.current) {
        setScrapers(prev =>
          prev.map(scraper => scraper.id === id ? data : scraper)
        );

        if (selectedScraper && selectedScraper.id === id) {
          setSelectedScraper(data);
        }

        // Trigger a refresh to update the list
        setRefreshTrigger(prev => prev + 1);
      }

      return data;
    } catch (err) {
      if (isMounted.current) {
        setError(err.response?.data?.message || `Failed to update scraper with ID ${id}`);
        console.error('Error updating scraper:', err);
      }
      return null;
    } finally {
      if (isMounted.current) {
        setLoading(false);
      }
    }
  }, [selectedScraper]);

  // Delete a scraper
  const removeScraper = useCallback(async (id) => {
    try {
      setLoading(true);
      setError(null);
      await deleteScraper(id);

      if (isMounted.current) {
        setScrapers(prev => prev.filter(scraper => scraper.id !== id));

        if (selectedScraper && selectedScraper.id === id) {
          setSelectedScraper(null);
        }

        // Remove from status map
        setScraperStatus(prev => {
          const newStatus = { ...prev };
          delete newStatus[id];
          return newStatus;
        });
      }

      return true;
    } catch (err) {
      if (isMounted.current) {
        setError(err.response?.data?.message || `Failed to delete scraper with ID ${id}`);
        console.error('Error deleting scraper:', err);
      }
      return false;
    } finally {
      if (isMounted.current) {
        setLoading(false);
      }
    }
  }, [selectedScraper]);

  // Start a scraper
  const start = useCallback(async (id) => {
    try {
      setLoading(true);
      setError(null);
      const result = await startScraper(id);

      if (isMounted.current) {
        // Update status after starting
        const statusData = await getScraperStatus(id);

        // Update the status in the status map
        setScraperStatus(prev => ({
          ...prev,
          [id]: statusData
        }));

        // If this is the selected scraper, update its status
        if (id === selectedScraper?.id) {
          setStatus(statusData);
        }
      }

      return result;
    } catch (err) {
      if (isMounted.current) {
        setError(err.response?.data?.message || `Failed to start scraper with ID ${id}`);
        console.error('Error starting scraper:', err);
      }
      return null;
    } finally {
      if (isMounted.current) {
        setLoading(false);
      }
    }
  }, [selectedScraper]);

  // Stop a scraper
  const stop = useCallback(async (id) => {
    try {
      setLoading(true);
      setError(null);
      const result = await stopScraper(id);

      if (isMounted.current) {
        // Update status after stopping
        const statusData = await getScraperStatus(id);

        // Update the status in the status map
        setScraperStatus(prev => ({
          ...prev,
          [id]: statusData
        }));

        // If this is the selected scraper, update its status
        if (id === selectedScraper?.id) {
          setStatus(statusData);
        }
      }

      return result;
    } catch (err) {
      if (isMounted.current) {
        setError(err.response?.data?.message || `Failed to stop scraper with ID ${id}`);
        console.error('Error stopping scraper:', err);
      }
      return null;
    } finally {
      if (isMounted.current) {
        setLoading(false);
      }
    }
  }, [selectedScraper]);

  // Fetch scraper status
  const fetchScraperStatus = useCallback(async (id) => {
    try {
      const data = await getScraperStatus(id);

      if (isMounted.current) {
        setStatus(data);

        // Also update the status in the status map
        setScraperStatus(prev => ({
          ...prev,
          [id]: data
        }));
      }

      return data;
    } catch (err) {
      console.error(`Failed to fetch status for scraper with ID ${id}`, err);
      return null;
    }
  }, []);

  // Fetch scraper logs
  const fetchScraperLogs = useCallback(async (id, limit = 100) => {
    try {
      const data = await getScraperLogs(id, limit);

      if (isMounted.current) {
        setLogs(data);
      }

      return data;
    } catch (err) {
      console.error(`Failed to fetch logs for scraper with ID ${id}`, err);
      return [];
    }
  }, []);

  // Set up polling for status updates
  useEffect(() => {
    if (scrapers.length === 0) return;

    const pollStatuses = async () => {
      try {
        const statusPromises = scrapers.map(scraper =>
          getScraperStatus(scraper.id)
            .then(statusData => ({ id: scraper.id, status: statusData }))
            .catch(() => ({ id: scraper.id, status: { isRunning: false, hasErrors: false } }))
        );

        const statuses = await Promise.all(statusPromises);
        const statusMap = statuses.reduce((acc, { id, status }) => {
          acc[id] = status;
          return acc;
        }, {});

        if (isMounted.current) {
          setScraperStatus(statusMap);
        }
      } catch (error) {
        console.error('Error polling scraper statuses:', error);
      }
    };

    // Poll every 10 seconds
    const intervalId = setInterval(pollStatuses, 10000);

    // Clean up on unmount
    return () => clearInterval(intervalId);
  }, [scrapers]);

  // Function to refresh all data
  const refreshAll = useCallback(() => {
    setRefreshTrigger(prev => prev + 1);
  }, []);

  return {
    scrapers,
    selectedScraper,
    loading,
    error,
    logs,
    status,
    scraperStatus,
    fetchScrapers,
    fetchScraper,
    addScraper,
    editScraper,
    removeScraper,
    start,
    stop,
    fetchScraperStatus,
    fetchScraperLogs,
    refreshAll
  };
};

export default useScrapers;
