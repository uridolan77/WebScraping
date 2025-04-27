using System;

namespace WebScraper.HeadlessBrowser
{
    /// <summary>
    /// Options for configuring the headless browser
    /// </summary>
    public class HeadlessBrowserOptions
    {
        /// <summary>
        /// Gets or sets whether to run browser in headless mode (no visible UI)
        /// </summary>
        public bool Headless { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the browser type to use
        /// </summary>
        public BrowserType BrowserType { get; set; } = BrowserType.Chromium;
        
        /// <summary>
        /// Gets or sets the timeout for navigation operations in milliseconds
        /// </summary>
        public int NavigationTimeout { get; set; } = 30000;
        
        /// <summary>
        /// Gets or sets the timeout for waiting operations in milliseconds
        /// </summary>
        public int WaitTimeout { get; set; } = 10000;
        
        /// <summary>
        /// Gets or sets the timeout for browser launch in milliseconds
        /// </summary>
        public int LaunchTimeout { get; set; } = 30000;
        
        /// <summary>
        /// Gets or sets whether to enable JavaScript
        /// </summary>
        public bool JavaScriptEnabled { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the directory to save screenshots to
        /// </summary>
        public string ScreenshotDirectory { get; set; }
        
        /// <summary>
        /// Gets or sets whether to take screenshots automatically
        /// </summary>
        public bool TakeScreenshots { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the user agent string
        /// </summary>
        public string UserAgent { get; set; }
        
        /// <summary>
        /// Gets or sets whether to ignore HTTPS errors
        /// </summary>
        public bool IgnoreHttpsErrors { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to bypass Content Security Policy
        /// </summary>
        public bool BypassCSP { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the viewport size
        /// </summary>
        public ViewportSize Viewport { get; set; } = new ViewportSize { Width = 1280, Height = 800 };
        
        /// <summary>
        /// Gets or sets whether to log JavaScript errors from the page
        /// </summary>
        public bool LogJavaScriptErrors { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the slowMo option that slows down Playwright operations by the specified amount of milliseconds
        /// </summary>
        public int SlowMotion { get; set; } = 0;
    }
}