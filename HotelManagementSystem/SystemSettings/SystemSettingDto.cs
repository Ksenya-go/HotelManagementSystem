namespace HotelManagementSystem.Application.SystemSettings;

public sealed record SystemSettingDto(
    int Id,
    string Key,
    string Value,
    string Description);


