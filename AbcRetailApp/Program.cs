using AbcRetailApp.Data;
using AbcRetailApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;



var builder = WebApplication.CreateBuilder(args);

// Identity database (SQL Server / LocalDB) — separate from Azure Storage data
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Reasonable password rules for a coursework demo — tighten if your module requires stricter rules
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, AbcRetailApp.Services.NoOpEmailSender>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // needed for Identity's built-in UI (login/register pages use Razor Pages)builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSingleton<AbcRetailApp.Services.TableStorageService>();
builder.Services.AddSingleton<AbcRetailApp.Services.BlobStorageService>();
builder.Services.AddSingleton<AbcRetailApp.Services.QueueStorageService>();
builder.Services.AddSingleton<AbcRetailApp.Services.FileStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Seed roles and default admin account on startup
using (var scope = app.Services.CreateScope())
{
    await AbcRetailApp.Data.DbSeeder.SeedRolesAndAdminAsync(scope.ServiceProvider);
}

app.Run();