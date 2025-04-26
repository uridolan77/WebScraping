import React, { useState } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Container, Box } from '@mui/material';
import CssBaseline from '@mui/material/CssBaseline';
import { createTheme, ThemeProvider } from '@mui/material/styles';

import Navbar from './components/Navbar';
import Dashboard from './pages/Dashboard';
import Configuration from './pages/Configuration';
import Results from './pages/Results';
import ResultDetail from './pages/ResultDetail';
import ScraperList from './pages/ScraperList'; // Add this import

// Create a theme instance
const theme = createTheme({
    palette: {
        primary: {
            main: '#1976d2',
        },
        secondary: {
            main: '#dc004e',
        },
        background: {
            default: '#f5f5f5',
        },
    },
});

function App() {
    // State to track if scraper is running
    const [isRunning, setIsRunning] = useState(false);
    const [scrapingStats, setScrapingStats] = useState({
        urlsProcessed: 0,
        startTime: null,
        endTime: null,
        elapsedTime: null
    });

    return (
        <ThemeProvider theme={theme}>
            <CssBaseline />
            <Router>
                <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
                    <Navbar isRunning={isRunning} />

                    <Container component="main" sx={{ flexGrow: 1, py: 3 }}>
                        <Routes>
                            <Route path="/" element={<Dashboard
                                isRunning={isRunning}
                                setIsRunning={setIsRunning}
                                scrapingStats={scrapingStats}
                                setScrapingStats={setScrapingStats}
                            />} />

                            <Route path="/configure" element={<Configuration
                                isRunning={isRunning}
                                setIsRunning={setIsRunning}
                                setScrapingStats={setScrapingStats}
                            />} />

                            <Route path="/scrapers" element={<ScraperList />} /> {/* Add this route */}

                            <Route path="/results" element={<Results />} />

                            <Route path="/results/:url" element={<ResultDetail />} />
                        </Routes>
                    </Container>

                    <Box component="footer" sx={{ py: 2, bgcolor: 'background.paper', textAlign: 'center' }}>
                        Web Scraper Interface © {new Date().getFullYear()}
                    </Box>
                </Box>
            </Router>
        </ThemeProvider>
    );
}

export default App;
