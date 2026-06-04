using CenterManagement.Domain.Entities;
using CenterManagement.Infrastructure.Persistence;
using CenterManagement.Infrastructure.DependencyInjection;
using CenterManagement.Application.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using CenterManagement.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<CenterManagementDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.LoginPath = "/Auth/Login";
    opts.AccessDeniedPath = "/Auth/AccessDenied";
    opts.ExpireTimeSpan = TimeSpan.FromHours(8);
    opts.SlidingExpiration = true;
});

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    opts.AddPolicy("AdminOrInstructor", p => p.RequireRole("Admin", "Instructor"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var userManager =
        services.GetRequiredService<
            UserManager<ApplicationUser>>();

    var roleManager =
        services.GetRequiredService<
            RoleManager<IdentityRole>>();

    var dbContext =
        services.GetRequiredService<
            CenterManagementDbContext>();

    await IdentitySeeder
        .SeedRolesAndAdminAsync(
            userManager,
            roleManager,
            dbContext);
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();