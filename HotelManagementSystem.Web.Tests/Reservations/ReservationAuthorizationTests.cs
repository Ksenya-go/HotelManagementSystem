using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HotelManagementSystem.Web.Tests;

public sealed class ReservationAuthorizationTests : 
    IClassFixture<HotelManagementSystemWebApplicationFactory>
{
    private readonly HotelManagementSystemWebApplicationFactory factory;

    public ReservationAuthorizationTests
        (HotelManagementSystemWebApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Employee_CannotDeleteReservation()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            EmployeeEmail, HotelManagementSystemWebApplicationFactory.EmployeePassword);
        var token = await ReadAntiforgeryTokenAsync(await client.GetAsync
            ("/Reservations"));

        using var response = await PostAsync(client, "/Reservations/9999/Delete", token);

        Assert.True(response.StatusCode == HttpStatusCode.Forbidden 
            || response.StatusCode == HttpStatusCode.Redirect);
        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            Assert.DoesNotContain("/Reservations", response.Headers.Location?.
                OriginalString);
        }
    }

    [Fact]
    public async Task Admin_CanReachDeleteReservationEndpoint()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            AdminEmail, HotelManagementSystemWebApplicationFactory.AdminPassword);
        var token = await ReadAntiforgeryTokenAsync(await client.
            GetAsync("/Reservations"));

        using var response = await PostAsync(client, "/Reservations/9999/Delete", token);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Reservations", response.Headers.Location?.OriginalString);
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

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, 
        string path, string token)
    {
        return PostAsync(client, path, token, new Dictionary<string, string>());
    }

    private static Task<HttpResponseMessage> PostAsync(
        HttpClient client,
        string path,
        string token,
        Dictionary<string, string> values)
    {
        values["__RequestVerificationToken"] = token;
        return client.PostAsync(path, new FormUrlEncodedContent(values));
    }

    private static async Task<string> ReadAntiforgeryTokenAsync
        (HttpResponseMessage response)
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
}
