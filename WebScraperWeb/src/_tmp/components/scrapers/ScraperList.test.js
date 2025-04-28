import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import ScraperList from './ScraperList';
import { useScrapers } from '../../contexts/ScraperContext';

// Mock the useScrapers hook
jest.mock('../../contexts/ScraperContext');

// Mock the useNavigate hook
jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useNavigate: () => jest.fn(),
}));

describe('ScraperList Component', () => {
  const mockScrapers = [
    {
      id: 'scraper1',
      name: 'Test Scraper 1',
      baseUrl: 'https://example1.com',
      lastRun: '2023-01-01T12:00:00Z',
      urlsProcessed: 100
    },
    {
      id: 'scraper2',
      name: 'Test Scraper 2',
      baseUrl: 'https://example2.com',
      lastRun: null,
      urlsProcessed: 0
    }
  ];
  
  const mockScraperStatus = {
    scraper1: { isRunning: false, hasErrors: false, urlsProcessed: 100 },
    scraper2: { isRunning: true, hasErrors: false, urlsProcessed: 50 }
  };
  
  const mockFetchScrapers = jest.fn();
  const mockStartScraper = jest.fn();
  const mockStopScraper = jest.fn();
  const mockDeleteScraper = jest.fn();
  const mockRefreshAll = jest.fn();
  
  beforeEach(() => {
    // Reset all mocks
    jest.clearAllMocks();
    
    // Setup mock implementation for useScrapers
    useScrapers.mockReturnValue({
      scrapers: mockScrapers,
      loading: false,
      error: null,
      scraperStatus: mockScraperStatus,
      fetchScrapers: mockFetchScrapers,
      start: mockStartScraper,
      stop: mockStopScraper,
      removeScraper: mockDeleteScraper,
      refreshAll: mockRefreshAll
    });
  });
  
  test('renders the scraper list with scrapers', () => {
    render(
      <MemoryRouter>
        <ScraperList />
      </MemoryRouter>
    );
    
    // Check if the component renders correctly
    expect(screen.getByText('Scrapers')).toBeInTheDocument();
    expect(screen.getByText('New Scraper')).toBeInTheDocument();
    
    // Check if scrapers are rendered
    expect(screen.getByText('Test Scraper 1')).toBeInTheDocument();
    expect(screen.getByText('Test Scraper 2')).toBeInTheDocument();
    expect(screen.getByText('https://example1.com')).toBeInTheDocument();
    expect(screen.getByText('https://example2.com')).toBeInTheDocument();
    
    // Check status chips
    expect(screen.getByText('Idle')).toBeInTheDocument();
    expect(screen.getByText('Running')).toBeInTheDocument();
    
    // Check if action buttons are rendered
    expect(screen.getAllByTitle('Edit Scraper').length).toBe(2);
    expect(screen.getAllByTitle('Delete Scraper').length).toBe(2);
    expect(screen.getByTitle('Start Scraper')).toBeInTheDocument();
    expect(screen.getByTitle('Stop Scraper')).toBeInTheDocument();
  });
  
  test('renders loading state correctly', () => {
    // Mock loading state
    useScrapers.mockReturnValue({
      scrapers: [],
      loading: true,
      error: null,
      scraperStatus: {},
      fetchScrapers: mockFetchScrapers,
      start: mockStartScraper,
      stop: mockStopScraper,
      removeScraper: mockDeleteScraper,
      refreshAll: mockRefreshAll
    });
    
    render(
      <MemoryRouter>
        <ScraperList />
      </MemoryRouter>
    );
    
    // Check if loading indicator is shown
    expect(screen.getByRole('progressbar')).toBeInTheDocument();
    
    // Check that the table is not rendered
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });
  
  test('renders empty state correctly', () => {
    // Mock empty scrapers list
    useScrapers.mockReturnValue({
      scrapers: [],
      loading: false,
      error: null,
      scraperStatus: {},
      fetchScrapers: mockFetchScrapers,
      start: mockStartScraper,
      stop: mockStopScraper,
      removeScraper: mockDeleteScraper,
      refreshAll: mockRefreshAll
    });
    
    render(
      <MemoryRouter>
        <ScraperList />
      </MemoryRouter>
    );
    
    // Check if empty state message is shown
    expect(screen.getByText('No scrapers found. Create your first scraper to get started.')).toBeInTheDocument();
    
    // Check if the "New Scraper" button is shown in the empty state
    const newScraperButtons = screen.getAllByText('New Scraper');
    expect(newScraperButtons.length).toBe(2); // One in header, one in empty state
  });
  
  test('filters scrapers by search term', () => {
    render(
      <MemoryRouter>
        <ScraperList />
      </MemoryRouter>
    );
    
    // Enter search term
    const searchInput = screen.getByPlaceholderText('Search scrapers...');
    fireEvent.change(searchInput, { target: { value: 'Scraper 1' } });
    
    // Check if only the matching scraper is shown
    expect(screen.getByText('Test Scraper 1')).toBeInTheDocument();
    expect(screen.queryByText('Test Scraper 2')).not.toBeInTheDocument();
    
    // Clear search term
    fireEvent.change(searchInput, { target: { value: '' } });
    
    // Check if both scrapers are shown again
    expect(screen.getByText('Test Scraper 1')).toBeInTheDocument();
    expect(screen.getByText('Test Scraper 2')).toBeInTheDocument();
  });
  
  test('filters scrapers by status', () => {
    render(
      <MemoryRouter>
        <ScraperList />
      </MemoryRouter>
    );
    
    // Select "Running" status filter
    const statusSelect = screen.getByLabelText('Status');
    fireEvent.mouseDown(statusSelect);
    fireEvent.click(screen.getByText('Running'));
    
    // Check if only the running scraper is shown
    expect(screen.queryByText('Test Scraper 1')).not.toBeInTheDocument();
    expect(screen.getByText('Test Scraper 2')).toBeInTheDocument();
    
    // Select "All Status" filter
    fireEvent.mouseDown(statusSelect);
    fireEvent.click(screen.getByText('All Status'));
    
    // Check if both scrapers are shown again
    expect(screen.getByText('Test Scraper 1')).toBeInTheDocument();
    expect(screen.getByText('Test Scraper 2')).toBeInTheDocument();
  });
  
  test('calls start scraper function when start button is clicked', async () => {
    render(
      <MemoryRouter>
        <ScraperList />
      </MemoryRouter>
    );
    
    // Click the start button for the first scraper
    const startButton = screen.getByTitle('Start Scraper');
    fireEvent.click(startButton);
    
    // Check if the start function was called with the correct ID
    expect(mockStartScraper).toHaveBeenCalledWith('scraper1');
  });
  
  test('calls stop scraper function when stop button is clicked', async () => {
    render(
      <MemoryRouter>
        <ScraperList />
      </MemoryRouter>
    );
    
    // Click the stop button for the second scraper
    const stopButton = screen.getByTitle('Stop Scraper');
    fireEvent.click(stopButton);
    
    // Check if the stop function was called with the correct ID
    expect(mockStopScraper).toHaveBeenCalledWith('scraper2');
  });
  
  test('shows delete confirmation dialog and deletes scraper when confirmed', async () => {
    render(
      <MemoryRouter>
        <ScraperList />
      </MemoryRouter>
    );
    
    // Click the delete button for the first scraper
    const deleteButtons = screen.getAllByTitle('Delete Scraper');
    fireEvent.click(deleteButtons[0]);
    
    // Check if the confirmation dialog is shown
    expect(screen.getByText('Delete Scraper')).toBeInTheDocument();
    expect(screen.getByText(/Are you sure you want to delete the scraper/)).toBeInTheDocument();
    
    // Click the delete button in the dialog
    const confirmDeleteButton = screen.getByRole('button', { name: 'Delete' });
    fireEvent.click(confirmDeleteButton);
    
    // Check if the delete function was called with the correct ID
    expect(mockDeleteScraper).toHaveBeenCalledWith('scraper1');
  });
  
  test('calls refresh function when refresh button is clicked', () => {
    render(
      <MemoryRouter>
        <ScraperList />
      </MemoryRouter>
    );
    
    // Click the refresh button
    const refreshButton = screen.getByRole('button', { name: 'Refresh' });
    fireEvent.click(refreshButton);
    
    // Check if the refresh function was called
    expect(mockRefreshAll).toHaveBeenCalled();
  });
  
  test('shows error notification when there is an error', () => {
    // Mock error state
    useScrapers.mockReturnValue({
      scrapers: mockScrapers,
      loading: false,
      error: 'Failed to fetch scrapers',
      scraperStatus: mockScraperStatus,
      fetchScrapers: mockFetchScrapers,
      start: mockStartScraper,
      stop: mockStopScraper,
      removeScraper: mockDeleteScraper,
      refreshAll: mockRefreshAll
    });
    
    render(
      <MemoryRouter>
        <ScraperList />
      </MemoryRouter>
    );
    
    // Check if the error notification is shown
    expect(screen.getByText('Failed to fetch scrapers')).toBeInTheDocument();
  });
});
