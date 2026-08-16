using HotelManagementSystem.Persistence.EfCore.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HotelManagementSystem.Web.Tests.Admin;

public sealed class AdminUsersIdentityTests
{
    [Fact]
    public async Task CreateUser_WithValidData_CreatesEmployeeWithHashedPassword()
    {
        await using var fixture = new AdminUsersTestFixture();
        await fixture.SeedRoleAsync("Employee");

        var createdUser = await fixture.CreateUserAsync("new.employee@example.com", 
            "Employee123", "New Employee");
        var created = await FindByEmailAsync(fixture, "new.employee@example.com");

        Assert.NotNull(created);
        Assert.Equal("new.employee@example.com", created!.Email);
        Assert.Equal("New Employee", created.FullName);
        Assert.NotEqual("Employee123", created.PasswordHash);
        Assert.Contains("Employee", await fixture.GetRolesAsync(created.Id));
        Assert.Equal(createdUser.Id, created.Id);
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ReturnsDuplicateErrorAndDoesNotCreateSecondUser()
    {
        await using var fixture = new AdminUsersTestFixture();
        await fixture.CreateUserAsync("duplicate@example.com");
        await using var scope = fixture.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager
            <ApplicationUser>>();
        var duplicate = new ApplicationUser
        {
            UserName = "duplicate@example.com",
            Email = "duplicate@example.com",
            EmailConfirmed = true,
            FullName = "Duplicate"
        };

        var result = await userManager.CreateAsync(duplicate, "Password123");
        var count = await userManager.Users.CountAsync(user => user.Email == 
        "duplicate@example.com");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code is "DuplicateUserName" 
        or "DuplicateEmail");
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CreateUser_WithInvalidEmailOrPassword_ReturnsValidationErrors()
    {
        await using var fixture = new AdminUsersTestFixture();
        await using var scope = fixture.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager
            <ApplicationUser>>();
        var user = new ApplicationUser
        {
            UserName = "not-an-email",
            Email = "not-an-email",
            EmailConfirmed = true,
            FullName = "Invalid"
        };

        var result = await userManager.CreateAsync(user, "short");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "PasswordTooShort");
        Assert.Empty(await userManager.Users.ToListAsync());
    }

    [Fact]
    public async Task UpdateUser_WithValidData_UpdatesUserAndRole()
    {
        await using var fixture = new AdminUsersTestFixture();
        var user = await fixture.CreateUserAsync();
        await fixture.SeedRoleAsync("Admin");
        await using var scope = fixture.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService
            <UserManager<ApplicationUser>>();
        var savedUser = await userManager.FindByIdAsync(user.Id);
        savedUser!.FullName = "Updated Employee";
        savedUser.Email = "updated@example.com";
        savedUser.UserName = "updated@example.com";
        var updateResult = await userManager.UpdateAsync(savedUser);
        var removeResult = await userManager.RemoveFromRolesAsync(savedUser, 
            await userManager.GetRolesAsync(savedUser));
        var roleResult = await userManager.AddToRoleAsync(savedUser, "Admin");

        var reloaded = await fixture.FindUserAsync(user.Id);
        Assert.True(updateResult.Succeeded);
        Assert.True(removeResult.Succeeded);
        Assert.True(roleResult.Succeeded);
        Assert.Equal("Updated Employee", reloaded!.FullName);
        Assert.Equal("updated@example.com", reloaded.Email);
        Assert.Contains("Admin", await fixture.GetRolesAsync(user.Id));
    }

    [Fact]
    public async Task UpdateUser_WithNonExistingId_ReturnsFailure()
    {
        await using var fixture = new AdminUsersTestFixture();
        await using var scope = fixture.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService
            <UserManager<ApplicationUser>>();

        var user = await userManager.FindByIdAsync("missing-user-id");

        Assert.Null(user);
    }

    [Fact]
    public async Task Lockout_WithActiveUser_BlocksUserAndPreventsPasswordSignIn()
    {
        await using var fixture = new AdminUsersTestFixture();
        var user = await fixture.CreateUserAsync("lockout@example.com", "Password123");
        await using var scope = fixture.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager
            <ApplicationUser>>();
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager
            <ApplicationUser>>();
        var savedUser = await userManager.FindByIdAsync(user.Id);
        savedUser!.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
        var updateResult = await userManager.UpdateAsync(savedUser);
        var signIn = await signInManager.CheckPasswordSignInAsync(savedUser, 
            "Password123", lockoutOnFailure: false);

        var reloaded = await fixture.FindUserAsync(user.Id);
        Assert.True(updateResult.Succeeded);
        Assert.True(reloaded!.LockoutEnd > DateTimeOffset.UtcNow);
        Assert.True(signIn.IsLockedOut);
    }

    [Fact]
    public async Task Lockout_WhenAlreadyBlocked_TogglesBackToActive()
    {
        await using var fixture = new AdminUsersTestFixture();
        var user = await fixture.CreateUserAsync("unlock@example.com", "Password123", 
            locked: true);
        await using var scope = fixture.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager
            <ApplicationUser>>();
        var savedUser = await userManager.FindByIdAsync(user.Id);
        savedUser!.LockoutEnd = null;
        var result = await userManager.UpdateAsync(savedUser);

        var reloaded = await fixture.FindUserAsync(user.Id);
        Assert.True(result.Succeeded);
        Assert.Null(reloaded!.LockoutEnd);
    }

    [Fact]
    public async Task ChangeRole_WithExistingAdmin_AllowsRoleChangeBecauseNoSuchRestrictionExists()
    {
        await using var fixture = new AdminUsersTestFixture();
        await fixture.CreateUserAsync("existing.admin@example.com", "Admin123", 
            "Admin", "Admin");
        var employee = await fixture.CreateUserAsync();
        await fixture.SeedRoleAsync("Admin");
        await using var scope = fixture.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager
            <ApplicationUser>>();
        var target = await userManager.FindByIdAsync(employee.Id);
        var removeResult = await userManager.RemoveFromRolesAsync(target!, 
            await userManager.GetRolesAsync(target!));
        var addResult = await userManager.AddToRoleAsync(target!, "Admin");

        Assert.True(removeResult.Succeeded);
        Assert.True(addResult.Succeeded);
        Assert.Contains("Admin", await fixture.GetRolesAsync(employee.Id));
    }

    private static async Task<ApplicationUser?> FindByEmailAsync(AdminUsersTestFixture 
        fixture, string email)
    {
        await using var scope = fixture.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<UserManager
            <ApplicationUser>>().FindByEmailAsync(email);
    }
}
