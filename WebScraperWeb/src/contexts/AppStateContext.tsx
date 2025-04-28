import React, { createContext, useContext, useState, useCallback, ReactNode } from 'react';

// Define the app state interface
interface AppState {
  sidebarOpen: boolean;
  notifications: Notification[];
  unreadNotificationsCount: number;
  lastRefreshTime: Date | null;
}

// Define notification interface
interface Notification {
  id: string;
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
  timestamp: Date;
  read: boolean;
}

// Define the context interface
interface AppStateContextType extends AppState {
  toggleSidebar: () => void;
  setSidebarOpen: (open: boolean) => void;
  addNotification: (notification: Omit<Notification, 'id' | 'timestamp' | 'read'>) => void;
  markNotificationAsRead: (id: string) => void;
  markAllNotificationsAsRead: () => void;
  clearNotifications: () => void;
  removeNotification: (id: string) => void;
  setLastRefreshTime: (time: Date) => void;
}

// Create the context with default values
const AppStateContext = createContext<AppStateContextType>({
  sidebarOpen: true,
  notifications: [],
  unreadNotificationsCount: 0,
  lastRefreshTime: null,
  toggleSidebar: () => {},
  setSidebarOpen: () => {},
  addNotification: () => {},
  markNotificationAsRead: () => {},
  markAllNotificationsAsRead: () => {},
  clearNotifications: () => {},
  removeNotification: () => {},
  setLastRefreshTime: () => {}
});

// Custom hook to use the app state context
export const useAppState = () => useContext(AppStateContext);

interface AppStateProviderProps {
  children: ReactNode;
}

// Provider component
export const AppStateProvider: React.FC<AppStateProviderProps> = ({ children }) => {
  // Initialize state
  const [sidebarOpen, setSidebarOpen] = useState<boolean>(true);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [lastRefreshTime, setLastRefreshTime] = useState<Date | null>(null);

  // Calculate unread notifications count
  const unreadNotificationsCount = notifications.filter(n => !n.read).length;

  // Toggle sidebar
  const toggleSidebar = useCallback(() => {
    setSidebarOpen(prev => !prev);
  }, []);

  // Add a new notification
  const addNotification = useCallback((notification: Omit<Notification, 'id' | 'timestamp' | 'read'>) => {
    const newNotification: Notification = {
      ...notification,
      id: Math.random().toString(36).substring(2, 11),
      timestamp: new Date(),
      read: false
    };
    
    setNotifications(prev => [newNotification, ...prev]);
  }, []);

  // Mark a notification as read
  const markNotificationAsRead = useCallback((id: string) => {
    setNotifications(prev => 
      prev.map(notification => 
        notification.id === id 
          ? { ...notification, read: true } 
          : notification
      )
    );
  }, []);

  // Mark all notifications as read
  const markAllNotificationsAsRead = useCallback(() => {
    setNotifications(prev => 
      prev.map(notification => ({ ...notification, read: true }))
    );
  }, []);

  // Clear all notifications
  const clearNotifications = useCallback(() => {
    setNotifications([]);
  }, []);

  // Remove a specific notification
  const removeNotification = useCallback((id: string) => {
    setNotifications(prev => prev.filter(notification => notification.id !== id));
  }, []);

  // Update last refresh time
  const handleSetLastRefreshTime = useCallback((time: Date) => {
    setLastRefreshTime(time);
  }, []);

  // Create the context value
  const value: AppStateContextType = {
    sidebarOpen,
    notifications,
    unreadNotificationsCount,
    lastRefreshTime,
    toggleSidebar,
    setSidebarOpen,
    addNotification,
    markNotificationAsRead,
    markAllNotificationsAsRead,
    clearNotifications,
    removeNotification,
    setLastRefreshTime: handleSetLastRefreshTime
  };

  return (
    <AppStateContext.Provider value={value}>
      {children}
    </AppStateContext.Provider>
  );
};

export default AppStateContext;
