import React, { useState, useEffect } from 'react';
import {
  Container,
  Typography,
  Box,
  Paper,
  Grid,
  Card,
  CardContent,
  CardHeader,
  Tabs,
  Tab,
  Divider,
  Alert,
  CircularProgress,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Button,
} from '@mui/material';
import { useScrapers } from '../hooks';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import {
  BarChart,
  Bar,
  LineChart,
  Line,
  PieChart,
  Pie,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  Cell,
} from 'recharts';
import FilterListIcon from '@mui/icons-material/FilterList';
import DownloadIcon from '@mui/icons-material/Download';
import RefreshIcon from '@mui/icons-material/Refresh';

// Tab Panel component
function TabPanel(props) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`analytics-tab-${index}`}
      aria-labelledby={`analytics-tab-${index}`}
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

// Generate sample data for the charts
const generateSampleScraperData = (days = 30) => {
  const data = [];
  const today = new Date();
  
  for (let i = 0; i < days; i++) {
    const date = new Date(today);
    date.setDate(date.getDate() - i);
    data.push({
      date: date.toISOString().split('T')[0],
      pages: Math.floor(Math.random() * 300) + 50,
      errors: Math.floor(Math.random() * 20),
      new: Math.floor(Math.random() * 30),
      changed: Math.floor(Math.random() * 40),
    });
  }
  
  return data.reverse();
};

const generateSampleContentTypes = () => {
  return [
    { name: 'HTML', value: 68 },
    { name: 'PDF', value: 15 },
    { name: 'DOC/DOCX', value: 8 },
    { name: 'Excel', value: 6 },
    { name: 'Others', value: 3 },
  ];
};

const generateSampleRegulatoryData = () => {
  return [
    { name: 'High Impact', value: 12 },
    { name: 'Medium Impact', value: 23 },
    { name: 'Low Impact', value: 45 },
    { name: 'No Impact', value: 20 },
  ];
};

// Chart colors
const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884d8', '#82ca9d'];

