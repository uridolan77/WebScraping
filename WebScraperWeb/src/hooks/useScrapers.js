// src/hooks/useScrapers.js
import { useState, useEffect, useCallback } from 'react';
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

  // Fetch all scrapers
  const fetchScrapers = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getAllScrapers();
      setScrapers(data);
      return data;
    } catch (err) {
      setError('Failed to fetch scrapers');
      console.error(err);
      return [];
    } finally {
      setLoading(false);
    }
  }, []);

  // Fetch a single scraper by ID
  const fetchScraper = useCallback(async (id) => {
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
  }, []);

  // Create a new scraper
  const addScraper = useCallback(async (scraperData) => {
    try {
      setLoading(true);
      setError(null);
      const data = await createScraper(scraperData);
      setScrapers(prev => [...prev, data]);
      return data;
    } catch (err) {
      setError('Failed to create scraper');
      console.error(err);
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  // Update an existing scraper
  const editScraper = useCallback(async (id, scraperData) => {
    try {
      setLoading(true);
      setError(null);
      const data = await updateScraper(id, scraperData);
      setScrapers(prev => 
        prev.map(scraper => scraper.id === id ? data : scraper)
      );
      if (selectedScraper && selectedScraper.id === id) {
        setSelectedScraper(data);
      }
      return data;
    } catch (err) {
      setError(`Failed to update scraper with ID ${id}`);
      console.error(err);
      return null;
    } finally {
      setLoading(false);
    }
  }, [selectedScraper]);

  // Delete a scraper
  const removeScraper = useCallback(async (id) => {
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
      return false;
    } finally {
      setLoading(false);
    }
  }, [selectedScraper]);

  // Start a scraper
  const start = useCallback(async (id) => {
    try {
      setLoading(true);
      setError(null);
      const result = await startScraper(id);
      // Update status after starting
      await fetchScraperStatus(id);
      return result;
    } catch (err) {
      setError(`Failed to start scraper with ID ${id}`);
      console.error(err);
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  // Stop a scraper
  const stop = useCallback(async (id) => {
    try {
      setLoading(true);
      setError(null);
      const result = await stopScraper(id);
      // Update status after stopping
      await fetchScraperStatus(id);
      return result;
    } catch (err) {
      setError(`Failed to stop scraper with ID ${id}`);
      console.error(err);
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  // Fetch scraper status
  const fetchScraperStatus = useCallback(async (id) => {
    try {
      const data = await getScraperStatus(id);
      setStatus(data);
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
      setLogs(data);
      return data;
    } catch (err) {
      console.error(`Failed to fetch logs for scraper with ID ${id}`, err);
      return [];
    }
  }, []);

  return {
    scrapers,
    selectedScraper,
    loading,
    error,
    logs,
    status,
    fetchScrapers,
    fetchScraper,
    addScraper,
    editScraper,
    removeScraper,
    start,
    stop,
    fetchScraperStatus,
    fetchScraperLogs
  };
};

export default useScrapers;
