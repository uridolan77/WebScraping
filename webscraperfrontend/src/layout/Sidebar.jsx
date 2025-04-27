import { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import {
  Box,
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Typography,
  Collapse,
  Divider
} from '@mui/material';
import DashboardIcon from '@mui/icons-material/Dashboard';
import WebIcon from '@mui/icons-material/Web';
import ScheduleIcon from '@mui/icons-material/Schedule';
import BarChartIcon from '@mui/icons-material/BarChart';
import NotificationsIcon from '@mui/icons-material/Notifications';
import SettingsIcon from '@mui/icons-material/Settings';
import HelpIcon from '@mui/icons-material/Help';
import CodeIcon from '@mui/icons-material/Code';
import AddIcon from '@mui/icons-material/Add';
import ExpandLess from '@mui/icons-material/ExpandLess';
import ExpandMore from '@mui/icons-material/ExpandMore';

const Sidebar = ({ drawerWidth, mobileOpen, onDrawerToggle }) => {
  const location = useLocation();
  const navigate = useNavigate();
  const [open, setOpen] = useState({
    scrapers: true,
    schedules: false
  });

  const handleClick = (section) => {
    setOpen({
      ...open,
      [section]: !open[section]
    });
  };

  const isActive = (path) => {
    return location.pathname === path || location.pathname.startsWith(`${path}/`);
  };

  // Define navigation items
  const mainNavItems = [
    {
      text: 'Dashboard',
      icon: <DashboardIcon />,
      path: '/'
    },
    {
      text: 'Scrapers',
      icon: <WebIcon />,
      path: '/scrapers',
      expandable: true,
      open: open.scrapers,
      children: [
        {
          text: 'All Scrapers',
          path: '/scrapers'
        },
        {
          text: 'Add Scraper',
          path: '/scrapers/new',
          icon: <AddIcon fontSize="small" />
        }
      ]
    },
    {
      text: 'Schedules',
      icon: <ScheduleIcon />,
      path: '/schedules',
      expandable: true,
      open: open.schedules,
      children: [
        {
          text: 'All Schedules',
          path: '/schedules'
        },
        {
          text: 'Add Schedule',
          path: '/schedules/new',
          icon: <AddIcon fontSize="small" />
        }
      ]
    },
    {
      text: 'Analytics',
      icon: <BarChartIcon />,
      path: '/analytics'
    }
  ];

  const secondaryNavItems = [
    {
      text: 'Notifications',
      icon: <NotificationsIcon />,
      path: '/notifications'
    },
    {
      text: 'Settings',
      icon: <SettingsIcon />,
      path: '/settings'
    },
    {
      text: 'API Documentation',
      icon: <CodeIcon />,
      path: '/api-docs'
    },
    {
      text: 'Help & Support',
      icon: <HelpIcon />,
      path: '/help'
    }
  ];

  const drawer = (
    <div>
      <Toolbar sx={{ 
        display: 'flex', 
        alignItems: 'center', 
        justifyContent: 'center',
        borderBottom: '1px solid rgba(0, 0, 0, 0.12)'
      }}>
        <WebIcon color="primary" sx={{ mr: 1 }} />
        <Typography variant="h6" color="primary" fontWeight={600}>
          Web Scraper
        </Typography>
      </Toolbar>

      <List>
        {mainNavItems.map((item) => (
          <Box key={item.text}>
            <ListItem disablePadding>
              <ListItemButton
                onClick={() => {
                  if (item.expandable) {
                    handleClick(item.text.toLowerCase());
                  } else {
                    navigate(item.path);
                  }
                }}
                selected={isActive(item.path) && !item.expandable}
                sx={{
                  '&.Mui-selected': {
                    backgroundColor: 'primary.light',
                    color: 'primary.contrastText',
                    '&:hover': {
                      backgroundColor: 'primary.main',
                    },
                    '& .MuiListItemIcon-root': {
                      color: 'primary.contrastText',
                    },
                  },
                }}
              >
                <ListItemIcon sx={{ 
                  minWidth: 40,
                  color: isActive(item.path) ? 'primary.main' : 'inherit',
                }}>
                  {item.icon}
                </ListItemIcon>
                <ListItemText primary={item.text} />
                {item.expandable && (
                  item.open ? <ExpandLess /> : <ExpandMore />
                )}
              </ListItemButton>
            </ListItem>

            {item.expandable && (
              <Collapse in={item.open} timeout="auto" unmountOnExit>
                <List component="div" disablePadding>
                  {item.children.map((child) => (
                    <ListItemButton
                      key={child.text}
                      sx={{ pl: 4 }}
                      selected={isActive(child.path)}
                      onClick={() => navigate(child.path)}
                    >
                      {child.icon && (
                        <ListItemIcon sx={{ minWidth: 36 }}>
                          {child.icon}
                        </ListItemIcon>
                      )}
                      <ListItemText primary={child.text} />
                    </ListItemButton>
                  ))}
                </List>
              </Collapse>
            )}
          </Box>
        ))}
      </List>

      <Divider sx={{ mt: 2, mb: 2 }} />

      <List>
        {secondaryNavItems.map((item) => (
          <ListItem key={item.text} disablePadding>
            <ListItemButton
              onClick={() => navigate(item.path)}
              selected={isActive(item.path)}
            >
              <ListItemIcon sx={{ minWidth: 40 }}>
                {item.icon}
              </ListItemIcon>
              <ListItemText primary={item.text} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>
    </div>
  );

  return (
    <Box
      component="nav"
      sx={{ width: { sm: drawerWidth }, flexShrink: { sm: 0 } }}
      aria-label="navigation menu"
    >
      {/* Mobile drawer */}
      <Drawer
        variant="temporary"
        open={mobileOpen}
        onClose={onDrawerToggle}
        ModalProps={{
          keepMounted: true, // Better open performance on mobile
        }}
        sx={{
          display: { xs: 'block', sm: 'none' },
          '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth },
        }}
      >
        {drawer}
      </Drawer>
      
      {/* Desktop drawer */}
      <Drawer
        variant="permanent"
        sx={{
          display: { xs: 'none', sm: 'block' },
          '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth },
        }}
        open
      >
        {drawer}
      </Drawer>
    </Box>
  );
};

export default Sidebar;