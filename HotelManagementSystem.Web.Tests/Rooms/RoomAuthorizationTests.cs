using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HotelManagementSystem.Web.Tests.Rooms;

public sealed class RoomAuthorizationTests : 
    IClassFixture<HotelManagementSystemWebApplicationFactory>
{
    private readonly HotelManagementSystemWebApplicationFactory factory;

    public RoomAuthorizationTests(HotelManagementSystemWebApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Employee_CanAccessRoomsIndex()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            EmployeeEmail, HotelManagementSystemWebApplicationFactory.EmployeePassword);

        using var response = await client.GetAsync("/Rooms");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AnonymousUser_CannotAccessRoomsIndex()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync("/Rooms");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Employee_CannotDeleteRoom()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            EmployeeEmail, HotelManagementSystemWebApplicationFactory.EmployeePassword);
        var token = await ReadAntiforgeryTokenAsync(await client.GetAsync("/Rooms"));

        using var response = await PostAsync(client, "/Rooms/9999/Delete", token);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.
            OriginalString);
    }

    [Fact]
    public async Task Admin_CanReachDeleteRoomEndpoint()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            AdminEmail, HotelManagementSystemWebApplicationFactory.AdminPassword);
        var token = await ReadAntiforgeryTokenAsync(await client.GetAsync("/Rooms"));

        using var response = await PostAsync(client, "/Rooms/9999/Delete", token);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Rooms", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Employee_CanOpenBookingForValidDates()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            EmployeeEmail, HotelManagementSystemWebApplicationFactory.EmployeePassword);

        using var response = await client.GetAsync
            ("/Rooms/Booking?startDate=2030-01-10&endDate=2030-01-12&guestsCount=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Booking_ShouldReturnOkWithValidationMessage_WhenDateRangeIsInvalid()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.EmployeeEmail, HotelManagementSystemWebApplicationFactory.EmployeePassword);

        using var response = await client.GetAsync
            ("/Rooms/Booking?startDate=2030-01-12&endDate=2030-01-12");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Дата закінчення має бути пізнішою за дату початку", 
            System.Net.WebUtility.HtmlDecode(body), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RoomsIndex_ShouldNotReturnServerError_WhenFloorIsNotNumeric()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.
            EmployeeEmail, HotelManagementSystemWebApplicationFactory.EmployeePassword);

        using var response = await client.GetAsync("/Rooms?floor=not-a-number");

        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
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

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, 
        string path, string token, Dictionary<string, string> values)
    {
        values["__RequestVerificationToken"] = token;
        return client.PostAsync(path, new FormUrlEncodedContent(values));
    }

    private static async Task<string> ReadAntiforgeryTokenAsync(HttpResponseMessage 
        response)
    {
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(html, "<input[^>]+name=\"__RequestVerificationToken\"[^>]+value=\"([^\"]+)\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        Assert.True(match.Success, "The response did not contain an antiforgery token.");
        return match.Groups[1].Value;
    }
}
