Okay, let's plan a comprehensive React frontend dashboard UI for your WebScraper API. This plan maps the features you requested to the available API endpoints identified in your C# code.

**I. Core Technology Choices**

* **Framework:** React
* **UI Library:** Consider Material UI (MUI), Ant Design, or Chakra UI for a rich set of pre-built dashboard components (tables, forms, charts, layout elements).
* **Routing:** React Router for navigating between dashboard sections.
* **State Management:** Redux Toolkit or Zustand are good choices for managing API data, loading states, and UI state across the application. React Context API can work for simpler state needs.
* **Data Fetching:** Use `axios` or the built-in `Workspace` API. Consider using React Query or SWR to simplify data fetching, caching, and synchronization.
* **Charting:** Recharts, Chart.js (via react-chartjs-2), or Nivo for visualizing analytics data.
* **Styling:** CSS Modules, Styled Components, or Tailwind CSS, depending on preference.

**II. Dashboard Structure & Layout**

* **Main Layout:**
    * Persistent Sidebar: For navigation between major sections.
    * Top Bar: Could display user information, global search, or quick actions like "Create Scraper".
    * Main Content Area: Renders the active view based on the selected route.
* **Routing (Example):**
    * `/`: Overview/Dashboard Home
    * `/scrapers`: List of all scrapers
    * `/scrapers/new`: Create new scraper form
    * `/scrapers/:id`: Scraper Detail View (with nested tabs/routes for Logs, Results, Config, etc.)
    * `/scrapers/:id/edit`: Edit scraper configuration form
    * `/schedules`: List of all schedules
    * `/schedules/new`: Create new schedule form
    * `/schedules/:id/edit`: Edit schedule form
    * `/analytics`: Global analytics dashboard
    * `/settings`: Application/User settings (if applicable)

**III. Dashboard Sections and Features (Mapping to API Endpoints)**

1.  **Overview (`/`)**
    * **Goal:** Provide a high-level summary of the scraper system's health and activity.
    * **Components:** Dashboard widgets using cards or summary components.
    * **Data Fetching:**
        * **Scraper Status Counts (Running, Idle, Error):** Requires fetching status for all scrapers (`GET api/Scraper/{id}/status` looped or a new backend summary endpoint).
        * **Overall Analytics Summary:** `GET api/Analytics/summary`.
        * **Recent Activity Feed:** Fetch recent logs from key scrapers (`GET api/Scraper/{id}/logs`) or implement a global log endpoint[cite: 69].
        * **Content Change Frequency:** `GET api/Analytics/content-change-frequency`.
        * **Usage Statistics:** `GET api/Analytics/usage-statistics` (potentially with a default date range).
        * **Error Distribution Chart:** `GET api/Analytics/error-distribution`.

2.  **Scraper Management (`/scrapers`, `/scrapers/new`, `/scrapers/:id`, `/scrapers/:id/edit`)**
    * **Goal:** Allow users to view, create, edit, delete, start, stop, and monitor individual scrapers.
    * **Components:**
        * **List View (`/scrapers`):**
            * Data Table: Displaying scrapers (`GET api/Scraper`)[cite: 69].
            * Columns: Name, Status (fetch `GET api/Scraper/{id}/status` for each, consider polling/WebSockets)[cite: 69], Start URL, Last Run, Actions.
            * Actions: Start (`POST api/Scraper/{id}/start`)[cite: 69], Stop (`POST api/Scraper/{id}/stop`)[cite: 69], View Details (link), Edit Config (link), Delete (`DELETE api/Scraper/{id}`)[cite: 69].
            * "Create New" Button.
        * **Create/Edit View (`/scrapers/new`, `/scrapers/:id/edit`):**
            * Multi-section Form: Based on `ScraperConfigModel.cs`. Use `POST api/Scraper` to create[cite: 69], `PUT api/Scraper/{id}` to update[cite: 69]. Sections could include:
                * Basic Info (Name, URLs)
                * Crawling Behavior (Depth, Concurrency, Delays, Links, Robots.txt)
                * Change Detection & Versioning
                * Adaptive Crawling & Rate Limiting
                * Monitoring & Notifications (Email)
                * Content Extraction (Selectors)
                * Proxy Settings
                * Webhook Settings
        * **Detail View (`/scrapers/:id`):**
            * Tabbed Interface or Sectioned Page:
                * **Status:** Display detailed status (`GET api/Scraper/{id}/status`)[cite: 69], Start/Stop controls.
                * **Configuration:** Display current config (`GET api/Scraper/{id}`)[cite: 69], link to edit view.
                * **Logs:** Paginated/filterable log viewer (`GET api/Scraper/{id}/logs`)[cite: 69].
                * **Results:** Data grid for viewing scraped content (`GET api/Scraper/results?scraperId={id}`)[cite: 69]. Include search, pagination, and potentially view content versions (needs API endpoint like `GET api/Scraper/results/{contentId}/versions`).
                * **Changes:** List/Table of detected changes (`GET api/Scraper/{id}/changes`)[cite: 69].
                * **Documents:** List/Table of processed documents (`GET api/Scraper/{id}/documents`)[cite: 69].
                * **Analytics:** Charts and stats for this scraper (`GET api/Analytics/scrapers/{id}`, `GET api/Analytics/scrapers/{id}/performance`, `GET api/Analytics/scrapers/{id}/metrics`).
                * **Monitoring Settings:** View/Edit form (`POST api/Scraper/{id}/monitor`)[cite: 69].
                * **Schedules:** List associated schedules (`GET api/Scheduling/scraper/{scraperId}`)[cite: 83], link to manage them.
                * **Webhooks:** View/Edit form (`PUT api/Notifications/scraper/{id}/webhook-config`)[cite: 82], Test button (`POST api/Notifications/webhook-test`)[cite: 82].
                * **Other Actions:** Compress Content (`POST api/Scraper/{id}/compress`)[cite: 69].

