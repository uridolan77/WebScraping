import React from 'react';
import { Box } from '@mui/material';

/**
 * TabPanel component for tab content
 * @param {Object} props - Component props
 * @param {React.ReactNode} props.children - The content to be displayed in the tab panel
 * @param {number} props.value - Current active tab value
 * @param {number} props.index - The index of this tab panel
 */
const TabPanel = (props) => {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`scraper-tab-${index}`}
      aria-labelledby={`scraper-tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ p: 3 }}>
          {children}
        </Box>
      )}
    </div>
  );
};

/**
 * Helper function for tab accessibility
 * @param {number} index - The index of the tab
 * @returns {Object} - Props for the tab
 */
export const a11yProps = (index) => {
  return {
    id: `scraper-tab-${index}`,
    'aria-controls': `scraper-tabpanel-${index}`,
  };
};

export default TabPanel;