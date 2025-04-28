import React from 'react';
import { Container } from '@mui/material';
import MonitoringDashboard from '../components/monitoring/MonitoringDashboard';

const Monitoring = () => {
  return (
    <Container maxWidth="lg">
      <MonitoringDashboard />
    </Container>
  );
};

export default Monitoring;
