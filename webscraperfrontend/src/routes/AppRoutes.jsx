import { Routes, Route, Navigate } from 'react-router-dom';

// Layout
import MainLayout from '../layout/MainLayout';

// Pages
import OverviewPage from '../pages/OverviewPage';
import NotFoundPage from '../pages/NotFoundPage';
import ScraperListPage from '../pages/ScraperListPage';
import CreateScraperPage from '../pages/CreateScraperPage';
import EditScraperPage from '../pages/EditScraperPage';
import ScraperDetailPage from '../pages/ScraperDetailPage';
import ScheduleListPage from '../pages/ScheduleListPage';
import CreateSchedulePage from '../pages/CreateSchedulePage';
import EditSchedulePage from '../pages/EditSchedulePage';
import AnalyticsPage from '../pages/AnalyticsPage';

const AppRoutes = () => {
  return (
    <Routes>
      {/* Main routes with layout */}
      <Route path="/" element={<MainLayout />}>
        {/* Dashboard */}
        <Route index element={<OverviewPage />} />
        
        {/* Scrapers */}
        <Route path="scrapers">
          <Route index element={<ScraperListPage />} />
          <Route path="new" element={<CreateScraperPage />} />
          <Route path=":id" element={<ScraperDetailPage />} />
          <Route path=":id/edit" element={<EditScraperPage />} />
        </Route>
        
        {/* Schedules */}
        <Route path="schedules">
          <Route index element={<ScheduleListPage />} />
          <Route path="new" element={<CreateSchedulePage />} />
          <Route path=":id/edit" element={<EditSchedulePage />} />
        </Route>
        
        {/* Analytics */}
        <Route path="analytics" element={<AnalyticsPage />} />
        
        {/* Redirect legacy paths */}
        <Route path="dashboard" element={<Navigate to="/" replace />} />
        
        {/* 404 Not Found */}
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
};

export default AppRoutes;