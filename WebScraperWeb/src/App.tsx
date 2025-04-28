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

// Common components
import Header from './components/common/Header';
import Sidebar from './components/common/Sidebar';
import PrivateRoute from './components/common/PrivateRoute';

// Pages
import Dashboard from './pages/Dashboard';
import Login from './pages/Login';

// Title updater component
const TitleUpdater: React.FC = () => {
  const location = useLocation();
  const setPageTitle = React.useContext(PageTitleContext);

  useEffect(() => {
    const path = location.pathname;

    if (path === '/' || path === '/dashboard') {
      setPageTitle('Dashboard');
    } else if (path === '/login') {
      setPageTitle('Login');
    }
  }, [location, setPageTitle]);

  return null;
};

// Create a context for the page title
const PageTitleContext = React.createContext<React.Dispatch<React.SetStateAction<string>>>(() => {});

function App() {
  const [pageTitle, setPageTitle] = useState('Dashboard');
  const [sidebarOpen, setSidebarOpen] = useState(true);

  // Handle sidebar toggle
  const handleMenuToggle = () => {
    setSidebarOpen(!sidebarOpen);
  };

  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <CssBaseline />
        <AuthProvider>
          <PageTitleContext.Provider value={setPageTitle}>
            <Router>
              <Routes>
                <Route path="/login" element={<Login />} />
                <Route
                  path="/*"
                  element={
                    <PrivateRoute>
                      <Box sx={{ display: 'flex', height: '100vh' }}>
                        <Header title={pageTitle} onMenuToggle={handleMenuToggle} />
                        <Sidebar open={sidebarOpen} />
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
                          <TitleUpdater />
                          <Routes>
                            <Route path="/" element={<Dashboard />} />
                            <Route path="/dashboard" element={<Dashboard />} />
                            {/* Add more routes here as needed */}
                          </Routes>
                        </Box>
                      </Box>
                    </PrivateRoute>
                  }
                />
              </Routes>
            </Router>
          </PageTitleContext.Provider>
        </AuthProvider>
      </ThemeProvider>
      {process.env.NODE_ENV === 'development' && <ReactQueryDevtools initialIsOpen={false} />}
    </QueryClientProvider>
  );
}

export default App;
