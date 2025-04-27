import { Box, Button, Typography, Container } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';
import HomeIcon from '@mui/icons-material/Home';

const NotFoundPage = () => {
  return (
    <Container sx={{ 
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      justifyContent: 'center',
      height: '70vh',
      textAlign: 'center'
    }}>
      <ErrorOutlineIcon sx={{ fontSize: 100, color: 'text.secondary', mb: 2 }} />
      <Typography variant="h2" gutterBottom>
        404
      </Typography>
      <Typography variant="h4" gutterBottom>
        Page Not Found
      </Typography>
      <Typography variant="body1" color="text.secondary" paragraph sx={{ maxWidth: 500 }}>
        The page you are looking for doesn't exist or has been moved.
      </Typography>
      <Button 
        variant="contained" 
        color="primary" 
        component={RouterLink} 
        to="/"
        startIcon={<HomeIcon />}
        sx={{ mt: 2 }}
      >
        Back to Dashboard
      </Button>
    </Container>
  );
};

export default NotFoundPage;