import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, useLocation } from 'react-router-dom';
import { CssBaseline, Box, Toolbar } from '@mui/material';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import queryClient from './lib/queryClient';

// Theme provider
import { ThemeProvider } from './contexts/ThemeContext';

// Contexts
import { AuthProvider } from './contexts/AuthContext';
import { ScraperProvider } from './contexts/ScraperContext';
import { AppStateProvider } from './contexts/AppStateContext';

// Common components
import Header from './components/common/Header';
import Sidebar from './components/common/Sidebar';
import ErrorBoundary from './components/common/ErrorBoundary';

// Pages
import Dashboard from './pages/Dashboard';
import ScraperList from './pages/ScraperList';
import ScraperDetail from './pages/ScraperDetail';
import ScraperCreate from './pages/ScraperCreate';
import ScraperEdit from './pages/ScraperEdit';
import Analytics from './pages/Analytics';
import Monitoring from './pages/Monitoring';
import Scheduling from './pages/Scheduling';
import Notifications from './pages/Notifications';
import Settings from './pages/Settings';

// Title updater component
const TitleUpdater: React.FC = () => {
  const location = useLocation();
  const setPageTitle = React.useContext(PageTitleContext);

  useEffect(() => {
    const path = location.pathname;

    if (path === '/' || path === '/dashboard') {
      setPageTitle('Dashboard');
    } else if (path.startsWith('/scrapers')) {
      if (path === '/scrapers') {
        setPageTitle('Scrapers');
      } else if (path.includes('/create')) {
        setPageTitle('Create Scraper');
      } else if (path.includes('/edit')) {
        setPageTitle('Edit Scraper');
      } else {
        setPageTitle('Scraper Details');
      }
    } else if (path.startsWith('/analytics')) {
      setPageTitle('Analytics');
    } else if (path.startsWith('/monitoring')) {
      setPageTitle('Monitoring');
    } else if (path.startsWith('/scheduling')) {
      setPageTitle('Scheduling');
    } else if (path.startsWith('/notifications')) {
      setPageTitle('Notifications');
    } else if (path.startsWith('/settings')) {
      setPageTitle('Settings');
    }
  }, [location, setPageTitle]);

  return null;
};

// Create a context for the page title
const PageTitleContext = React.createContext<React.Dispatch<React.SetStateAction<string>>>(() => {});

function App() {
  const [pageTitle, setPageTitle] = useState('Dashboard');

  // We'll use the AppState context for sidebar management instead of local state
  const handleMenuToggle = () => {
    // This is just a placeholder - the actual implementation will use the AppState context
    // The Header component will use useAppState() to get the toggleSidebar function
  };

  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <CssBaseline />
        <AuthProvider>
          <AppStateProvider>
            <ScraperProvider>
              <PageTitleContext.Provider value={setPageTitle}>
                <Router>
                  <Box sx={{ display: 'flex', height: '100vh' }}>
                    <Header title={pageTitle} onMenuToggle={handleMenuToggle} />
                    <Sidebar />
                    <Box
                      component="main"
                      sx={{
                        flexGrow: 1,
                        p: 3,
                        width: '100%',
                        transition: (theme) => theme.transitions.create(['margin', 'width'], {
                          easing: theme.transitions.easing.sharp,
                          duration: theme.transitions.duration.leavingScreen,
                        }),
                      }}
                    >
                      <Toolbar /> {/* This creates space below the app bar */}
                      <ErrorBoundary>
                        <TitleUpdater />
                        <Routes>
                          <Route path="/" element={<Dashboard />} />
                          <Route path="/scrapers" element={<ScraperList />} />
                          <Route path="/scrapers/create" element={<ScraperCreate />} />
                          <Route path="/scrapers/:id" element={<ScraperDetail />} />
                          <Route path="/scrapers/:id/edit" element={<ScraperEdit />} />
                          <Route path="/analytics" element={<Analytics />} />
                          <Route path="/monitoring" element={<Monitoring />} />
                          <Route path="/scheduling" element={<Scheduling />} />
                          <Route path="/notifications" element={<Notifications />} />
                          <Route path="/settings" element={<Settings />} />
                        </Routes>
                      </ErrorBoundary>
                    </Box>
                  </Box>
                </Router>
              </PageTitleContext.Provider>
            </ScraperProvider>
          </AppStateProvider>
        </AuthProvider>
      </ThemeProvider>
      {process.env.NODE_ENV === 'development' && <ReactQueryDevtools initialIsOpen={false} />}
    </QueryClientProvider>
  );
}

export default App;
