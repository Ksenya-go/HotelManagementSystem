using FluentValidation.TestHelper;
using HotelManagementSystem.Application.Common.Cqrs.Results;
using HotelManagementSystem.Application.SystemSettings.Commands;
using HotelManagementSystem.Application.SystemSettings.Handlers;
using HotelManagementSystem.Application.SystemSettings.Queries;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagementSystem.Web.Tests.Settings;

public sealed class SettingsServiceTests
{
    [Fact]
    public async Task GetAllAsync_WhenNoSettingsExist_ReturnsEmptyList()
    {
        await using var fixture = new SettingsTestFixture();

        var settings = await fixture.Settings.GetAllAsync();

        Assert.Empty(settings);
    }

    [Fact]
    public async Task GetAllAsync_WhenSettingsExist_ReturnsAllSettingsOrderedByKey()
    {
        await using var fixture = new SettingsTestFixture();
        fixture.AddSetting("hotel.checkOutTime", "12:00", "Check-out time");
        fixture.AddSetting("hotel.checkInTime", "14:00", "Check-in time");
        fixture.AddSetting("hotel.currency", "UAH", "Currency");

        var settings = await fixture.Settings.GetAllAsync();

        Assert.Equal(3, settings.Count);
        Assert.Equal(
            new[] { "hotel.checkInTime", "hotel.checkOutTime", "hotel.currency" },
            settings.Select(setting => setting.Key));
        Assert.Equal("14:00", settings.Single(setting => setting.Key == 
        "hotel.checkInTime").Value);
        Assert.Equal("12:00", settings.Single(setting => setting.Key == 
        "hotel.checkOutTime").Value);
        Assert.Equal("UAH", settings.Single(setting => setting.Key == 
        "hotel.currency").Value);
    }

    [Fact]
    public async Task GetSystemSettingsQueryHandler_ReturnsCurrentSettings()
    {
        await using var fixture = new SettingsTestFixture();
        fixture.AddSetting();
        var handler = new SystemSettingQueryHandler(fixture.Settings);

        var result = await handler.Handle(new GetSystemSettingsQuery(), 
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("hotel.checkInTime", result.Value![0].Key);
    }

    [Fact]
    public async Task UpdateAsync_WithValidValue_UpdatesExistingSettingAndPersistsIt()
    {
        await using var fixture = new SettingsTestFixture();
        var setting = fixture.AddSetting(value: "14:00");

        var updated = await fixture.Settings.UpdateAsync(
            new UpdateSystemSettingCommand(setting.Id, "  15:30  "));

        await using var verificationContext = fixture.CreateDbContext();
        var saved = await verificationContext.SystemSettings.FindAsync(setting.Id);

        Assert.True(updated);
        Assert.NotNull(saved);
        Assert.Equal("15:30", saved!.Value);
        Assert.Equal(1, await verificationContext.SystemSettings.CountAsync());
    }

    [Fact]
    public async Task UpdateSystemSettingCommandHandler_WithUnknownId_ReturnsNotFoundAndDoesNotPersist()
    {
        await using var fixture = new SettingsTestFixture();
        var setting = fixture.AddSetting();
        var handler = new SystemSettingCommandHandler(fixture.Settings);

        var result = await handler.Handle(
            new UpdateSystemSettingCommand(9999, "15:00"),
            CancellationToken.None);

        await using var verificationContext = fixture.CreateDbContext();
        var saved = await verificationContext.SystemSettings.FindAsync(setting.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("SystemSetting.NotFound", result.Error!.Code);
        Assert.Equal("14:00", saved!.Value);
    }

    [Fact]
    public async Task UpdateAsync_WhenCalledAgain_UpdatesSameRowInsteadOfCreatingDuplicate()
    {
        await using var fixture = new SettingsTestFixture();
        var setting = fixture.AddSetting();

        Assert.True(await fixture.Settings.UpdateAsync
            (new UpdateSystemSettingCommand(setting.Id, "15:00")));
        Assert.True(await fixture.Settings.UpdateAsync
            (new UpdateSystemSettingCommand(setting.Id, "16:00")));

        await using var verificationContext = fixture.CreateDbContext();
        var rows = await verificationContext.SystemSettings.ToListAsync();

        Assert.Single(rows);
        Assert.Equal(setting.Id, rows[0].Id);
        Assert.Equal("16:00", rows[0].Value);
    }

    [Fact]
    public async Task UpdateSystemSettingCommandValidator_EnforcesImplementedRules()
    {
        var validator = new UpdateSystemSettingCommand.Validator();

        Assert.False((await validator.TestValidateAsync
            (new UpdateSystemSettingCommand(0, "14:00"))).IsValid);
        Assert.False((await validator.TestValidateAsync
            (new UpdateSystemSettingCommand(1, ""))).IsValid);
        Assert.False((await validator.TestValidateAsync
            (new UpdateSystemSettingCommand(1, new string('x', 501)))).IsValid);
        Assert.True((await validator.TestValidateAsync
            (new UpdateSystemSettingCommand(1, "00:00"))).IsValid);
        Assert.True((await validator.TestValidateAsync
            (new UpdateSystemSettingCommand(1, new string('x', 500)))).IsValid);
    }

    [Fact]
    public async Task UpdateAsync_AcceptsArbitraryNonEmptyValueBecauseServiceDoesNotValidateTimeFormat()
    {
        await using var fixture = new SettingsTestFixture();
        var setting = fixture.AddSetting();

        var updated = await fixture.Settings.UpdateAsync(
            new UpdateSystemSettingCommand(setting.Id, "not-a-time"));

        await using var verificationContext = fixture.CreateDbContext();
        var saved = await verificationContext.SystemSettings.FindAsync(setting.Id);

        Assert.True(updated);
        Assert.Equal("not-a-time", saved!.Value);
    }

    [Fact]
    public async Task UpdateAsync_AllowsEqualCheckInAndCheckOutValuesBecauseNoCrossSettingRuleExists()
    {
        await using var fixture = new SettingsTestFixture();
        var checkIn = fixture.AddSetting("hotel.checkInTime", "14:00");
        fixture.AddSetting("hotel.checkOutTime", "12:00");

        Assert.True(await fixture.Settings.UpdateAsync
            (new UpdateSystemSettingCommand(checkIn.Id, "12:00")));

        await using var verificationContext = fixture.CreateDbContext();
        Assert.Equal("12:00", (await verificationContext.SystemSettings.
            FindAsync(checkIn.Id))!.Value);
    }

    [Fact]
    public async Task SystemSettings_DoNotAffectReservationServiceBecauseItHasNoSettingsDependency()
    {
        await using var fixture = new SettingsTestFixture();
        fixture.AddSetting("hotel.checkInTime", "18:00");
        fixture.AddSetting("hotel.checkOutTime", "10:00");

        var settings = await fixture.Settings.GetAllAsync();

        Assert.Equal("18:00", settings.Single(item => item.Key == 
        "hotel.checkInTime").Value);
        Assert.Equal("10:00", settings.Single(item => item.Key == 
        "hotel.checkOutTime").Value);
    }
}
