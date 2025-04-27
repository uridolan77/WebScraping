import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import { Box, Toolbar } from '@mui/material';
import Sidebar from './Sidebar';
import Topbar from './Topbar';

const drawerWidth = 240;

const MainLayout = () => {
  const [mobileOpen, setMobileOpen] = useState(false);
  
  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };
  
  return (
    <Box sx={{ display: 'flex' }}>
      <Topbar 
        drawerWidth={drawerWidth} 
        onDrawerToggle={handleDrawerToggle}
      />
      
      <Sidebar 
        drawerWidth={drawerWidth} 
        mobileOpen={mobileOpen} 
        onDrawerToggle={handleDrawerToggle}
      />
      
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          width: { sm: `calc(100% - ${drawerWidth}px)` },
          minHeight: '100vh',
          backgroundColor: 'background.default'
        }}
      >
        <Toolbar /> {/* This provides spacing below the fixed app bar */}
        <Outlet /> {/* This renders the current route's element */}
      </Box>
    </Box>
  );
};

export default MainLayout;