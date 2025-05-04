import React from 'react';
import { Chip } from '@mui/material';

/**
 * Status chip component that displays a colored chip based on the scraper status
 * @param {Object} props - Component props
 * @param {Object} props.status - The status object containing isRunning and hasErrors properties
 */
const StatusChip = ({ status }) => {
  if (!status) return <Chip label="Unknown" color="default" />;

  if (status.isRunning) {
    return <Chip label="Running" color="success" />;
  } else if (status.hasErrors) {
    return <Chip label="Error" color="error" />;
  } else {
    return <Chip label="Idle" color="default" />;
  }
};

export default StatusChip;