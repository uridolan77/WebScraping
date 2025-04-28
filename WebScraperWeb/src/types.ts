// Common types used throughout the application

// User type
export interface User {
  name: string;
  email: string;
  role: 'admin' | 'user' | 'viewer';
}

// Auth state type
export interface AuthState {
  currentUser: User | null;
  isAuthenticated: boolean;
  loading: boolean;
  error: string | null;
}

// Scraper type
export interface Scraper {
  id: string;
  name: string;
  url: string;
  baseUrl: string;
  description?: string;
  createdAt?: string;
  updatedAt?: string;
  isActive?: boolean;
  email?: string;
  config?: Record<string, any>;
  status?: ScraperStatus;
  urlsProcessed?: number;
  lastRun?: string;
}

// Scraper status type
export interface ScraperStatus {
  id: string;
  isRunning: boolean;
  lastRun?: string;
  nextRun?: string;
  status: 'idle' | 'running' | 'error' | 'completed';
  progress?: number;
  urlsProcessed?: number;
  urlsQueued?: number;
  errorsCount?: number;
  message?: string;
  hasErrors?: boolean;
  startTime?: string;
  currentUrl?: string;
  requestsPerMinute?: number;
  avgResponseTime?: number;
  estimatedCompletion?: string;
  memoryUsage?: number;
  errorMessage?: string;
}

// Scraper result type
export interface ScraperResult {
  id: string;
  scraperId: string;
  url: string;
  timestamp: string;
  content?: string;
  metadata?: Record<string, any>;
  changes?: ScraperChange[];
}

// Scraper change type
export interface ScraperChange {
  id: string;
  scraperId: string;
  url: string;
  timestamp: string;
  type: 'added' | 'modified' | 'removed';
  path?: string;
  oldValue?: string;
  newValue?: string;
}

// Notification type
export interface Notification {
  id: string;
  title: string;
  message: string;
  timestamp: string;
  read: boolean;
  type: 'info' | 'warning' | 'error' | 'success';
  scraperId?: string;
  url?: string;
}

// Scheduled task type
export interface ScheduledTask {
  id: string;
  scraperId: string;
  name: string;
  schedule: string;
  isActive: boolean;
  lastRun?: string;
  nextRun?: string;
  config?: Record<string, any>;
}

// System issue type
export interface SystemIssue {
  id: string;
  title: string;
  description: string;
  timestamp: string;
  severity: 'low' | 'medium' | 'high' | 'critical';
  status: 'open' | 'in_progress' | 'resolved';
  scraperId?: string;
}

// Analytics data types
export interface AnalyticsSummary {
  totalScrapers: number;
  activeScrapers: number;
  totalUrlsProcessed: number;
  totalDocumentsProcessed: number;
  totalContentChanges: number;
  avgRequestTime?: number;
  storageUsed?: number;
  totalErrors?: number;
  successRate?: number;
}

export interface TimeSeriesDataPoint {
  timestamp: string;
  value: number;
}

export interface PerformanceMetrics {
  avgRequestTime: number;
  avgRequestsPerMinute: number;
  avgMemoryUsage: number;
  successRate: number;
  timeSeriesData: TimeSeriesDataPoint[];
}

export interface ContentTypeDistribution {
  contentTypes: Array<{ name: string; value: number }>;
  domainDistribution: Array<{ name: string; value: number }>;
  sizeDistribution: Array<{ name: string; value: number }>;
}

export interface ContentChangeAnalytics {
  totalChanges: number;
  changesOverTime: Array<{
    date: string;
    added: number;
    modified: number;
    removed: number;
  }>;
}

// API response types
export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageCount: number;
  currentPage: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface ApiError {
  message: string;
  statusCode: number;
  details?: string;
}

// UI component types
export interface TableColumn {
  id: string;
  label: any; // ReactNode
  minWidth?: number;
  width?: number | string;
  maxWidth?: number;
  align?: 'left' | 'right' | 'center';
  sortable?: boolean;
  render?: (value: any, row: any) => any; // ReactNode
}
