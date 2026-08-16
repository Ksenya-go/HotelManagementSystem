using System.Net;
using System.Text.RegularExpressions;
using HotelManagementSystem.Persistence.EfCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HotelManagementSystem.Web.Tests.Settings;

public sealed class SettingsAuthorizationTests : IClassFixture<HotelManagementSystemWebApplicationFactory>
{
    private readonly HotelManagementSystemWebApplicationFactory factory;

    public SettingsAuthorizationTests(HotelManagementSystemWebApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Admin_CanAccessSettings()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            AdminEmail, HotelManagementSystemWebApplicationFactory.AdminPassword);

        using var response = await client.GetAsync("/Admin/Settings");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Employee_CannotAccessSettings()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            EmployeeEmail, HotelManagementSystemWebApplicationFactory.EmployeePassword);

        using var response = await client.GetAsync("/Admin/Settings");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.
            Location?.OriginalString);
    }

    [Fact]
    public async Task AnonymousUser_IsRedirectedToLogin()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync("/Admin/Settings");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Admin_CanUpdateExistingSettingAndChangeIsPersisted()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            AdminEmail, HotelManagementSystemWebApplicationFactory.AdminPassword);
        var settingId = await FindOrCreateSettingAsync("hotel.checkInTime", "14:00");
        var token = await ReadAntiforgeryTokenAsync(await client.GetAsync
            ("/Admin/Settings"));

        using var response = await PostAsync(client, "/Admin/Settings/Update", 
            token, new Dictionary<string, string>
        {
            ["Id"] = settingId.ToString(),
            ["Value"] = "15:30"
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Admin/Settings", response.Headers.Location?.OriginalString);
        Assert.Equal("15:30", await ReadSettingValueAsync(settingId));
    }

    [Fact]
    public async Task Admin_PostWithoutAntiforgeryToken_IsRejected()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            AdminEmail, HotelManagementSystemWebApplicationFactory.AdminPassword);
        var settingId = await FindOrCreateSettingAsync("hotel.checkOutTime", "12:00");

        using var response = await client.PostAsync("/Admin/Settings/Update", 
            new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Id"] = settingId.ToString(),
            ["Value"] = "11:00"
        }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("12:00", await ReadSettingValueAsync(settingId));
    }

    [Fact]
    public async Task Admin_PostWithEmptyValue_ReturnsValidationViewAndDoesNotPersist()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.AdminEmail,
            HotelManagementSystemWebApplicationFactory.AdminPassword);
        var settingId = await FindOrCreateSettingAsync("hotel.currency", "UAH");
        var token = await ReadAntiforgeryTokenAsync(await client.GetAsync
            ("/Admin/Settings"));

        using var response = await PostAsync(client, "/Admin/Settings/Update", 
            token, new Dictionary<string, string>
        {
            ["Id"] = settingId.ToString(),
            ["Value"] = "   "
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("UAH", await ReadSettingValueAsync(settingId));
    }

    [Fact]
    public async Task Employee_CannotUpdateSettingsThroughProtectedPost()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            EmployeeEmail, HotelManagementSystemWebApplicationFactory.EmployeePassword);
        var settingId = await FindOrCreateSettingAsync("hotel.checkInTime", "14:00");
        var initialValue = await ReadSettingValueAsync(settingId);

        using var response = await client.PostAsync("/Admin/Settings/Update", 
            new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Id"] = settingId.ToString(),
            ["Value"] = "16:00"
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.
            OriginalString);
        Assert.Equal(initialValue, await ReadSettingValueAsync(settingId));
    }

    [Fact]
    public async Task AnonymousUser_CannotUpdateSettingsThroughProtectedPost()
    {
        using var client = CreateClient();

        using var response = await client.PostAsync("/Admin/Settings/Update",
            new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Id"] = "1",
            ["Value"] = "16:00"
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString);
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
        var loginPage = await client.GetAsync("/Account/Login");
        var token = await ReadAntiforgeryTokenAsync(loginPage);
        using var response = await PostAsync(client, "/Account/Login", 
            token, new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password,
            ["RememberMe"] = "false"
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private async Task<int> FindOrCreateSettingAsync(string key, string value)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var setting = await db.SystemSettings.SingleOrDefaultAsync
            (item => item.Key == key);
        if (setting is null)
        {
            setting = new Domain.Entities.SystemSetting(key, value, key);
            db.SystemSettings.Add(setting);
            await db.SaveChangesAsync();
        }

        return setting.Id;
    }

    private async Task<string> ReadSettingValueAsync(int id)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return (await db.SystemSettings.AsNoTracking().SingleAsync
            (setting => setting.Id == id)).Value;
    }

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
}
