import { Routes, Route, Navigate } from 'react-router-dom';

// Import pages
import OverviewPage from '../pages/OverviewPage';
import ScraperListPage from '../pages/ScraperListPage';
import ScraperDetailPage from '../pages/ScraperDetailPage';
import CreateScraperPage from '../pages/CreateScraperPage';
import EditScraperPage from '../pages/EditScraperPage';
import ScheduleListPage from '../pages/ScheduleListPage';
import CreateSchedulePage from '../pages/CreateSchedulePage';
import EditSchedulePage from '../pages/EditSchedulePage';
import AnalyticsPage from '../pages/AnalyticsPage';
import NotFoundPage from '../pages/NotFoundPage';

const AppRouter = () => {
  return (
    <Routes>
      <Route path="/" element={<OverviewPage />} />
      
      {/* Scraper routes */}
      <Route path="/scrapers" element={<ScraperListPage />} />
      <Route path="/scrapers/new" element={<CreateScraperPage />} />
      <Route path="/scrapers/:id" element={<ScraperDetailPage />} />
      <Route path="/scrapers/:id/edit" element={<EditScraperPage />} />
      
      {/* Schedule routes */}
      <Route path="/schedules" element={<ScheduleListPage />} />
      <Route path="/schedules/new" element={<CreateSchedulePage />} />
      <Route path="/schedules/:id/edit" element={<EditSchedulePage />} />
      
      {/* Analytics route */}
      <Route path="/analytics" element={<AnalyticsPage />} />
      
      {/* 404 and redirect */}
      <Route path="/404" element={<NotFoundPage />} />
      <Route path="*" element={<Navigate to="/404" replace />} />
    </Routes>
  );
};

export default AppRouter;