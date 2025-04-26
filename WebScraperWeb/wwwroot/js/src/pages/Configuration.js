import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    Typography,
    Paper,
    Box,
    TextField,
    Switch,
    FormControlLabel,
    Button,
    Divider,
    Accordion,
    AccordionSummary,
    AccordionDetails,
    Slider,
    Grid,
    Alert
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import SaveIcon from '@mui/icons-material/Save';
import RestoreIcon from '@mui/icons-material/Restore';

import { startScraping } from '../services/api';

const Configuration = ({ isRunning, setIsRunning, setScrapingStats }) => {
    const navigate = useNavigate();
    const [error, setError] = useState('');
    const [config, setConfig] = useState({
        // Basic settings
        startUrl: 'https://example.com',
        baseUrl: 'https://example.com',
        outputDirectory: 'ScrapedData',
        delayBetweenRequests: 1000,
        maxConcurrentRequests: 5,
        maxDepth: 5,
        followExternalLinks: false,
        respectRobotsTxt: true,

        // Header/footer pattern learning
        autoLearnHeaderFooter: true,
        learningPagesCount: 5,

        // Content Change Detection
        enableChangeDetection: true,
        trackContentVersions: true,
        maxVersionsToKeep: 5,

        // Adaptive Crawling
        enableAdaptiveCrawling: true,
        priorityQueueSize: 100,
        adjustDepthBasedOnQuality: true,

        // Smart Rate Limiting
        enableAdaptiveRateLimiting: true,
        minDelayBetweenRequests: 500,
        maxDelayBetweenRequests: 5000,
        monitorResponseTimes: true
    });

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target;
        setConfig({
            ...config,
            [name]: type === 'checkbox' ? checked : value
        });
    };

    const handleSliderChange = (name) => (event, newValue) => {
        setConfig({
            ...config,
            [name]: newValue
        });
    };

    const handleNumberInputChange = (name) => (event) => {
        const value = event.target.value === '' ? '' : Number(event.target.value);
        setConfig({
            ...config,
            [name]: value
        });
    };

    const saveConfigToLocalStorage = () => {
        localStorage.setItem('scraperConfig', JSON.stringify(config));
        alert('Configuration saved to browser storage!');
    };

    const loadConfigFromLocalStorage = () => {
        const savedConfig = localStorage.getItem('scraperConfig');
        if (savedConfig) {
            setConfig(JSON.parse(savedConfig));
            alert('Configuration loaded from browser storage!');
        } else {
            alert('No saved configuration found!');
        }
    };

    const handleStartScraping = async () => {
        try {
            setError('');

            // Validate URL
            try {
                new URL(config.startUrl);
            } catch (e) {
                setError('Please enter a valid URL (including http:// or https://)');
                return;
            }

            // Start scraping
            await startScraping(config);
            setIsRunning(true);

            // Reset stats
            setScrapingStats({
                urlsProcessed: 0,
                startTime: new Date().toISOString(),
                endTime: null,
                elapsedTime: '00:00:00'
            });

            // Navigate to dashboard
            navigate('/');
        } catch (error) {
            console.error('Failed to start scraping:', error);
            setError(error.message || 'Failed to start scraping. Check the server logs.');
        }
    };

    return (
        <Box>
            <Typography variant="h4" gutterBottom>
                Scraper Configuration
            </Typography>

            {error && (
                <Alert severity="error" sx={{ mb: 2 }}>
                    {error}
                </Alert>
            )}

            <Paper sx={{ p: 3 }}>
                {/* Basic Configuration */}
                <Typography variant="h6" gutterBottom>
                    Basic Settings
                </Typography>

                <Grid container spacing={2}>
                    <Grid item xs={12} md={6}>
                        <TextField
                            fullWidth
                            label="Start URL"
                            name="startUrl"
                            value={config.startUrl}
                            onChange={handleChange}
                            margin="normal"
                            variant="outlined"
                            disabled={isRunning}
                            helperText="The URL where scraping will begin"
                        />
                    </Grid>

                    <Grid item xs={12} md={6}>
                        <TextField
                            fullWidth
                            label="Base URL"
                            name="baseUrl"
                            value={config.baseUrl}
                            onChange={handleChange}
                            margin="normal"
                            variant="outlined"
                            disabled={isRunning}
                            helperText="Domain to stay within (if not following external links)"
                        />
                    </Grid>

                    <Grid item xs={12} md={6}>
                        <TextField
                            fullWidth
                            label="Output Directory"
                            name="outputDirectory"
                            value={config.outputDirectory}
                            onChange={handleChange}
                            margin="normal"
                            variant="outlined"
                            disabled={isRunning}
                            helperText="Where scraped data will be stored"
                        />
                    </Grid>

                    <Grid item xs={12} md={6}>
                        <TextField
                            fullWidth
                            label="Max Depth"
                            name="maxDepth"
                            type="number"
                            value={config.maxDepth}
                            onChange={handleNumberInputChange('maxDepth')}
                            margin="normal"
                            variant="outlined"
                            disabled={isRunning}
                            InputProps={{ inputProps: { min: 1, max: 20 } }}
                            helperText="How many links deep to crawl from start URL"
                        />
                    </Grid>

                    <Grid item xs={12} md={6}>
                        <Box sx={{ width: '100%', mt: 2 }}>
                            <Typography gutterBottom>
                                Delay Between Requests (ms)
                            </Typography>
                            <Slider
                                value={config.delayBetweenRequests}
                                onChange={handleSliderChange('delayBetweenRequests')}
                                disabled={isRunning}
                                min={100}
                                max={5000}
                                step={100}
                                valueLabelDisplay="auto"
                                marks={[
                                    { value: 100, label: '100ms' },
                                    { value: 1000, label: '1s' },
                                    { value: 5000, label: '5s' }
                                ]}
                            />
                        </Box>
                    </Grid>

                    <Grid item xs={12} md={6}>
                        <Box sx={{ width: '100%', mt: 2 }}>
                            <Typography gutterBottom>
                                Max Concurrent Requests
                            </Typography>
                            <Slider
                                value={config.maxConcurrentRequests}
                                onChange={handleSliderChange('maxConcurrentRequests')}
                                disabled={isRunning}
                                min={1}
                                max={20}
                                step={1}
                                valueLabelDisplay="auto"
                                marks={[
                                    { value: 1, label: '1' },
                                    { value: 10, label: '10' },
                                    { value: 20, label: '20' }
                                ]}
                            />
                        </Box>
                    </Grid>

                    <Grid item xs={12} md={6}>
                        <FormControlLabel
                            control={
                                <Switch
                                    checked={config.followExternalLinks}
                                    onChange={handleChange}
                                    name="followExternalLinks"
                                    color="primary"
                                    disabled={isRunning}
                                />
                            }
                            label="Follow External Links"
                        />
                    </Grid>

                    <Grid item xs={12} md={6}>
                        <FormControlLabel
                            control={
                                <Switch
                                    checked={config.respectRobotsTxt}
                                    onChange={handleChange}
                                    name="respectRobotsTxt"
                                    color="primary"
                                    disabled={isRunning}
                                />
                            }
                            label="Respect robots.txt"
                        />
                    </Grid>
                </Grid>

                <Divider sx={{ my: 3 }} />

                {/* Advanced Configuration Sections */}
                <Typography variant="h6" gutterBottom>
                    Advanced Settings
                </Typography>

                {/* Pattern Learning */}
                <Accordion>
                    <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                        <Typography>Header/Footer Pattern Learning</Typography>
                    </AccordionSummary>
                    <AccordionDetails>
                        <Grid container spacing={2}>
                            <Grid item xs={12}>
                                <FormControlLabel
                                    control={
                                        <Switch
                                            checked={config.autoLearnHeaderFooter}
                                            onChange={handleChange}
                                            name="autoLearnHeaderFooter"
                                            color="primary"
                                            disabled={isRunning}
                                        />
                                    }
                                    label="Auto-learn Header and Footer Patterns"
                                />
                            </Grid>

                            <Grid item xs={12}>
                                <Box sx={{ width: '100%', mt: 1 }}>
                                    <Typography gutterBottom>
                                        Learning Pages Count
                                    </Typography>
                                    <Slider
                                        value={config.learningPagesCount}
                                        onChange={handleSliderChange('learningPagesCount')}
                                        disabled={isRunning || !config.autoLearnHeaderFooter}
                                        min={3}
                                        max={20}
                                        step={1}
                                        valueLabelDisplay="auto"
                                        marks={[
                                            { value: 3, label: '3' },
                                            { value: 10, label: '10' },
                                            { value: 20, label: '20' }
                                        ]}
                                    />
                                </Box>
                            </Grid>
                        </Grid>
                    </AccordionDetails>
                </Accordion>

                {/* Content Change Detection */}
                <Accordion>
                    <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                        <Typography>Content Change Detection</Typography>
                    </AccordionSummary>
                    <AccordionDetails>
                        <Grid container spacing={2}>
                            <Grid item xs={12} md={6}>
                                <FormControlLabel
                                    control={
                                        <Switch
                                            checked={config.enableChangeDetection}
                                            onChange={handleChange}
                                            name="enableChangeDetection"
                                            color="primary"
                                            disabled={isRunning}
                                        />
                                    }
                                    label="Enable Change Detection"
                                />
                            </Grid>

                            <Grid item xs={12} md={6}>
                                <FormControlLabel
                                    control={
                                        <Switch
                                            checked={config.trackContentVersions}
                                            onChange={handleChange}
                                            name="trackContentVersions"
                                            color="primary"
                                            disabled={isRunning || !config.enableChangeDetection}
                                        />
                                    }
                                    label="Track Content Versions"
                                />
                            </Grid>

                            <Grid item xs={12}>
                                <Box sx={{ width: '100%', mt: 1 }}>
                                    <Typography gutterBottom>
                                        Max Versions to Keep
                                    </Typography>
                                    <Slider
                                        value={config.maxVersionsToKeep}
                                        onChange={handleSliderChange('maxVersionsToKeep')}
                                        disabled={isRunning || !config.enableChangeDetection || !config.trackContentVersions}
                                        min={1}
                                        max={20}
                                        step={1}
                                        valueLabelDisplay="auto"
                                        marks={[
                                            { value: 1, label: '1' },
                                            { value: 5, label: '5' },
                                            { value: 20, label: '20' }
                                        ]}
                                    />
                                </Box>
                            </Grid>
                        </Grid>
                    </AccordionDetails>
                </Accordion>

                {/* Adaptive Crawling */}
                <Accordion>
                    <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                        <Typography>Adaptive Crawling Strategies</Typography>
                    </AccordionSummary>
                    <AccordionDetails>
                        <Grid container spacing={2}>
                            <Grid item xs={12} md={6}>
                                <FormControlLabel
                                    control={
                                        <Switch
                                            checked={config.enableAdaptiveCrawling}
                                            onChange={handleChange}
                                            name="enableAdaptiveCrawling"
                                            color="primary"
                                            disabled={isRunning}
                                        />
                                    }
                                    label="Enable Adaptive Crawling"
                                />
                            </Grid>

                            <Grid item xs={12} md={6}>
                                <FormControlLabel
                                    control={
                                        <Switch
                                            checked={config.adjustDepthBasedOnQuality}
                                            onChange={handleChange}
                                            name="adjustDepthBasedOnQuality"
                                            color="primary"
                                            disabled={isRunning || !config.enableAdaptiveCrawling}
                                        />
                                    }
                                    label="Adjust Depth Based on Content Quality"
                                />
                            </Grid>

                            <Grid item xs={12}>
                                <Box sx={{ width: '100%', mt: 1 }}>
                                    <Typography gutterBottom>
                                        Priority Queue Size
                                    </Typography>
                                    <Slider
                                        value={config.priorityQueueSize}
                                        onChange={handleSliderChange('priorityQueueSize')}
                                        disabled={isRunning || !config.enableAdaptiveCrawling}
                                        min={10}
                                        max={1000}
                                        step={10}
                                        valueLabelDisplay="auto"
                                        marks={[
                                            { value: 10, label: '10' },
                                            { value: 100, label: '100' },
                                            { value: 1000, label: '1000' }
                                        ]}
                                    />
                                </Box>
                            </Grid>
                        </Grid>
                    </AccordionDetails>
                </Accordion>

                {/* Smart Rate Limiting */}
                <Accordion>
                    <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                        <Typography>Smart Rate Limiting</Typography>
                    </AccordionSummary>
                    <AccordionDetails>
                        <Grid container spacing={2}>
                            <Grid item xs={12} md={6}>
                                <FormControlLabel
                                    control={
                                        <Switch
                                            checked={config.enableAdaptiveRateLimiting}
                                            onChange={handleChange}
                                            name="enableAdaptiveRateLimiting"
                                            color="primary"
                                            disabled={isRunning}
                                        />
                                    }
                                    label="Enable Adaptive Rate Limiting"
                                />
                            </Grid>

                            <Grid item xs={12} md={6}>
                                <FormControlLabel
                                    control={
                                        <Switch
                                            checked={config.monitorResponseTimes}
                                            onChange={handleChange}
                                            name="monitorResponseTimes"
                                            color="primary"
                                            disabled={isRunning || !config.enableAdaptiveRateLimiting}
                                        />
                                    }
                                    label="Monitor Response Times"
                                />
                            </Grid>

                            <Grid item xs={12} md={6}>
                                <Box sx={{ width: '100%', mt: 1 }}>
                                    <Typography gutterBottom>
                                        Min Delay Between Requests (ms)
                                    </Typography>
                                    <Slider
                                        value={config.minDelayBetweenRequests}
                                        onChange={handleSliderChange('minDelayBetweenRequests')}
                                        disabled={isRunning || !config.enableAdaptiveRateLimiting}
                                        min={100}
                                        max={2000}
                                        step={100}
                                        valueLabelDisplay="auto"
                                        marks={[
                                            { value: 100, label: '100ms' },
                                            { value: 1000, label: '1s' },
                                            { value: 2000, label: '2s' }
                                        ]}
                                    />
                                </Box>
                            </Grid>

                            <Grid item xs={12} md={6}>
                                <Box sx={{ width: '100%', mt: 1 }}>
                                    <Typography gutterBottom>
                                        Max Delay Between Requests (ms)
                                    </Typography>
                                    <Slider
                                        value={config.maxDelayBetweenRequests}
                                        onChange={handleSliderChange('maxDelayBetweenRequests')}
                                        disabled={isRunning || !config.enableAdaptiveRateLimiting}
                                        min={1000}
                                        max={10000}
                                        step={500}
                                        valueLabelDisplay="auto"
                                        marks={[
                                            { value: 1000, label: '1s' },
                                            { value: 5000, label: '5s' },
                                            { value: 10000, label: '10s' }
                                        ]}
                                    />
                                </Box>
                            </Grid>
                        </Grid>
                    </AccordionDetails>
                </Accordion>

                <Box sx={{ mt: 3, display: 'flex', gap: 2, justifyContent: 'space-between' }}>
                    <Box>
                        <Button
                            variant="outlined"
                            startIcon={<SaveIcon />}
                            onClick={saveConfigToLocalStorage}
                            sx={{ mr: 1 }}
                        >
                            Save Config
                        </Button>
                        <Button
                            variant="outlined"
                            startIcon={<RestoreIcon />}
                            onClick={loadConfigFromLocalStorage}
                        >
                            Load Config
                        </Button>
                    </Box>

                    <Button
                        variant="contained"
                        color="primary"
                        size="large"
                        startIcon={<PlayArrowIcon />}
                        onClick={handleStartScraping}
                        disabled={isRunning}
                    >
                        Start Scraping
                    </Button>
                </Box>
            </Paper>
        </Box>
    );
};

export default Configuration;