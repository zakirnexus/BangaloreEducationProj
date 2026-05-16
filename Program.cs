using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURATION =====
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

// ===== SERVICES =====
builder.Services.AddControllersWithViews();

// Proper DI registration
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ReCaptchaService>();
builder.Services.AddScoped<ContentService>();
builder.Services.AddScoped<SidebarService>();
builder.Services.AddScoped<NavService>();

// Caching
builder.Services.AddMemoryCache();

// HttpClient
builder.Services.AddHttpClient();

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("EditorOrAdmin", policy => policy.RequireRole("Admin", "Editor"));
});

// ===== BUILD APP =====
var app = builder.Build();

// ===== MIDDLEWARE PIPELINE =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ===== SEO ROUTES (Order matters - specific first) =====

// College routes
app.MapControllerRoute(
    name: "college-list",
    pattern: "{course}-colleges-in-{city}",
    defaults: new { controller = "College", action = "List" });

app.MapControllerRoute(
    name: "college-detail",
    pattern: "college/{slug}",
    defaults: new { controller = "College", action = "Details" });

// School routes
app.MapControllerRoute(
    name: "school-list",
    pattern: "{board}-schools-in-{city}",
    defaults: new { controller = "School", action = "List" });

app.MapControllerRoute(
    name: "school-detail",
    pattern: "school/{slug}",
    defaults: new { controller = "School", action = "Details" });

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();