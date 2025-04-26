import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { createTheme, ThemeProvider, CssBaseline, Box, Container } from '@mui/material';
import { blue, purple } from '@mui/material/colors';

// Components
import MainLayout from './layouts/MainLayout';

// Pages
import ScraperList from './pages/ScraperList';
import Dashboard from './pages/Dashboard';
import Configuration from './pages/Configuration';
import Results from './pages/Results';
import ResultDetail from './pages/ResultDetail';
import NotFound from './pages/NotFound';

// Create a theme
const theme = createTheme({
  palette: {
    mode: 'light',
    primary: blue,
    secondary: purple
  }
});

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Router>
        <Box>
          <Routes>
            <Route path="/" element={<MainLayout />}>
              {/* Redirect root to scrapers list */}
              <Route index element={<Navigate to="/scrapers" replace />} />
              
              {/* Scrapers list (main page) */}
              <Route path="scrapers" element={<ScraperList />} />
              
              {/* Dashboard for a specific scraper */}
              <Route path="dashboard/:id" element={<Dashboard />} />
              
              {/* Configure new scraper */}
              <Route path="configure" element={<Configuration />} />
              
              {/* Edit existing scraper */}
              <Route path="configure/:id" element={<Configuration />} />
              
              {/* Results list */}
              <Route path="results" element={<Results />} />
              
              {/* Result detail */}
              <Route path="results/:url" element={<ResultDetail />} />
              
              {/* 404 page */}
              <Route path="*" element={<NotFound />} />
            </Route>
          </Routes>
        </Box>
      </Router>
    </ThemeProvider>
  );
}

export default App;