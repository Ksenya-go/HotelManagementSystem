using HotelManagementSystem.Persistence.EfCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace HotelManagementSystem.Web.Tests;

public sealed class HotelManagementSystemWebApplicationFactory : WebApplicationFactory
    <Program>
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");

    public const string AdminEmail = "admin.tests@example.com";
    public const string AdminPassword = "AdminPass123";
    public const string EmployeeEmail = "employee.tests@example.com";
    public const string EmployeePassword = "EmployeePass123";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            connection.Open();

            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connection)
                    .EnableServiceProviderCaching(false));

            services.AddSingleton<IHostedService, TestDatabaseInitializer>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            connection.Dispose();
        }

        base.Dispose(disposing);
    }

    private static async Task SeedIdentityAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "Employee" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                EnsureSucceeded(result);
            }
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        await CreateUserAsync(userManager, AdminEmail, AdminPassword, 
            "Test Admin", "Admin");
        await CreateUserAsync(userManager, EmployeeEmail, EmployeePassword, 
            "Test Employee", "Employee");
    }

    private static async Task CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName
            };

            var createResult = await userManager.CreateAsync(user, password);
            EnsureSucceeded(createResult);
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);
            EnsureSucceeded(roleResult);
        }
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", 
                result.Errors.Select(error => error.Description)));
        }
    }

    private sealed class TestDatabaseInitializer(IServiceScopeFactory scopeFactory) : 
        IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.EnsureCreatedAsync(cancellationToken);
            await SeedIdentityAsync(scope.ServiceProvider);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
