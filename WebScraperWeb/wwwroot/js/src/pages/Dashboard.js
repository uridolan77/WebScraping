import React, { useState, useEffect, useRef } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import {
    Typography,
    Paper,
    Grid,
    Button,
    Box,
    Card,
    CardContent,
    CircularProgress,
    List,
    ListItem,
    ListItemText,
    Divider
} from '@mui/material';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import StopIcon from '@mui/icons-material/Stop';
import SettingsIcon from '@mui/icons-material/Settings';
import QueryStatsIcon from '@mui/icons-material/QueryStats';
import FolderIcon from '@mui/icons-material/Folder';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import LinkIcon from '@mui/icons-material/Link';

import { fetchScraperStatus, stopScraping, fetchLogs } from '../services/api';

const Dashboard = ({ isRunning, setIsRunning, scrapingStats, setScrapingStats }) => {
    const [logs, setLogs] = useState([]);
    const logsEndRef = useRef(null);

    const scrollToBottom = () => {
        logsEndRef.current?.scrollIntoView({ behavior: "smooth" });
    }

    const updateStatus = async () => {
        try {
            const status = await fetchScraperStatus();
            setIsRunning(status.isRunning);
            setScrapingStats({
                urlsProcessed: status.urlsProcessed || 0,
                startTime: status.startTime,
                endTime: status.endTime,
                elapsedTime: status.elapsedTime
            });
        } catch (error) {
            console.error('Failed to fetch status:', error);
        }
    };

    const updateLogs = async () => {
        try {
            const result = await fetchLogs();
            setLogs(result.logs);
            scrollToBottom();
        } catch (error) {
            console.error('Failed to fetch logs:', error);
        }
    };

    useEffect(() => {
        // Initial load
        updateStatus();
        updateLogs();

        // Set up polling for updates
        const statusInterval = setInterval(updateStatus, 3000);
        const logsInterval = setInterval(updateLogs, 5000);

        return () => {
            clearInterval(statusInterval);
            clearInterval(logsInterval);
        };
    }, []);

    useEffect(() => {
        scrollToBottom();
    }, [logs]);

    const handleStopScraping = async () => {
        try {
            await stopScraping();
            setIsRunning(false);
            updateStatus();
        } catch (error) {
            console.error('Failed to stop scraping:', error);
        }
    };

    return (
        <Box>
            <Typography variant="h4" gutterBottom>
                Dashboard
            </Typography>

            <Grid container spacing={3}>
                {/* Status Cards */}
                <Grid item xs={12} md={8}>
                    <Grid container spacing={2}>
                        <Grid item xs={12} sm={6} md={4}>
                            <Card>
                                <CardContent>
                                    <Typography color="textSecondary" gutterBottom>
                                        Status
                                    </Typography>
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                        {isRunning ? (
                                            <CircularProgress size={20} color="secondary" />
                                        ) : (
                                            <QueryStatsIcon />
                                        )}
                                        <Typography variant="h6">
                                            {isRunning ? 'Running' : 'Idle'}
                                        </Typography>
                                    </Box>
                                </CardContent>
                            </Card>
                        </Grid>

                        <Grid item xs={12} sm={6} md={4}>
                            <Card>
                                <CardContent>
                                    <Typography color="textSecondary" gutterBottom>
                                        URLs Processed
                                    </Typography>
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                        <LinkIcon />
                                        <Typography variant="h6">
                                            {scrapingStats.urlsProcessed}
                                        </Typography>
                                    </Box>
                                </CardContent>
                            </Card>
                        </Grid>

                        <Grid item xs={12} sm={6} md={4}>
                            <Card>
                                <CardContent>
                                    <Typography color="textSecondary" gutterBottom>
                                        Duration
                                    </Typography>
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                        <AccessTimeIcon />
                                        <Typography variant="h6">
                                            {scrapingStats.elapsedTime || '00:00:00'}
                                        </Typography>
                                    </Box>
                                </CardContent>
                            </Card>
                        </Grid>
                    </Grid>

                    {/* Log Display */}
                    <Paper sx={{ mt: 3, p: 2, maxHeight: '500px', overflow: 'auto' }}>
                        <Typography variant="h6" gutterBottom>
                            Logs
                        </Typography>
                        <List dense>
                            {logs.length > 0 ? (
                                logs.map((log, index) => (
                                    <React.Fragment key={index}>
                                        <ListItem>
                                            <ListItemText
                                                primary={log}
                                                sx={{
                                                    '& .MuiListItemText-primary': {
                                                        fontFamily: 'monospace',
                                                        fontSize: '0.875rem'
                                                    }
                                                }}
                                            />
                                        </ListItem>
                                        {index < logs.length - 1 && <Divider />}
                                    </React.Fragment>
                                ))
                            ) : (
                                <ListItem>
                                    <ListItemText primary="No logs available" />
                                </ListItem>
                            )}
                            <div ref={logsEndRef} />
                        </List>
                    </Paper>
                </Grid>

                {/* Actions */}
                <Grid item xs={12} md={4}>
                    <Paper sx={{ p: 2 }}>
                        <Typography variant="h6" gutterBottom>
                            Actions
                        </Typography>

                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                            <Button
                                component={RouterLink}
                                to="/configure"
                                variant="contained"
                                color="primary"
                                startIcon={<SettingsIcon />}
                                disabled={isRunning}
                                fullWidth
                            >
                                Configure and Start
                            </Button>

                            <Button
                                variant="contained"
                                color="secondary"
                                startIcon={<StopIcon />}
                                onClick={handleStopScraping}
                                disabled={!isRunning}
                                fullWidth
                            >
                                Stop Scraping
                            </Button>

                            <Button
                                component={RouterLink}
                                to="/results"
                                variant="outlined"
                                startIcon={<FolderIcon />}
                                fullWidth
                            >
                                View Results
                            </Button>
                        </Box>
                    </Paper>

                    {/* Time Info */}
                    {(scrapingStats.startTime || scrapingStats.endTime) && (
                        <Paper sx={{ p: 2, mt: 2 }}>
                            <Typography variant="h6" gutterBottom>
                                Time Information
                            </Typography>

                            {scrapingStats.startTime && (
                                <Typography variant="body2" gutterBottom>
                                    <strong>Started:</strong> {new Date(scrapingStats.startTime).toLocaleString()}
                                </Typography>
                            )}

                            {scrapingStats.endTime && (
                                <Typography variant="body2" gutterBottom>
                                    <strong>Finished:</strong> {new Date(scrapingStats.endTime).toLocaleString()}
                                </Typography>
                            )}
                        </Paper>
                    )}
                </Grid>
            </Grid>
        </Box>
    );
};

export default Dashboard;