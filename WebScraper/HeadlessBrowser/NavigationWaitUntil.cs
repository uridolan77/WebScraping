namespace WebScraper.HeadlessBrowser
{
    /// <summary>
    /// The condition to wait for before considering navigation complete
    /// </summary>
    public enum NavigationWaitUntil
    {
        /// <summary>
        /// Wait until the DOMContentLoaded event is fired
        /// </summary>
        DOMContentLoaded,
        
        /// <summary>
        /// Wait until the load event is fired
        /// </summary>
        Load,
        
        /// <summary>
        /// Wait until the network is idle (no requests for at least 500ms)
        /// </summary>
        NetworkIdle,
        
        /// <summary>
        /// Wait until there are no more than 0 network connections for at least 500ms
        /// </summary>
        NetworkAlmostIdle
    }
}