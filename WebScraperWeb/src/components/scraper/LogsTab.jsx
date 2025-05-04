import React from 'react';
import { 
  Box, 
  Button, 
  Card, 
  CardContent, 
  Typography, 
  CircularProgress 
} from '@mui/material';
import { Refresh as RefreshIcon } from '@mui/icons-material';

/**
 * Component for the Logs tab in the scraper details page
 */
const LogsTab = ({ logs, isLogsLoading, isActionInProgress, onRefreshLogs }) => {
  return (
    <>
      <Box sx={{ mb: 2, display: 'flex', justifyContent: 'flex-end' }}>
        <Button
          variant="outlined"
          startIcon={<RefreshIcon />}
          onClick={onRefreshLogs}
          disabled={isActionInProgress || isLogsLoading}
        >
          Refresh Logs
        </Button>
      </Box>

      <Card>
        <CardContent>
          {isLogsLoading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
              <CircularProgress />
            </Box>
          ) : (
            <Box sx={{ maxHeight: 500, overflow: 'auto' }}>
              {logs && logs.length > 0 ? (
                logs.map((log, index) => (
                  <Box
                    key={index}
                    sx={{
                      mb: 1,
                      p: 1,
                      bgcolor: 'background.default',
                      borderRadius: 1,
                      borderLeft: 4,
                      borderColor:
                        log.level === 'error' ? 'error.main' :
                        log.level === 'warning' ? 'warning.main' :
                        log.level === 'info' ? 'info.main' :
                        'grey.400'
                    }}
                  >
                    <Typography variant="body2" color="textSecondary">
                      {new Date(log.timestamp).toLocaleString()} - {log.level?.toUpperCase() || 'INFO'}
                    </Typography>
                    <Typography variant="body2">
                      {log.message}
                    </Typography>
                    {log.source && (
                      <Typography variant="caption" color="textSecondary">
                        Source: {log.source}
                      </Typography>
                    )}
                  </Box>
                ))
              ) : (
                <Typography variant="body1" sx={{ p: 2, textAlign: 'center' }}>
                  No logs available
                </Typography>
              )}
            </Box>
          )}
        </CardContent>
      </Card>
    </>
  );
};

export default LogsTab;