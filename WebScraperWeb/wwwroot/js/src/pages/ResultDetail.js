import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
    Typography,
    Paper,
    Box,
    Button,
    Divider,
    LinearProgress,
    Alert,
    Chip,
    IconButton,
    Tabs,
    Tab,
    Card,
    CardContent,
    Link,
    TableContainer,
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableRow
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import LinkIcon from '@mui/icons-material/Link';
import CalendarTodayIcon from '@mui/icons-material/CalendarToday';
import FormatListNumberedIcon from '@mui/icons-material/FormatListNumbered';
import LaunchIcon from '@mui/icons-material/Launch';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import ArticleIcon from '@mui/icons-material/Article';
import CodeIcon from '@mui/icons-material/Code';
import TimelineIcon from '@mui/icons-material/Timeline';

import { fetchResultDetail } from '../services/api';

// TabPanel component for the detail view tabs
function TabPanel(props) {
    const { children, value, index, ...other } = props;

    return (
        <div
            role="tabpanel"
            hidden={value !== index}
            id={`tabpanel-${index}`}
            aria-labelledby={`tab-${index}`}
            {...other}
        >
            {value === index && (
                <Box sx={{ p: 3 }}>
                    {children}
                </Box>
            )}
        </div>
    );
}

const ResultDetail = () => {
    const { url } = useParams();
    const navigate = useNavigate();
    const [pageData, setPageData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [tabValue, setTabValue] = useState(0);
    const [copied, setCopied] = useState(false);

    useEffect(() => {
        const fetchData = async () => {
            try {
                setLoading(true);
                const data = await fetchResultDetail(url);
                setPageData(data);
            } catch (err) {
                setError('Failed to load page details: ' + err.message);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [url]);

    const handleGoBack = () => {
        navigate('/results');
    };

    const handleTabChange = (event, newValue) => {
        setTabValue(newValue);
    };

    const copyToClipboard = (text) => {
        navigator.clipboard.writeText(text).then(() => {
            setCopied(true);
            setTimeout(() => setCopied(false), 2000);
        });
    };

    const formatDate = (dateString) => {
        if (!dateString) return 'N/A';
        return new Date(dateString).toLocaleString();
    };

    if (loading) {
        return (
            <Box>
                <Button
                    startIcon={<ArrowBackIcon />}
                    onClick={handleGoBack}
                    sx={{ mb: 2 }}
                >
                    Back to Results
                </Button>
                <Typography variant="h4" gutterBottom>
                    Loading Page Details...
                </Typography>
                <LinearProgress />
            </Box>
        );
    }

    if (error) {
        return (
            <Box>
                <Button
                    startIcon={<ArrowBackIcon />}
                    onClick={handleGoBack}
                    sx={{ mb: 2 }}
                >
                    Back to Results
                </Button>
                <Alert severity="error">{error}</Alert>
            </Box>
        );
    }

    if (!pageData) {
        return (
            <Box>
                <Button
                    startIcon={<ArrowBackIcon />}
                    onClick={handleGoBack}
                    sx={{ mb: 2 }}
                >
                    Back to Results
                </Button>
                <Alert severity="info">No data found for this URL.</Alert>
            </Box>
        );
    }

    const decodedUrl = decodeURIComponent(url);

    return (
        <Box>
            <Button
                startIcon={<ArrowBackIcon />}
                onClick={handleGoBack}
                sx={{ mb: 2 }}
            >
                Back to Results
            </Button>

            <Typography variant="h4" gutterBottom>
                Page Details
            </Typography>

            {/* Page Info Card */}
            <Card sx={{ mb: 3 }}>
                <CardContent>
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
                            <LinkIcon color="primary" />
                            <Typography variant="h6" sx={{ wordBreak: 'break-all' }}>
                                {decodedUrl}
                            </Typography>
                            <IconButton
                                href={decodedUrl}
                                target="_blank"
                                rel="noreferrer"
                                color="secondary"
                                size="small"
                                title="Open in new tab"
                            >
                                <LaunchIcon />
                            </IconButton>
                        </Box>

                        <Divider />

                        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 3 }}>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <CalendarTodayIcon />
                                <Typography>
                                    <strong>Scraped:</strong> {formatDate(pageData.scrapedDateTime)}
                                </Typography>
                            </Box>

                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <FormatListNumberedIcon />
                                <Typography>
                                    <strong>Depth:</strong> {pageData.depth}
                                </Typography>
                            </Box>

                            <Chip
                                label={`Level ${pageData.depth}`}
                                color={pageData.depth < 3 ? "primary" : pageData.depth < 5 ? "secondary" : "default"}
                            />
                        </Box>
                    </Box>
                </CardContent>
            </Card>

            {/* Content Tabs */}
            <Paper sx={{ width: '100%' }}>
                <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
                    <Tabs
                        value={tabValue}
                        onChange={handleTabChange}
                        aria-label="page content tabs"
                    >
                        <Tab icon={<ArticleIcon />} iconPosition="start" label="Extracted Content" />
                        <Tab icon={<CodeIcon />} iconPosition="start" label="Raw HTML" />
                        <Tab icon={<TimelineIcon />} iconPosition="start" label="Changes History" />
                    </Tabs>
                </Box>

                {/* Extracted Content Tab */}
                <TabPanel value={tabValue} index={0}>
                    <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
                        <Button
                            startIcon={<ContentCopyIcon />}
                            onClick={() => copyToClipboard(pageData.textContent)}
                            variant="outlined"
                            size="small"
                        >
                            {copied ? 'Copied!' : 'Copy Text'}
                        </Button>
                    </Box>

                    <Paper
                        elevation={0}
                        variant="outlined"
                        sx={{
                            p: 2,
                            maxHeight: '600px',
                            overflow: 'auto',
                            whiteSpace: 'pre-wrap'
                        }}
                    >
                        {pageData.textContent || 'No content extracted.'}
                    </Paper>
                </TabPanel>

                {/* Raw HTML Tab */}
                <TabPanel value={tabValue} index={1}>
                    <Alert severity="info" sx={{ mb: 2 }}>
                        This tab would show the raw HTML of the page if it were available in the API response.
                    </Alert>

                    <Paper
                        elevation={0}
                        variant="outlined"
                        sx={{
                            p: 2,
                            maxHeight: '600px',
                            overflow: 'auto',
                            backgroundColor: '#f5f5f5',
                            fontFamily: 'monospace',
                            fontSize: '0.875rem'
                        }}
                    >
                        {'<html>\n  <head>\n    <title>Example Page</title>\n  </head>\n  <body>\n    <!-- HTML content would appear here -->\n  </body>\n</html>'}
                    </Paper>
                </TabPanel>

                {/* Changes History Tab */}
                <TabPanel value={tabValue} index={2}>
                    <Alert severity="info" sx={{ mb: 2 }}>
                        This tab would show the content change history if version tracking is enabled in the scraper.
                    </Alert>

                    <TableContainer component={Paper} variant="outlined">
                        <Table>
                            <TableHead>
                                <TableRow>
                                    <TableCell>Version</TableCell>
                                    <TableCell>Date</TableCell>
                                    <TableCell>Change Type</TableCell>
                                    <TableCell>Changes</TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                <TableRow>
                                    <TableCell>1</TableCell>
                                    <TableCell>{formatDate(pageData.scrapedDateTime)}</TableCell>
                                    <TableCell>
                                        <Chip label="Initial" color="primary" size="small" />
                                    </TableCell>
                                    <TableCell>Initial scrape</TableCell>
                                </TableRow>
                                <TableRow>
                                    <TableCell colSpan={4} align="center">
                                        <Typography variant="body2" color="textSecondary">
                                            No changes detected after initial scrape
                                        </Typography>
                                    </TableCell>
                                </TableRow>
                            </TableBody>
                        </Table>
                    </TableContainer>
                </TabPanel>
            </Paper>
        </Box>
    );
};

export default ResultDetail;