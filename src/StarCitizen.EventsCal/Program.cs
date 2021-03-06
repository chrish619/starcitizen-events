using StarCitizen.EventsCal.Hubs;
using StarCitizen.EventsCal.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddControllersWithViews();

builder.Services
    .AddSignalR();

builder.Services
    .AddSingleton<EventCalendarStoreBacking>()
    .AddHostedService<GoogleDocSourceFetcher>()
    .AddOptions<GoogleDocSourceFetcherOptions>()
    .BindConfiguration("SheetsSource");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<EventsHub>("/events-hub");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();
