import { Alert, AlertTitle, Box, Button } from '@mui/material';
import RefreshIcon from '@mui/icons-material/Refresh';

const ErrorMessage = ({
  title = 'Error',
  message,
  onRetry = null,
}) => {
  return (
    <Box sx={{ my: 2 }}>
      <Alert 
        severity="error"
        action={
          onRetry && (
            <Button 
              color="inherit" 
              size="small" 
              startIcon={<RefreshIcon />}
              onClick={onRetry}
            >
              Retry
            </Button>
          )
        }
      >
        <AlertTitle>{title}</AlertTitle>
        {message || 'An unexpected error occurred.'}
      </Alert>
    </Box>
  );
};

export default ErrorMessage;