import React from 'react';
import { 
  Dialog, DialogTitle, DialogContent, DialogActions,
  Box, Typography, Button, IconButton, Tooltip
} from '@mui/material';
import {
  ContentCopy as CopyIcon,
  OpenInNew as OpenInNewIcon,
  GetApp as DownloadIcon
} from '@mui/icons-material';
import { extractFilename } from '../../../utils/urlUtils';

const ViewItemDialog = ({ open, onClose, selectedItem }) => {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="md"
      fullWidth
    >
      <DialogTitle>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Typography variant="h6">
            {selectedItem?.url ? extractFilename(selectedItem.url) || 'Content View' : 'Content View'}
          </Typography>
          <Box>
            <Tooltip title="Copy URL">
              <IconButton 
                size="small"
                onClick={() => {
                  if (selectedItem?.url) {
                    navigator.clipboard.writeText(selectedItem.url);
                  }
                }}
              >
                <CopyIcon fontSize="small" />
              </IconButton>
            </Tooltip>
            <Tooltip title="Open in New Tab">
              <IconButton 
                size="small"
                component="a"
                href={selectedItem?.url}
                target="_blank"
                rel="noopener noreferrer"
              >
                <OpenInNewIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          </Box>
        </Box>
      </DialogTitle>
      <DialogContent dividers>
        {selectedItem?.contentType?.toLowerCase() === 'html' ? (
          <iframe
            src={selectedItem.url}
            title="Content Preview"
            width="100%"
            height="500px"
            style={{ border: 'none' }}
          />
        ) : selectedItem?.contentType?.toLowerCase() === 'pdf' ? (
          <embed
            src={selectedItem.url}
            type="application/pdf"
            width="100%"
            height="500px"
          />
        ) : (
          <Box 
            component="pre" 
            sx={{ 
              p: 2, 
              bgcolor: 'grey.100', 
              borderRadius: 1, 
              overflow: 'auto',
              maxHeight: '500px',
              whiteSpace: 'pre-wrap',
              wordBreak: 'break-word'
            }}
          >
            {selectedItem?.content || 'No content available'}
          </Box>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
        {selectedItem?.downloadUrl && (
          <Button 
            component="a"
            href={selectedItem.downloadUrl}
            download
            startIcon={<DownloadIcon />}
          >
            Download
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
};

export default ViewItemDialog;
