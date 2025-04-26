import React, { useState, useEffect } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import {
    Typography,
    Paper,
    Box,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    TablePagination,
    Button,
    TextField,
    InputAdornment,
    LinearProgress,
    Alert,
    Chip,
    IconButton,
    Card,
    CardContent
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import LinkIcon from '@mui/icons-material/Link';
import VisibilityIcon from '@mui/icons-material/Visibility';
import LaunchIcon from '@mui/icons-material/Launch';
import CalendarTodayIcon from '@mui/icons-material/CalendarToday';
import FormatListNumberedIcon from '@mui/icons-material/FormatListNumbered';

import { fetchResults } from '../services/api';

const Results = () => {
    const [results, setResults] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [page, setPage] = useState(0);
    const [rowsPerPage, setRowsPerPage] = useState(10);
    const [totalCount, setTotalCount] = useState(0);
    const [search, setSearch] = useState('');
    const [searchInput, setSearchInput] = useState('');

    const fetchData = async () => {
        try {
            setLoading(true);
            const data = await fetchResults(page + 1, rowsPerPage, search);
            setResults(data.results);
            setTotalCount(data.totalCount);
        } catch (err) {
            setError('Failed to load results. ' + err.message);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchData();
    }, [page, rowsPerPage, search]);

    const handleChangePage = (event, newPage) => {
        setPage(newPage);
    };

    const handleChangeRowsPerPage = (event) => {
        setRowsPerPage(parseInt(event.target.value, 10));
        setPage(0);
    };

    const handleSearch = () => {
        setSearch(searchInput);
        setPage(0);
    };

    const handleSearchKeyDown = (e) => {
        if (e.key === 'Enter') {
            handleSearch();
        }
    };

    const formatDate = (dateString) => {
        if (!dateString) return 'N/A';
        return new Date(dateString).toLocaleString();
    };

    return (
        <Box>
            <Typography variant="h4" gutterBottom>
                Scraping Results
            </Typography>

            {/* Summary Card */}
            <Card sx={{ mb: 3 }}>
                <CardContent>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 3, justifyContent: 'space-around' }}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <FormatListNumberedIcon color="primary" />
                            <Typography variant="h6">{totalCount} Pages Scraped</Typography>
                        </Box>

                        <Box sx={{ display: 'flex', gap: 1 }}>
                            <TextField
                                variant="outlined"
                                size="small"
                                placeholder="Search URLs or content..."
                                value={searchInput}
                                onChange={(e) => setSearchInput(e.target.value)}
                                onKeyDown={handleSearchKeyDown}
                                InputProps={{
                                    startAdornment: (
                                        <InputAdornment position="start">
                                            <SearchIcon />
                                        </InputAdornment>
                                    ),
                                }}
                            />
                            <Button
                                variant="contained"
                                onClick={handleSearch}
                                disabled={loading}
                            >
                                Search
                            </Button>
                        </Box>
                    </Box>
                </CardContent>
            </Card>

            {/* Results Table */}
            <Paper sx={{ width: '100%', overflow: 'hidden' }}>
                {loading && <LinearProgress />}

                {error && (
                    <Alert severity="error" sx={{ m: 2 }}>
                        {error}
                    </Alert>
                )}

                {!loading && results && Object.keys(results).length === 0 && (
                    <Alert severity="info" sx={{ m: 2 }}>
                        No results found. Try a different search or start a scraping job.
                    </Alert>
                )}

                {Object.keys(results).length > 0 && (
                    <>
                        <TableContainer sx={{ maxHeight: 600 }}>
                            <Table stickyHeader>
                                <TableHead>
                                    <TableRow>
                                        <TableCell>URL</TableCell>
                                        <TableCell>Scraped Date</TableCell>
                                        <TableCell>Depth</TableCell>
                                        <TableCell>Content Preview</TableCell>
                                        <TableCell align="center">Actions</TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {Object.entries(results).map(([url, data]) => (
                                        <TableRow hover key={url}>
                                            <TableCell sx={{ maxWidth: 300 }}>
                                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                    <LinkIcon fontSize="small" color="primary" />
                                                    <Typography
                                                        variant="body2"
                                                        sx={{
                                                            textOverflow: 'ellipsis',
                                                            overflow: 'hidden',
                                                            whiteSpace: 'nowrap'
                                                        }}
                                                    >
                                                        {url}
                                                    </Typography>
                                                </Box>
                                            </TableCell>
                                            <TableCell>
                                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                    <CalendarTodayIcon fontSize="small" />
                                                    {formatDate(data.scrapedDateTime)}
                                                </Box>
                                            </TableCell>
                                            <TableCell>
                                                <Chip
                                                    label={`Level ${data.depth}`}
                                                    size="small"
                                                    color={data.depth < 3 ? "primary" : data.depth < 5 ? "secondary" : "default"}
                                                />
                                            </TableCell>
                                            <TableCell>
                                                <Typography
                                                    variant="body2"
                                                    sx={{
                                                        textOverflow: 'ellipsis',
                                                        overflow: 'hidden',
                                                        whiteSpace: 'nowrap',
                                                        maxWidth: 400
                                                    }}
                                                >
                                                    {data.contentPreview}
                                                </Typography>
                                            </TableCell>
                                            <TableCell align="center">
                                                <Box sx={{ display: 'flex', justifyContent: 'center' }}>
                                                    <IconButton
                                                        component={RouterLink}
                                                        to={`/results/${encodeURIComponent(url)}`}
                                                        color="primary"
                                                        size="small"
                                                        title="View details"
                                                    >
                                                        <VisibilityIcon />
                                                    </IconButton>
                                                    <IconButton
                                                        href={url}
                                                        target="_blank"
                                                        rel="noreferrer"
                                                        color="secondary"
                                                        size="small"
                                                        title="Open in new tab"
                                                    >
                                                        <LaunchIcon />
                                                    </IconButton>
                                                </Box>
                                            </TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </TableContainer>
                        <TablePagination
                            rowsPerPageOptions={[10, 25, 50, 100]}
                            component="div"
                            count={totalCount}
                            rowsPerPage={rowsPerPage}
                            page={page}
                            onPageChange={handleChangePage}
                            onRowsPerPageChange={handleChangeRowsPerPage}
                        />
                    </>
                )}
            </Paper>
        </Box>
    );
};

export default Results;