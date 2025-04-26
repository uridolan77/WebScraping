import React from 'react';
import { Link as RouterLink, useLocation } from 'react-router-dom';
import { AppBar, Toolbar, Typography, Button, Box, IconButton, Tooltip } from '@mui/material';
import AppsIcon from '@mui/icons-material/Apps';
import MonitorHeartIcon from '@mui/icons-material/MonitorHeart';
import AddIcon from '@mui/icons-material/Add';
import FolderIcon from '@mui/icons-material/Folder';
import GitHubIcon from '@mui/icons-material/GitHub';

const Navbar = () => {
  const location = useLocation();

  const isActive = (pathPrefix) => {
    const path = location.pathname;
    return path === pathPrefix || path.startsWith(`${pathPrefix}/`);
  };

  return (
    <AppBar position="static">
      <Toolbar>
        <Typography variant="h6" component={RouterLink} to="/" sx={{ 
          flexGrow: 0, 
          mr: 2,
          textDecoration: 'none',
          color: 'inherit'
        }}>
          Web Scraper
        </Typography>
        
        <Box sx={{ display: 'flex', gap: 1, flexGrow: 1 }}>
          <Button
            component={RouterLink}
            to="/scrapers"
            color="inherit"
            startIcon={<AppsIcon />}
            variant={isActive('/scrapers') || isActive('/dashboard') ? 'outlined' : 'text'}
          >
            My Scrapers
          </Button>
          
          <Button
            component={RouterLink}
            to="/results"
            color="inherit"
            startIcon={<FolderIcon />}
            variant={isActive('/results') ? 'outlined' : 'text'}
          >
            Results
          </Button>
        </Box>
        
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Tooltip title="Create New Scraper">
            <Button
              component={RouterLink}
              to="/configure"
              color="inherit"
              startIcon={<AddIcon />}
              variant="text"
              sx={{ 
                border: '1px solid rgba(255, 255, 255, 0.3)',
                '&:hover': {
                  backgroundColor: 'rgba(255, 255, 255, 0.1)',
                  border: '1px solid rgba(255, 255, 255, 0.6)'
                }
              }}
            >
              New Scraper
            </Button>
          </Tooltip>
          
          <Tooltip title="View on GitHub">
            <IconButton
              color="inherit"
              component="a"
              href="https://github.com/yourusername/web-scraper"
              target="_blank"
              rel="noopener noreferrer"
            >
              <GitHubIcon />
            </IconButton>
          </Tooltip>
        </Box>
      </Toolbar>
    </AppBar>
  );
};

export default Navbar;