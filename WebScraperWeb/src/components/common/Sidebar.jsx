import React from 'react';
import {
  Drawer,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Divider,
  Toolbar,
  Box,
  Typography,
  ListSubheader
} from '@mui/material';
import {
  Dashboard as DashboardIcon,
  Web as WebIcon,
  Analytics as AnalyticsIcon,
  Speed as MonitoringIcon,
  Schedule as ScheduleIcon,
  Notifications as NotificationsIcon,
  Settings as SettingsIcon,
  Add as AddIcon
} from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';

const drawerWidth = 240;

// Main menu items without the nested items
const menuItems = [
  { text: 'Dashboard', icon: <DashboardIcon />, path: '/' },
  // Scrapers is handled separately below
  { text: 'Monitoring', icon: <MonitoringIcon />, path: '/monitoring' },
  { text: 'Analytics', icon: <AnalyticsIcon />, path: '/analytics' },
  { text: 'Scheduling', icon: <ScheduleIcon />, path: '/scheduling' },
  { text: 'Notifications', icon: <NotificationsIcon />, path: '/notifications' },
  { text: 'Settings', icon: <SettingsIcon />, path: '/settings' }
];

// Scraper related items
const scraperItems = [
  { text: 'All Scrapers', icon: <WebIcon />, path: '/scrapers' },
  { text: 'Create New Scraper', icon: <AddIcon />, path: '/scrapers/create' }
];

const Sidebar = ({ open }) => {
  const navigate = useNavigate();
  const location = useLocation();

  const handleNavigation = (path) => {
    navigate(path);
  };

  // Check if the current path is a scraper-related path
  const isScraperSection = location.pathname.startsWith('/scrapers');

  return (
    <Drawer
      variant="permanent"
      sx={{
        width: drawerWidth,
        flexShrink: 0,
        [`& .MuiDrawer-paper`]: {
          width: drawerWidth,
          boxSizing: 'border-box',
          transform: open ? 'translateX(0)' : 'translateX(-100%)',
          transition: (theme) => theme.transitions.create('transform', {
            easing: theme.transitions.easing.sharp,
            duration: theme.transitions.duration.enteringScreen,
          }),
        },
      }}
      open={open}
    >
      <Toolbar />
      <Box sx={{ overflow: 'auto' }}>
        <Box sx={{ p: 2 }}>
          <Typography variant="h6" component="div">
            WebScraper
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Regulatory Content Management
          </Typography>
        </Box>
        <Divider />
        <List>
          {/* Dashboard item */}
          <ListItem
            button
            onClick={() => handleNavigation('/')}
            selected={location.pathname === '/'}
            sx={{
              '&.Mui-selected': {
                backgroundColor: 'primary.light',
                '&:hover': {
                  backgroundColor: 'primary.light',
                },
              },
            }}
          >
            <ListItemIcon>
              <DashboardIcon />
            </ListItemIcon>
            <ListItemText primary="Dashboard" />
          </ListItem>
          
          {/* Scrapers section */}
          <List
            component="div"
            subheader={
              <ListSubheader component="div" id="scrapers-subheader">
                Scrapers
              </ListSubheader>
            }
            disablePadding
          >
            {scraperItems.map((item) => (
              <ListItem
                button
                key={item.text}
                onClick={() => handleNavigation(item.path)}
                selected={location.pathname === item.path}
                sx={{
                  pl: 4,
                  '&.Mui-selected': {
                    backgroundColor: 'primary.light',
                    '&:hover': {
                      backgroundColor: 'primary.light',
                    },
                  },
                }}
              >
                <ListItemIcon>
                  {item.icon}
                </ListItemIcon>
                <ListItemText primary={item.text} />
              </ListItem>
            ))}
          </List>
          
          {/* Remaining main menu items */}
          {menuItems.slice(1).map((item) => (
            <ListItem
              button
              key={item.text}
              onClick={() => handleNavigation(item.path)}
              selected={location.pathname === item.path}
              sx={{
                '&.Mui-selected': {
                  backgroundColor: 'primary.light',
                  '&:hover': {
                    backgroundColor: 'primary.light',
                  },
                },
              }}
            >
              <ListItemIcon>
                {item.icon}
              </ListItemIcon>
              <ListItemText primary={item.text} />
            </ListItem>
          ))}
        </List>
      </Box>
    </Drawer>
  );
};

export default Sidebar;
