import React, { createContext, useState, useEffect, useContext } from 'react';

// Create the context
const AuthContext = createContext();

// Custom hook to use the auth context
export const useAuth = () => {
  return useContext(AuthContext);
};

// Provider component
export const AuthProvider = ({ children }) => {
  const [currentUser, setCurrentUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Check if user is logged in on initial load
  useEffect(() => {
    const token = localStorage.getItem('auth_token');
    if (token) {
      // For now, we'll just set a simple user object
      // In a real app, you might want to validate the token with the server
      setCurrentUser({
        name: 'Admin User',
        email: 'admin@example.com',
        role: 'admin'
      });
    }
    setLoading(false);
  }, []);

  // Login function
  const login = async (email, password) => {
    try {
      setError(null);
      // In a real app, you would make an API call here
      // For now, we'll simulate a successful login
      const fakeToken = 'fake-jwt-token';
      localStorage.setItem('auth_token', fakeToken);
      
      setCurrentUser({
        name: 'Admin User',
        email: email,
        role: 'admin'
      });
      
      return true;
    } catch (err) {
      setError(err.message || 'Failed to login');
      return false;
    }
  };

  // Logout function
  const logout = () => {
    localStorage.removeItem('auth_token');
    setCurrentUser(null);
  };

  const value = {
    currentUser,
    login,
    logout,
    error,
    loading
  };

  return (
    <AuthContext.Provider value={value}>
      {!loading && children}
    </AuthContext.Provider>
  );
};

export default AuthContext;
