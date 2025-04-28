import React, { useState } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
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
// Note: These will be implemented later, for now we'll use placeholders
const Dashboard = () => <div>Dashboard Page</div>;
const ScraperList = () => <div>Scraper List Page</div>;
const ScraperDetail = () => <div>Scraper Detail Page</div>;
const ScraperCreate = () => <div>Create Scraper Page</div>;
const Analytics = () => <div>Analytics Page</div>;
const Monitoring = () => <div>Monitoring Page</div>;
const Scheduling = () => <div>Scheduling Page</div>;
const Notifications = () => <div>Notifications Page</div>;
const Settings = () => <div>Settings Page</div>;

function App() {
  const [menuOpen, setMenuOpen] = useState(true);
  const [pageTitle, setPageTitle] = useState('Dashboard');

  const handleMenuToggle = () => {
    setMenuOpen(!menuOpen);
  };

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <AuthProvider>
        <ScraperProvider>
          <Router>
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
                    <Route path="/scrapers/:id" element={<ScraperDetail />} />
                    <Route path="/scrapers/create" element={<ScraperCreate />} />
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
