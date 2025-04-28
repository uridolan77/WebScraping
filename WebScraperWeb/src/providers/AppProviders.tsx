import React, { ReactNode } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { CssBaseline } from '@mui/material';
import QueryProvider from './QueryProvider';
import ThemeProvider from './ThemeProvider';
import AuthProvider from './AuthProvider';
import AppStateProvider from './AppStateProvider';

interface AppProvidersProps {
  children: ReactNode;
}

export const AppProviders: React.FC<AppProvidersProps> = ({ children }) => {
  return (
    <QueryProvider>
      <ThemeProvider>
        <CssBaseline />
        <AuthProvider>
          <AppStateProvider>
            <BrowserRouter>
              {children}
            </BrowserRouter>
          </AppStateProvider>
        </AuthProvider>
      </ThemeProvider>
    </QueryProvider>
  );
};

export default AppProviders;
