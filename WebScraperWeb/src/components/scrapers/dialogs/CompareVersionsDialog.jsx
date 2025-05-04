import React from 'react';
import { 
  Dialog, DialogTitle, DialogContent, DialogActions,
  Box, Typography, Button, Grid
} from '@mui/material';

const CompareVersionsDialog = ({ open, onClose, compareItems }) => {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="lg"
      fullWidth
    >
      <DialogTitle>Compare Versions</DialogTitle>
      <DialogContent dividers>
        <Grid container spacing={2}>
          <Grid item xs={6}>
            <Typography variant="subtitle1" gutterBottom>Previous Version</Typography>
            <Box 
              component="pre" 
              sx={{ 
                p: 2, 
                bgcolor: 'grey.100', 
                borderRadius: 1, 
                overflow: 'auto',
                height: '500px',
                whiteSpace: 'pre-wrap',
                wordBreak: 'break-word'
              }}
            >
              {compareItems.old?.content || 'No previous content available'}
            </Box>
          </Grid>
          <Grid item xs={6}>
            <Typography variant="subtitle1" gutterBottom>Current Version</Typography>
            <Box 
              component="pre" 
              sx={{ 
                p: 2, 
                bgcolor: 'grey.100', 
                borderRadius: 1, 
                overflow: 'auto',
                height: '500px',
                whiteSpace: 'pre-wrap',
                wordBreak: 'break-word'
              }}
            >
              {compareItems.new?.content || 'No current content available'}
            </Box>
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
};

export default CompareVersionsDialog;
