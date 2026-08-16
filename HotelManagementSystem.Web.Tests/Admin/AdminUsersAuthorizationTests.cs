using System.Net;
using System.Text.RegularExpressions;
using HotelManagementSystem.Persistence.EfCore.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HotelManagementSystem.Web.Tests.Admin;

public sealed class AdminUsersAuthorizationTests : 
    IClassFixture<HotelManagementSystemWebApplicationFactory>
{
    private readonly HotelManagementSystemWebApplicationFactory factory;

    public AdminUsersAuthorizationTests(HotelManagementSystemWebApplicationFactory 
        factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task AnonymousUser_CannotAccessAdminUsers()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync("/Admin/Users");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Employee_CannotAccessAdminUsers()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            EmployeeEmail, HotelManagementSystemWebApplicationFactory.EmployeePassword);

        using var response = await client.GetAsync("/Admin/Users");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.
            OriginalString);
    }

    [Fact]
    public async Task Admin_CanAccessAdminUsersAndListSeededUsers()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            AdminEmail, HotelManagementSystemWebApplicationFactory.AdminPassword);

        using var response = await client.GetAsync("/Admin/Users");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(HotelManagementSystemWebApplicationFactory.AdminEmail, 
            body, StringComparison.Ordinal);
        Assert.Contains(HotelManagementSystemWebApplicationFactory.EmployeeEmail, 
            body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AdminUsers_ListCanBeEmptyWhenIdentityStoreHasNoUsers()
    {
        await using var fixture = new AdminUsersTestFixture();
        await using var scope = fixture.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager
            <ApplicationUser>>();

        Assert.Empty(await userManager.Users.ToListAsync());
    }

    [Fact]
    public async Task Employee_CannotCreateUserThroughProtectedPost()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            EmployeeEmail, HotelManagementSystemWebApplicationFactory.EmployeePassword);

        using var response = await client.PostAsync("/Admin/Users/Create", 
            new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["FullName"] = "Unauthorized User",
            ["Email"] = $"unauthorized-{Guid.NewGuid():N}@example.com",
            ["Password"] = "Password123",
            ["Role"] = "Employee"
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.
            OriginalString);
    }

    [Fact]
    public async Task Employee_CannotEditUserThroughProtectedPost()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            EmployeeEmail, HotelManagementSystemWebApplicationFactory.EmployeePassword);

        using var response = await client.PostAsync("/Admin/Users/not-real/Edit", 
            new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Id"] = "not-real",
            ["FullName"] = "Unauthorized Edit",
            ["Email"] = "edit@example.com",
            ["Role"] = "Employee"
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.
            OriginalString);
    }

    [Fact]
    public async Task Employee_CannotToggleLockoutThroughProtectedPost()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            EmployeeEmail, HotelManagementSystemWebApplicationFactory.EmployeePassword);

        using var response = await client.PostAsync("/Admin/Users/not-real/ToggleLockout", new FormUrlEncodedContent([]));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.
            OriginalString);
    }

    [Fact]
    public async Task Employee_CannotChangeRoleThroughProtectedPost()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            EmployeeEmail, HotelManagementSystemWebApplicationFactory.EmployeePassword);

        using var response = await client.PostAsync("/Admin/Users/not-real/ChangeRole",
            new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["role"] = "Admin"
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.
            OriginalString);
    }

    [Fact]
    public async Task AdminUsers_PostWithoutAntiforgeryToken_IsRejected()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            AdminEmail, HotelManagementSystemWebApplicationFactory.AdminPassword);

        using var response = await client.PostAsync(
            "/Admin/Users/Create",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["FullName"] = "Missing Token",
                ["Email"] = $"missing-token-{Guid.NewGuid():N}@example.com",
                ["Password"] = "Password123",
                ["Role"] = "Employee"
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanCreateEmployeeAndPersistsHashedPassword()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            AdminEmail, HotelManagementSystemWebApplicationFactory.AdminPassword);
        var token = await ReadAntiforgeryTokenAsync(await client.GetAsync("/Rooms"));
        var email = $"created-{Guid.NewGuid():N}@example.com";

        using var response = await PostAsync(client, "/Admin/Users/Create", 
            token, new Dictionary<string, string>
        {
            ["FullName"] = "Created Employee",
            ["Email"] = email,
            ["Password"] = "Password123",
            ["Role"] = "Employee"
        });

        var saved = await FindByEmailAsync(email);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Admin/Users", response.Headers.Location?.OriginalString);
        Assert.NotNull(saved);
        Assert.Equal("Created Employee", saved!.FullName);
        Assert.NotEqual("Password123", saved.PasswordHash);
        Assert.Contains("Employee", await GetRolesAsync(saved.Id));
    }

    [Fact]
    public async Task Admin_CreateWithDuplicateEmail_ReturnsFormAndDoesNotCreateSecondUser()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            AdminEmail, HotelManagementSystemWebApplicationFactory.AdminPassword);
        var token = await ReadAntiforgeryTokenAsync(await client.GetAsync("/Rooms"));

        using var response = await PostAsync(client, "/Admin/Users/Create", 
            token, new Dictionary<string, string>
        {
            ["FullName"] = "Duplicate Employee",
            ["Email"] = HotelManagementSystemWebApplicationFactory.EmployeeEmail,
            ["Password"] = "Password123",
            ["Role"] = "Employee"
        });
        var body = await response.Content.ReadAsStringAsync();
        var count = await CountByEmailAsync(HotelManagementSystemWebApplicationFactory.
            EmployeeEmail);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(body);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Admin_EditWithValidData_PersistsChanges()
    {
        var target = await CreateTestEmployeeAsync();
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.AdminEmail,
            HotelManagementSystemWebApplicationFactory.AdminPassword);
        var token = await ReadAntiforgeryTokenAsync(await client.GetAsync("/Rooms"));
        var email = $"edited-{Guid.NewGuid():N}@example.com";

        using var response = await PostAsync(client, $"/Admin/Users/{target.Id}/Edit",
            token, new Dictionary<string, string>
        {
            ["Id"] = target.Id,
            ["FullName"] = "Edited Employee",
            ["Email"] = email,
            ["Role"] = "Employee",
            ["NewPassword"] = ""
        });
        var saved = await FindByIdAsync(target.Id);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("Edited Employee", saved!.FullName);
        Assert.Equal(email, saved.Email);
        Assert.Contains("Employee", await GetRolesAsync(target.Id));
    }

    [Fact]
    public async Task Admin_EditNonExistingUser_ReturnsNotFound()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.AdminEmail,
            HotelManagementSystemWebApplicationFactory.AdminPassword);
        var token = await ReadAntiforgeryTokenAsync(await client.GetAsync("/Rooms"));

        using var response = await PostAsync(client, "/Admin/Users/missing-user/Edit",
            token, new Dictionary<string, string>
        {
            ["Id"] = "missing-user",
            ["FullName"] = "Missing",
            ["Email"] = "missing@example.com",
            ["Role"] = "Employee"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Admin_ToggleLockout_BlocksAndUnblocksUser()
    {
        var target = await CreateTestEmployeeAsync();
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.AdminEmail,
            HotelManagementSystemWebApplicationFactory.AdminPassword);
        var token = await ReadAntiforgeryTokenAsync(await client.GetAsync("/Rooms"));

        using var blockResponse = await PostAsync(client, $"/Admin/Users/{target.Id}/ToggleLockout", token);
        var blocked = await FindByIdAsync(target.Id);
        using var unblockResponse = await PostAsync(client, $"/Admin/Users/{target.Id}/ToggleLockout", token);
        var unblocked = await FindByIdAsync(target.Id);

        Assert.Equal(HttpStatusCode.Redirect, blockResponse.StatusCode);
        Assert.True(blocked!.LockoutEnd > DateTimeOffset.UtcNow);
        Assert.Equal(HttpStatusCode.Redirect, unblockResponse.StatusCode);
        Assert.Null(unblocked!.LockoutEnd);
    }

    [Fact]
    public async Task Admin_CannotToggleOwnLockout()
    {
        var admin = await FindByEmailAsync(HotelManagementSystemWebApplicationFactory.
            AdminEmail);
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            AdminEmail, HotelManagementSystemWebApplicationFactory.AdminPassword);
        var token = await ReadAntiforgeryTokenAsync(await client.GetAsync("/Rooms"));

        using var response = await PostAsync(client, $"/Admin/Users/{admin!.Id}/ToggleLockout", token);
        var saved = await FindByIdAsync(admin.Id);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Null(saved!.LockoutEnd);
    }

    [Fact]
    public async Task Admin_ChangeRole_AllowsAdminRoleWhenAnotherAdminExists()
    {
        var target = await CreateTestEmployeeAsync();
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            AdminEmail, HotelManagementSystemWebApplicationFactory.AdminPassword);
        var token = await ReadAntiforgeryTokenAsync(await client.GetAsync("/Rooms"));

        using var response = await PostAsync(client, $"/Admin/Users/{target.Id}/ChangeRole", token, new Dictionary<string, string>
        {
            ["role"] = "Admin"
        });
        var roles = await GetRolesAsync(target.Id);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("Admin", roles);
    }

    [Fact]
    public async Task Admin_ChangeRoleWithInvalidRole_DoesNotChangeUserRole()
    {
        var target = await CreateTestEmployeeAsync();
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            AdminEmail, HotelManagementSystemWebApplicationFactory.AdminPassword);
        var token = await ReadAntiforgeryTokenAsync(await client.GetAsync("/Rooms"));

        using var response = await PostAsync(client, $"/Admin/Users/{target.Id}/ChangeRole", token, new Dictionary<string, string>
        {
            ["role"] = "Manager"
        });
        var roles = await GetRolesAsync(target.Id);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(["Employee"], roles);
    }

    private HttpClient CreateClient() => factory.CreateClient
        (new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    private static async Task LoginAsync(HttpClient client, string email,
        string password)
    {
        var page = await client.GetAsync("/Account/Login");
        var token = await ReadAntiforgeryTokenAsync(page);
        using var response = await PostAsync(client, "/Account/Login", token, 
            new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password,
            ["RememberMe"] = "false"
        });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, 
        string path, string token)
        => PostAsync(client, path, token, new Dictionary<string, string>());

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, 
        string path, string token, Dictionary<string, string> values)
    {
        values["__RequestVerificationToken"] = token;
        return client.PostAsync(path, new FormUrlEncodedContent(values));
    }

    private static async Task<string> ReadAntiforgeryTokenAsync
        (HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(html, "<input[^>]+name=\"__RequestVerificationToken\"[^>]+value=\"([^\"]+)\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        Assert.True(match.Success, "The response did not contain an antiforgery token.");
        return match.Groups[1].Value;
    }

    private async Task<ApplicationUser> CreateTestEmployeeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService
            <UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            UserName = $"target-{Guid.NewGuid():N}@example.com",
            Email = $"target-{Guid.NewGuid():N}@example.com",
            EmailConfirmed = true,
            FullName = "Target Employee"
        };
        var result = await userManager.CreateAsync(user, "Password123");
        Assert.True(result.Succeeded, string.Join("; ", result.Errors.
            Select(error => error.Description)));
        var roleResult = await userManager.AddToRoleAsync(user, "Employee");
        Assert.True(roleResult.Succeeded);
        return user;
    }

    private async Task<ApplicationUser?> FindByEmailAsync(string email)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<UserManager
            <ApplicationUser>>().FindByEmailAsync(email);
    }

    private async Task<ApplicationUser?> FindByIdAsync(string id)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<UserManager
            <ApplicationUser>>().FindByIdAsync(id);
    }

    private async Task<IList<string>> GetRolesAsync(string id)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager
            <ApplicationUser>>();
        var user = await userManager.FindByIdAsync(id);
        return user is null ? [] : await userManager.GetRolesAsync(user);
    }

    private async Task<int> CountByEmailAsync(string email)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<UserManager
            <ApplicationUser>>().Users.CountAsync(user => user.Email == email);
    }
}
