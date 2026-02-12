using HRDashboard.Data;
using HRDashboard.Models;  
using HRDashboard.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;    


var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole(); // Logs to console
builder.Logging.AddDebug();   // Logs to Debug output
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
// Add services to the container.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login"; // Set the custom login path
    options.AccessDeniedPath = "/access-denied"; // Optional: Redirect when access is denied
});
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IUserRegistrationService, UserRegistrationService>();
// Cloudinary service and settings
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.Configure<FileUploadSettings>(builder.Configuration.GetSection("FileUploadSettings"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddSingleton<GoogleSheetsClient>();
builder.Services.AddScoped<SheetSyncService>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedRoleAsync(roleManager);
    // After roles seeded, attempt to sync Google Sheet rows into the local database.
    var sheetSync = scope.ServiceProvider.GetService<SheetSyncService>();
    if (sheetSync != null)
    {
        await sheetSync.SyncAsync();
    }
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "login",
    pattern: "login",
    defaults:new { Controller= "login",Action="Index"});

app.Run();
