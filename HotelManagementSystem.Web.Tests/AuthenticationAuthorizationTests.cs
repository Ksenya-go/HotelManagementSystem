using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HotelManagementSystem.Web.Tests;

public sealed class AuthenticationAuthorizationTests : IClassFixture<HotelManagementSystemWebApplicationFactory>
{
    private readonly HotelManagementSystemWebApplicationFactory factory;

    public AuthenticationAuthorizationTests(HotelManagementSystemWebApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Login_WithValidCredentials_RedirectsToReservations()
    {
        using var client = CreateClient();
        var loginPage = await client.GetAsync("/Account/Login");
        var loginToken = await ReadAntiforgeryTokenAsync(loginPage);

        using var response = await PostFormAsync(client, "/Account/Login", 
            loginToken, new Dictionary<string, string>
        {
            ["Email"] = HotelManagementSystemWebApplicationFactory.EmployeeEmail,
            ["Password"] = HotelManagementSystemWebApplicationFactory.EmployeePassword,
            ["RememberMe"] = "false"
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);

        using var reservations = await client.GetAsync("/Reservations");
        Assert.Equal(HttpStatusCode.OK, reservations.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_DoesNotAuthenticate()
    {
        using var client = CreateClient();
        var loginPage = await client.GetAsync("/Account/Login");
        var loginToken = await ReadAntiforgeryTokenAsync(loginPage);

        using var response = await PostFormAsync(client, "/Account/Login", 
            loginToken, new Dictionary<string, string>
        {
            ["Email"] = HotelManagementSystemWebApplicationFactory.EmployeeEmail,
            ["Password"] = "WrongPassword123",
            ["RememberMe"] = "false"
        });

        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Невірний", WebUtility.HtmlDecode(body), 
            StringComparison.Ordinal);

        using var reservations = await client.GetAsync("/Reservations");
        Assert.Equal(HttpStatusCode.Redirect, reservations.StatusCode);
        Assert.Contains("/Account/Login", reservations.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_DoesNotAuthenticate()
    {
        using var client = CreateClient();
        var loginPage = await client.GetAsync("/Account/Login");
        var loginToken = await ReadAntiforgeryTokenAsync(loginPage);

        using var response = await PostFormAsync(client, 
            "/Account/Login", loginToken, new Dictionary<string, string>
        {
            ["Email"] = "unknown.tests@example.com",
            ["Password"] = "AnyPassword123",
            ["RememberMe"] = "false"
        });

        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Невірний", WebUtility.HtmlDecode(body), 
            StringComparison.Ordinal);

        using var reservations = await client.GetAsync("/Reservations");
        Assert.Equal(HttpStatusCode.Redirect, reservations.StatusCode);
        Assert.Contains("/Account/Login", reservations.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Logout_AuthenticatedUser_CannotAccessProtectedPage()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.EmployeeEmail, HotelManagementSystemWebApplicationFactory.EmployeePassword);

        using var beforeLogout = await client.GetAsync("/Reservations");
        Assert.Equal(HttpStatusCode.OK, beforeLogout.StatusCode);

        var page = await client.GetAsync("/Reservations");
        var logoutToken = await ReadAntiforgeryTokenAsync(page);
        using var logout = await PostFormAsync(client, "/Account/Logout", 
            logoutToken, new Dictionary<string, string>());

        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
        Assert.Equal("/Account/Login", logout.Headers.Location?.OriginalString);

        using var afterLogout = await client.GetAsync("/Reservations");
        Assert.Equal(HttpStatusCode.Redirect, afterLogout.StatusCode);
        Assert.Contains("/Account/Login", afterLogout.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Employee_CannotAccessAdminSettings()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.EmployeeEmail, HotelManagementSystemWebApplicationFactory.EmployeePassword);

        foreach (var path in new[] { "/Admin/Settings", "/Admin/Users", 
            "/Admin/RoomTypes" })
        {
            using var response = await client.GetAsync(path);
            AssertAuthorizationDenied(response);
        }
    }

    [Fact]
    public async Task Admin_CanAccessAdminSections()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.AdminEmail, HotelManagementSystemWebApplicationFactory.AdminPassword);

        foreach (var path in new[] { "/Admin/Users", "/Admin/Settings", 
            "/Admin/RoomTypes" })
        {
            using var response = await client.GetAsync(path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    private HttpClient CreateClient() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    private static async Task LoginAsync(HttpClient client, string email, string password)
    {
        var loginPage = await client.GetAsync("/Account/Login");
        var token = await ReadAntiforgeryTokenAsync(loginPage);
        using var response = await PostFormAsync(client, "/Account/Login", token, new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password,
            ["RememberMe"] = "false"
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private static async Task<HttpResponseMessage> PostFormAsync(
        HttpClient client,
        string path,
        string antiforgeryToken,
        Dictionary<string, string> values)
    {
        values["__RequestVerificationToken"] = antiforgeryToken;
        return await client.PostAsync(path, new FormUrlEncodedContent(values));
    }

    private static async Task<string> ReadAntiforgeryTokenAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(
            html,
            "<input[^>]+name=\"__RequestVerificationToken\"[^>]+value=\"([^\"]+)\"",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        Assert.True(match.Success, "The response did not contain an antiforgery token.");
        return match.Groups[1].Value;
    }

    private static void AssertAuthorizationDenied(HttpResponseMessage response)
    {
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.Redirect,
            $"Expected 403 or redirect, received {(int)response.StatusCode} {response.StatusCode}.");

        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            Assert.Contains("/Account/AccessDenied", response.Headers.Location?.OriginalString);
        }
    }
}
