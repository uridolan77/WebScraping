import React from 'react';
import { Link as RouterLink, useLocation } from 'react-router-dom';
import { AppBar, Toolbar, Typography, Button, Box, Chip } from '@mui/material';
import HomeIcon from '@mui/icons-material/Home';
import SettingsIcon from '@mui/icons-material/Settings';
import DataUsageIcon from '@mui/icons-material/DataUsage';
import FolderIcon from '@mui/icons-material/Folder';

const Navbar = ({ isRunning }) => {
    const location = useLocation();

    const isActive = (path) => {
        return location.pathname === path;
    };

    return (
  
      < AppBar position = "static" >
  
        < Toolbar >
  
          < Typography variant = "h6" component = "div" sx ={ { flexGrow: 1 } }>
            Web Scraper
          </ Typography >


        {
        isRunning && (
          < Chip
            label = "Scraping in Progress"
            color = "secondary"
            sx ={ { mr: 2 } }
          />
        )}


        < Box sx ={ { display: 'flex', gap: 1 } }>
          < Button
            component ={ RouterLink}
    to = "/"
            color = "inherit"
            startIcon ={< HomeIcon />}
    variant ={ isActive('/') ? 'outlined' : 'text'}
          >
            Dashboard
          </ Button >


          < Button
            component ={ RouterLink}
    to = "/configure"
            color = "inherit"
            startIcon ={< SettingsIcon />}
    variant ={ isActive('/configure') ? 'outlined' : 'text'}
          >
            Configure
          </ Button >


          < Button
            component ={ RouterLink}
    to = "/results"
            color = "inherit"
            startIcon ={< FolderIcon />}
    variant ={ isActive('/results') ? 'outlined' : 'text'}
          >
            Results
          </ Button >
        </ Box >
      </ Toolbar >
    </ AppBar >
  );
}
;

export default Navbar;