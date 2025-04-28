using System;
using System.IO;
using System.Threading.Tasks;
using WebScraper.Processing;

namespace WebScraper.RegulatoryContent
{
    // Extension class for PdfDocumentHandler to add missing methods
    public static class PdfDocumentHandlerExtension
    {
        // Add ExtractTextAsync method overload for byte[] parameter
        public static async Task<string> ExtractTextAsync(this PdfDocumentHandler handler, byte[] pdfBytes)
        {
            try
            {
                // Create a temporary file to save the bytes
                string tempPath = Path.Combine(Path.GetTempPath(), $"temppdf_{Guid.NewGuid()}.pdf");
                File.WriteAllBytes(tempPath, pdfBytes);
                
                // Process the file
                var result = await handler.ExtractTextAsync(tempPath);
                
                // Clean up the temp file
                try { File.Delete(tempPath); } catch { }
                
                return result.Text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting text from PDF bytes: {ex.Message}");
                return $"[PDF EXTRACTION ERROR: {ex.Message}]";
            }
        }
    }
}
