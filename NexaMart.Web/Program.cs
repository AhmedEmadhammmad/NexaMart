using Microsoft.AspNetCore.Authentication.Cookies;
using NexaMart.Application.Extensions;
using NexaMart.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Add MVC Controllers and Views
builder.Services.AddControllersWithViews();

// 2. Configure Cookie Authentication for MVC
var rememberMeDays = builder.Configuration.GetValue<int>("JwtSettings:RememberMeDays", 2);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "NexaMart.Auth";
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(rememberMeDays);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// 3. Configure Role & Policy Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StaffOnly", policy => 
        policy.RequireRole("Admin", "SuperAdmin"));
    options.AddPolicy("SuperAdminOnly", policy => 
        policy.RequireRole("SuperAdmin"));
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
    options.AddPolicy("CustomerOnly", policy => 
        policy.RequireRole("Customer"));
});

// 4. Register Application and Infrastructure Layer services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 5. Automatic Database Migration and Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var seederLogger = services.GetRequiredService<ILogger<Program>>();
        seederLogger.LogInformation("Checking database state and executing seeders...");
        await NexaMart.Infrastructure.Data.Seed.DbSeeder.SeedAsync(app.Services);
        seederLogger.LogInformation("Database verified and seeded successfully.");
    }
    catch (Exception ex)
    {
        var seederLogger = services.GetRequiredService<ILogger<Program>>();
        seederLogger.LogError(ex, "An error occurred during database seeding: {Message}", ex.Message);
    }
}

app.Run();

