import { Box, Typography, Breadcrumbs, Link } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';
import NavigateNextIcon from '@mui/icons-material/NavigateNext';

const PageHeader = ({ 
  title, 
  subtitle = null, 
  breadcrumbs = [],
  action = null 
}) => {
  return (
    <Box sx={{ mb: 4 }}>
      {breadcrumbs.length > 0 && (
        <Breadcrumbs 
          separator={<NavigateNextIcon fontSize="small" />} 
          aria-label="breadcrumb"
          sx={{ mb: 2 }}
        >
          {breadcrumbs.map((crumb, index) => {
            const isLast = index === breadcrumbs.length - 1;
            
            return isLast ? (
              <Typography key={index} color="text.primary">
                {crumb.label}
              </Typography>
            ) : (
              <Link 
                key={index} 
                component={RouterLink} 
                to={crumb.path} 
                underline="hover"
                color="inherit"
              >
                {crumb.label}
              </Link>
            );
          })}
        </Breadcrumbs>
      )}
      
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Box>
          <Typography variant="h4" component="h1" gutterBottom={!!subtitle}>
            {title}
          </Typography>
          
          {subtitle && (
            <Typography variant="subtitle1" color="text.secondary">
              {subtitle}
            </Typography>
          )}
        </Box>
        
        {action && (
          <Box>
            {action}
          </Box>
        )}
      </Box>
    </Box>
  );
};

export default PageHeader;