3.  **Scheduling Management (`/schedules`, `/schedules/new`, `/schedules/:id/edit`)**
    * **Goal:** Manage scheduled execution of scrapers.
    * **Components:**
        * **List View (`/schedules`):**
            * Data Table: Displaying all schedules (`GET api/Scheduling`)[cite: 83].
            * Columns: Schedule Name, Scraper Name (requires lookup), Cron Expression, Enabled, Next Run, Last Run, Actions.
            * Actions: Edit (link), Delete (`DELETE api/Scheduling/{id}`)[cite: 83].
            * "Create New Schedule" Button.
        * **Create/Edit View (`/schedules/new`, `/schedules/:id/edit`):**
            * Form: Based on `ScheduleConfig` model[cite: 110].
            * Scraper Selector (fetch `GET api/Scraper`).
            * Cron Expression Input: With validation help (`POST api/Scheduling/validate-cron`) [cite: 83] showing next predicted run times.
            * Use `POST api/Scheduling/scraper/{scraperId}` to create[cite: 83], `PUT api/Scheduling/{id}` to update[cite: 83].

4.  **Global Analytics (`/analytics`)**
    * **Goal:** Provide aggregated analytics across all scrapers.
    * **Components:** Charts and summary statistics.
    * **Data Fetching:**
        * Overall Summary: `GET api/Analytics/summary`.
        * Popular Domains: `GET api/Analytics/popular-domains`.
        * Content Change Frequency: `GET api/Analytics/content-change-frequency`.
        * Usage Statistics: `GET api/Analytics/usage-statistics` (with date range filters).
        * Error Distribution: `GET api/Analytics/error-distribution`.

5.  **Notifications/Webhooks (Potentially part of Scraper Detail or Settings)**
    * **Goal:** Configure and test webhook notifications.
    * **Components:** Configuration form, Test button.
    * **Data Fetching:**
        * View/Update Config: `PUT api/Notifications/scraper/{id}/webhook-config` (requires GET endpoint or storing config client-side)[cite: 82].
        * Test Webhook: `POST api/Notifications/webhook-test`[cite: 82].
        * Send Custom Notification: `POST api/Notifications/scraper/{id}/notify`[cite: 82].

**IV. API Interaction Layer**

* Create a centralized API service module in React (e.g., `apiClient.js`) to handle requests using `axios` or `Workspace`.
* Define functions for each endpoint (e.g., `WorkspaceScrapers`, `startScraper`, `updateSchedule`).
* Manage the API base URL (likely from environment variables).
* Implement consistent error handling.

**V. State Management Considerations**

* Store lists (scrapers, schedules) and individual details fetched from the API.
* Track loading states for API requests.
* Handle API errors gracefully, providing user feedback.
* Potentially normalize data (e.g., store scrapers in an object keyed by ID) for efficient lookups.

This plan provides a detailed blueprint for building your React dashboard, ensuring all backend functionalities are accessible through the UI. Remember to break down the implementation into smaller, manageable components.