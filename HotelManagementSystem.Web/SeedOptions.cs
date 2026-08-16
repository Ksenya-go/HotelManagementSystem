namespace HotelManagementSystem.Web;

public sealed class SeedOptions
{
    public string[] Roles { get; init; } = [];
    public SystemSettingSeed[] SystemSettings { get; init; } = [];
}

public sealed class SystemSettingSeed
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
