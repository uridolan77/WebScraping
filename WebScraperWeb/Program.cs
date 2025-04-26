var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add SPA static files support
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "wwwroot";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSpaStaticFiles(); // Add this line

app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

// Configure SPA handling
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "wwwroot";

    // Only if you want to use the development server instead of the static files
    if (app.Environment.IsDevelopment())
    {
        // If you want to use the React development server
        // spa.UseReactDevelopmentServer(npmScript: "dev");
    }
});

app.Run();