import { useState } from 'react';
import { Box, CssBaseline } from '@mui/material';
import Sidebar from './Sidebar';
import Topbar from './Topbar';

const drawerWidth = 240;

const MainLayout = ({ children }) => {
  const [mobileOpen, setMobileOpen] = useState(false);

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  return (
    <Box sx={{ display: 'flex' }}>
      <CssBaseline />
      <Topbar 
        drawerWidth={drawerWidth} 
        handleDrawerToggle={handleDrawerToggle} 
      />
      <Sidebar 
        drawerWidth={drawerWidth} 
        mobileOpen={mobileOpen} 
        handleDrawerToggle={handleDrawerToggle} 
      />
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          width: { sm: `calc(100% - ${drawerWidth}px)` },
          ml: { sm: `${drawerWidth}px` },
          minHeight: '100vh',
          backgroundColor: theme => theme.palette.background.default,
        }}
      >
        {/* Toolbar offset */}
        <Box sx={{ height: 64 }} />
        {children}
      </Box>
    </Box>
  );
};

export default MainLayout;