import React from 'react';
import {
  Paper,
  Typography,
  Box,
  Grid,
  Chip,
  Link
} from '@mui/material';
import PublicIcon from '@mui/icons-material/Public';
import FilterAltIcon from '@mui/icons-material/FilterAlt';
import LinkIcon from '@mui/icons-material/Link';
import SettingsIcon from '@mui/icons-material/Settings';
import DataObjectIcon from '@mui/icons-material/DataObject';
import LowPriorityIcon from '@mui/icons-material/LowPriority';
import TimerIcon from '@mui/icons-material/Timer';

/**
 * Component to display a summary of scraper configuration
 */
const ConfigSummary = ({ scraperConfig }) => {
  if (!scraperConfig) return null;
  
  // Helper function to truncate long strings
  const truncate = (str, maxLength = 40) => {
    if (!str) return '';
    return str.length > maxLength ? `${str.substring(0, maxLength)}...` : str;
  };
  
  return (
    <Paper sx={{ p: 2, mt: 2 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
        <SettingsIcon sx={{ mr: 1, color: 'text.secondary' }} />
        <Typography variant="h6">
          Configuration Summary
        </Typography>
      </Box>
      
      <Grid container spacing={2}>
        <Grid item xs={12}>
          <Box sx={{ display: 'flex', alignItems: 'flex-start', mb: 1 }}>
            <PublicIcon fontSize="small" sx={{ mr: 1, mt: 0.5, color: 'primary.main' }} />
            <Box>
              <Typography variant="body2" sx={{ fontWeight: 'bold' }}>
                Start URL:
              </Typography>
              <Link href={scraperConfig.startUrl} target="_blank" rel="noopener noreferrer" sx={{ wordBreak: 'break-all' }}>
                {scraperConfig.startUrl}
              </Link>
            </Box>
          </Box>
        </Grid>
        
        <Grid item xs={12} sm={6}>
          <Box sx={{ display: 'flex', alignItems: 'flex-start', mb: 1 }}>
            <FilterAltIcon fontSize="small" sx={{ mr: 1, mt: 0.5, color: 'success.main' }} />
            <Box>
              <Typography variant="body2" sx={{ fontWeight: 'bold' }}>
                URL Filtering:
              </Typography>
              <Typography variant="body2">
                {scraperConfig.urlPattern ? (
                  <>
                    Pattern: <code>{truncate(scraperConfig.urlPattern)}</code>
                  </>
                ) : (
                  'No URL filtering configured'
                )}
              </Typography>
            </Box>
          </Box>
        </Grid>
        
        <Grid item xs={12} sm={6}>
          <Box sx={{ display: 'flex', alignItems: 'flex-start', mb: 1 }}>
            <LinkIcon fontSize="small" sx={{ mr: 1, mt: 0.5, color: 'secondary.main' }} />
            <Box>
              <Typography variant="body2" sx={{ fontWeight: 'bold' }}>
                Link Extraction:
              </Typography>
              <Typography variant="body2">
                {scraperConfig.linkSelector ? (
                  <>
                    Selector: <code>{truncate(scraperConfig.linkSelector)}</code>
                  </>
                ) : (
                  'Using default link extraction'
                )}
              </Typography>
            </Box>
          </Box>
        </Grid>
        
        <Grid item xs={12} sm={6}>
          <Box sx={{ display: 'flex', alignItems: 'flex-start', mb: 1 }}>
            <DataObjectIcon fontSize="small" sx={{ mr: 1, mt: 0.5, color: 'info.main' }} />
            <Box>
              <Typography variant="body2" sx={{ fontWeight: 'bold' }}>
                Data Extraction:
              </Typography>
              <Typography variant="body2">
                {scraperConfig.contentSelectors && Object.keys(scraperConfig.contentSelectors).length > 0 ? (
                  `${Object.keys(scraperConfig.contentSelectors).length} selector(s) defined`
                ) : (
                  'No content selectors defined'
                )}
              </Typography>
            </Box>
          </Box>
        </Grid>
        
        <Grid item xs={12} sm={6}>
          <Box sx={{ display: 'flex', alignItems: 'flex-start', mb: 1 }}>
            <LowPriorityIcon fontSize="small" sx={{ mr: 1, mt: 0.5, color: 'warning.main' }} />
            <Box>
              <Typography variant="body2" sx={{ fontWeight: 'bold' }}>
                Crawling Strategy:
              </Typography>
              <Typography variant="body2">
                {scraperConfig.crawlStrategy || 'Default'}
              </Typography>
            </Box>
          </Box>
        </Grid>
        
        <Grid item xs={12}>
          <Box sx={{ display: 'flex', alignItems: 'center', flexWrap: 'wrap', mt: 2, gap: 1 }}>
            <TimerIcon fontSize="small" color="action" />
            <Typography variant="body2" sx={{ fontWeight: 'bold', mr: 1 }}>
              Rate Limiting:
            </Typography>
            
            {scraperConfig.requestDelayMs > 0 && (
              <Chip 
                size="small" 
                label={`${scraperConfig.requestDelayMs}ms delay`} 
                variant="outlined" 
              />
            )}
            
            {scraperConfig.maxConcurrentRequests > 0 && (
              <Chip 
                size="small" 
                label={`${scraperConfig.maxConcurrentRequests} concurrent requests`} 
                variant="outlined"
              />
            )}
            
            {scraperConfig.maxDepth > 0 && (
              <Chip 
                size="small" 
                label={`Max depth: ${scraperConfig.maxDepth}`} 
                variant="outlined"
              />
            )}
            
            {scraperConfig.maxPages > 0 && (
              <Chip 
                size="small" 
                label={`Max pages: ${scraperConfig.maxPages}`} 
                variant="outlined"
              />
            )}
            
            {scraperConfig.respectRobotsTxt && (
              <Chip 
                size="small" 
                label="Respects robots.txt" 
                color="success" 
                variant="outlined" 
              />
            )}
          </Box>
        </Grid>
      </Grid>
    </Paper>
  );
};

export default ConfigSummary;