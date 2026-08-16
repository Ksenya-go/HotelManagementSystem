using HotelManagementSystem.Domain.Entities;
using HotelManagementSystem.Persistence.EfCore.Identity;
using HotelManagementSystem.Persistence.EfCore.SystemSettings;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Web.Tests.Settings;

public sealed class SettingsTestFixture : IAsyncDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");

    public ApplicationDbContext DbContext { get; }
    public SystemSettingService Settings { get; }

    public SettingsTestFixture()
    {
        connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        DbContext = new ApplicationDbContext(options);
        DbContext.Database.EnsureCreated();
        Settings = new SystemSettingService(DbContext);
    }

    public SystemSetting AddSetting(
        string key = "hotel.checkInTime",
        string value = "14:00",
        string description = "Check-in time")
    {
        var setting = new SystemSetting(key, value, description);
        DbContext.SystemSettings.Add(setting);
        DbContext.SaveChanges();
        return setting;
    }

    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        return new ApplicationDbContext(options);
    }

    public async ValueTask DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await connection.DisposeAsync();
    }
}
