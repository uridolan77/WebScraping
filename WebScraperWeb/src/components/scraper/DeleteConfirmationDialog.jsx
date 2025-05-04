import React from 'react';
import { Box, Paper, Typography, Button, CircularProgress } from '@mui/material';

/**
 * Component for the delete confirmation dialog
 */
const DeleteConfirmationDialog = ({ 
  open, 
  onClose, 
  onConfirm, 
  isDeleting 
}) => {
  return (
    <Box
      component="div"
      sx={{
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        backgroundColor: 'rgba(0, 0, 0, 0.5)',
        display: open ? 'flex' : 'none',
        alignItems: 'center',
        justifyContent: 'center',
        zIndex: 9999,
      }}
      onClick={() => !isDeleting && onClose()}
    >
      <Paper
        sx={{
          p: 3,
          width: '100%',
          maxWidth: 500,
          mx: 2,
        }}
        onClick={(e) => e.stopPropagation()}
      >
        <Typography variant="h6" gutterBottom>
          Delete Scraper
        </Typography>

        <Typography variant="body1" sx={{ mb: 3 }}>
          Are you sure you want to delete this scraper? This action cannot be undone.
        </Typography>

        <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
          <Button
            variant="outlined"
            onClick={onClose}
            disabled={isDeleting}
          >
            Cancel
          </Button>

          <Button
            variant="contained"
            color="error"
            onClick={onConfirm}
            disabled={isDeleting}
          >
            {isDeleting ? (
              <CircularProgress size={24} color="inherit" />
            ) : (
              'Delete'
            )}
          </Button>
        </Box>
      </Paper>
    </Box>
  );
};

export default DeleteConfirmationDialog;