const Analytics = () => {
  const { getScrapers } = useScrapers();
  const [activeTab, setActiveTab] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [scrapers, setScrapers] = useState([]);
  const [selectedScraper, setSelectedScraper] = useState('all');
  const [dateRange, setDateRange] = useState({
    startDate: new Date(new Date().setDate(new Date().getDate() - 30)), // 30 days ago
    endDate: new Date(), // Today
  });
  const [showFilters, setShowFilters] = useState(false);
  
  // Sample data for charts
  const [scrapingActivityData, setScrapingActivityData] = useState([]);
  const [contentTypeData, setContentTypeData] = useState([]);
  const [regulatoryImpactData, setRegulatoryImpactData] = useState([]);

  // Load data
  useEffect(() => {
    const fetchData = async () => {
      try {
        setIsLoading(true);
        
        // Load scrapers from the API
        const scrapersData = await getScrapers();
        setScrapers(scrapersData || []);
        
        // Load sample data for charts
        // In a real application, this would come from an analytics API
        setScrapingActivityData(generateSampleScraperData(30));
        setContentTypeData(generateSampleContentTypes());
        setRegulatoryImpactData(generateSampleRegulatoryData());
        
      } catch (error) {
        console.error('Error loading analytics data:', error);
      } finally {
        setIsLoading(false);
      }
    };
    
    fetchData();
  }, [getScrapers]);

  // Handle tab change
  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
  };
  
  // Handle scraper selection
  const handleScraperChange = (event) => {
    setSelectedScraper(event.target.value);
  };
  
  // Handle date range change
  const handleStartDateChange = (newDate) => {
    setDateRange({ ...dateRange, startDate: newDate });
  };
  
  const handleEndDateChange = (newDate) => {
    setDateRange({ ...dateRange, endDate: newDate });
  };
  
  // Handle refresh
  const handleRefresh = () => {
    setIsLoading(true);
    
    // Simulate data refresh
    setTimeout(() => {
      setScrapingActivityData(generateSampleScraperData(30));
      setContentTypeData(generateSampleContentTypes());
      setRegulatoryImpactData(generateSampleRegulatoryData());
      setIsLoading(false);
    }, 1000);
  };
  
  // Format date for display
  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  };
  
  if (isLoading) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns}>
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Typography variant="h4" gutterBottom>
            Analytics Dashboard
          </Typography>
          <Typography variant="body1" color="textSecondary">
            Analyze your scraping performance and content insights
          </Typography>
        </Box>
        
        {/* Filters */}
        <Paper sx={{ p: 2, mb: 4 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6">
              Analytics Controls
            </Typography>
            <Box>
              <Button 
                variant="outlined" 
                startIcon={<FilterListIcon />} 
                onClick={() => setShowFilters(!showFilters)}
                sx={{ mr: 1 }}
              >
                {showFilters ? 'Hide Filters' : 'Show Filters'}
              </Button>
              <Button 
                variant="outlined" 
                startIcon={<RefreshIcon />} 
                onClick={handleRefresh}
                sx={{ mr: 1 }}
              >
                Refresh
              </Button>
              <Button 
                variant="outlined" 
                startIcon={<DownloadIcon />}
              >
                Export
              </Button>
            </Box>
          </Box>
          
          {showFilters && (
            <Grid container spacing={3} sx={{ mt: 1 }}>
              <Grid item xs={12} md={4}>
                <FormControl fullWidth>
                  <InputLabel id="scraper-select-label">Scraper</InputLabel>
                  <Select
                    labelId="scraper-select-label"
                    id="scraper-select"
                    value={selectedScraper}
                    label="Scraper"
                    onChange={handleScraperChange}
                  >
                    <MenuItem value="all">All Scrapers</MenuItem>
                    {scrapers.map((scraper) => (
                      <MenuItem key={scraper.id} value={scraper.id}>
                        {scraper.name}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={4}>
                <DatePicker
                  label="Start Date"
                  value={dateRange.startDate}
                  onChange={handleStartDateChange}
                  renderInput={(params) => <TextField {...params} fullWidth />}
                />
              </Grid>
              <Grid item xs={12} md={4}>
                <DatePicker
                  label="End Date"
                  value={dateRange.endDate}
                  onChange={handleEndDateChange}
                  renderInput={(params) => <TextField {...params} fullWidth />}
                />
              </Grid>
            </Grid>
          )}
        </Paper>
        
        {/* Stats Summary */}
        <Grid container spacing={3} sx={{ mb: 4 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="h5" component="div" sx={{ fontWeight: 'bold' }}>
                  14,328
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Total Pages Scraped
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="h5" component="div" sx={{ fontWeight: 'bold' }}>
                  873
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Regulatory Documents
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="h5" component="div" sx={{ fontWeight: 'bold' }}>
                  128
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Significant Changes
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="h5" component="div" sx={{ fontWeight: 'bold' }}>
                  12
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  High Impact Changes
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
        
        {/* Tabs */}
        <Paper sx={{ mb: 4 }}>
          <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
            <Tabs
              value={activeTab}
              onChange={handleTabChange}
              indicatorColor="primary"
              textColor="primary"
              variant="scrollable"
              scrollButtons="auto"
            >
              <Tab label="Scraping Activity" />
              <Tab label="Content Analysis" />
              <Tab label="Regulatory Impact" />
            </Tabs>
          </Box>
          
          {/* Scraping Activity Tab */}
          <TabPanel value={activeTab} index={0}>
            <Typography variant="h6" gutterBottom>
              Scraping Activity Over Time
            </Typography>
            <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
              Total pages scraped, errors encountered, and new/changed content detected over time.
            </Typography>
            
            <Box sx={{ height: 400 }}>
              <ResponsiveContainer width="100%" height="100%">
                <LineChart
                  data={scrapingActivityData}
                  margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="date" tickFormatter={formatDate} />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Line 
                    type="monotone" 
                    dataKey="pages" 
                    name="Pages Scraped" 
                    stroke="#8884d8" 
                    activeDot={{ r: 8 }} 
                  />
                  <Line 
                    type="monotone" 
                    dataKey="errors" 
                    name="Errors" 
                    stroke="#ff5252" 
                  />
                  <Line 
                    type="monotone" 
                    dataKey="new" 
                    name="New Content" 
                    stroke="#4caf50" 
                  />
                  <Line 
                    type="monotone" 
                    dataKey="changed" 
                    name="Changed Content" 
                    stroke="#fb8c00" 
                  />
                </LineChart>
              </ResponsiveContainer>
            </Box>
            
            <Divider sx={{ my: 4 }} />
            
            <Typography variant="h6" gutterBottom>
              Scraper Performance
            </Typography>
            <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
              Comparing the performance of different scrapers in your system.
            </Typography>
            
            <Box sx={{ height: 400 }}>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart
                  layout="vertical"
                  data={[
                    { name: 'UKGC Website', pages: 4328, time: 127 },
                    { name: 'Regulatory News', pages: 2873, time: 98 },
                    { name: 'Industry Updates', pages: 3142, time: 112 },
                    { name: 'Compliance Docs', pages: 1983, time: 76 },
                    { name: 'Market Intelligence', pages: 2002, time: 83 },
                  ]}
                  margin={{ top: 5, right: 30, left: 100, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis type="number" />
                  <YAxis dataKey="name" type="category" />
                  <Tooltip />
                  <Legend />
                  <Bar dataKey="pages" name="Pages Scraped" fill="#8884d8" />
                  <Bar dataKey="time" name="Avg. Time (ms)" fill="#82ca9d" />
                </BarChart>
              </ResponsiveContainer>
            </Box>
          </TabPanel>
          
          {/* Content Analysis Tab */}
          <TabPanel value={activeTab} index={1}>
            <Grid container spacing={4}>
              <Grid item xs={12} md={6}>
                <Typography variant="h6" gutterBottom>
                  Content Types Distribution
                </Typography>
                <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
                  Breakdown of different document types found during scraping.
                </Typography>
                
                <Box sx={{ height: 300 }}>
                  <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                      <Pie
                        data={contentTypeData}
                        cx="50%"
                        cy="50%"
                        labelLine={false}
                        label={({ name, percent }) => `${name} (${(percent * 100).toFixed(0)}%)`}
                        outerRadius={100}
                        fill="#8884d8"
                        dataKey="value"
                      >
                        {contentTypeData.map((entry, index) => (
                          <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                        ))}
                      </Pie>
                      <Tooltip formatter={(value) => [`${value} documents`, 'Count']} />
                    </PieChart>
                  </ResponsiveContainer>
                </Box>
              </Grid>
              
              <Grid item xs={12} md={6}>
                <Typography variant="h6" gutterBottom>
                  Content Change Frequency
                </Typography>
                <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
                  How often content changes across different scraped domains.
                </Typography>
                
                <Box sx={{ height: 300 }}>
                  <ResponsiveContainer width="100%" height="100%">
                    <BarChart
                      data={[
                        { name: 'Daily', count: 23 },
                        { name: 'Weekly', count: 45 },
                        { name: 'Monthly', count: 98 },
                        { name: 'Quarterly', count: 75 },
                        { name: 'Rarely', count: 42 },
                      ]}
                      margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                    >
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="name" />
                      <YAxis />
                      <Tooltip />
                      <Legend />
                      <Bar dataKey="count" name="Number of URLs" fill="#8884d8" />
                    </BarChart>
                  </ResponsiveContainer>
                </Box>
              </Grid>
              
              <Grid item xs={12}>
                <Divider sx={{ my: 2 }} />
                <Typography variant="h6" gutterBottom>
                  Content Size Distribution
                </Typography>
                <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
                  Size distribution of documents categorized by type.
                </Typography>
                
                <Box sx={{ height: 400 }}>
                  <ResponsiveContainer width="100%" height="100%">
                    <BarChart
                      data={[
                        { name: '<100KB', HTML: 125, PDF: 32, DOC: 17, XLS: 12 },
                        { name: '100KB-500KB', HTML: 86, PDF: 45, DOC: 23, XLS: 18 },
                        { name: '500KB-1MB', HTML: 52, PDF: 38, DOC: 15, XLS: 9 },
                        { name: '1MB-5MB', HTML: 23, PDF: 27, DOC: 11, XLS: 7 },
                        { name: '>5MB', HTML: 7, PDF: 18, DOC: 5, XLS: 3 },
                      ]}
                      margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
                    >
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="name" />
                      <YAxis />
                      <Tooltip />
                      <Legend />
                      <Bar dataKey="HTML" stackId="a" fill="#8884d8" />
                      <Bar dataKey="PDF" stackId="a" fill="#82ca9d" />
                      <Bar dataKey="DOC" stackId="a" fill="#ffc658" />
                      <Bar dataKey="XLS" stackId="a" fill="#ff8042" />
                    </BarChart>
                  </ResponsiveContainer>
                </Box>
              </Grid>
            </Grid>
          </TabPanel>
          
          {/* Regulatory Impact Tab */}
          <TabPanel value={activeTab} index={2}>
            <Grid container spacing={4}>
              <Grid item xs={12} md={6}>
                <Typography variant="h6" gutterBottom>
                  Regulatory Impact Distribution
                </Typography>
                <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
                  Distribution of regulatory changes by impact level.
                </Typography>
                
                <Box sx={{ height: 300 }}>
                  <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                      <Pie
                        data={regulatoryImpactData}
                        cx="50%"
                        cy="50%"
                        labelLine={false}
                        label={({ name, percent }) => `${name} (${(percent * 100).toFixed(0)}%)`}
                        outerRadius={100}
                        fill="#8884d8"
                        dataKey="value"
                      >
                        {regulatoryImpactData.map((entry, index) => (
                          <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                        ))}
                      </Pie>
                      <Tooltip formatter={(value) => [`${value} changes`, 'Count']} />
                    </PieChart>
                  </ResponsiveContainer>
                </Box>
              </Grid>
              
              <Grid item xs={12} md={6}>
                <Typography variant="h6" gutterBottom>
                  Regulatory Change Timeline
                </Typography>
                <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
                  Regulatory changes over time, categorized by impact level.
                </Typography>
                
                <Box sx={{ height: 300 }}>
                  <ResponsiveContainer width="100%" height="100%">
                    <LineChart
                      data={[
                        { month: 'Jan', high: 3, medium: 5, low: 12 },
                        { month: 'Feb', high: 2, medium: 7, low: 10 },
                        { month: 'Mar', high: 4, medium: 6, low: 14 },
                        { month: 'Apr', high: 3, medium: 5, low: 9 },
                        { month: 'May', high: 0, medium: 3, low: 7 },
                      ]}
                      margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                    >
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="month" />
                      <YAxis />
                      <Tooltip />
                      <Legend />
                      <Line type="monotone" dataKey="high" name="High Impact" stroke="#ff5252" />
                      <Line type="monotone" dataKey="medium" name="Medium Impact" stroke="#fb8c00" />
                      <Line type="monotone" dataKey="low" name="Low Impact" stroke="#4caf50" />
                    </LineChart>
                  </ResponsiveContainer>
                </Box>
              </Grid>
              
              <Grid item xs={12}>
                <Divider sx={{ my: 2 }} />
                <Typography variant="h6" gutterBottom>
                  Recent High-Impact Regulatory Changes
                </Typography>
                
                <Alert severity="info" sx={{ mb: 2 }}>
                  12 high-impact regulatory changes have been detected in the last 30 days.
                </Alert>
                
                <Box sx={{ mt: 2 }}>
                  {[1, 2, 3].map((i) => (
                    <Card key={i} sx={{ mb: 2 }}>
                      <CardContent>
                        <Typography variant="subtitle1" sx={{ fontWeight: 'bold' }}>
                          {i === 1 ? 'UKGC Updates Licensing Conditions' : 
                           i === 2 ? 'New Responsible Gambling Measures' : 
                           'AML Compliance Framework Changes'}
                        </Typography>
                        <Typography variant="body2" color="textSecondary" gutterBottom>
                          Detected on: {i === 1 ? 'Apr 28, 2025' : 
                                         i === 2 ? 'Apr 15, 2025' : 
                                         'Apr 02, 2025'}
                        </Typography>
                        <Typography variant="body2">
                          {i === 1 ? 'The UK Gambling Commission has updated sections 8.1 through 8.7 of the licensing conditions, adding new requirements for operators regarding customer verification processes.' : 
                           i === 2 ? 'New measures for responsible gambling have been introduced, requiring additional customer protection mechanisms to be implemented by July 2025.' : 
                           'Anti-Money Laundering compliance framework has been updated with stricter reporting requirements and monitoring obligations for operators.'}
                        </Typography>
                      </CardContent>
                    </Card>
                  ))}
                  <Box sx={{ display: 'flex', justifyContent: 'center', mt: 2 }}>
                    <Button variant="outlined">
                      View All High-Impact Changes
                    </Button>
                  </Box>
                </Box>
              </Grid>
            </Grid>
          </TabPanel>
        </Paper>
      </Container>
    </LocalizationProvider>
  );
};

export default Analytics;