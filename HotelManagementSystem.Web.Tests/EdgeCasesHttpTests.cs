using System.Net;
using HotelManagementSystem.Web.Tests;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HotelManagementSystem.Web.Tests;

public sealed class EdgeCasesHttpTests : 
    IClassFixture<HotelManagementSystemWebApplicationFactory>
{
    private readonly HotelManagementSystemWebApplicationFactory factory;

    public EdgeCasesHttpTests(HotelManagementSystemWebApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Theory]
    [InlineData("/Rooms?minPrice=abc")]
    [InlineData("/Rooms?pageNumber=-1")]
    [InlineData("/Rooms?pageNumber=999999")]
    [InlineData("/Rooms?minPrice=-100")]
    [InlineData("/Rooms?minPrice=500&maxPrice=100")]
    [InlineData("/Rooms?roomType=%F0%9F%8F%A8")]
    public async Task Employee_RoomFilterBoundaryQuery_ShouldNotReturnServerError
        (string path)
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.EmployeeEmail,
            HotelManagementSystemWebApplicationFactory.EmployeePassword);

        using var response = await client.GetAsync(path);

        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task Employee_RoomFilterWithVeryLongSearchValue_ShouldNotReturnServerError()
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.EmployeeEmail,
            HotelManagementSystemWebApplicationFactory.EmployeePassword);

        using var response = await client.GetAsync($"/Rooms?roomType={new string
            ('x', 5000)}");

        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }


    [Theory]
    [InlineData("/Rooms/Booking?pageNumber=0")]
    [InlineData("/Rooms/Booking?pageNumber=-1")]
    [InlineData("/Rooms/Booking?pageNumber=999999")]
    [InlineData("/Rooms/Booking?guestsCount=0")]
    [InlineData("/Rooms/Booking?guestsCount=-1")]
    [InlineData("/Rooms/Booking?startDate=2030-01-02&endDate=2030-01-02")]
    public async Task Employee_BookingBoundaryQuery_ShouldNotReturnServerError(string path)
    {
        using var client = CreateClient();
        await LoginAsync(client, HotelManagementSystemWebApplicationFactory.EmployeeEmail,
            HotelManagementSystemWebApplicationFactory.EmployeePassword);

        using var response = await client.GetAsync(path);

        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    private HttpClient CreateClient() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    private static async Task LoginAsync(HttpClient client, string email, 
        string password)
    {
        using var loginPage = await client.GetAsync("/Account/Login");
        var html = await loginPage.Content.ReadAsStringAsync();
        var match = System.Text.RegularExpressions.Regex.Match(
            html,
            "<input[^>]+name=\"__RequestVerificationToken\"[^>]+value=\"([^\"]+)\"",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | 
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);
        Assert.True(match.Success);

        using var response = await client.PostAsync(
            "/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = email,
                ["Password"] = password,
                ["RememberMe"] = "false",
                ["__RequestVerificationToken"] = match.Groups[1].Value
            }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }
}
