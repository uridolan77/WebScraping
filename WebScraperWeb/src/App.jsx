import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, useLocation } from 'react-router-dom';
import { ThemeProvider, CssBaseline, Box, Toolbar } from '@mui/material';
import theme from './theme';

// Contexts
import { AuthProvider } from './contexts/AuthContext';
import { ScraperProvider } from './contexts/ScraperContext';

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

function App() {
  const [menuOpen, setMenuOpen] = useState(true);
  const [pageTitle, setPageTitle] = useState('Dashboard');

  // Title updater component
  const TitleUpdater = () => {
    const location = useLocation();

    useEffect(() => {
      const path = location.pathname;
      if (path === '/') {
        setPageTitle('Dashboard');
      } else if (path.startsWith('/scrapers/create')) {
        setPageTitle('Create Scraper');
      } else if (path.startsWith('/scrapers') && path.endsWith('/edit')) {
        setPageTitle('Edit Scraper');
      } else if (path.startsWith('/scrapers') && path.includes('/')) {
        setPageTitle('Scraper Details');
      } else if (path.startsWith('/scrapers')) {
        setPageTitle('Scrapers');
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
    }, [location]);

    return null;
  };

  const handleMenuToggle = () => {
    setMenuOpen(!menuOpen);
  };

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <AuthProvider>
        <ScraperProvider>
          <Router>
            <TitleUpdater />
            <Box sx={{ display: 'flex', height: '100vh' }}>
              <Header title={pageTitle} onMenuToggle={handleMenuToggle} />
              <Sidebar open={menuOpen} />
              <Box
                component="main"
                sx={{
                  flexGrow: 1,
                  p: 3,
                  width: { sm: `calc(100% - ${menuOpen ? 240 : 0}px)` },
                  ml: { sm: menuOpen ? '240px' : 0 },
                  transition: (theme) => theme.transitions.create(['margin', 'width'], {
                    easing: theme.transitions.easing.sharp,
                    duration: theme.transitions.duration.leavingScreen,
                  }),
                }}
              >
                <Toolbar /> {/* This creates space below the app bar */}
                <ErrorBoundary>
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
        </ScraperProvider>
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;
