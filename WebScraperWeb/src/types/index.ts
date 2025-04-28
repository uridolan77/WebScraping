// Common types used throughout the application

// User related types
export interface User {
  id?: string;
  name: string;
  email: string;
  role: string;
}

export interface AuthState {
  currentUser: User | null;
  isAuthenticated: boolean;
  loading: boolean;
  error: string | null;
}

// Scraper related types
export interface Scraper {
  id: string;
  name: string;
  startUrl: string;
  baseUrl: string;
  createdAt: string;
  lastRun?: string;
  outputDirectory: string;
  delayBetweenRequests: number;
  maxConcurrentRequests: number;
  maxDepth: number;
  followExternalLinks: boolean;
  respectRobotsTxt: boolean;
  enableChangeDetection: boolean;
  trackContentVersions: boolean;
  maxVersionsToKeep: number;
  enableAdaptiveCrawling: boolean;
  autoLearnHeaderFooter: boolean;
  learningPagesCount: number;
  [key: string]: any; // For additional properties
}

export interface ScraperStatus {
  id: string;
  isRunning: boolean;
  hasErrors: boolean;
  lastRunStatus?: string;
  progress?: number;
  pagesProcessed?: number;
  errorsCount?: number;
  startTime?: string;
  endTime?: string;
  [key: string]: any; // For additional properties
}

export interface ScraperState {
  scrapers: Scraper[];
  selectedScraper: Scraper | null;
  scraperStatus: Record<string, ScraperStatus>;
  loading: boolean;
  error: string | null;
}

// Analytics related types
export interface AnalyticsData {
  id?: string;
  timeframe?: string;
  totalPages?: number;
  totalChanges?: number;
  scraperPerformance?: ScraperPerformance[];
  [key: string]: any; // For additional properties
}

export interface ScraperPerformance {
  id: string;
  name: string;
  pagesScraped: number;
  changesDetected: number;
  averageProcessingTime: number;
  [key: string]: any; // For additional properties
}

export interface ContentChange {
  id: string;
  url: string;
  changeType: string;
  detectedAt: string;
  significance: number;
  changeDetails?: string;
  [key: string]: any; // For additional properties
}

export interface PerformanceMetric {
  id?: string;
  avgCrawlTime: number;
  avgProcessingTime: number;
  avgMemoryUsage: number;
  totalErrors: number;
  [key: string]: any; // For additional properties
}

export interface ContentTypeDistribution {
  id?: string;
  breakdown: ContentTypeBreakdown[];
  [key: string]: any; // For additional properties
}

export interface ContentTypeBreakdown {
  type: string;
  count: number;
  percentage: number;
  [key: string]: any; // For additional properties
}

// Scheduling related types
export interface ScheduledTask {
  id: string;
  name: string;
  scraperId: string;
  scraperName: string;
  schedule: string; // Cron expression
  status: 'active' | 'paused';
  nextRun?: string;
  lastRun?: string;
  createdAt: string;
  [key: string]: any; // For additional properties
}

// Monitoring related types
export interface SystemHealth {
  status: 'healthy' | 'degraded' | 'unhealthy';
  cpuUsage: number;
  memoryUsage: number;
  diskUsage: number;
  uptime: number;
  [key: string]: any; // For additional properties
}

export interface ServiceStatus {
  id: string;
  name: string;
  status: 'running' | 'stopped' | 'error';
  lastChecked: string;
  [key: string]: any; // For additional properties
}

export interface SystemIssue {
  id: string;
  type: 'error' | 'warning' | 'info';
  message: string;
  timestamp: string;
  resolved: boolean;
  [key: string]: any; // For additional properties
}

// API related types
export interface ApiResponse<T> {
  data: T;
  status: number;
  message?: string;
  pagination?: PaginationInfo;
}

export interface PaginationInfo {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ApiError {
  message: string;
  status: number;
  errors?: Record<string, string[]>;
}

// UI related types
export interface TableColumn {
  id: string;
  label: React.ReactNode;
  minWidth?: number;
  width?: number | string;
  maxWidth?: number;
  align?: 'left' | 'right' | 'center';
  sortable?: boolean;
  render?: (value: any, row: any) => React.ReactNode;
}

export interface TabPanelProps {
  children?: React.ReactNode;
  value: number;
  index: number;
}

export interface BreadcrumbItem {
  text: string;
  path: string;
}

export interface PageHeaderProps {
  title: string;
  subtitle?: string;
  actionText?: string;
  onActionClick?: () => void;
  breadcrumbs?: BreadcrumbItem[];
}

// Form related types
export interface FormField {
  name: string;
  label: string;
  type: 'text' | 'number' | 'select' | 'checkbox' | 'textarea' | 'date';
  required?: boolean;
  options?: { value: string | number; label: string }[];
  defaultValue?: any;
  helperText?: string;
  min?: number;
  max?: number;
  step?: number;
  fullWidth?: boolean;
  disabled?: boolean;
  multiline?: boolean;
  rows?: number;
}
