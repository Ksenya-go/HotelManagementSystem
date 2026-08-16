using HotelManagementSystem.Persistence.EfCore.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HotelManagementSystem.Web.Tests.Admin;

public sealed class AdminUsersTestFixture : IAsyncDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly ServiceProvider services;

    public AdminUsersTestFixture()
    {
        connection.Open();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(builder => builder.ClearProviders());
        serviceCollection.AddDataProtection();
        serviceCollection.AddDbContext<ApplicationDbContext>(options 
            => options.UseSqlite(connection));
        serviceCollection.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services = serviceCollection.BuildServiceProvider();
        using var scope = services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().
            Database.EnsureCreated();
    }

    public async Task SeedRoleAsync(string role)
    {
        await using var scope = CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager
            <IdentityRole>>();
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(role));
            EnsureSucceeded(result);
        }
    }

    public async Task<ApplicationUser> CreateUserAsync(
        string email = "employee@example.com",
        string password = "Employee123",
        string fullName = "Test Employee",
        string? role = "Employee",
        bool locked = false)
    {
        await SeedRoleAsync(role ?? "Employee");
        await using var scope = CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager
            <ApplicationUser>>();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            LockoutEnd = locked ? DateTimeOffset.UtcNow.AddYears(100) : null
        };

        var result = await userManager.CreateAsync(user, password);
        EnsureSucceeded(result);
        if (!string.IsNullOrWhiteSpace(role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);
            EnsureSucceeded(roleResult);
        }

        return user;
    }

    public AsyncServiceScope CreateAsyncScope() => services.CreateAsyncScope();

    public async Task<ApplicationUser?> FindUserAsync(string id)
    {
        await using var scope = CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<UserManager
            <ApplicationUser>>().FindByIdAsync(id);
    }

    public async Task<IList<string>> GetRolesAsync(string id)
    {
        await using var scope = CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager
            <ApplicationUser>>();
        var user = await userManager.FindByIdAsync(id);
        return user is null ? [] : await userManager.GetRolesAsync(user);
    }

    public async ValueTask DisposeAsync()
    {
        await services.DisposeAsync();
        await connection.DisposeAsync();
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.
                Select(error => error.Description)));
        }
    }
}
