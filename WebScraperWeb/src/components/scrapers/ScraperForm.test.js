import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import ScraperForm from './ScraperForm';

// Mock the validators
jest.mock('../../utils/validators', () => ({
  isValidUrl: jest.fn(url => url.startsWith('http')),
}));

describe('ScraperForm Component', () => {
  const mockSubmit = jest.fn();
  const defaultProps = {
    initialValues: {},
    onSubmit: mockSubmit,
    isSubmitting: false,
    isEditMode: false,
  };

  beforeEach(() => {
    mockSubmit.mockClear();
    window.history.back = jest.fn();
  });

  test('renders the form with default values', () => {
    render(<ScraperForm {...defaultProps} />);
    
    // Check if basic form elements are rendered
    expect(screen.getByLabelText(/Scraper Name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Scraper ID/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Notification Email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Start URL/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Base URL/i)).toBeInTheDocument();
    
    // Check if buttons are rendered
    expect(screen.getByRole('button', { name: /Cancel/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Create Scraper/i })).toBeInTheDocument();
  });

  test('renders the form with provided initial values', () => {
    const initialValues = {
      name: 'Test Scraper',
      id: 'test-scraper',
      email: 'test@example.com',
      startUrl: 'https://example.com',
      baseUrl: 'https://example.com',
      outputDirectory: 'TestOutput',
      maxDepth: 10,
      maxConcurrentRequests: 8,
      delayBetweenRequests: 2000,
    };
    
    render(<ScraperForm {...defaultProps} initialValues={initialValues} />);
    
    // Check if form fields have the initial values
    expect(screen.getByLabelText(/Scraper Name/i)).toHaveValue('Test Scraper');
    expect(screen.getByLabelText(/Scraper ID/i)).toHaveValue('test-scraper');
    expect(screen.getByLabelText(/Notification Email/i)).toHaveValue('test@example.com');
    expect(screen.getByLabelText(/Start URL/i)).toHaveValue('https://example.com');
    expect(screen.getByLabelText(/Base URL/i)).toHaveValue('https://example.com');
    expect(screen.getByLabelText(/Output Directory/i)).toHaveValue('TestOutput');
  });

  test('shows edit mode button text when isEditMode is true', () => {
    render(<ScraperForm {...defaultProps} isEditMode={true} />);
    
    expect(screen.getByRole('button', { name: /Update Scraper/i })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /Create Scraper/i })).not.toBeInTheDocument();
  });

  test('disables form submission when required fields are empty', async () => {
    render(<ScraperForm {...defaultProps} />);
    
    // Try to submit the form without filling required fields
    const submitButton = screen.getByRole('button', { name: /Create Scraper/i });
    
    // The button should be disabled
    expect(submitButton).toBeDisabled();
  });

  test('validates form fields and shows error messages', async () => {
    render(<ScraperForm {...defaultProps} />);
    
    // Fill in invalid values
    const nameInput = screen.getByLabelText(/Scraper Name/i);
    const emailInput = screen.getByLabelText(/Notification Email/i);
    const startUrlInput = screen.getByLabelText(/Start URL/i);
    
    await userEvent.type(nameInput, 'Test');
    await userEvent.type(emailInput, 'invalid-email');
    await userEvent.type(startUrlInput, 'invalid-url');
    
    // Trigger validation by blurring the fields
    fireEvent.blur(nameInput);
    fireEvent.blur(emailInput);
    fireEvent.blur(startUrlInput);
    
    // Check for error messages
    await waitFor(() => {
      expect(screen.getByText(/Enter a valid email/i)).toBeInTheDocument();
      expect(screen.getByText(/Enter a valid URL/i)).toBeInTheDocument();
    });
  });

  test('auto-generates ID from name', async () => {
    render(<ScraperForm {...defaultProps} />);
    
    // Type a name with spaces and special characters
    const nameInput = screen.getByLabelText(/Scraper Name/i);
    await userEvent.type(nameInput, 'Test Scraper 123!@#');
    
    // Check if ID is auto-generated correctly
    await waitFor(() => {
      const idInput = screen.getByLabelText(/Scraper ID/i);
      expect(idInput).toHaveValue('test-scraper-123');
    });
  });

  test('auto-generates baseUrl from startUrl', async () => {
    render(<ScraperForm {...defaultProps} />);
    
    // Type a start URL
    const startUrlInput = screen.getByLabelText(/Start URL/i);
    await userEvent.type(startUrlInput, 'https://example.com/page');
    
    // Check if base URL is auto-generated correctly
    await waitFor(() => {
      const baseUrlInput = screen.getByLabelText(/Base URL/i);
      expect(baseUrlInput).toHaveValue('https://example.com');
    });
  });

  test('submits the form with valid data', async () => {
    render(<ScraperForm {...defaultProps} />);
    
    // Fill in valid values
    await userEvent.type(screen.getByLabelText(/Scraper Name/i), 'Test Scraper');
    await userEvent.type(screen.getByLabelText(/Scraper ID/i), 'test-scraper');
    await userEvent.type(screen.getByLabelText(/Notification Email/i), 'test@example.com');
    await userEvent.type(screen.getByLabelText(/Start URL/i), 'https://example.com');
    await userEvent.type(screen.getByLabelText(/Base URL/i), 'https://example.com');
    
    // Submit the form
    const submitButton = screen.getByRole('button', { name: /Create Scraper/i });
    expect(submitButton).not.toBeDisabled();
    await userEvent.click(submitButton);
    
    // Check if onSubmit was called with the correct data
    await waitFor(() => {
      expect(mockSubmit).toHaveBeenCalledTimes(1);
      expect(mockSubmit).toHaveBeenCalledWith(expect.objectContaining({
        name: 'Test Scraper',
        id: 'test-scraper',
        email: 'test@example.com',
        startUrl: 'https://example.com',
        baseUrl: 'https://example.com',
      }));
    });
  });

  test('disables form when isSubmitting is true', () => {
    render(<ScraperForm {...defaultProps} isSubmitting={true} />);
    
    // Check if form fields are disabled
    expect(screen.getByLabelText(/Scraper Name/i)).toBeDisabled();
    expect(screen.getByLabelText(/Scraper ID/i)).toBeDisabled();
    expect(screen.getByLabelText(/Notification Email/i)).toBeDisabled();
    
    // Check if buttons are disabled
    expect(screen.getByRole('button', { name: /Cancel/i })).toBeDisabled();
    expect(screen.getByRole('button', { name: /Saving/i })).toBeInTheDocument();
  });

  test('shows advanced options when button is clicked', async () => {
    render(<ScraperForm {...defaultProps} />);
    
    // Advanced options should be hidden initially
    expect(screen.queryByText(/Notification Webhook URL/i)).not.toBeInTheDocument();
    
    // Click the button to show advanced options
    await userEvent.click(screen.getByRole('button', { name: /Show Advanced Options/i }));
    
    // Advanced options should be visible now
    expect(screen.getByText(/Notification Webhook URL/i)).toBeInTheDocument();
    
    // Click again to hide
    await userEvent.click(screen.getByRole('button', { name: /Hide Advanced Options/i }));
    
    // Advanced options should be hidden again
    expect(screen.queryByText(/Notification Webhook URL/i)).not.toBeInTheDocument();
  });

  test('calls window.history.back when cancel button is clicked', async () => {
    render(<ScraperForm {...defaultProps} />);
    
    await userEvent.click(screen.getByRole('button', { name: /Cancel/i }));
    
    expect(window.history.back).toHaveBeenCalledTimes(1);
  });
});
