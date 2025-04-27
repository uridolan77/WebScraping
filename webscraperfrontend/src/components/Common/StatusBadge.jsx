import { Chip } from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ErrorIcon from '@mui/icons-material/Error';
import PauseIcon from '@mui/icons-material/Pause';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import HourglassEmptyIcon from '@mui/icons-material/HourglassEmpty';
import HelpOutlineIcon from '@mui/icons-material/HelpOutline';

const StatusBadge = ({ status, size = 'medium' }) => {
  // Define settings for different status types
  const statusConfig = {
    running: {
      label: 'Running',
      color: 'success',
      icon: <PlayArrowIcon fontSize="small" />
    },
    idle: {
      label: 'Idle',
      color: 'default',
      icon: <PauseIcon fontSize="small" />
    },
    completed: {
      label: 'Completed',
      color: 'primary',
      icon: <CheckCircleIcon fontSize="small" />
    },
    error: {
      label: 'Error',
      color: 'error',
      icon: <ErrorIcon fontSize="small" />
    },
    pending: {
      label: 'Pending',
      color: 'warning',
      icon: <HourglassEmptyIcon fontSize="small" />
    }
  };

  // Get the config for this status, or use a default
  const config = statusConfig[status?.toLowerCase()] || {
    label: status || 'Unknown',
    color: 'default',
    icon: <HelpOutlineIcon fontSize="small" />
  };

  return (
    <Chip
      icon={config.icon}
      label={config.label}
      color={config.color}
      size={size}
      variant="outlined"
    />
  );
};

export default StatusBadge;