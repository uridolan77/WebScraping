import { 
  AppBar, 
  Toolbar, 
  Typography, 
  IconButton, 
  Box,
  Button,
  useMediaQuery,
  useTheme
} from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import AddIcon from '@mui/icons-material/Add';
import { useNavigate, useLocation } from 'react-router-dom';

const Topbar = ({ drawerWidth, handleDrawerToggle }) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const navigate = useNavigate();
  const location = useLocation();

  // Determine if we should show the "Create" button
  const showCreateButton = location.pathname === '/scrapers' || location.pathname === '/schedules';
  
  // Determine the create button path and text
  const getCreateConfig = () => {
    if (location.pathname === '/scrapers') {
      return { path: '/scrapers/new', text: 'New Scraper' };
    } else if (location.pathname === '/schedules') {
      return { path: '/schedules/new', text: 'New Schedule' };
    }
    return { path: '', text: '' };
  };

  const { path: createPath, text: createText } = getCreateConfig();

  return (
    <AppBar
      position="fixed"
      sx={{
        width: { sm: `calc(100% - ${drawerWidth}px)` },
        ml: { sm: `${drawerWidth}px` },
      }}
    >
      <Toolbar>
        <IconButton
          color="inherit"
          edge="start"
          onClick={handleDrawerToggle}
          sx={{ mr: 2, display: { sm: 'none' } }}
        >
          <MenuIcon />
        </IconButton>
        
        <Typography variant="h6" noWrap component="div" sx={{ flexGrow: 1 }}>
          {/* Dynamic page title based on current route */}
          {location.pathname === '/' && 'Dashboard'}
          {location.pathname === '/scrapers' && 'Scrapers'}
          {location.pathname === '/schedules' && 'Schedules'}
          {location.pathname === '/analytics' && 'Analytics'}
          {location.pathname.includes('/scrapers/new') && 'Create Scraper'}
          {location.pathname.includes('/scrapers/') && !location.pathname.includes('/new') && !location.pathname.includes('/edit') && 'Scraper Details'}
          {location.pathname.includes('/scrapers/') && location.pathname.includes('/edit') && 'Edit Scraper'}
          {location.pathname.includes('/schedules/new') && 'Create Schedule'}
          {location.pathname.includes('/schedules/') && location.pathname.includes('/edit') && 'Edit Schedule'}
        </Typography>

        {/* Create button - only show on list pages */}
        {showCreateButton && (
          <Button 
            variant="contained" 
            color="secondary" 
            startIcon={<AddIcon />}
            onClick={() => navigate(createPath)}
          >
            {isMobile ? '' : createText}
          </Button>
        )}
      </Toolbar>
    </AppBar>
  );
};

export default Topbar;