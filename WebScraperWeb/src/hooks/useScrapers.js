import { useContext } from 'react';
import ScraperContext from '../contexts/ScraperContext';

// Custom hook to use the scraper context
export default function useScrapers() {
  return useContext(ScraperContext);
}
