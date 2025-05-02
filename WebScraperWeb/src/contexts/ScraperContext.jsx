import React, { createContext, useState, useEffect, useContext } from 'react';
import {
  getAllScrapers,
  getScraper,
  getScraperStatus,
  createScraper,
  updateScraper,
  deleteScraper,
  startScraper,
  stopScraper
} from '../api/scrapers';

// Create the context
const ScraperContext = createContext();

// Custom hook to use the scraper context
export const useScrapers = () => {
  return useContext(ScraperContext);
};

// Provider component
export const ScraperProvider = ({ children }) => {
  const [scrapers, setScrapers] = useState([]);
  const [selectedScraper, setSelectedScraper] = useState(null);
  const [scraperStatus, setScraperStatus] = useState({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  // Fetch all scrapers
  useEffect(() => {
    const fetchScrapers = async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await getAllScrapers();
        setScrapers(data);
      } catch (err) {
        setError('Failed to fetch scrapers');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchScrapers();
  }, [refreshTrigger]);

  // Fetch status for all scrapers
  useEffect(() => {
    if (scrapers.length === 0) return;

    const fetchScraperStatuses = async () => {
      try {
        const statusPromises = scrapers.map(scraper =>
          getScraperStatus(scraper.id)
            .then(status => ({ id: scraper.id, status }))
            .catch(err => ({ id: scraper.id, status: { isRunning: false, error: true } }))
        );

        const statuses = await Promise.all(statusPromises);

        const statusMap = statuses.reduce((acc, { id, status }) => {
          acc[id] = status;
          return acc;
        }, {});

        setScraperStatus(statusMap);
      } catch (err) {
        console.error('Error fetching scraper statuses:', err);
      }
    };

    fetchScraperStatuses();

    // Set up polling for status updates
    const intervalId = setInterval(fetchScraperStatuses, 30000); // Poll every 30 seconds

    return () => clearInterval(intervalId);
  }, [scrapers]);

  // Function to fetch a single scraper by ID
  const fetchScraper = async (id) => {
    try {
      setLoading(true);
      setError(null);
      const data = await getScraper(id);
      setSelectedScraper(data);
      return data;
    } catch (err) {
      setError(`Failed to fetch scraper with ID ${id}`);
      console.error(err);
      return null;
    } finally {
      setLoading(false);
    }
  };

  // Function to refresh the scrapers list
  const refreshScrapers = () => {
    setRefreshTrigger(prev => prev + 1);
  };

  // Function to add a new scraper
  const addScraper = async (scraperData) => {
    try {
      setLoading(true);
      setError(null);
      const data = await createScraper(scraperData);
      setScrapers(prev => [...prev, data]);
      return data;
    } catch (err) {
      setError('Failed to create scraper');
      console.error(err);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Function to edit a scraper
  const editScraper = async (id, scraperData) => {
    try {
      setLoading(true);
      setError(null);
      const data = await updateScraper(id, scraperData);
      setScrapers(prev => prev.map(scraper =>
        scraper.id === id ? data : scraper
      ));
      if (selectedScraper && selectedScraper.id === id) {
        setSelectedScraper(data);
      }
      return data;
    } catch (err) {
      setError(`Failed to update scraper with ID ${id}`);
      console.error(err);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Function to remove a scraper
  const removeScraper = async (id) => {
    try {
      setLoading(true);
      setError(null);
      await deleteScraper(id);
      setScrapers(prev => prev.filter(scraper => scraper.id !== id));
      if (selectedScraper && selectedScraper.id === id) {
        setSelectedScraper(null);
      }
      return true;
    } catch (err) {
      setError(`Failed to delete scraper with ID ${id}`);
      console.error(err);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Function to start a scraper
  const start = async (id) => {
    try {
      setError(null);
      const result = await startScraper(id);

      // Update the status in the local state
      setScraperStatus(prev => ({
        ...prev,
        [id]: { ...prev[id], isRunning: true }
      }));

      return result;
    } catch (err) {
      setError(`Failed to start scraper with ID ${id}`);
      console.error(err);
      throw err;
    }
  };

  // Function to stop a scraper
  const stop = async (id) => {
    try {
      setError(null);
      const result = await stopScraper(id);

      // Update the status in the local state
      setScraperStatus(prev => ({
        ...prev,
        [id]: { ...prev[id], isRunning: false }
      }));

      return result;
    } catch (err) {
      setError(`Failed to stop scraper with ID ${id}`);
      console.error(err);
      throw err;
    }
  };

  const value = {
    scrapers,
    selectedScraper,
    scraperStatus,
    loading,
    error,
    getScrapers: getAllScrapers,
    getScraper: fetchScraper,
    refreshScrapers,
    createScraper: addScraper,
    updateScraper: editScraper,
    deleteScraper: removeScraper,
    startScraper: start,
    stopScraper: stop,
    getScraperStatus
  };

  return (
    <ScraperContext.Provider value={value}>
      {children}
    </ScraperContext.Provider>
  );
};

export default ScraperContext;