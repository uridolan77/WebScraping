# WebScraper Backoffice

A React-based administration interface for managing web scrapers, monitoring their status, analyzing results, and configuring notifications.

## Project Structure

```bash
webscraper-backoffice/
├── public/
│   ├── index.html
│   └── favicon.ico
├── src/
│   ├── api/
│   │   ├── index.js            # API client setup
│   │   ├── scrapers.js         # Scraper API functions
│   │   ├── analytics.js        # Analytics API functions
│   │   ├── notifications.js    # Notifications API functions
│   │   ├── scheduling.js       # Scheduling API functions
│   │   └── state.js            # State management API functions
│   ├── components/
│   │   ├── common/             # Reusable components
│   │   ├── dashboard/          # Dashboard components
│   │   ├── scrapers/           # Scraper management components
│   │   ├── monitoring/         # Monitoring components
│   │   ├── analytics/          # Analytics components
│   │   ├── scheduling/         # Scheduling components
│   │   └── notifications/      # Notification components
│   ├── contexts/
│   │   ├── AuthContext.jsx     # Authentication context
│   │   └── ScraperContext.jsx  # Scraper data context
│   ├── hooks/
│   │   ├── useScrapers.js      # Custom hook for scraper data
│   │   ├── useAnalytics.js     # Custom hook for analytics data
│   │   └── ...
│   ├── pages/
│   │   ├── Dashboard.jsx       # Main dashboard
│   │   ├── ScraperList.jsx     # List of scrapers
│   │   ├── ScraperDetail.jsx   # View/edit scraper details
│   │   ├── ScraperCreate.jsx   # Create new scraper
│   │   ├── Analytics.jsx       # Analytics view
│   │   ├── Monitoring.jsx      # Monitoring view
│   │   ├── Scheduling.jsx      # Scheduling view
│   │   └── Settings.jsx        # Global settings
│   ├── utils/
│   │   ├── formatters.js       # Data formatting utilities
│   │   ├── validators.js       # Form validation utilities
│   │   └── helpers.js          # Misc helper functions
│   ├── App.jsx                 # Main app component
│   ├── index.jsx               # Entry point
│   └── theme.js                # UI theme configuration
├── .env                        # Environment variables
├── package.json                # Dependencies
└── README.md                   # Documentation
```

## Available Scripts

In the project directory, you can run:

### `npm start`

Runs the app in the development mode.\
Open [http://localhost:3000](http://localhost:3000) to view it in your browser.

### `npm test`

Launches the test runner in the interactive watch mode.

### `npm run build`

Builds the app for production to the `build` folder.

## Environment Variables

Create a `.env` file in the root directory with the following variables:

```env
REACT_APP_API_URL=https://localhost:7143/api
```

## Features

- **Scraper Management**: Create, configure, and manage web scrapers
- **Monitoring**: Real-time monitoring of scraper status and performance
- **Analytics**: Visualize data about scraper performance and content changes
- **Scheduling**: Set up automated scraping schedules
- **Notifications**: Configure alerts for important events and content changes
- **Content Browser**: Browse and search through scraped content

## Recent Improvements

### TypeScript Integration

- Strong type checking for improved code quality
- Type definitions for all major entities and API responses
- Normalized file extensions (.tsx for components, .ts for utilities)

### Enhanced State Management

- React Query for efficient data fetching and caching
- Proper loading and error states for all asynchronous operations
- Robust context architecture for global application state

### Performance Optimizations

- Virtualized tables for efficient rendering of large datasets
- Component memoization with React.memo
- Pagination for data-heavy views
- Optimized rendering with useMemo and useCallback

## API Integration

The backoffice connects to the WebScraper API to manage scrapers and retrieve data. The API client is configured in `src/api/index.js` and individual API modules handle specific functionality.

## Authentication

Basic authentication is implemented using JWT tokens stored in localStorage. The `AuthContext` provides login/logout functionality and user state management.

## Theming

Material-UI theming is configured in `src/theme.js`. The application uses a responsive design that works well on desktop and tablet devices.
