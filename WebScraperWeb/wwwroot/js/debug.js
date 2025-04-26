// Debug script to help diagnose issues
console.log('🔧 Debug script loaded');

// Function to check if we're on the configuration page
function checkIfConfigPage() {
    const path = window.location.pathname;
    console.log('📍 Current path:', path);
    
    if (path.includes('/configure/')) {
        console.log('🔍 On configuration page with ID:', path.split('/configure/')[1]);
        
        // Add a button to manually trigger API call
        setTimeout(() => {
            const debugButton = document.createElement('button');
            debugButton.textContent = 'Debug API Call';
            debugButton.style.position = 'fixed';
            debugButton.style.bottom = '20px';
            debugButton.style.right = '20px';
            debugButton.style.zIndex = '9999';
            debugButton.style.padding = '10px';
            debugButton.style.backgroundColor = 'red';
            debugButton.style.color = 'white';
            debugButton.style.border = 'none';
            debugButton.style.borderRadius = '5px';
            debugButton.style.cursor = 'pointer';
            
            debugButton.onclick = function() {
                const id = path.split('/configure/')[1];
                console.log('🚀 Manual API call for ID:', id);
                
                // Make direct API call
                fetch(`/api/scraper`, {
                    headers: {
                        'Accept': 'application/json'
                    }
                })
                .then(response => {
                    console.log('📊 All scrapers response status:', response.status);
                    return response.text();
                })
                .then(text => {
                    console.log('📝 All scrapers response text:', text);
                    try {
                        const scrapers = JSON.parse(text);
                        console.log('📋 All scrapers:', scrapers);
                        
                        // Find matching scraper
                        const matchingScraper = scrapers.find(s => 
                            s.id === id || 
                            s.Id === id || 
                            s.name === id || 
                            s.Name === id
                        );
                        
                        if (matchingScraper) {
                            console.log('✅ Found matching scraper:', matchingScraper);
                            
                            // Get actual ID
                            const actualId = matchingScraper.id || matchingScraper.Id;
                            console.log('🔑 Actual scraper ID:', actualId);
                            
                            // Fetch specific scraper
                            fetch(`/api/scraper/${actualId}`, {
                                headers: {
                                    'Accept': 'application/json'
                                }
                            })
                            .then(response => {
                                console.log('📊 Specific scraper response status:', response.status);
                                return response.text();
                            })
                            .then(text => {
                                console.log('📝 Specific scraper response text:', text);
                                try {
                                    const data = JSON.parse(text);
                                    console.log('📋 Specific scraper data:', data);
                                    
                                    // Display data on page
                                    const debugInfo = document.createElement('div');
                                    debugInfo.style.position = 'fixed';
                                    debugInfo.style.top = '50px';
                                    debugInfo.style.right = '20px';
                                    debugInfo.style.zIndex = '9999';
                                    debugInfo.style.padding = '10px';
                                    debugInfo.style.backgroundColor = 'rgba(0, 0, 0, 0.8)';
                                    debugInfo.style.color = 'white';
                                    debugInfo.style.borderRadius = '5px';
                                    debugInfo.style.maxWidth = '400px';
                                    debugInfo.style.maxHeight = '400px';
                                    debugInfo.style.overflow = 'auto';
                                    
                                    debugInfo.innerHTML = `
                                        <h3>Scraper Data</h3>
                                        <pre>${JSON.stringify(data, null, 2)}</pre>
                                        <button id="close-debug">Close</button>
                                    `;
                                    
                                    document.body.appendChild(debugInfo);
                                    
                                    document.getElementById('close-debug').onclick = function() {
                                        document.body.removeChild(debugInfo);
                                    };
                                } catch (e) {
                                    console.error('❌ Error parsing specific scraper response:', e);
                                }
                            })
                            .catch(error => {
                                console.error('❌ Error fetching specific scraper:', error);
                            });
                        } else {
                            console.error('❌ No matching scraper found for ID:', id);
                        }
                    } catch (e) {
                        console.error('❌ Error parsing all scrapers response:', e);
                    }
                })
                .catch(error => {
                    console.error('❌ Error fetching all scrapers:', error);
                });
            };
            
            document.body.appendChild(debugButton);
        }, 1000);
    }
}

// Run when the page loads
window.addEventListener('load', function() {
    console.log('🔄 Page loaded');
    checkIfConfigPage();
});

// Also run when the URL changes (for single-page apps)
window.addEventListener('popstate', function() {
    console.log('🔄 URL changed');
    checkIfConfigPage();
});
