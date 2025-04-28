import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import ErrorBoundary from './ErrorBoundary';

// Create a component that throws an error
const ErrorThrowingComponent = ({ shouldThrow }) => {
  if (shouldThrow) {
    throw new Error('Test error');
  }
  return <div>No error</div>;
};

// Mock console.error to avoid test output pollution
const originalConsoleError = console.error;
beforeAll(() => {
  console.error = jest.fn();
});

afterAll(() => {
  console.error = originalConsoleError;
});

describe('ErrorBoundary Component', () => {
  beforeEach(() => {
    console.error.mockClear();
  });

  test('renders children when there is no error', () => {
    render(
      <ErrorBoundary>
        <div data-testid="child">Child Component</div>
      </ErrorBoundary>
    );
    
    expect(screen.getByTestId('child')).toBeInTheDocument();
    expect(screen.getByText('Child Component')).toBeInTheDocument();
  });

  test('renders fallback UI when there is an error', () => {
    // We need to spy on console.error because React will log the error
    const spy = jest.spyOn(console, 'error');
    spy.mockImplementation(() => {});
    
    render(
      <ErrorBoundary>
        <ErrorThrowingComponent shouldThrow={true} />
      </ErrorBoundary>
    );
    
    // Check if the fallback UI is rendered
    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    expect(screen.getByText(/Test error/)).toBeInTheDocument();
    expect(screen.getByText('Try Again')).toBeInTheDocument();
    expect(screen.getByText('Go to Dashboard')).toBeInTheDocument();
    
    // Restore console.error
    spy.mockRestore();
  });

  test('renders custom fallback when provided', () => {
    // We need to spy on console.error because React will log the error
    const spy = jest.spyOn(console, 'error');
    spy.mockImplementation(() => {});
    
    const customFallback = (error, errorInfo, resetErrorBoundary) => (
      <div data-testid="custom-fallback">
        <p>Custom Error: {error.message}</p>
        <button onClick={resetErrorBoundary}>Reset</button>
      </div>
    );
    
    render(
      <ErrorBoundary fallback={customFallback}>
        <ErrorThrowingComponent shouldThrow={true} />
      </ErrorBoundary>
    );
    
    // Check if the custom fallback UI is rendered
    expect(screen.getByTestId('custom-fallback')).toBeInTheDocument();
    expect(screen.getByText('Custom Error: Test error')).toBeInTheDocument();
    expect(screen.getByText('Reset')).toBeInTheDocument();
    
    // Restore console.error
    spy.mockRestore();
  });

  test('resets error state when "Try Again" button is clicked', () => {
    // Mock window.location.href
    const originalLocation = window.location;
    delete window.location;
    window.location = { href: jest.fn() };
    
    // We need to spy on console.error because React will log the error
    const spy = jest.spyOn(console, 'error');
    spy.mockImplementation(() => {});
    
    const TestComponent = ({ shouldThrow, setShouldThrow }) => {
      return (
        <div>
          <button onClick={() => setShouldThrow(false)}>Fix Error</button>
          <ErrorThrowingComponent shouldThrow={shouldThrow} />
        </div>
      );
    };
    
    const TestContainer = () => {
      const [shouldThrow, setShouldThrow] = React.useState(true);
      
      return (
        <ErrorBoundary>
          <TestComponent shouldThrow={shouldThrow} setShouldThrow={setShouldThrow} />
        </ErrorBoundary>
      );
    };
    
    render(<TestContainer />);
    
    // Check if the fallback UI is rendered
    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    
    // Click the "Try Again" button
    fireEvent.click(screen.getByText('Try Again'));
    
    // The component should still show the error because we haven't fixed the root cause
    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    
    // Restore console.error and window.location
    spy.mockRestore();
    window.location = originalLocation;
  });

  test('navigates to home when "Go to Dashboard" button is clicked', () => {
    // Mock window.location.href
    const originalLocation = window.location;
    delete window.location;
    window.location = { href: jest.fn() };
    
    // We need to spy on console.error because React will log the error
    const spy = jest.spyOn(console, 'error');
    spy.mockImplementation(() => {});
    
    render(
      <ErrorBoundary>
        <ErrorThrowingComponent shouldThrow={true} />
      </ErrorBoundary>
    );
    
    // Check if the fallback UI is rendered
    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    
    // Click the "Go to Dashboard" button
    fireEvent.click(screen.getByText('Go to Dashboard'));
    
    // Check if window.location.href was set to '/'
    expect(window.location.href).toHaveBeenCalledWith('/');
    
    // Restore console.error and window.location
    spy.mockRestore();
    window.location = originalLocation;
  });

  test('calls onReset prop when provided', () => {
    // We need to spy on console.error because React will log the error
    const spy = jest.spyOn(console, 'error');
    spy.mockImplementation(() => {});
    
    const onReset = jest.fn();
    
    render(
      <ErrorBoundary onReset={onReset}>
        <ErrorThrowingComponent shouldThrow={true} />
      </ErrorBoundary>
    );
    
    // Check if the fallback UI is rendered
    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    
    // Click the "Try Again" button
    fireEvent.click(screen.getByText('Try Again'));
    
    // Check if onReset was called
    expect(onReset).toHaveBeenCalledTimes(1);
    
    // Restore console.error
    spy.mockRestore();
  });
});
