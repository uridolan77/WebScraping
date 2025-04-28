import React, { createContext, useState, useEffect, useContext } from 'react';
import { getAllScrapers, getScraper, getScraperStatus } from '../api/scrapers';

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
    const fetchScraperStatuses = async () => {
      if (scrapers.length === 0) return;

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

  const value = {
    scrapers,
    selectedScraper,
    scraperStatus,
    loading,
    error,
    fetchScraper,
    refreshScrapers
  };

  return (
    <ScraperContext.Provider value={value}>
      {children}
    </ScraperContext.Provider>
  );
};

export default ScraperContext;
