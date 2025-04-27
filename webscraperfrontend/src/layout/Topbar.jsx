import { useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import { 
  AppBar, 
  Avatar,
  Badge,
  Box,
  IconButton, 
  Toolbar, 
  Typography, 
  Menu,
  MenuItem,
  Tooltip
} from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import NotificationsIcon from '@mui/icons-material/Notifications';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import SettingsIcon from '@mui/icons-material/Settings';
import HelpIcon from '@mui/icons-material/Help';
import WebIcon from '@mui/icons-material/Web';

const Topbar = ({ drawerWidth, onDrawerToggle }) => {
  const [notificationsAnchorEl, setNotificationsAnchorEl] = useState(null);
  const [userMenuAnchorEl, setUserMenuAnchorEl] = useState(null);

  const handleNotificationsClick = (event) => {
    setNotificationsAnchorEl(event.currentTarget);
  };
  
  const handleUserMenuClick = (event) => {
    setUserMenuAnchorEl(event.currentTarget);
  };
  
  const handleMenuClose = () => {
    setNotificationsAnchorEl(null);
    setUserMenuAnchorEl(null);
  };

  return (
    <AppBar
      position="fixed"
      sx={{
        width: { sm: `calc(100% - ${drawerWidth}px)` },
        ml: { sm: `${drawerWidth}px` },
        boxShadow: 1
      }}
    >
      <Toolbar>
        <IconButton
          color="inherit"
          aria-label="open drawer"
          edge="start"
          onClick={onDrawerToggle}
          sx={{ mr: 2, display: { sm: 'none' } }}
        >
          <MenuIcon />
        </IconButton>
        
        <RouterLink to="/" style={{ textDecoration: 'none', color: 'inherit', display: 'flex', alignItems: 'center' }}>
          <WebIcon sx={{ mr: 1 }} />
          <Typography
            variant="h6"
            noWrap
            component="div"
            sx={{ display: { xs: 'none', sm: 'block' } }}
          >
            WebScraping Dashboard
          </Typography>
        </RouterLink>
        
        <Box sx={{ flexGrow: 1 }} />
        
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          {/* Notifications */}
          <Tooltip title="Notifications">
            <IconButton 
              color="inherit" 
              onClick={handleNotificationsClick}
              size="large"
            >
              <Badge badgeContent={3} color="error">
                <NotificationsIcon />
              </Badge>
            </IconButton>
          </Tooltip>
          <Menu
            anchorEl={notificationsAnchorEl}
            open={Boolean(notificationsAnchorEl)}
            onClose={handleMenuClose}
            PaperProps={{
              sx: { width: 320, maxHeight: 400, mt: 1.5 }
            }}
            anchorOrigin={{
              vertical: 'bottom',
              horizontal: 'right',
            }}
            transformOrigin={{
              vertical: 'top',
              horizontal: 'right',
            }}
          >
            <MenuItem onClick={handleMenuClose}>Scraping job "UKGC Monitor" completed</MenuItem>
            <MenuItem onClick={handleMenuClose}>Error detected in "News Scraper"</MenuItem>
            <MenuItem onClick={handleMenuClose}>Content change detected on example.com</MenuItem>
          </Menu>
          
          {/* Help */}
          <Tooltip title="Help">
            <IconButton color="inherit" size="large">
              <HelpIcon />
            </IconButton>
          </Tooltip>
          
          {/* Settings */}
          <Tooltip title="Settings">
            <IconButton color="inherit" size="large">
              <SettingsIcon />
            </IconButton>
          </Tooltip>
          
          {/* User Menu */}
          <Tooltip title="Account">
            <IconButton 
              color="inherit"
              onClick={handleUserMenuClick}
              size="large"
              edge="end"
              sx={{ ml: 1 }}
            >
              <Avatar sx={{ width: 32, height: 32, bgcolor: 'primary.dark' }}>
                <AccountCircleIcon />
              </Avatar>
            </IconButton>
          </Tooltip>
          <Menu
            anchorEl={userMenuAnchorEl}
            open={Boolean(userMenuAnchorEl)}
            onClose={handleMenuClose}
            PaperProps={{
              sx: { width: 200, mt: 1.5 }
            }}
            anchorOrigin={{
              vertical: 'bottom',
              horizontal: 'right',
            }}
            transformOrigin={{
              vertical: 'top',
              horizontal: 'right',
            }}
          >
            <MenuItem onClick={handleMenuClose}>My Profile</MenuItem>
            <MenuItem onClick={handleMenuClose}>Account Settings</MenuItem>
            <MenuItem onClick={handleMenuClose}>API Keys</MenuItem>
            <MenuItem onClick={handleMenuClose}>Sign Out</MenuItem>
          </Menu>
        </Box>
      </Toolbar>
    </AppBar>
  );
};

export default Topbar;