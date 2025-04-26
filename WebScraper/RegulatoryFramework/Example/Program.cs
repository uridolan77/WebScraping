using System;
using System.Threading.Tasks;

namespace WebScraper.RegulatoryFramework.Example
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Regulatory Scraper Framework Example");
            Console.WriteLine("====================================");
            
            await RegulatoryScraperExample.RunExampleAsync();
            
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}