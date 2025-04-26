using Microsoft.AspNetCore.StaticFiles;
using System.Net.Http;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add SPA static files support
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "wwwroot";
});

// Add HTTP client for API proxy
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Configure static files with correct MIME types
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Set cache control headers
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");

        // Ensure JavaScript files have the correct MIME type
        if (ctx.File.Name.EndsWith(".js"))
        {
            ctx.Context.Response.ContentType = "application/javascript";
        }
    }
});

app.UseSpaStaticFiles(); // Add this line

app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

// Add API proxy middleware
app.Map("/api", appBuilder =>
{
    appBuilder.Run(async context =>
    {
        // Create a handler that ignores SSL certificate errors (for development only)
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        // Create a new HttpClient with the handler
        using var httpClient = new HttpClient(handler);

        // Add Accept header to request JSON
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        // Forward the request to the actual API
        var apiUrl = "https://localhost:7143/api" + context.Request.Path + context.Request.QueryString;
        Console.WriteLine($"Proxying request to: {apiUrl}");

        try
        {
            HttpResponseMessage response;

            // Handle request based on method
            if (HttpMethods.IsGet(context.Request.Method))
            {
                // Simple GET request
                Console.WriteLine($"Sending GET request to {apiUrl}");
                response = await httpClient.GetAsync(apiUrl);
            }
            else if (HttpMethods.IsPost(context.Request.Method) ||
                     HttpMethods.IsPut(context.Request.Method) ||
                     HttpMethods.IsPatch(context.Request.Method))
            {
                // POST, PUT, PATCH with body
                Console.WriteLine($"Sending {context.Request.Method} request to {apiUrl}");

                // Read the request body
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();

                // Create the content
                var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

                // Send the request
                if (HttpMethods.IsPost(context.Request.Method))
                {
                    response = await httpClient.PostAsync(apiUrl, content);
                }
                else if (HttpMethods.IsPut(context.Request.Method))
                {
                    response = await httpClient.PutAsync(apiUrl, content);
                }
                else // PATCH
                {
                    var request = new HttpRequestMessage(HttpMethod.Patch, apiUrl)
                    {
                        Content = content
                    };
                    response = await httpClient.SendAsync(request);
                }
            }
            else if (HttpMethods.IsDelete(context.Request.Method))
            {
                // DELETE request
                Console.WriteLine($"Sending DELETE request to {apiUrl}");
                response = await httpClient.DeleteAsync(apiUrl);
            }
            else
            {
                // Unsupported method
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                await context.Response.WriteAsync($"Method {context.Request.Method} not supported");
                return;
            }

            Console.WriteLine($"Received response: {(int)response.StatusCode} {response.ReasonPhrase}");

            // Copy the response status code
            context.Response.StatusCode = (int)response.StatusCode;

            // Set content type to JSON
            context.Response.ContentType = "application/json";

            // Read the response content
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response content: {responseContent}");

            // Write the response content
            await context.Response.WriteAsync(responseContent);

            // Log success
            Console.WriteLine($"Successfully proxied request to {apiUrl}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error proxying request to {apiUrl}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");

            // Return a JSON error response
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = "API Proxy Error",
                message = ex.Message,
                url = apiUrl,
                method = context.Request.Method
            };

            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(jsonResponse);
        }
    });
});

// Configure SPA handling
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "wwwroot";

    // Set default page for SPA
    spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            // No caching for index.html to ensure fresh content
            ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            ctx.Context.Response.Headers.Append("Pragma", "no-cache");
            ctx.Context.Response.Headers.Append("Expires", "0");
        }
    };

    // Only if you want to use the development server instead of the static files
    if (app.Environment.IsDevelopment())
    {
        // If you want to use the React development server
        // spa.UseReactDevelopmentServer(npmScript: "dev");
    }
});

app.Run();