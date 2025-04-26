import React, { useState } from 'react';
import { Outlet, Link as RouterLink, useLocation } from 'react-router-dom';
import {
  AppBar,
  Box,
  Toolbar,
  IconButton,
  Typography,
  Container,
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Divider,
  useTheme,
  useMediaQuery
} from '@mui/material';

// Icons
import MenuIcon from '@mui/icons-material/Menu';
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft';
import DashboardIcon from '@mui/icons-material/Dashboard';
import SettingsIcon from '@mui/icons-material/Settings';
import FolderIcon from '@mui/icons-material/Folder';
import HomeIcon from '@mui/icons-material/Home';
import WebIcon from '@mui/icons-material/Web';

const DRAWER_WIDTH = 240;

const MainLayout = () => {
  const theme = useTheme();
  const location = useLocation();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const [drawerOpen, setDrawerOpen] = useState(!isMobile);

  const toggleDrawer = () => {
    setDrawerOpen(!drawerOpen);
  };

  const closeDrawerIfMobile = () => {
    if (isMobile) {
      setDrawerOpen(false);
    }
  };

  // Navigation items
  const navItems = [
    { text: 'My Scrapers', icon: <HomeIcon />, path: '/scrapers' },
    { text: 'Configure', icon: <SettingsIcon />, path: '/configure' },
    { text: 'Results', icon: <FolderIcon />, path: '/results' },
  ];

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      {/* App Bar */}
      <AppBar 
        position="fixed" 
        sx={{ 
          zIndex: (theme) => theme.zIndex.drawer + 1,
          boxShadow: 1
        }}
      >
        <Toolbar>
          <IconButton
            color="inherit"
            edge="start"
            onClick={toggleDrawer}
            sx={{ mr: 2 }}
          >
            <MenuIcon />
          </IconButton>
          <WebIcon sx={{ mr: 1 }} />
          <Typography variant="h6" noWrap component="div" sx={{ flexGrow: 1 }}>
            Web Scraper
          </Typography>
        </Toolbar>
      </AppBar>

      {/* Side Drawer */}
      <Drawer
        variant={isMobile ? 'temporary' : 'persistent'}
        open={drawerOpen}
        onClose={toggleDrawer}
        sx={{
          width: DRAWER_WIDTH,
          flexShrink: 0,
          '& .MuiDrawer-paper': {
            width: DRAWER_WIDTH,
            boxSizing: 'border-box',
            mt: '64px',
            height: 'calc(100% - 64px)',
          },
        }}
      >
        <Box sx={{ pt: 2 }}>
          <List>
            {navItems.map((item) => (
              <ListItem key={item.text} disablePadding>
                <ListItemButton
                  component={RouterLink}
                  to={item.path}
                  selected={location.pathname === item.path}
                  onClick={closeDrawerIfMobile}
                >
                  <ListItemIcon>{item.icon}</ListItemIcon>
                  <ListItemText primary={item.text} />
                </ListItemButton>
              </ListItem>
            ))}
          </List>
        </Box>
      </Drawer>

      {/* Main Content */}
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          pt: 8,
          pl: drawerOpen && !isMobile ? `${DRAWER_WIDTH}px` : 3,
          transition: theme.transitions.create('padding', {
            easing: theme.transitions.easing.sharp,
            duration: theme.transitions.duration.leavingScreen,
          }),
        }}
      >
        <Toolbar />
        <Container maxWidth="xl" sx={{ mt: 2 }}>
          <Outlet />
        </Container>
      </Box>
    </Box>
  );
};

export default MainLayout;