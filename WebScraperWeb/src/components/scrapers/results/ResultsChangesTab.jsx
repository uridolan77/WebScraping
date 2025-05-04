import React from 'react';
import { 
  Box, Typography, Paper, Table, TableBody, TableCell, 
  TableContainer, TableHead, TableRow, Chip, Button,
  IconButton, Tooltip, CircularProgress, Alert
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  Compare as CompareIcon,
  OpenInNew as OpenInNewIcon
} from '@mui/icons-material';
import { format } from 'date-fns';

const ResultsChangesTab = ({ 
  changes, 
  loading, 
  error, 
  handleCompareItems,
  setTabValue
}) => {
  // Render change type chip
  const renderChangeTypeChip = (changeType) => {
    switch (changeType?.toLowerCase()) {
      case 'added':
        return <Chip label="Added" size="small" color="success" />;
      case 'removed':
        return <Chip label="Removed" size="small" color="error" />;
      case 'modified':
        return <Chip label="Modified" size="small" color="warning" />;
      default:
        return <Chip label={changeType || 'Unknown'} size="small" />;
    }
  };

  return (
    <>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h6">Content Changes</Typography>
        <Button
          variant="outlined"
          startIcon={loading ? <CircularProgress size={20} /> : <RefreshIcon />}
          onClick={() => setTabValue(1)} // Refresh by re-setting the tab value
          disabled={loading}
        >
          Refresh
        </Button>
      </Box>
      
      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : error ? (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      ) : changes.length > 0 ? (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>URL</TableCell>
                <TableCell>Change Type</TableCell>
                <TableCell>Detected At</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {changes.map((change, index) => (
                <TableRow key={index} hover>
                  <TableCell>
                    <Tooltip title={change.url}>
                      <Typography 
                        variant="body2" 
                        sx={{ 
                          maxWidth: 300, 
                          overflow: 'hidden', 
                          textOverflow: 'ellipsis', 
                          whiteSpace: 'nowrap' 
                        }}
                      >
                        {change.url}
                      </Typography>
                    </Tooltip>
                  </TableCell>
                  <TableCell>{renderChangeTypeChip(change.changeType)}</TableCell>
                  <TableCell>
                    {change.detectedAt ? format(new Date(change.detectedAt), 'yyyy-MM-dd HH:mm') : ''}
                  </TableCell>
                  <TableCell>
                    <Box sx={{ display: 'flex', gap: 1 }}>
                      <Tooltip title="Compare Versions">
                        <IconButton 
                          size="small"
                          onClick={() => handleCompareItems(change.previousVersion, change.currentVersion)}
                          disabled={!change.previousVersion || !change.currentVersion}
                        >
                          <CompareIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Open URL">
                        <IconButton 
                          size="small"
                          component="a"
                          href={change.url}
                          target="_blank"
                          rel="noopener noreferrer"
                        >
                          <OpenInNewIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </Box>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      ) : (
        <Alert severity="info">
          No content changes detected yet.
        </Alert>
      )}
    </>
  );
};

export default ResultsChangesTab;
